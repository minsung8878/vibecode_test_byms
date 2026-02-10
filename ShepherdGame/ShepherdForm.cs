using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShepherdGame
{
    public partial class ShepherdForm : Form
    {
        private int currentLevel = 1;
        private int targetSheepCount;
        private int liveSheepCount;
        private int deathSheepCount;
        private int startSheepCount;

        private List<Sheep> sheepList = new List<Sheep>();
        private List<Wolf> wolfList = new List<Wolf>();
        private Rectangle targetArea;
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();

        private float baseWolfSpeed = 1.0f;
        private float baseSheepSpeed = 1.5f;

        public ShepherdForm()
        {
            this.Text = "Shepherd Game";
            this.Size = new Size(800, 600);
            this.DoubleBuffered = true;
            this.MouseDown += (s, e) => HandleMouseClick(e.Location);

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 20; // ~50 FPS
            gameTimer.Tick += (s, e) => UpdateGameState();

            InitializeLevel();
        }

        private void InitializeLevel()
        {
            sheepList.Clear();
            wolfList.Clear();
            deathSheepCount = 0;
            liveSheepCount = 0;

            startSheepCount = Math.Min(currentLevel + 1, 10);
            targetSheepCount = (int)Math.Round(startSheepCount * 0.7);

            // Set up Target Area
            float areaPercentage;
            if (currentLevel <= 10)
                areaPercentage = (float)(random.NextDouble() * 0.05 + 0.15); // 15% - 20%
            else
                areaPercentage = (float)(random.NextDouble() * 0.05 + 0.10); // 10% - 15%

            int areaWidth = (int)(this.ClientSize.Width * Math.Sqrt(areaPercentage));
            int areaHeight = (int)(this.ClientSize.Height * Math.Sqrt(areaPercentage));
            targetArea = new Rectangle(
                random.Next(50, this.ClientSize.Width - areaWidth - 50),
                random.Next(50, this.ClientSize.Height - areaHeight - 50),
                areaWidth,
                areaHeight
            );

            // Create Sheep
            for (int i = 0; i < startSheepCount; i++)
            {
                PointF pos;
                do {
                    pos = new PointF(random.Next(20, this.ClientSize.Width - 40), random.Next(20, this.ClientSize.Height - 40));
                } while (targetArea.Contains(Point.Round(pos)));

                sheepList.Add(new Sheep(pos, baseSheepSpeed));
            }

            // Create Wolves
            if (currentLevel >= 5)
            {
                int wolfCount = Math.Min(currentLevel - 4, 5);
                float speedBonus = 0;
                if (currentLevel > 9) speedBonus = (currentLevel - 9) * 0.1f;

                for (int i = 0; i < wolfCount; i++)
                {
                    PointF pos;
                    do {
                        pos = new PointF(random.Next(20, this.ClientSize.Width - 40), random.Next(20, this.ClientSize.Height - 40));
                    } while (targetArea.Contains(Point.Round(pos)));

                    wolfList.Add(new Wolf(pos, baseWolfSpeed + speedBonus));
                }
            }

            liveSheepCount = sheepList.Count;
            gameTimer.Start();
        }

        private void UpdateGameState()
        {
            Rectangle bounds = this.ClientRectangle;

            // Move Sheep
            foreach (var sheep in sheepList)
            {
                Rectangle moveBounds = sheep.IsSafe ? targetArea : bounds;
                sheep.Move(moveBounds);

                if (!sheep.IsSafe && targetArea.Contains(Point.Round(new PointF(sheep.Position.X + sheep.Size.Width/2, sheep.Position.Y + sheep.Size.Height/2))))
                {
                    sheep.IsSafe = true;
                }
            }

            // Move Wolves
            foreach (var wolf in wolfList)
            {
                float wolfCurrentSpeed = wolf.IsBoosting ? wolf.Speed * 2 : wolf.Speed;
                PointF nextPos = new PointF(wolf.Position.X + wolf.Velocity.X * wolfCurrentSpeed, wolf.Position.Y + wolf.Velocity.Y * wolfCurrentSpeed);
                if (targetArea.IntersectsWith(new Rectangle(Point.Round(nextPos), wolf.Size)))
                {
                    // Bounce off target area
                    wolf.Velocity = new PointF(-wolf.Velocity.X, -wolf.Velocity.Y);
                }
                wolf.Move(bounds);

                // Check collision with sheep
                var sheepToKill = new List<Sheep>();
                foreach (var sheep in sheepList)
                {
                    if (!sheep.IsSafe && wolf.CheckCollision(sheep))
                    {
                        sheepToKill.Add(sheep);
                    }
                }

                foreach (var sheep in sheepToKill)
                {
                    sheepList.Remove(sheep);
                    deathSheepCount++;
                }
            }
            liveSheepCount = sheepList.Count(s => !s.IsSafe);

            int safeCount = sheepList.Count(s => s.IsSafe);

            // Victory Check
            if (safeCount >= targetSheepCount)
            {
                gameTimer.Stop();
                DialogResult res = MessageBox.Show($"Level {currentLevel} Cleared!", "Victory", MessageBoxButtons.OK);
                if (res == DialogResult.OK)
                {
                    currentLevel++;
                    InitializeLevel();
                }
            }
            // Failure Check
            else if (deathSheepCount >= startSheepCount * 0.3)
            {
                gameTimer.Stop();
                DialogResult res = MessageBox.Show("Game Over! Try again?", "Failure", MessageBoxButtons.RetryCancel);
                if (res == DialogResult.Retry)
                {
                    ResetGame(false);
                }
                else
                {
                    ResetGame(true);
                }
            }

            this.Invalidate();
        }

        private void HandleMouseClick(Point clickPos)
        {
            PointF clickPoint = new PointF(clickPos.X, clickPos.Y);

            foreach (var sheep in sheepList)
            {
                if (sheep.GetBounds().Contains(clickPoint))
                {
                    sheep.OnClicked(true, clickPoint);
                }
                else if (GetDistance(sheep.Position, clickPoint) < 50)
                {
                    sheep.OnClicked(false, clickPoint);
                }
            }

            foreach (var wolf in wolfList)
            {
                if (wolf.GetBounds().Contains(clickPoint))
                {
                    wolf.OnClicked(true, clickPoint);
                }
                else if (GetDistance(wolf.Position, clickPoint) < 50)
                {
                    wolf.OnClicked(false, clickPoint);
                }
            }
        }

        private float GetDistance(PointF p1, PointF p2)
        {
            float dx = p1.X - p2.X;
            float dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void ResetGame(bool fullReset)
        {
            if (fullReset) currentLevel = 1;
            InitializeLevel();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Draw Target Area
            using (Brush b = new SolidBrush(Color.FromArgb(100, 0, 255, 0)))
            {
                g.FillRectangle(b, targetArea);
                g.DrawRectangle(Pens.Green, targetArea);
            }

            // Draw Sheep
            foreach (var sheep in sheepList)
            {
                g.FillEllipse(Brushes.Blue, sheep.GetBounds());
            }

            // Draw Wolves
            foreach (var wolf in wolfList)
            {
                g.FillEllipse(Brushes.Red, wolf.GetBounds());
            }

            // Draw HUD
            string hud = $"Level: {currentLevel} | Target: {targetSheepCount} | Live: {sheepList.Count} | Death: {deathSheepCount} | Safe: {sheepList.Count(s => s.IsSafe)}";
            g.DrawString(hud, new Font("Arial", 12), Brushes.Black, new PointF(10, 10));
        }
    }
}

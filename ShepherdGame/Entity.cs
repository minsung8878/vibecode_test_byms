using System;
using System.Drawing;

namespace ShepherdGame
{
    public abstract class Entity
    {
        public PointF Position { get; set; }
        public PointF Velocity { get; set; }
        public float Speed { get; set; }
        public float BaseSpeed { get; set; }
        public Size Size { get; set; }
        public bool IsBoosting { get; set; }
        public DateTime BoostEndTime { get; set; }

        public Entity(PointF position, float speed, Size size)
        {
            Position = position;
            BaseSpeed = speed;
            Speed = speed;
            Size = size;
            Velocity = GetRandomVelocity();
        }

        public virtual void Move(Rectangle bounds)
        {
            float currentSpeed = IsBoosting ? Speed * 2 : Speed;

            if (IsBoosting && DateTime.Now > BoostEndTime)
            {
                IsBoosting = false;
            }

            Position = new PointF(
                Position.X + Velocity.X * currentSpeed,
                Position.Y + Velocity.Y * currentSpeed
            );

            // Bounce off walls
            if (Position.X < bounds.Left || Position.X + Size.Width > bounds.Right)
            {
                Velocity = new PointF(-Velocity.X, Velocity.Y);
                Position = new PointF(Math.Max(bounds.Left, Math.Min(Position.X, bounds.Right - Size.Width)), Position.Y);
            }
            if (Position.Y < bounds.Top || Position.Y + Size.Height > bounds.Bottom)
            {
                Velocity = new PointF(Velocity.X, -Velocity.Y);
                Position = new PointF(Position.X, Math.Max(bounds.Top, Math.Min(Position.Y, bounds.Bottom - Size.Height)));
            }
        }

        protected PointF GetRandomVelocity()
        {
            Random rand = new Random();
            double angle = rand.NextDouble() * Math.PI * 2;
            return new PointF((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public virtual void ApplyBoost(float durationSeconds)
        {
            IsBoosting = true;
            BoostEndTime = DateTime.Now.AddSeconds(durationSeconds);
        }

        public virtual void FleeFrom(PointF targetPos)
        {
            float dx = Position.X + Size.Width / 2 - targetPos.X;
            float dy = Position.Y + Size.Height / 2 - targetPos.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                Velocity = new PointF(dx / distance, dy / distance);
                ApplyBoost(1.0f);
            }
        }

        public virtual void RandomFlee()
        {
            Velocity = GetRandomVelocity();
            ApplyBoost(1.0f);
        }

        public RectangleF GetBounds()
        {
            return new RectangleF(Position.X, Position.Y, Size.Width, Size.Height);
        }
    }
}

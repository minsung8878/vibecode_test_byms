using System;
using System.Drawing;

namespace ShepherdGame
{
    public class Wolf : Entity
    {
        public Wolf(PointF position, float speed) : base(position, speed, new Size(25, 25))
        {
        }

        public override void Move(Rectangle bounds)
        {
            // Wolf cannot enter TargetArea - this will be handled in ShepherdForm's UpdateGameState
            base.Move(bounds);
        }

        public bool CheckCollision(Sheep target)
        {
            return GetBounds().IntersectsWith(target.GetBounds());
        }

        public void OnClicked(bool isDirect, PointF clickPos)
        {
            if (isDirect)
            {
                RandomFlee();
            }
            else
            {
                FleeFrom(clickPos);
            }
        }
    }
}

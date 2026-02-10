using System;
using System.Drawing;

namespace ShepherdGame
{
    public class Sheep : Entity
    {
        public bool IsSafe { get; set; }

        public Sheep(PointF position, float speed) : base(position, speed, new Size(20, 20))
        {
            IsSafe = false;
        }

        public override void Move(Rectangle bounds)
        {
            // If safe, bounds are restricted to the TargetArea in ShepherdForm logic
            base.Move(bounds);
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

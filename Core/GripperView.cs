using System;
using System.Collections.Generic;
using System.Text;

namespace FooEditEngine
{
    struct GripperRectangle
    {
        public Gripper TopLeft;
        public Gripper TopRight;
        public Gripper BottomLeft;
        public Gripper BottomRight;
        public GripperRectangle(Gripper bottom_left,Gripper bottom_right)
        {
            this.TopLeft = null;
            this.TopRight = null;
            this.BottomLeft = bottom_left;
            this.BottomRight = bottom_right;
        }
    }
    class Gripper
    {
        public const int GripperWidth = 10;
        public const int HitAreaWidth = 48;

        public Gripper()
        {
            this.Enabled = false;
        }
        public bool Enabled
        {
            get;
            set;
        }
        public Rectangle Rectangle
        {
            get;
            set;
        }
        public Rectangle HitArea
        {
            get;
            set;
        }
        public bool IsHit(Point p)
        {
            return this.Enabled && this.HitArea.IsHit(p);
        }

        public void Move(EditView view,TextPoint tp)
        {
            this.Rectangle = view.GetRectFromTextPoint(tp, Gripper.GripperWidth, Gripper.GripperWidth);
            this.HitArea = view.GetRectFromTextPoint(tp, Gripper.HitAreaWidth, Gripper.HitAreaWidth);
        }

        public void MoveByIndex(EditView view, int index)
        {
            this.Rectangle = view.GetRectFromIndex(index, Gripper.GripperWidth, Gripper.GripperWidth);
            this.HitArea = view.GetRectFromIndex(index, Gripper.HitAreaWidth, Gripper.HitAreaWidth);
        }

        public Point AdjustPoint(Point p)
        {
            Rectangle gripperRect = this.HitArea;

            if (gripperRect.IsHit(p))
                p.Y = gripperRect.Y - 1;
            else
                p.Y -= gripperRect.Height;

            //if (p.Y < this.Render.TextArea.Y)
            //    p.Y = this.Render.TextArea.Y;
            return p;
        }

        public void Draw(ITextRender render)
        {
            if (this.Enabled && this.Rectangle != Rectangle.Empty)
            {
                Rectangle gripperRect = this.Rectangle;
                double radius = gripperRect.Width / 2;
                Point point;
                point = new Point(gripperRect.X + radius, gripperRect.Y + radius);
                render.DrawGripper(point, radius);
            }
        }

        public bool Equals(Gripper other)
        {
            return this.Rectangle == other.Rectangle;
        }
    }
}


using System;
using System.Drawing;
using System.Windows;

namespace Advent.Common.Interop
{
    [Serializable]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Height
        {
            get
            {
                return this.Bottom - this.Top;
            }
        }

        public int Width
        {
            get
            {
                return this.Right - this.Left;
            }
        }

        public System.Windows.Size Size
        {
            get
            {
                return new System.Windows.Size((double)this.Width, (double)this.Height);
            }
        }

        public System.Windows.Point Location
        {
            get
            {
                return new System.Windows.Point((double)this.Left, (double)this.Top);
            }
        }

        public RECT(int left_, int top_, int right_, int bottom_)
        {
            this.Left = left_;
            this.Top = top_;
            this.Right = right_;
            this.Bottom = bottom_;
        }

        public static implicit operator Rectangle(Advent.Common.Interop.RECT rect)
        {
            return rect.ToRectangle();
        }

        public static implicit operator Advent.Common.Interop.RECT(Rectangle rect)
        {
            return Advent.Common.Interop.RECT.FromRectangle(rect);
        }

        public Rectangle ToRectangle()
        {
            return Rectangle.FromLTRB(this.Left, this.Top, this.Right, this.Bottom);
        }

        public static Advent.Common.Interop.RECT FromRectangle(Rectangle rectangle)
        {
            return new Advent.Common.Interop.RECT(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public override int GetHashCode()
        {
            return this.Left ^ (this.Top << 13 | this.Top >> 19) ^ (this.Width << 26 | this.Width >> 6) ^ (this.Height << 7 | this.Height >> 25);
        }
    }
}

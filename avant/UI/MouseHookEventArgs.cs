
using System.Windows.Forms;

namespace Advent.Common.UI
{
    public class MouseHookEventArgs : MouseEventArgs
    {
        public bool Handled { get; set; }

        public MouseHookEventArgs(MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
        }
    }
}

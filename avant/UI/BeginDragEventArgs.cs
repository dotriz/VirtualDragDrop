
using System.Windows;

namespace Advent.Common.UI
{
    public delegate void BeginDragEventHandler(object sender, BeginDragEventArgs e);

    public class BeginDragEventArgs : RoutedEventArgs
    {
        public Point DragPoint { get; private set; }

        public BeginDragEventArgs(object source, Point dragPoint)
            : base(Advent.Common.UI.DragDrop.DragEvent, source)
        {
            this.DragPoint = dragPoint;
        }
    }
}

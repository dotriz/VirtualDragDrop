
using System;
using System.Windows;

namespace Advent.Common.UI
{
    public class EnhancedDragEventArgs : EventArgs
    {
        public IDataObject DataObject { get; set; }

        public DragDropEffects Effects { get; set; }
    }
}

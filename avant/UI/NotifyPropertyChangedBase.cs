
using System.ComponentModel;
using System.Threading;

namespace Advent.Common.UI
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                PropertyChangedEventHandler changedEventHandler = this.propertyChanged;
                PropertyChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this.propertyChanged, comparand + value, comparand);
                }
                while (changedEventHandler != comparand);
            }
            remove
            {
                PropertyChangedEventHandler changedEventHandler = this.propertyChanged;
                PropertyChangedEventHandler comparand;
                do
                {
                    comparand = changedEventHandler;
                    changedEventHandler = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref this.propertyChanged, comparand - value, comparand);
                }
                while (changedEventHandler != comparand);
            }
        }

        protected virtual void OnPropertyChanged(string property)
        {
            if (this.propertyChanged == null)
                return;
            this.propertyChanged((object)this, new PropertyChangedEventArgs(property));
        }
    }
}

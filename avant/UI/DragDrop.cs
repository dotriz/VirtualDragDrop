
using Advent.Common.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Advent.Common.UI
{
    public enum DragMouseButton
    {
        Left = 0,
        Middle = 1,
        Right = 2,
        XButton1 = 3,
        XButton2 = 4,
        None = 2147483647,
    }

    public static class DragDrop
    {
        public static readonly DependencyPropertyKey IsDropOverPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsDropOver", typeof(bool), typeof(Advent.Common.UI.DragDrop), new PropertyMetadata((object)false));
        public static readonly DependencyProperty AllowEnhancedDropProperty = DependencyProperty.RegisterAttached("AllowEnhancedDrop", typeof(bool), typeof(Advent.Common.UI.DragDrop), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, new PropertyChangedCallback(Advent.Common.UI.DragDrop.OnAllowEnhancedDropChanged)));
        public static readonly DependencyProperty DropDescriptionProperty = DependencyProperty.RegisterAttached("DropDescription", typeof(string), typeof(Advent.Common.UI.DragDrop));
        public static readonly DependencyProperty DropDescriptionInsertProperty = DependencyProperty.RegisterAttached("DropDescriptionInsert", typeof(string), typeof(Advent.Common.UI.DragDrop));
        public static readonly DependencyProperty IsDropOverProperty = Advent.Common.UI.DragDrop.IsDropOverPropertyKey.DependencyProperty;
        public static readonly DependencyProperty DragMouseButtonProperty = DependencyProperty.RegisterAttached("DragMouseButton", typeof(DragMouseButton), typeof(Advent.Common.UI.DragDrop), (PropertyMetadata)new FrameworkPropertyMetadata((object)DragMouseButton.None, new PropertyChangedCallback(Advent.Common.UI.DragDrop.OnDragMouseButtonChanged)));
        public static readonly RoutedEvent DragEvent = EventManager.RegisterRoutedEvent("Drag", RoutingStrategy.Bubble, typeof(BeginDragEventHandler), typeof(Advent.Common.UI.DragDrop));
        private static readonly List<UIElement> dragElements = new List<UIElement>();
        private static bool isDragging;
        private static Point dragPoint;
        private static object dragEnterOriginalSource;
        private static bool isDragEnterHandled;
        private static EventHandler<EnhancedDragEventArgs> enhancedDragStarted;
        private static EventHandler<EnhancedDragEventArgs> enhancedDragEnded;

        public static event EventHandler<EnhancedDragEventArgs> EnhancedDragStarted
        {
            add
            {
                EventHandler<EnhancedDragEventArgs> eventHandler1 = Advent.Common.UI.DragDrop.enhancedDragStarted;
                EventHandler<EnhancedDragEventArgs> comparand;
                do
                {
                    comparand = eventHandler1;
                    EventHandler<EnhancedDragEventArgs> eventHandler2 = comparand + value;
                    eventHandler1 = Interlocked.CompareExchange<EventHandler<EnhancedDragEventArgs>>(ref Advent.Common.UI.DragDrop.enhancedDragStarted, eventHandler2, comparand);
                }
                while (eventHandler1 != comparand);
            }
            remove
            {
                EventHandler<EnhancedDragEventArgs> eventHandler1 = Advent.Common.UI.DragDrop.enhancedDragStarted;
                EventHandler<EnhancedDragEventArgs> comparand;
                do
                {
                    comparand = eventHandler1;
                    EventHandler<EnhancedDragEventArgs> eventHandler2 = comparand - value;
                    eventHandler1 = Interlocked.CompareExchange<EventHandler<EnhancedDragEventArgs>>(ref Advent.Common.UI.DragDrop.enhancedDragStarted, eventHandler2, comparand);
                }
                while (eventHandler1 != comparand);
            }
        }

        public static event EventHandler<EnhancedDragEventArgs> EnhancedDragEnded
        {
            add
            {
                EventHandler<EnhancedDragEventArgs> eventHandler1 = Advent.Common.UI.DragDrop.enhancedDragEnded;
                EventHandler<EnhancedDragEventArgs> comparand;
                do
                {
                    comparand = eventHandler1;
                    EventHandler<EnhancedDragEventArgs> eventHandler2 = comparand + value;
                    eventHandler1 = Interlocked.CompareExchange<EventHandler<EnhancedDragEventArgs>>(ref Advent.Common.UI.DragDrop.enhancedDragEnded, eventHandler2, comparand);
                }
                while (eventHandler1 != comparand);
            }
            remove
            {
                EventHandler<EnhancedDragEventArgs> eventHandler1 = Advent.Common.UI.DragDrop.enhancedDragEnded;
                EventHandler<EnhancedDragEventArgs> comparand;
                do
                {
                    comparand = eventHandler1;
                    EventHandler<EnhancedDragEventArgs> eventHandler2 = comparand - value;
                    eventHandler1 = Interlocked.CompareExchange<EventHandler<EnhancedDragEventArgs>>(ref Advent.Common.UI.DragDrop.enhancedDragEnded, eventHandler2, comparand);
                }
                while (eventHandler1 != comparand);
            }
        }

        static DragDrop()
        {
        }

        public static void SetAllowEnhancedDrop(DependencyObject element, bool value)
        {
            element.SetValue(Advent.Common.UI.DragDrop.AllowEnhancedDropProperty, value);
        }

        public static bool GetAllowEnhancedDrop(DependencyObject element)
        {
            return (bool)element.GetValue(Advent.Common.UI.DragDrop.AllowEnhancedDropProperty);
        }

        public static void SetDropDescription(DependencyObject element, string dropDescription)
        {
            element.SetValue(Advent.Common.UI.DragDrop.DropDescriptionProperty, (object)dropDescription);
        }

        public static string GetDropDescription(DependencyObject element)
        {
            return (string)element.GetValue(Advent.Common.UI.DragDrop.DropDescriptionProperty);
        }

        public static void SetDropDescriptionInsert(DependencyObject element, string insert)
        {
            element.SetValue(Advent.Common.UI.DragDrop.DropDescriptionInsertProperty, (object)insert);
        }

        public static string GetDropDescriptionInsert(DependencyObject element)
        {
            return (string)element.GetValue(Advent.Common.UI.DragDrop.DropDescriptionInsertProperty);
        }

        public static bool GetIsDropOver(DependencyObject element)
        {
            return (bool)element.GetValue(Advent.Common.UI.DragDrop.IsDropOverProperty);
        }

        public static DragMouseButton GetDragMouseButton(DependencyObject element)
        {
            return (DragMouseButton)element.GetValue(Advent.Common.UI.DragDrop.DragMouseButtonProperty);
        }

        public static void SetDragMouseButton(DependencyObject element, DragMouseButton button)
        {
            element.SetValue(Advent.Common.UI.DragDrop.DragMouseButtonProperty, (object)button);
        }

        public static void AddDragHandler(DependencyObject sender, BeginDragEventHandler handler)
        {
            ((UIElement)sender).AddHandler(Advent.Common.UI.DragDrop.DragEvent, (Delegate)handler);
        }

        public static void RemoveDragHandler(DependencyObject sender, BeginDragEventHandler handler)
        {
            ((UIElement)sender).RemoveHandler(Advent.Common.UI.DragDrop.DragEvent, (Delegate)handler);
        }

        internal static void OnDragStarted(EnhancedDragEventArgs e)
        {
            if (Advent.Common.UI.DragDrop.enhancedDragStarted == null)
                return;
            Advent.Common.UI.DragDrop.enhancedDragStarted((object)null, e);
        }

        internal static void OnDragEnded(EnhancedDragEventArgs e)
        {
            if (Advent.Common.UI.DragDrop.enhancedDragEnded == null)
                return;
            Advent.Common.UI.DragDrop.enhancedDragEnded((object)null, e);
        }

        private static void OnDragMouseButtonChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            UIElement uiElement = (UIElement)obj;
            DragMouseButton dragMouseButton = (DragMouseButton)args.NewValue;
            if ((DragMouseButton)args.OldValue == DragMouseButton.None)
            {
                uiElement.MouseDown += new MouseButtonEventHandler(Advent.Common.UI.DragDrop.ElementMouseDown);
                uiElement.MouseMove += new MouseEventHandler(Advent.Common.UI.DragDrop.ElementMouseMove);
                uiElement.MouseUp += new MouseButtonEventHandler(Advent.Common.UI.DragDrop.ElementMouseUp);
            }
            else
            {
                if (dragMouseButton != DragMouseButton.None)
                    return;
                uiElement.MouseDown -= new MouseButtonEventHandler(Advent.Common.UI.DragDrop.ElementMouseDown);
                uiElement.MouseMove -= new MouseEventHandler(Advent.Common.UI.DragDrop.ElementMouseMove);
                uiElement.MouseUp -= new MouseButtonEventHandler(Advent.Common.UI.DragDrop.ElementMouseUp);
            }
        }

        private static void ElementMouseUp(object sender, MouseButtonEventArgs e)
        {
            Advent.Common.UI.DragDrop.isDragging = false;
        }

        private static void ElementMouseMove(object sender, MouseEventArgs e)
        {
            if (!Advent.Common.UI.DragDrop.isDragging)
                return;
            Point position = e.GetPosition((IInputElement)sender);
            if (Math.Abs(position.X - Advent.Common.UI.DragDrop.dragPoint.X) <= SystemParameters.MinimumHorizontalDragDistance && Math.Abs(position.Y - Advent.Common.UI.DragDrop.dragPoint.Y) <= SystemParameters.MinimumVerticalDragDistance)
                return;
            ((UIElement)sender).RaiseEvent((RoutedEventArgs)new BeginDragEventArgs(e.Source, Advent.Common.UI.DragDrop.dragPoint));
            Advent.Common.UI.DragDrop.isDragging = false;
        }

        private static void ElementMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMouseButton dragMouseButton = (DragMouseButton)((DependencyObject)sender).GetValue(Advent.Common.UI.DragDrop.DragMouseButtonProperty);
            if (dragMouseButton == DragMouseButton.None || e.ChangedButton != (MouseButton)dragMouseButton || e.ButtonState != MouseButtonState.Pressed)
                return;
            Advent.Common.UI.DragDrop.isDragging = true;
            Advent.Common.UI.DragDrop.dragPoint = e.GetPosition((IInputElement)sender);
        }

        private static void OnAllowEnhancedDropChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            UIElement uiElement1 = (UIElement)obj;
            if (uiElement1 == null)
                return;
            UIElement uiElement2 = (UIElement)UIExtensions.GetParent((DependencyObject)uiElement1);
            if (uiElement2 == null)
                return;
            if ((bool)args.OldValue)
            {
                uiElement2.RemoveHandler(UIElement.DragEnterEvent, (Delegate)new DragEventHandler(Advent.Common.UI.DragDrop.ParentDragEnter));
                uiElement1.RemoveHandler(UIElement.DragOverEvent, (Delegate)new DragEventHandler(Advent.Common.UI.DragDrop.ElementDragOver));
                uiElement1.PreviewDragLeave -= new DragEventHandler(Advent.Common.UI.DragDrop.ElementPreviewDragLeave);
                uiElement1.PreviewDrop -= new DragEventHandler(Advent.Common.UI.DragDrop.ElementPreviewDrop);
                uiElement1.PreviewQueryContinueDrag -= new QueryContinueDragEventHandler(Advent.Common.UI.DragDrop.ElementPreviewQueryContinueDrag);
                Advent.Common.UI.DragDrop.dragElements.Remove(uiElement1);
            }
            if (!(bool)args.NewValue)
                return;
            uiElement2.AddHandler(UIElement.DragEnterEvent, (Delegate)new DragEventHandler(Advent.Common.UI.DragDrop.ParentDragEnter), true);
            uiElement1.AddHandler(UIElement.DragOverEvent, (Delegate)new DragEventHandler(Advent.Common.UI.DragDrop.ElementDragOver), true);
            uiElement1.PreviewDragLeave += new DragEventHandler(Advent.Common.UI.DragDrop.ElementPreviewDragLeave);
            uiElement1.PreviewDrop += new DragEventHandler(Advent.Common.UI.DragDrop.ElementPreviewDrop);
            uiElement1.PreviewQueryContinueDrag += new QueryContinueDragEventHandler(Advent.Common.UI.DragDrop.ElementPreviewQueryContinueDrag);
            Advent.Common.UI.DragDrop.dragElements.Add(uiElement1);
        }

        private static void ElementPreviewQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (!e.EscapePressed)
                return;
            ((DependencyObject)sender).SetValue(Advent.Common.UI.DragDrop.IsDropOverPropertyKey, (object)false);
            DataObjectExtensions.DragLeave();
            Advent.Common.UI.DragDrop.dragEnterOriginalSource = (object)null;
        }

        private static void ElementPreviewDrop(object sender, DragEventArgs e)
        {
            ((DependencyObject)sender).SetValue(Advent.Common.UI.DragDrop.IsDropOverPropertyKey, (object)false);
            DataObjectExtensions.Drop(e.Data, Mouse.GetPosition((IInputElement)UIExtensions.GetAncestor<Window>((DependencyObject)sender)), e.Effects);
            Advent.Common.UI.DragDrop.dragEnterOriginalSource = (object)null;
        }

        private static void ElementDragOver(object sender, DragEventArgs e)
        {
            DataObjectExtensions.DragOver(Mouse.GetPosition((IInputElement)UIExtensions.GetAncestor<Window>((DependencyObject)sender)), e.Effects);
        }

        private static void ElementPreviewDragLeave(object sender, DragEventArgs e)
        {
            UIElement uiElement = (UIElement)sender;
            uiElement.SetValue(Advent.Common.UI.DragDrop.IsDropOverPropertyKey, (object)false);
            POINT lpPoint;
            NativeMethods.GetCursorPos(out lpPoint);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual)uiElement, uiElement.PointFromScreen(new Point((double)lpPoint.X, (double)lpPoint.Y)));
            if (hitTestResult == null || hitTestResult.VisualHit == null || hitTestResult.VisualHit != sender && !((Visual)sender).IsAncestorOf(hitTestResult.VisualHit))
                DataObjectExtensions.DragLeave(e.Data);
            Advent.Common.UI.DragDrop.dragEnterOriginalSource = (object)null;
        }

        private static void ParentDragEnter(object sender, DragEventArgs e)
        {
            if (e.OriginalSource != Advent.Common.UI.DragDrop.dragEnterOriginalSource)
            {
                Advent.Common.UI.DragDrop.dragEnterOriginalSource = e.OriginalSource;
                Advent.Common.UI.DragDrop.isDragEnterHandled = false;
            }
            if (Advent.Common.UI.DragDrop.isDragEnterHandled)
                return;
            UIElement uiElement1 = (UIElement)null;
            foreach (UIElement uiElement2 in Advent.Common.UI.DragDrop.dragElements)
            {
                if (uiElement2.IsAncestorOf((DependencyObject)e.OriginalSource) && VisualTreeHelper.GetParent((DependencyObject)uiElement2) == sender)
                {
                    uiElement1 = uiElement2;
                    break;
                }
            }
            if (uiElement1 == null)
                return;
            Window ancestor = UIExtensions.GetAncestor<Window>((DependencyObject)uiElement1);
            string descriptionMessage = Advent.Common.UI.DragDrop.GetDropDescription((DependencyObject)uiElement1);
            if (descriptionMessage != null)
                descriptionMessage = descriptionMessage.Replace("{0}", "%1");
            uiElement1.SetValue(Advent.Common.UI.DragDrop.IsDropOverPropertyKey, (object)true);
            DataObjectExtensions.DragEnter(ancestor, e.Data, Mouse.GetPosition((IInputElement)ancestor), e.Effects, descriptionMessage, Advent.Common.UI.DragDrop.GetDropDescriptionInsert((DependencyObject)uiElement1));
            Advent.Common.UI.DragDrop.isDragEnterHandled = true;
        }
    }
}

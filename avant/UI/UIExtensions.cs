
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Advent.Common.UI
{
    public static class UIExtensions
    {
        public static T GetAncestor<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
                return default(T);
            T obj = parent as T;
            if ((object)obj != null)
                return obj;
            else
                return UIExtensions.GetAncestor<T>(parent);
        }

        public static DependencyObject GetParent(this DependencyObject child)
        {
            if (child == null)
                return (DependencyObject)null;
            ContentElement reference = child as ContentElement;
            if (reference == null)
                return VisualTreeHelper.GetParent(child);
            DependencyObject parent = ContentOperations.GetParent(reference);
            if (parent != null)
                return parent;
            FrameworkContentElement frameworkContentElement = reference as FrameworkContentElement;
            if (frameworkContentElement == null)
                return (DependencyObject)null;
            else
                return frameworkContentElement.Parent;
        }

        public static bool IsDescendantOf(this DependencyObject child, DependencyObject ancestor)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            if (parent == null)
                return false;
            if (parent == ancestor)
                return true;
            else
                return UIExtensions.IsDescendantOf(parent, ancestor);
        }

        public static BitmapSource Resize(this BitmapSource image, int height)
        {
            BitmapSource bitmapSource;
            if (image.Height <= (double)height)
            {
                bitmapSource = image;
            }
            else
            {
                TransformedBitmap transformedBitmap = new TransformedBitmap();
                transformedBitmap.BeginInit();
                transformedBitmap.Source = image;
                int pixelHeight = transformedBitmap.Source.PixelHeight;
                int pixelWidth = transformedBitmap.Source.PixelWidth;
                double scaleX = (double)(transformedBitmap.Source.PixelWidth * height / pixelHeight) / (double)pixelWidth;
                double scaleY = (double)height / (double)pixelHeight;
                transformedBitmap.Transform = (Transform)new TransformGroup()
                {
                    Children = {
            (Transform) new ScaleTransform(scaleX, scaleY)
          }
                };
                transformedBitmap.EndInit();
                WriteableBitmap writeableBitmap = new WriteableBitmap((BitmapSource)transformedBitmap);
                writeableBitmap.Freeze();
                bitmapSource = (BitmapSource)writeableBitmap;
            }
            return bitmapSource;
        }

        public static Image ToImage(this UIElement element)
        {
            return new Image()
            {
                Source = UIExtensions.ToImageSource(element)
            };
        }

        public static ImageSource ToImageSource(this UIElement element)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)element.RenderSize.Width, (int)element.RenderSize.Height, 0.0, 0.0, PixelFormats.Pbgra32);
            renderTargetBitmap.Render((Visual)element);
            return (ImageSource)renderTargetBitmap;
        }
    }
}

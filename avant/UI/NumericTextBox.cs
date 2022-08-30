
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Advent.Common.UI
{
    public class NumericTextBox : TextBox
    {
        static NumericTextBox()
        {
            FrameworkPropertyMetadata propertyMetadata = new FrameworkPropertyMetadata();
            propertyMetadata.CoerceValueCallback = new CoerceValueCallback(NumericTextBox.ForceText);
            TextBox.TextProperty.OverrideMetadata(typeof(NumericTextBox), (PropertyMetadata)propertyMetadata);
        }

        public NumericTextBox()
        {
            this.TextAlignment = TextAlignment.Right;
            this.CommandBindings.Add(new CommandBinding((ICommand)ApplicationCommands.Paste, (ExecutedRoutedEventHandler)null, new CanExecuteRoutedEventHandler(NumericTextBox.CancelCommand)));
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = !NumericTextBox.AreAllValidNumericChars((IEnumerable<char>)e.Text);
            base.OnPreviewTextInput(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
            base.OnPreviewKeyDown(e);
        }

        private static bool AreAllValidNumericChars(IEnumerable<char> str)
        {
            foreach (char c in str)
            {
                if (!char.IsNumber(c))
                    return false;
            }
            return true;
        }

        private static void CancelCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private static object ForceText(DependencyObject sender, object value)
        {
            if (!NumericTextBox.AreAllValidNumericChars((IEnumerable<char>)(string)value))
                throw new ArgumentException();
            else
                return value;
        }
    }
}

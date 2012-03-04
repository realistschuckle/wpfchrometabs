using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace ChromiumTabs
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChromiumTabs"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ChromiumTabs;assembly=ChromiumTabs"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:ChromiumTabPanel/>
    ///
    /// </summary>
    [ToolboxItem(false)]
    public class ChromiumTabPanel : Panel
    {
        static ChromiumTabPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChromiumTabPanel), new FrameworkPropertyMetadata(typeof(ChromiumTabPanel)));
        }

        public ChromiumTabPanel()
        {
            this.maxTabWidth = 100.0;
            this.minTabWidth = 50.0;
            this.leftMargin = 50.0;
            this.rightMargin = 0.0;
            this.overlap = 10.0;
            this.defaultMeasureHeight = 30.0;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            Point start = new Point(0, Math.Round(this.finalSize.Height));
            Point end = new Point(this.finalSize.Width, Math.Round(this.finalSize.Height));
            Color penColor = (Color)ColorConverter.ConvertFromString("#FF999999");
            Brush brush = new SolidColorBrush(penColor);
            Pen pen = new Pen(brush, .5);
            dc.DrawLine(pen, start, end);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double activeWidth = finalSize.Width - this.leftMargin - this.rightMargin;
            double tabWidth = Math.Min(Math.Max(activeWidth / this.Children.Count, this.minTabWidth), this.maxTabWidth);
            this.finalSize = finalSize;
            double offset = leftMargin;
            foreach (UIElement element in this.Children)
            {
                double thickness = 0.0;
                ChromiumTabItem item = ItemsControl.ContainerFromElement(this.ParentTabControl, element) as ChromiumTabItem;
                thickness = item.Margin.Bottom;
                element.Arrange(new Rect(offset, 0, tabWidth, finalSize.Height - thickness));
                offset += tabWidth - overlap;
            }
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double activeWidth = double.IsPositiveInfinity(availableSize.Width) ? 500 : availableSize.Width - this.leftMargin - this.rightMargin;
            double tabWidth = Math.Min(Math.Max(activeWidth / this.Children.Count, this.minTabWidth), this.maxTabWidth);
            double height = double.IsPositiveInfinity(availableSize.Height) ? this.defaultMeasureHeight : availableSize.Height;
            Size resultSize = new Size(0, availableSize.Height);
            foreach (UIElement child in this.Children)
            {
                ChromiumTabItem item = ItemsControl.ContainerFromElement(this.ParentTabControl, child) as ChromiumTabItem;
                Size tabSize = new Size(tabWidth, height - item.Margin.Bottom);
                child.Measure(tabSize);
                resultSize.Width += child.DesiredSize.Width - overlap;
            }
            return resultSize;
        }

        private ChromiumTabControl ParentTabControl
        {
            get { return Parent as ChromiumTabControl; }
        }

        private Size finalSize;
        private double overlap;
        private double leftMargin;
        private double rightMargin;
        private double maxTabWidth;
        private double minTabWidth;
        private double defaultMeasureHeight;
    }
}

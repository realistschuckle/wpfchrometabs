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

namespace ChromeTabs
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
    public class ChromeTabPanel : Panel
    {
        static ChromeTabPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChromeTabPanel), new FrameworkPropertyMetadata(typeof(ChromeTabPanel)));
        }

        public ChromeTabPanel()
        {
            this.maxTabWidth = 125.0;
            this.minTabWidth = 40.0;
            this.leftMargin = 50.0;
            this.rightMargin = 30.0;
            this.overlap = 10.0;
            this.defaultMeasureHeight = 30.0;

            ComponentResourceKey key = new ComponentResourceKey(typeof(ChromeTabPanel), "addButtonStyle");
            Style addButtonStyle = (Style)this.FindResource(key);
            this.addButton = new Button { Style = addButtonStyle };
            this.addButtonSize = new Size(20, 12);
        }

        protected override int VisualChildrenCount
        {
            get { return base.VisualChildrenCount + 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == this.VisualChildrenCount - 1)
            {
                return this.addButton;
            }
            else if (index < this.VisualChildrenCount - 1)
            {
                return base.GetVisualChild(index);
            }
            throw new IndexOutOfRangeException("Not enough visual children in the ChromeTabPanel.");
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
            double tabWidth = Math.Min(Math.Max((activeWidth + (this.Children.Count - 1) * overlap)/ this.Children.Count, this.minTabWidth), this.maxTabWidth);
            this.finalSize = finalSize;
            double offset = leftMargin;
            foreach (UIElement element in this.Children)
            {
                double thickness = 0.0;
                ChromeTabItem item = ItemsControl.ContainerFromElement(this.ParentTabControl, element) as ChromeTabItem;
                thickness = item.Margin.Bottom;
                element.Arrange(new Rect(offset, 0, tabWidth, finalSize.Height - thickness));
                offset += tabWidth - overlap;
            }
            this.addButtonRect = new Rect(new Point(offset + overlap, (finalSize.Height - this.addButtonSize.Height) / 2), this.addButtonSize);
            this.addButton.Arrange(this.addButtonRect);
            return finalSize;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double activeWidth = double.IsPositiveInfinity(availableSize.Width) ? 500 : availableSize.Width - this.leftMargin - this.rightMargin;
            double tabWidth = Math.Min(Math.Max((activeWidth + (this.Children.Count - 1) * overlap) / this.Children.Count, this.minTabWidth), this.maxTabWidth);
            double height = double.IsPositiveInfinity(availableSize.Height) ? this.defaultMeasureHeight : availableSize.Height;
            Size resultSize = new Size(0, availableSize.Height);
            foreach (UIElement child in this.Children)
            {
                ChromeTabItem item = ItemsControl.ContainerFromElement(this.ParentTabControl, child) as ChromeTabItem;
                Size tabSize = new Size(tabWidth, height - item.Margin.Bottom);
                child.Measure(tabSize);
                resultSize.Width += child.DesiredSize.Width - overlap;
            }
            this.addButton.Measure(this.addButtonSize);
            resultSize.Width += this.addButtonSize.Width;
            return resultSize;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (this.addButtonRect.Contains(e.GetPosition(this)))
            {
                this.addButton.Background = Brushes.DarkGray;
                this.InvalidateVisual();
                return;
            }

            this.downPoint = e.GetPosition(this);
            HitTestResult result = VisualTreeHelper.HitTest(this, this.downPoint);
            if (result == null) { return; }
            DependencyObject source = result.VisualHit;
            while (source != null && !this.Children.Contains(source as UIElement))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            if (source == null) { return; }
            draggedTab = source as ChromeTabItem;
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (this.addButtonRect.Contains(e.GetPosition(this)) && this.addButton.Background != Brushes.White && this.addButton.Background != Brushes.DarkGray)
            {
                this.addButton.Background = Brushes.White;
                this.InvalidateVisual();
            }
            else if (!this.addButtonRect.Contains(e.GetPosition(this)) && this.addButton.Background != null)
            {
                this.addButton.Background = null;
                this.InvalidateVisual();
            }
            if (draggedTab == null) { return; }
            Point nowPoint = e.GetPosition(this);
            Thickness margin = new Thickness(nowPoint.X - this.downPoint.X, 0, this.downPoint.X - nowPoint.X, 0);
            draggedTab.SetValue(FrameworkElement.MarginProperty, margin);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            if (this.addButtonRect.Contains(e.GetPosition(this)) && this.addButton.Background == Brushes.DarkGray)
            {
                this.addButton.Background = null;
                this.InvalidateVisual();
                ParentTabControl.AddTab(new Label(), true); // HACK: Do something with default templates, here.
                return;
            }

            ParentTabControl.ChangeSelectedItem(draggedTab);
            draggedTab = null;
        }

        private ChromeTabControl ParentTabControl
        {
            get
            {
                if (this.parent == null)
                {
                    DependencyObject parent = this;
                    while (parent != null && !(parent is ChromeTabControl))
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                    this.parent = parent as ChromeTabControl;
                }
                return this.parent;
            }
        }

        private Size finalSize;
        private double overlap;
        private double leftMargin;
        private double rightMargin;
        private double maxTabWidth;
        private double minTabWidth;
        private double defaultMeasureHeight;
        private ChromeTabItem draggedTab;
        private Point downPoint;
        private ChromeTabControl parent;

        private Rect addButtonRect;
        private Size addButtonSize;
        private Button addButton;
    }
}

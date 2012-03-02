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
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;

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
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class ChromiumTabControl : Selector
    {
        public static readonly DependencyProperty SelectedContentProperty = DependencyProperty.Register("SelectedContent", typeof(object), typeof(ChromiumTabControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        static ChromiumTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChromiumTabControl), new FrameworkPropertyMetadata(typeof(ChromiumTabControl)));
        }

        public object SelectedContent
        {
            get { return (object)GetValue(SelectedContentProperty); }
            set { SetValue(SelectedContentProperty, value); }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.SetChildrenZ();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.SetChildrenZ();
            if (this.SelectedItem == null) { return; }
            ChromiumTabItem item = this.SelectedItem as ChromiumTabItem;
            this.SelectedContent = item.Content;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is ChromiumTabItem);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ChromiumTabItem();
        }

        private void SetChildrenZ()
        {
            int zindex = this.Items.Count - 1;
            foreach (UIElement element in this.Items)
            {
                if (element == null) { continue; }
                if (Selector.GetIsSelected(element))
                {
                    Panel.SetZIndex(element, this.Items.Count);
                }
                else
                {
                    Panel.SetZIndex(element, zindex);
                }
                zindex -= 1;
            }
        }
    }
}

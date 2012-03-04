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

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            bool somethingSelected = false;
            foreach (UIElement element in this.Items)
            {
                somethingSelected |= ChromiumTabItem.GetIsSelected(element);
            }
            if (!somethingSelected)
            {
                this.SelectedIndex = 0;
            }
        }

        public object SelectedContent
        {
            get { return (object)GetValue(SelectedContentProperty); }
            set { SetValue(SelectedContentProperty, value); }
        }

        internal void ChangeSelectedItem(ChromiumTabItem item)
        {
            for (int i = 0; i < this.Items.Count; i += 1)
            {
                object dep = this.Items[i];
                ChromiumTabItem tabItem = this.AsTabItem(dep);
                if (tabItem == item)
                {
                    this.SelectedIndex = i;
                    return;
                }
            }
        }

        public void AddTab(object tab, bool select)
        {
            this.Items.Add(tab);
            if (select || this.Items.Count == 1)
            {
                this.SelectedIndex = this.Items.Count - 1;
            }
        }

        public void RemoveTab(object tab)
        {
            int selectedIndex = this.SelectedIndex;
            bool removedSelectedTab = false;
            ChromiumTabItem removeItem = this.AsTabItem(tab);
            foreach (object item in this.Items)
            {
                ChromiumTabItem tabItem = this.AsTabItem(item);
                if (tabItem != null && tabItem == removeItem)
                {
                    if (tabItem.Content == this.SelectedContent)
                    {
                        removedSelectedTab = true;
                    }
                    if (this.ObjectToContainer.ContainsKey(tab))
                    {
                        this.ObjectToContainer.Remove(tab);
                    }
                    this.Items.Remove(item);
                    break;
                }
            }
            if (removedSelectedTab && this.Items.Count > 0)
            {
                this.SelectedItem = this.Items[Math.Min(selectedIndex, this.Items.Count - 1)];
            }
            else if (removedSelectedTab)
            {
                this.SelectedItem = null;
                this.SelectedContent = null;
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    if (ObjectToContainer.ContainsKey(item))
                    {
                        ObjectToContainer.Remove(item);
                    }
                }
            }
            this.SetChildrenZ();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            this.SetChildrenZ();
            if (e.AddedItems.Count == 0) { return; }
            foreach(UIElement element in this.Items)
            {
                if(element == e.AddedItems[0]) { continue; }
                ChromiumTabItem.SetIsSelected(element, false);
            }
            ChromiumTabItem item = this.AsTabItem(this.SelectedItem);
            this.SelectedContent = item != null? item.Content : null;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is ChromiumTabItem);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ChromiumTabItem{ Header = "New Tab" };
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            if (element != item)
            {
                ObjectToContainer[item] = element;
            }
        }

        private Dictionary<object, DependencyObject> ObjectToContainer
        {
            get
            {
                if (objectToContainerMap == null)
                {
                    objectToContainerMap = new Dictionary<object, DependencyObject>();
                }
                return objectToContainerMap;
            }
        }

        private ChromiumTabItem AsTabItem(object item)
        {
            ChromiumTabItem tabItem = item as ChromiumTabItem;
            if (tabItem == null && this.ObjectToContainer.ContainsKey(item))
            {
                tabItem = this.ObjectToContainer[item] as ChromiumTabItem;
            }
            return tabItem;
        }

        private void SetChildrenZ()
        {
            int zindex = this.Items.Count - 1;
            foreach (object element in this.Items)
            {
                ChromiumTabItem tabItem = this.AsTabItem(element);
                if (tabItem == null) { continue; }
                if (ChromiumTabItem.GetIsSelected(tabItem))
                {
                    Panel.SetZIndex(tabItem, this.Items.Count);
                }
                else
                {
                    Panel.SetZIndex(tabItem, zindex);
                }
                zindex -= 1;
            }
        }

        private Dictionary<object, DependencyObject> objectToContainerMap;
    }
}

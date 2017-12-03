using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace ImagesViewer.Controls
{
    public class ListBoxVisibleItemsRangeBehaviour : Behavior<ListBox>
    {
        #region VisibleItemsRange Property
        public bool HasVisibleScroll
        {
            get { return (bool)GetValue(HasVisibleScrollProperty); }
            set { SetValue(HasVisibleScrollProperty, value); }
        }

        public static readonly DependencyProperty HasVisibleScrollProperty =
    DependencyProperty.Register("HasVisibleScroll", typeof(bool), typeof(ListBoxVisibleItemsRangeBehaviour), new PropertyMetadata(false));


        public Range VisibleItemsRange
        {
            get { return (Range)GetValue(VisibleItemsRangeProperty); }
            set { SetValue(VisibleItemsRangeProperty, value); }
        }

        public static readonly DependencyProperty VisibleItemsRangeProperty =
            DependencyProperty.Register("VisibleItemsRange", typeof(Range), typeof(ListBoxVisibleItemsRangeBehaviour)
            );

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            _hasVirtualizingStackPanel = VirtualizingStackPanel.GetIsVirtualizing(AssociatedObject);
            if (_hasVirtualizingStackPanel)
                AssociatedObject.ItemContainerGenerator.StatusChanged += OnListBoxItemGeneratorStatusChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject != null)
            {
                if (_hasVirtualizingStackPanel)
                {
                    AssociatedObject.ItemContainerGenerator.StatusChanged -= OnListBoxItemGeneratorStatusChanged;
                    if (_virtualizingStackPanel != null)
                        _virtualizingStackPanel.ScrollOwner.ScrollChanged -= OnScrollChanged;
                }
            }
        }

        private void OnListBoxItemGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (AssociatedObject.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return;

            _virtualizingStackPanel = VisualTreeHelperEx.GetChildren<VirtualizingWrapPanel>(AssociatedObject).FirstOrDefault();
            if (_virtualizingStackPanel != null &&  _virtualizingStackPanel.ScrollOwner !=null )
            {
                AssociatedObject.ItemContainerGenerator.StatusChanged -= OnListBoxItemGeneratorStatusChanged;
                _virtualizingStackPanel.ScrollOwner.ScrollChanged += OnScrollChanged;
            }
        }

        private void RefreshRange() 
        {
            var scrollVisible = _virtualizingStackPanel.ScrollOwner.ComputedVerticalScrollBarVisibility == Visibility.Visible;
            HasVisibleScroll = scrollVisible;
            if (scrollVisible)
            {
                var visibleCount = _virtualizingStackPanel.Children.Count;
                var index = _virtualizingStackPanel.Children.OfType<ListBoxItem>().First().DataContext;
                VisibleItemsRange = new Range(AssociatedObject.ItemsSource.OfType<object>().ToList().IndexOf(index), visibleCount);
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            RefreshRange();
        }

        private VirtualizingWrapPanel _virtualizingStackPanel;
        private bool _hasVirtualizingStackPanel;

    }
}

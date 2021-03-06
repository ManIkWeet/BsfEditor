using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BsfEditor.View
{
    /// <summary>
    ///     Source: https://stackoverflow.com/a/4663799
    /// </summary>
    public class DataGridBehavior
    {
        #region DisplayRowNumber
        public static readonly DependencyProperty DisplayRowNumberProperty = DependencyProperty.RegisterAttached("DisplayRowNumber", typeof(bool),
            typeof(DataGridBehavior), new FrameworkPropertyMetadata(false, OnDisplayRowNumberChanged));

        public static bool GetDisplayRowNumber(DependencyObject target)
        {
            return (bool)target.GetValue(DisplayRowNumberProperty);
        }

        public static void SetDisplayRowNumber(DependencyObject target, bool value)
        {
            target.SetValue(DisplayRowNumberProperty, value);
        }

        private static void OnDisplayRowNumberChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!(target is DataGrid dataGrid)) return;
            if (!(bool)e.NewValue) return;

            void LoadedRowHandler(object sender, DataGridRowEventArgs ea)
            {
                if (GetDisplayRowNumber(dataGrid) == false)
                {
                    dataGrid.LoadingRow -= LoadedRowHandler;
                    return;
                }
                ea.Row.Header = ea.Row.GetIndex();
            }

            dataGrid.LoadingRow += LoadedRowHandler;

            void ItemsChangedHandler(object sender, ItemsChangedEventArgs ea)
            {
                if (GetDisplayRowNumber(dataGrid) == false)
                {
                    dataGrid.ItemContainerGenerator.ItemsChanged -= ItemsChangedHandler;
                    return;
                }
                GetVisualChildCollection<DataGridRow>(dataGrid).ForEach(d => d.Header = d.GetIndex());
            }

            dataGrid.ItemContainerGenerator.ItemsChanged += ItemsChangedHandler;
        }
        #endregion // DisplayRowNumber

        #region Get Visuals
        private static List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            var visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private static void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T visual)
                {
                    visualCollection.Add(visual);
                }
                GetVisualChildCollection(child, visualCollection);
            }
        }
        #endregion // Get Visuals
    }
}

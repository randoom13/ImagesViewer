using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImagesViewer.Controls
{
    public static class VisualTreeHelperEx
    {
        public static IEnumerable<T> GetChildren<T>(DependencyObject dobj) 
            where T : DependencyObject
        {
            if (dobj == null || VisualTreeHelper.GetChildrenCount(dobj) == 0)
                return Enumerable.Empty<T>();
            var children = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dobj); i++)
            {
                var internalDObj = VisualTreeHelper.GetChild(dobj, i);
                var child = internalDObj as T;
                if (child != null)
                    children.Add(child);
                else
                    children.AddRange(GetChildren<T>(internalDObj));
            }
            return children;
        }
    }
}

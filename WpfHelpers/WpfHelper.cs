using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WpfHelpers
{
    public static class WpfHelper
    {
        public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }
        public static List<TChildItem> FindVisualChildren<TChildItem>(
            this DependencyObject obj
                )
           where TChildItem : DependencyObject
        {
            var result = new List<TChildItem>();

            FindVisualChildren<TChildItem>(
                obj,
                result
                );

            return result;
        }

        public static void FindVisualChildren<TChildItem>(
            this DependencyObject obj,
            List<TChildItem> result
            )
           where TChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is TChildItem)
                {
                    result.Add(child as TChildItem);
                }
                else
                {
                    FindVisualChildren<TChildItem>(child, result);
                }
            }
        }

        //public static TChildItem FindVisualChild<TChildItem>(
        //    this DependencyObject obj
        //        )
        //   where TChildItem : DependencyObject
        //{
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(obj, i);

        //        if (child != null && child is TChildItem)
        //        {
        //            return (TChildItem) child;
        //        }
        //    }

        //    // Iterate through all immediate children
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
        //    {
        //        DependencyObject child = VisualTreeHelper.GetChild(obj, i);

        //        //if (child != null && child is TChildItem)
        //        //{
        //        //    return (TChildItem)child;
        //        //}
        //        //else
        //        {
        //            TChildItem childOfChild = FindVisualChild<TChildItem>(child);

        //            if (childOfChild != null)
        //            {
        //                return childOfChild;
        //            }
        //        }
        //    }

        //    return null;
        //}
    }
}

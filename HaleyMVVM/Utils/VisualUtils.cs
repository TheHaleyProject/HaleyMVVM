﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Windows.Media;

namespace Haley.Utils
{
    public static class VisualUtils
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            return InternalUtilsVisual.FindVisualChildren<T>(obj);
        }

        public static DependencyObject FindVisualChildren(DependencyObject obj,string target_name)
        {
            return InternalUtilsVisual.FindVisualChildren(obj,target_name);
        }

        public static T FindVisualParent<T>(DependencyObject input, int parentlevel) where T : DependencyObject
        {
            return InternalUtilsVisual.FindVisualParent<T>(input, parentlevel);
        }
    }
}
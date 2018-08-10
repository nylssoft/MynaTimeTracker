/*
    Myna Time Tracker
    Copyright (C) 2018 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeTracker
{
    public static class Extensions
    {
        #region Window extensions

        public static void RestorePosition(this Window window, double left, double top, double width, double height)
        {
            var virtualWidth = System.Windows.SystemParameters.VirtualScreenWidth;
            var virtualHeight = System.Windows.SystemParameters.VirtualScreenHeight;
            height = Math.Min(height, virtualHeight);
            width = Math.Min(width, virtualWidth);
            if (width >= window.MinWidth && height >= window.MinHeight)
            {
                if (top + height / 2 > virtualHeight)
                {
                    top = virtualHeight - height;
                }
                if (left + width / 2 > virtualWidth)
                {
                    left = virtualWidth - width;
                }
                window.Left = left;
                window.Top = top;
                window.Width = width;
                window.Height = height;
            }
        }

        #endregion

        #region string extensions

        public static string ReplaceSpecialFolder(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains("%MyDocuments%"))
                {
                    str = str.Replace("%MyDocuments%", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                }
                if (str.Contains("%ProgramData%"))
                {
                    str = str.Replace("%ProgramData%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
                }
                if (str.Contains("%Module%"))
                {
                    string moddir = AppDomain.CurrentDomain.BaseDirectory;
                    if (moddir.EndsWith("\\"))
                    {
                        moddir = moddir.Substring(0, moddir.Length - 1);
                    }
                    str = str.Replace("%Module%", moddir);
                }
            }
            return str;
        }

        #endregion

        #region ListBox extensions

        public static bool FocusItem(this ListBox listBox, int idx)
        {
            if (listBox.ItemContainerGenerator.ContainerFromIndex(idx) is ListBoxItem lbi)
            {
                return lbi.Focus();
            }
            return false;
        }

        #endregion

        #region ListViewItem extensions

        public static ListViewItem GetItemAt(this ListView listView, Point clientRelativePosition)
        {
            var hitTestResult = VisualTreeHelper.HitTest(listView, clientRelativePosition);
            var selectedItem = hitTestResult.VisualHit;
            while (selectedItem != null)
            {
                if (selectedItem is ListViewItem)
                {
                    break;
                }
                selectedItem = VisualTreeHelper.GetParent(selectedItem);
            }
            return selectedItem != null ? ((ListViewItem)selectedItem) : null;
        }

        public static bool FocusItem(this ListView listView, int idx)
        {
            if (listView.ItemContainerGenerator.ContainerFromIndex(idx) is ListViewItem lvi)
            {
                return lvi.Focus();
            }
            return false;
        }

        #endregion

        #region DateTime

        public static bool IsSameDay(this DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day;
        }

        #endregion
    }
}

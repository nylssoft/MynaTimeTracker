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
using System.Windows.Input;

namespace TimeTracker
{
    public class CustomCommands
    {
        public static readonly RoutedUICommand Next =
            new RoutedUICommand(
            Properties.Resources.CMD_NEXT,
            "Next",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Previous =
            new RoutedUICommand(
            Properties.Resources.CMD_PREVIOUS,
            "Previous",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Start =
            new RoutedUICommand(
            Properties.Resources.CMD_START,
            "Start",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Stop =
            new RoutedUICommand(
            Properties.Resources.CMD_STOP,
            "Stop",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Add =
            new RoutedUICommand(
            Properties.Resources.CMD_ADD,
            "Add",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.I, ModifierKeys.Control) });

        public static readonly RoutedUICommand Remove =
            new RoutedUICommand(
            Properties.Resources.CMD_DELETE,
            "Remove",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Delete) });

        public static readonly RoutedUICommand Edit =
            new RoutedUICommand(
            Properties.Resources.CMD_EDIT,
            "Edit",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.Enter) });

        public static readonly RoutedUICommand About =
            new RoutedUICommand(
            Properties.Resources.CMD_ABOUT,
            "About",
            typeof(CustomCommands));

        public static readonly RoutedUICommand Exit =
            new RoutedUICommand(
            Properties.Resources.CMD_EXIT,
            "Exit",
            typeof(CustomCommands),
            new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) });

        public static readonly RoutedUICommand ConfigureProjects =
            new RoutedUICommand(
            Properties.Resources.CMD_CONFIGURE_PROJECTS,
            "ConfigureProjects",
            typeof(CustomCommands));

        public static readonly RoutedUICommand CalculateOvertime =
            new RoutedUICommand(
            Properties.Resources.CMD_CALCULATE_OVERTIME,
            "CalculateOvertime",
            typeof(CustomCommands));

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeLines
{
    class TimeLineUtils
	{
        #region TimeSpan Related
        public static string GetTimeMark(TimeSpan time, TimeLineViewLevel lvl)
		{
			switch (lvl)
			{
				case TimeLineViewLevel.MilliSeconds:
					return time.TotalMilliseconds.ToString();
				case TimeLineViewLevel.Seconds:
					return time.TotalSeconds.ToString();
				case TimeLineViewLevel.Minutes:
					return time.TotalMinutes.ToString();
				case TimeLineViewLevel.Hours:
					return time.TotalHours.ToString();
				case TimeLineViewLevel.Days:
					return time.TotalDays.ToString();
				default:
					break;
			}
			return "N/A";
		}
		public static TimeSpan ConvertToTime(double value, TimeLineViewLevel lvl)
		{
			switch (lvl)
			{
				case TimeLineViewLevel.MilliSeconds:
					return TimeSpan.FromMilliseconds(value);
				case TimeLineViewLevel.Seconds:
					return TimeSpan.FromSeconds(value);
				case TimeLineViewLevel.Minutes:
					return TimeSpan.FromMinutes(value);
				case TimeLineViewLevel.Hours:
					return TimeSpan.FromHours(value);
				case TimeLineViewLevel.Days:
					return TimeSpan.FromDays(value);
				default:
					break;
			}
			return TimeSpan.MinValue;
		}
		public static Double ConvertTimeToDistance(TimeSpan span, TimeLineViewLevel lvl, Double unitSize)
		{
			Double value = unitSize;
			switch (lvl)
			{
				case TimeLineViewLevel.MilliSeconds:
					value = span.TotalMilliseconds * unitSize;
					break;
				case TimeLineViewLevel.Seconds:
					value = span.TotalSeconds * unitSize;
					break;
				case TimeLineViewLevel.Minutes:
					value = span.TotalMinutes * unitSize;
					break;
				case TimeLineViewLevel.Hours:
					value = span.TotalHours * unitSize;
					break;
				case TimeLineViewLevel.Days:
					value = span.TotalDays * unitSize;
					break;
				case TimeLineViewLevel.Weeks:
					value = (span.TotalDays / 7.0) * unitSize;
					break;
				case TimeLineViewLevel.Months:
					value = (span.TotalDays / 30.0) * unitSize;
					break;
				case TimeLineViewLevel.Years:
					value = (span.TotalDays / 365.0) * unitSize;
					break;
				default:
					break;
			}
			return value;


		}
		public static TimeSpan ConvertDistanceToTime(Double distance, TimeLineViewLevel lvl, Double unitSize)
		{
			double minutes, hours, days, weeks, months, years, milliseconds = 0;

			switch (lvl)
			{
				case TimeLineViewLevel.MilliSeconds:
					milliseconds = distance / unitSize;
					break;
				case TimeLineViewLevel.Seconds:
					milliseconds = distance / unitSize * 1000;
					break;
				case TimeLineViewLevel.Minutes:
					//value = span.TotalMinutes * unitSize;
					minutes = (distance / unitSize);
					//convert to milliseconds
					milliseconds = minutes * 60000;
					break;
				case TimeLineViewLevel.Hours:
					hours = (distance / unitSize);
					//convert to milliseconds
					milliseconds = hours * 60 * 60000;
					break;
				case TimeLineViewLevel.Days:
					days = (distance / unitSize);
					//convert to milliseconds
					milliseconds = days * 24 * 60 * 60000;
					break;
				case TimeLineViewLevel.Weeks:
					//value = (span.TotalDays / 7.0) * unitSize;
					weeks = (7 * distance / unitSize);
					//convert to milliseconds
					milliseconds = weeks * 7 * 24 * 60 * 60000;
					break;
				case TimeLineViewLevel.Months:
					months = (30 * distance / unitSize); ;
					//convert to milliseconds
					milliseconds = months * 30 * 24 * 60 * 60000;
					break;
				case TimeLineViewLevel.Years:
					years = (365 * distance / unitSize);
					//convert to milliseconds
					milliseconds = years * 365 * 24 * 60 * 60000;
					break;
				default:
					break;
			}
			long ticks = (long)(milliseconds * 10000);
			TimeSpan returner = new TimeSpan(ticks);
			return returner;

			//return new TimeSpan(0, 0, 0, 0, (int)milliseconds);
		}
        #endregion
        public static IEnumerable<TimeLineControl> FindAllTimeLineControls(DependencyObject depObj)
		{
			if (depObj == null)
				yield break;

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
				if (child != null && child is TimeLineControl)
				{
					yield return (TimeLineControl)child;
				}

				foreach (TimeLineControl childOfChild in FindAllTimeLineControls(child))
				{
					yield return childOfChild;
				}
			}			
		}
		public static IEnumerable<TreeViewItem> FindTreeViewItems(DependencyObject @this)
		{
			if (@this == null)
				return null;

			var result = new List<TreeViewItem>();

			var frameworkElement = @this as FrameworkElement;
			if (frameworkElement != null)
			{
				frameworkElement.ApplyTemplate();
			}

			Visual child = null;
			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(@this); i < count; i++)
			{
				child = VisualTreeHelper.GetChild(@this, i) as Visual;

				var treeViewItem = child as TreeViewItem;
				if (treeViewItem != null)
				{
					result.Add(treeViewItem);
					if (treeViewItem.IsExpanded)
					{
						foreach (var childTreeViewItem in FindTreeViewItems(child))
						{
							result.Add(childTreeViewItem);
						}
					}
				}
                else
                {
					foreach (var childTreeViewItem in FindTreeViewItems(child))
					{
						result.Add(childTreeViewItem);
					}
                }
			}
			return result;
		}
		public static IEnumerable<TimeLinesDataBase> FindAllTimeLinesData(ItemCollection data, bool bExpand = false)
		{
			if (data == null) return null;

			var res = new List<TimeLinesDataBase>();
			foreach (TimeLinesDataBase child in data)
				FindAllTimeLinesData_Recursive(child, ref res);
			return res;
		}
		static void FindAllTimeLinesData_Recursive(TimeLinesDataBase data, ref List<TimeLinesDataBase> res)
		{
			res.Add(data);
			if (data.IsExpanded == false) return;
			foreach (TimeLinesDataBase child in data.Childs)
				FindAllTimeLinesData_Recursive(child, ref res);
		}

		/// <summary>
		/// It is depenent on the structure of treeviewitem described in Generic.xaml
		/// </summary>
		public static double GetTreeViewHeaderHeight(DependencyObject @this)
        {
			if (VisualTreeHelper.GetChildrenCount(@this) == 0) 
				return 0;
			@this = VisualTreeHelper.GetChild(@this, 0);
			Grid grid = VisualTreeHelper.GetChild(@this, 0) as Grid;
			return grid.RowDefinitions[0].ActualHeight;
        }
	}
}

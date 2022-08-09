using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace TimeLineTool
{
    class TimeLineUtils
	{
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
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeLineTool;

namespace TimeLineTestApp
{
	public class TempDataType : ITimeLineDataItem
	{
		public TimeSpan? StartTime { get; set; }
		public TimeSpan? EndTime { get; set; }
		public String Name { get; set; }
	}
}

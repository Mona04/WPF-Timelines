using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeLineTool
{
	public interface ITimeLineDataItem
	{
		TimeSpan? StartTime { get; set; }
		TimeSpan? EndTime { get; set; }
	}
}

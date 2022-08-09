using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeLineTool
{
	public interface ITimeLineManager
	{
		Boolean CanAddToTimeLine(ITimeLineDataItem item);
	}
}

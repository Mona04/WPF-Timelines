using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeLines
{
	public interface ITimeLineManager
	{
		Boolean CanAddToTimeLine(ITimeLineData item);
	}
}

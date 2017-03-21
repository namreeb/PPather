using System;
using System.Collections.Generic;
using System.Text;

namespace Pather.Graph
{
	public interface ILocationHeuristics
	{
		float Score(float x, float y, float z);
	}
}

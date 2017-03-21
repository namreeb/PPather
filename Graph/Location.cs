using System;
using System.Collections.Generic;
using System.Text;

using Glider.Common.Objects;

namespace Pather.Graph {
	public class Location {
		private float x;
		private float y;
		private float z;

		public Location(GLocation l) {
			this.x = l.X; this.y = l.Y; this.z = l.Z;
		}

		public Location(float x, float y, float z) {
			this.x = x; this.y = y; this.z = z;
		}

		public float X { get { return x; } }
		public float Y { get { return y; } }
		public float Z { get { return z; } }

		public float GetDistanceTo(Location l) {
			float dx = x - l.X;
			float dy = y - l.Y;
			float dz = z - l.Z;
			return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public override String ToString() {
			//String s = String.Format(
			String s = "[" + x + "," + y + "," + z + "]";
			return s;
		}

		public Location InFrontOf(float heading, float d) {
			float nx = x + (float)Math.Cos(heading) * d;
			float ny = y + (float)Math.Sin(heading) * d;
			float nz = z;
			return new Location(nx, ny, nz);
		}
	}
}

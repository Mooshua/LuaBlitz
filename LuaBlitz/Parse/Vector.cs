using System.Runtime.InteropServices;
using System.Xml;

namespace LuaBlitz.Parse
{

	public struct Vector
	{
		public readonly long Line;
		public readonly long Column;
		public readonly long Index;


		public Vector(long line, long column, long index)
		{
			Line = line;
			Column = column;
			Index = index;
		}

		public Vector NewLine()
		{
			return new Vector(Line + 1, 1,Index + 1);
		}

		public Vector Next()
		{
			return new Vector(Line, Column + 1, Index + 1);
		}

		public Vector Offset(long offset)
		{
			return new Vector(Line, Column + offset, Index + offset);
		}

		public override string ToString()
		{
			return $"(line {Line}, column {Column})";
		}
	}
}
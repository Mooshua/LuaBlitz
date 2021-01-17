using System;
using System.Runtime.Serialization;

namespace LuaBlitz.Parse
{
	public class ParseException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public ParseException()
		{
		}

		public override string Message { get; }

		public ParseException(string message, Vector start, Vector end, String code)
		{
			this.Message = $"[ParseException] Failed with message \"{message}\" at {start.ToString()} -> {end.ToString()}, '{code}'";
		}
	}
}
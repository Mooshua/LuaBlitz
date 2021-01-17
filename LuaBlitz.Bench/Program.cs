using System;
using LuaBlitz.Parse;

namespace LuaBlitz.Bench
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var a =new Lexer("awesome() 23192.53e10 epic() e");
		}
	}
}
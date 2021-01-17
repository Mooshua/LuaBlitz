using System;
using LuaBlitz.Parse;

namespace LuaBlitz.Bench
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var a =new Lexer("awesome() 1_423_192 epic() e");
		}
	}
}
using System;
using LuaBlitz.Parse;

namespace LuaBlitz.Bench
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			var a =new Lexer(@"
function abc(test, test2 :string)
	return 'Hello \' world'
end
");
		}
	}
}
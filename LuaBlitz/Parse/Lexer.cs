using System;
using System.Collections;
using System.Net;

namespace LuaBlitz.Parse
{

	public enum TokenType
	{
		//	short tokens,
		//	like '/', '*', '('
		ShortToken,
		//	long tokens
		//	like 'function', 'local'
		LongToken,
		//	Literals
		//	literal, any free-floating text
		Literal,
		//	number literal, any number
		NumberLiteral,
		//	string literal, legit any string lol
		StringLiteral,
		//	Table literal does not exist,
		//	that's the parser's job.
		//	We'll include it anyways in case we need it
		TableLiteral,
		//	Comment; may be useful for code gen purposes/ "TO DO" searching
		Comment,
		//	And EOF, so we know when to kick the bucket.
		Eof,
		//	None, in case things go *that* badly.
		None,
	}

	public static class LexerConst
	{
		//	Long tokens (words)
		public static ArrayList LongTokens = new ArrayList
		{
			//	--> Lua 5.1
			//	Source: https://www.lua.org/manual/5.1/manual.html#2.1
			"and",
			"break",
			"do",
			"else", "elseif", "end",
			"false", "for", "function",
			"if", "in",
			"local",
			"nil", "not",
			"or",
			"repeat", "return",
			"then", "true",
			"until",
			"while",
			
			//	--> LuaU extension
			//	Source: https://roblox.github.io/luau/syntax.html
			"type",
			//	Not reserved, but linter should treat it as such.
			"continue",
		};

		//	Short tokens (**not words**)
		public static ArrayList ShortTokens = new ArrayList
		{
			//	--> Lua 5.1
			//	Source: 
		
			//	Math
			"+", "-", "*", "/", "&", "^", "#",
			//	Compare
			"=", "<", ">",
			//	Boxes
			"(", ")", "{", "}", "[", "]",
			
			//	Compare
			"==","~=","<=",">=",
			//	Dots
			"..","...",
			//	Comment excluded
			//	There's a ton of custom syntax for that
			//	We want to tokenize it properly.

			//	--> LuaU
			//	Source: https://roblox.github.io/luau/syntax.html
			
			//	Typing
			"->",
			",", ";", ":", ".",
			//	String and number literals are handled as part of parsing
			//	So we'll just ignore them here.
		};
	}

	public class Lexer
	{

		public string Code;
		private char[] _code;

		private long _pointer = 0;
		private Vector _vector = new Vector(1, 0, 0);
		private Vector _lastvec = new Vector(1, 0, 0);

		//	<summary>
		//	Read the next character and advance the vector
		//	</summary>
		private char ReadChar()
		{
			//	If we've passed the length of _code,
			//	We're at EOF.
			if (_pointer >= _code.LongLength)
			{
				//	Return 0.
				return (char) 0;
			}

			//	Save current vector
			_lastvec = _vector;

			//	Get the character at the pointer
			char c = _code[_pointer];
			if (c == '\n')
			{
				//	If it's a newline, advance vector to new row.
				_vector = _vector.NewLine();
			}
			else
			{
				//	Else, advance one column.
				_vector = _vector.Next();
			}

			//	Move pointer forwards for next iteration.
			_pointer++;

			//	Return C :)
			return c;
		}

		private void BackChar()
		{
			_vector = _lastvec;
			_pointer = _pointer - 1;
		}

		//	<summary>
		//	Get the code between start and end.
		//	Not checked for EOF.
		//	</summary>
		private string GetVectorSpan(Vector start, Vector end)
		{
			string s = "";
			for (long i = start.Index; i < end.Index; i++)
			{
				s = string.Concat(s, _code[i]);
			}

			return s;
		}

		//	<summary>
		//	Peek character "offset" away.
		//	Offset is optional.
		//	</summary>
		private char PeekChar(long offset = 1)
		{
			//	Keep a local pointer
			var pointer = _pointer;
			pointer = pointer + offset;
			//	Check to ensure we're within bounds
			if (pointer >= _code.LongLength)
			{
				//	We're out of bounds!
				//	Return null byte.
				return (char) 0;
			}

			//	We're within bounds
			//	Just get the thing and get out
			return _code[pointer];
		}

		//	<summary>
		//	Read a sequence of letters or digits
		//	Returns them :)
		//  </summary>
		private string ReadLettersOrDigits(char start)
		{
			string result = start.ToString();
			char c = start;
			while (char.IsLetterOrDigit(c = PeekChar(0)))
			{
				ReadChar();
				result = string.Concat(result, c);
			}

			//	Move back to account for the 1 char we read that wasn't correct.

			return result;
		}

		private string ReadDigits(char? start)
		{
			string result;
			if (start is not null)
			{
				result = start.ToString();
			}
			else
			{
				result = "";
			}

			char c = ' ';
			while (char.IsDigit(c = PeekChar()))
			{
				ReadChar();
				result = string.Concat(result, c);
			}

			return result;
		}

		private string ReadOp(char start)
		{

			//	Op can be up to 3 chars long
			//	And can overlap
			//	We're going to peek 3 chars ahead
			//	And see if 1-3 match;
			//	And choose the longest match

			//	ex: "..e"
			//	. = true,
			//	.. = true,
			//	-> .. is longest, so that's what we go with.

			var peeked = new char[] {start, PeekChar(1), PeekChar(2)};

			string f = peeked[0].ToString();
			string s = string.Concat(peeked[0], peeked[1]);
			string t = string.Concat(peeked[0], peeked[1], peeked[2]);

			//	"first" must be a short token;
			//	because there are no 1 length op tokens.
			var first = LexerConst.ShortTokens.Contains(f);
			var second = LexerConst.ShortTokens.Contains(s);
			var third = LexerConst.ShortTokens.Contains(t);

			if (third)
			{
				//	Advance pointer
				ReadChar();
				ReadChar();
				//ReadChar();
				//	return it
				return t;
			}
			else if (second)
			{
				ReadChar();
				//ReadChar();
				return s;
			}
			else if (first)
			{
				//ReadChar();
				return f;
			}

			return String.Empty;

		}

		public Lexer(string code)
		{
			Code = code;
			//	Char array for easy parsing.
			_code = code.ToCharArray();

			/*
			Token last;
			while ((last = GetNextToken()).Type != TokenType.Eof)
			{
				Console.WriteLine(last.ToStringVerbose());
			}
			Console.WriteLine("EOF");
			*/

			for (int i = 0; i < 16; i++)
			{
				Console.WriteLine(GetNextToken().ToStringVerbose());
			}

		}

		private Token GotToken(TokenType type, Vector start, string value, double? valueNumber)
		{
			Vector end = _vector.Offset(0);

			if (valueNumber is not null)
			{
				return new Token(type, start, end, GetVectorSpan(start, end), value, (double) valueNumber);
			}
			else if (value is not null)
			{
				return new Token(type, start, end, GetVectorSpan(start, end), value);
			}
			else
			{
				return new Token(type, start, end, GetVectorSpan(start, end));
			}
		}

		public Token GetNextToken()
		{
			{

				char whitespace;
				while (char.IsWhiteSpace(whitespace = ReadChar()))
				{
					//	We're skipping over whitespace.
					//	Ignore this loop.
				}

				if (whitespace == (char) 0)
				{
					return GotToken(TokenType.Eof, _vector.Offset(-1), ((char) 0).ToString(), null);
				}
			}

			BackChar();

			//	Move back 1 to account for the character we just read
			Vector start = _vector.Offset(0);
			//	We're now at not-whitespace.
			//	Let's go.

			char c = ReadChar();

			Console.WriteLine($"{c}, {(int) c} {(char) 0}, {c == (char) 0}");

			if (char.IsLetter(c))
			{
				//	We're a letter; most likely a token
				//	Record for literal or longtoken.
				string value = ReadLettersOrDigits(c);

				//	Check to see if we're a longtoken.
				if (LexerConst.LongTokens.Contains(value))
				{
					//	We're a long token!
					return GotToken(TokenType.LongToken, start, value, null);
				}
				else
				{
					//	Not a long token, must be literal.
					return GotToken(TokenType.Literal, start, value, null);
				}
			}
			else if (char.IsDigit(c))
			{
				//	Either a number or ESCAPED number.
				//	Check if we're 0. If so, peek ahead and see if we find "x", or "b"
				if (c == '0')
				{
					char p = PeekChar();
					if (p == 'b' || p == 'B')
					{
						//	We're binary
						//	Catch up
						ReadChar();
					}
					else if (p == 'x' || p == 'X')
					{
						//	We're hexadecimal
						//	Catch up
						ReadChar();
					}
				}
				
				//	We're an actual number. Strange.
				//	This rarely happens nowadays ;)

				//	LuaU adds support for _ as a separator, purely visual.
				//	We just need to.. well, accomodate for it.
				string number = c.ToString();
				while (true)
				{
					number = String.Concat(number, ReadDigits(null));

					//	What did we stop on?
					char ps = PeekChar(1);
					if (ps == '_')
					{
						//	Ignore the _, continue without even appending it.
						ReadChar();
					}
					else
					{
						break;
					}
				}
				//	Number should now be complete.
				//	We're expecting "e" or "." now.
				Console.WriteLine($"Number Result: {number}");
			}
			else
			{
				{
					string optest = ReadOp(c);
					if (optest == String.Empty)
					{
						//	ReadOp failed to find an op
						//	Continue with life.
					}
					else
					{
						//	ReadOp succeeded!
						//	Build our response.
						return GotToken(TokenType.ShortToken, start, optest, null);
					}
				}
			}

			return GotToken(TokenType.None, start, null, null);

		}

	}
}
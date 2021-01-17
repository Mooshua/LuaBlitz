using System;
using System.Collections;

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
		//	op tokens
		//	like '//', '...'
		OpToken,
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
		//	And EOF, so we know when to kick the bucket.
		Eof,
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

		//	One-char tokens.
		//	**ONLY USE CHAR**
		public static ArrayList ShortTokens = new ArrayList
		{
			//	--> Lua 5.1
			//	Source: https://www.lua.org/manual/5.1/manual.html#2.1

			//	Math
			'+', '-', '*', '/', '&', '^', '#',
			//	Compare
			'=', '<', '>',
			//	Boxes
			'(', ')', '{', '}', '[', ']',
			//	Comma, index, (LuaU Typing)
			',', ';', ':', '.'
		};

		//	Non-text tokens which are more than one char.
		public static ArrayList OpTokens = new ArrayList
		{
			//	--> Lua 5.1
			//	Source: 
			
			//	Compare
			"==","~=","<=",">=",
			//	Dots
			"..","...",
			
			//	--> LuaU
			//	Source: https://roblox.github.io/luau/syntax.html
			
			//	Typing
			"->",
			//	Numbers
			"0x", "0X",
			"0b", "0B",
			//	String literals are handled as part of string parsing, FYI.
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
			while (char.IsLetterOrDigit(c = ReadChar()))
			{
				result = string.Concat(result, c);
			}
			
			//	Move back to account for the 1 char we read that wasn't correct.
			BackChar();

			return result;
		}

		private string ReadDigits(char start)
		{
			string result = start.ToString();
			char c = start;
			while (char.IsDigit(c = ReadChar()))
			{
				result = string.Concat(result, c);
			}
			
			//	Move back cuz of the 1 incorrect char.
			BackChar();

			return result;
		}

		public Lexer(string code)
		{
			Code = code;
			//	Char array for easy parsing.
			_code = code.ToCharArray();
			
			this.GetNextToken();
		}

		public Token GetNextToken()
		{
			{

				while (char.IsWhiteSpace(ReadChar()))
				{
					//	We're skipping over whitespace.
					//	Ignore this loop.
				}
			}
			
			BackChar();

			//	Move back 1 to account for the character we just read
			Vector start = _vector.Offset(0);
			//	We're now at not-whitespace.
			//	Let's go.

			char c = ReadChar();

			TokenType type = TokenType.Eof;
			string value = null;
			double ?valueNumber = null;

			if (char.IsLetter(c))
			{
				//	We're a letter; most likely a token
				//	Record for literal or longtoken.
				value = ReadLettersOrDigits(c);
				
				//	Check to see if we're a longtoken.
				if (LexerConst.LongTokens.Contains(value))
				{
					//	We're a long token!
					type = TokenType.LongToken;
				}
				else
				{
					//	Not a long token, must be literal.
					type = TokenType.Literal;
				}
			}

			if (char.IsDigit(c))
			{
				//	Either a number or ESCAPED number.
				//	Check if we're 0. If so, peek ahead and see if we find "x", or "b"
				if (c == '0')
				{
					char p = PeekChar();
					if (p == 'b' || p == 'B')
					{
						//	We're binary
					} else if (p == 'x' || p == 'X')
					{
						//	We're hexadecimal
					}
					else
					{
						//	We're an actual number. Strange.
						//	This rarely happens nowasays ;)
						
					}
				}
			}
			
			//	End
			//	Capture output
			//	Submit token
			Vector end = _vector.Offset(0);
			if (value is not null)
			{
				return new Token(type, start, end, GetVectorSpan(start,end), value);
			} else if (valueNumber is not null)
			{
				return new Token(type, start, end, GetVectorSpan(start,end), value, valueNumber);
			}
			else
			{
				return new Token(type, start, end, GetVectorSpan(start,end));
			}

		}

	}
}
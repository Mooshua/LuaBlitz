namespace LuaBlitz.Parse
{
	public struct Token
	{
		public readonly TokenType Type;
		public readonly Vector Start;
		public readonly Vector End;
		public readonly string Code;

		public readonly string Value;
		public readonly double ?ValueNumber;

		public Token(TokenType token, Vector start, Vector end, string code)
		{
			Type = token;
			Start = start;
			End = end;
			Code = code;
			Value = code;
			ValueNumber = null;
		}
		
		public Token(TokenType token, Vector start, Vector end, string code, string value)
		{
			Type = token;
			Start = start;
			End = end;
			Code = code;
			Value = value;
			ValueNumber = null;
		}
		
		public Token(TokenType token, Vector start, Vector end, string code, string value, double valuen)
		{
			Type = token;
			Start = start;
			End = end;
			Code = code;
			Value = value;
			ValueNumber = valuen;
		}

		public override string ToString()
		{
			return $"{Start.ToString()} -> {End.ToString()}, '{Code}'";
		}
	}
}
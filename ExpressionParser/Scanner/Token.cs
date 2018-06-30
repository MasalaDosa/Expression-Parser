using System;
using System.Globalization;
using ExpressionParser.Exceptions;

namespace ExpressionParser.Scanner
{
    /// <summary>
    /// Structure  to represent a token.
    /// </summary>
    internal struct Token
    {
        internal TokenType TokenType {get; private set;}
        internal string Value { get; private set; }
        internal int Position { get; private set; }

        /// <summary>
        /// Return a token from TokenType.
        /// Value not important so empty
        /// </summary>
        Token(TokenType tokenType, int position)
        {
            TokenType = tokenType;
            Value = string.Empty;
            Position = position;
        }

        /// <summary>
        /// Return a RealNumber token from a double.
        /// </summary>
        Token(double n, int position)
        {
            TokenType = TokenType.Numeric;
            Value = n.ToString(NumberFormatInfo.InvariantInfo);
            Position = position;
        }

        /// <summary>
        /// Return a string token from a string.
        /// </summary>
        Token(string s, int position)
        {
            TokenType = TokenType.Text;
            Value = s;
            Position = position;
        }

        public override string ToString()
        {
            if (TokenType == TokenType.Numeric)
                return "Numeric: " + Double.Parse(Value, NumberFormatInfo.InvariantInfo);
            else if (TokenType == TokenType.Text)
                return "Text: " + "\"" + Value + "\"";
            else
                return TokenType.ToString();
        }

        /// <summary>
        /// Token from a double.
        /// </summary>
        internal static Token FromDouble(double d, int position)
        {
            return new Token(d, position);
        }

        /// <summary>
        /// Token from a string
        /// </summary>
        internal static Token FromString(string s, int position)
        {
            return new Token(s, position);
        }

        /// <summary>
        /// Return a Token from a Single Char.
        /// E.g. any token but TokenType.Numeric or TokenType.Text
        /// </summary>
        /// <param name="c"></param>
        static public Token FromSingleChar(char c, int position)
        {
            TokenType tokenType;
            switch (c)
            {
                case '(':
                    tokenType = TokenType.OpenParenthesis;
                    break;
                case ')':
                    tokenType = TokenType.CloseParenthesis;
                    break;
                case '+':
                    tokenType = TokenType.Plus;
                    break;
                case '-': 
                    tokenType = TokenType.Minus;
                    break;
                case '*':
                    tokenType = TokenType.Multiply;
                    break;
                case '/':
                    tokenType = TokenType.Divide;
                    break;
                case ',':
                    tokenType = TokenType.Separator;
                    break;
                default:
                    throw new ScannerExceptionWithPosition(string.Format("Cannot scan {0} as single char token at {1}.", c, position), position);
            }
            return new Token(tokenType, position);
        }

        /// <summary>
        /// An EOF Token
        /// </summary>
        /// <returns>The EOF.</returns>
        internal static Token EOF(int position = -1)
        {
            return new Token(TokenType.EOF, position);
        }
    }

}

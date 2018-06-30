using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ExpressionParser.Scanner
{
    public class TokenQueue
    {
        private Queue<Token> _queue = new Queue<Token>();

        internal Token PeekToken()
        {
            if (_queue.Count > 0)
                return _queue.Peek();
            else
                return Token.EOF();
        }

        internal Token NextToken()
        {
            if (_queue.Count > 0)
                return _queue.Dequeue();
            else
                return Token.EOF();
        }

        internal void PushToken(Token t)
        {
            _queue.Enqueue(t);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Token t in _queue)
            {
                sb.AppendLine(t.ToString());
            }
            return sb.ToString();
        }

        public string ToShortString()
        {
            return string.Join(
                ", ",
                _queue.Select(t => string.IsNullOrWhiteSpace(t.Value) ? t.TokenType.ToString() : t.Value)
            ) + "\n";
        }
    }

}

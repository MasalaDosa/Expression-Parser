using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using ExpressionParser.Exceptions;

namespace ExpressionParser.Scanner
{
    /// <summary>
    /// Scan the supplied expression.
    /// </summary>
    public class Scanner
    {
        int _position = 0;
        TextReader _expression = null;
        TokenQueue _tokens = null;

        public Scanner()
        {
        }

        /// <summary>
        /// Scans the text provided by the argument into a TokenQueue.
        /// This can be accessed via the Tokens property
        /// </summary>
        /// <returns></returns>
        public void Scan(string expression)
        {
            try
            {
                if (string.IsNullOrEmpty(expression))
                    throw new ScannerException("Cannot scan null or empty expression.");
                _position = 0;
                _expression = new StringReader(expression);
                _tokens = new TokenQueue();
                foreach (Token t in ScanInternal())
                {
                    _tokens.PushToken(t);
                }
            }
            finally
            {
                _expression.Dispose();
            }
        }

        /// <summary>
        /// Retrieve the tokenstream we have scanned.
        /// </summary>
        public TokenQueue Tokens
        {
            get { return _tokens; }
        }

        IEnumerable<Token> ScanInternal()
        {
            while (_expression.Peek() != -1)
            {
                var position = _position;
                char current = (char)_expression.Peek();
                if (Char.IsWhiteSpace(current))
                {
                    Read();
                }
                else if (IsSingleCharToken(current))
                {
                    Read();
                    yield return Token.FromSingleChar(current, position);
                }
                else if (Char.IsDigit(current) || current == '.')
                {
                    yield return Token.FromDouble(ScanReal(), position);
                }
                else if (Char.IsLetter(current))
                {
                    yield return Token.FromString(ScanName(), position);
                }
                else
                    throw new ScannerExceptionWithPosition(string.Format("Scanner encountered unexpected char {0} at {1}.", current, position), _position);
            }
            yield return Token.EOF(_position);
        }

        /// <summary>
        /// Helper function - the specified char a token in its own right?
        /// </summary>
        bool IsSingleCharToken(char c)
        {
            string singleCharTokens = "()+-*/,";
            return (singleCharTokens.Contains(c));
        }

        /// <summary>
        /// Scan a real number.
        /// </summary>
        double ScanReal()
        {
            char current;
            int i;
            double n = 0;
            current = (char)Read();

            if (Char.IsDigit(current))
            {
                i = int.Parse(current.ToString());
                n = i;
                while (char.IsDigit((char)_expression.Peek()))
                {
                    current = (char)Read();
                    i = int.Parse(current.ToString());
                    n = 10 * n + i;
                }
                if ((char)_expression.Peek() == '.') 
                {
                    Read();
                    return ScanFrac(n, 0.1);
                }
                else
                {
                    return n;
                }
            }
            else // if (current == '.') // Could check for this
            {
                return ScanFrac(n, 0.1);
            }

        }

        double ScanFrac(double n, double wt)
        {

            while (Char.IsDigit((char)_expression.Peek()))
            {
                char current = (char)Read();
                int i = int.Parse(current.ToString());
                n += wt * (i);
                wt /= 10.0;
            }
            return n;
        }

        /// <summary>
        /// Scan a name - this could be a function
        /// </summary>
        string ScanName()
        {
            StringBuilder sb = new StringBuilder();
            while (Char.IsLetterOrDigit((char)_expression.Peek()))
                sb.Append((char)Read());
            return sb.ToString();
        }

        int Read()
        {
            int r = _expression.Read();
            _position++;
            return r;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                if (_expression != null)
                {
                    _expression.Dispose();
                    _expression = null;
                }
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
        }

        ~Scanner()
        {
            Dispose(false);
        }

        #endregion
    }
}

using System;

namespace ExpressionParser.Exceptions
{
    /// <summary>
    /// Base class for Expression language Exceptions
    /// </summary>
    public class ExpressionException : Exception
    {
        public ExpressionException()
        { }

        public ExpressionException(string message)
            : base(message)
        { }

    }

    public class ParserException : ExpressionException
    {
        public ParserException()
            : base()
        { }

        public ParserException(string message)
            : base(message)
        { }
    }

    public class ParserExceptionWithPosistion : ParserException
    {
        public int Posistion { get; set; }

        public ParserExceptionWithPosistion()
            : base()
        { }

        public ParserExceptionWithPosistion(string message)
            : base(message)
        { }


        public ParserExceptionWithPosistion(int position)
            : base()
        {
            Posistion = position;
        }

        public ParserExceptionWithPosistion(string message, int ordinal)
            : base(message)
        {
            Posistion = ordinal;
        }

    }

    public class ScannerException : ExpressionException
    {
        public ScannerException()
            : base()
        { }

        public ScannerException(string message)
            : base(message)
        { }

    }

    public class ScannerExceptionWithPosition : ScannerException
    {
        public int Position { get; private set; }

        public ScannerExceptionWithPosition()
            : base()
        { }

        public ScannerExceptionWithPosition(string message)
            : base(message)
        { }

        public ScannerExceptionWithPosition(int position)
            : base()
        {
            Position = position;
        }

        public ScannerExceptionWithPosition(string message, int position)
            : base(message)
        {
            Position = position;
        }
    }
}

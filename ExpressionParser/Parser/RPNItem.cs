using System;
using System.Diagnostics;
using System.Globalization;

namespace ExpressionParser.Parser
{
    /// <summary>
    /// Represents items in Reverse Polish Notation.
    /// </summary>
    public abstract class RPNItem
    {
        public abstract override string ToString();
    }

    public abstract class OperatorItem : RPNItem
    {
        public abstract string OperatorName { get; }
        public override string ToString()
        {
            return OperatorName;
        }
    }

    public class BinaryOperatorItem : OperatorItem
    {
        string _value = null;
        const string _ops = "+-/*";
        public BinaryOperatorItem(string s)
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentNullException(nameof(s));
            if (s.Length != 1) throw new ArgumentException("Invalid Binary Operator.", nameof(s));
            if (_ops.Contains(s) == false) throw new ArgumentException("Invalid Binary Operator.", nameof(s));
            _value = s;
        }

        public override string OperatorName
        {
            get
            {
                switch (_value)
                {
                    case ("+"):
                        return ("ADD".ToUpperInvariant());
                    case ("-"):
                        return ("SUBTRACT".ToUpperInvariant());
                    case ("*"):
                        return ("MULTIPLY".ToUpperInvariant());
                    case ("/"):
                        return ("DIVIDE".ToUpperInvariant());
                    default:
                        Debug.Assert(false, $"Unexpected BinaryOperatorEntity: {_value}");
                        return string.Empty;
                }
            }
        }
    }

    public class FunctionOperatorItem : OperatorItem
    {
        string _value;
        public FunctionOperatorItem(string s)
        {

            _value = s.ToUpperInvariant();
        }

        public override string OperatorName
        {
            get { return _value; }
        }
    }

    public class NumericItem : RPNItem
    {
        double _value;

        public NumericItem(Double d)
        {
            _value = d;
        }

        public override string ToString()
        {
            return _value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public Double Value
        {
            get { return _value; }
        }
    }
}

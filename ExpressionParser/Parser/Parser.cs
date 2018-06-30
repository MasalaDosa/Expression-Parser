using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using ExpressionParser.Exceptions;
using ExpressionParser.Scanner;

namespace ExpressionParser.Parser
{
    /// <summary>
    /// Parses the following syntax.
    /// We can also include arbitrary functions.
    /// 
    /// Expr := Term ExprTail
    /// ExprTail := nil | '+' Term ExprTail | '-' Term ExprTail
    /// Term := Factor TermTail
    /// TermTail := nil | '*' Factor TermTail | '/' Factor TermTail
    /// Factor := UnaryPrefixOp FactorTail | FactorTail
    /// UnaryPrefixOp = '-'
    /// FactorTail := Number | '(' Expr ') | Function
    /// Number = := NumberToken 
    /// Function := Name '(' ArgList ')'
    /// Name := (One of a recognised/registered list of function Names)
    /// ArgList := nil | NonEmptyArgList - Arg List must contain the correct number of args
    /// NonEmptyArgList :=Expr ArgListTail
    /// ArgListTail : = nil | ',' NonEmptyArgList
    /// 
    /// </summary>
    public class Parser
    {
        // List of strings representing the expression in RPN.
        // We emit directly to this rather than going via a syntax tree
        List<RPNItem> _rpn = new List<RPNItem>();
        Stack<TreeNode> _treeNodes = new Stack<TreeNode>(0);

        /// <summary>
        /// Parse the supplied expression.
        /// If the expression is not valid, then a ParserException will be thrown.
        /// Finally we test to see that all tokens have been parsed.
        /// If not, this is likely to be an error in the expression, for example something like
        /// 4535+54345+5345345POWER(2, 3)
        /// </summary>
        public void ParseExpression(TokenQueue tokens)
        {
            ParseExpressionR(tokens);
            if (tokens.PeekToken().TokenType != TokenType.EOF)
                throw new ParserException(string.Format("Tokens remain after parsing: {0}.", tokens.ToString()));
            return;
        }

        /// <summary>
        /// Parse the supplied expression.
        /// If the expression is not valid, then a ParserException will be thrown.
        /// This method will be called recursively
        /// </summary>
        /// Expr := Term ExprTail
        void ParseExpressionR(TokenQueue tokens)
        {
            ParseTerm(tokens);
            ParseExprTail(tokens);
        }

        /// <summary>
        /// Term := Factor TermTail 
        /// </summary>
        void ParseTerm(TokenQueue tokens)
        {
            ParseFactor(tokens);
            ParseTermTail(tokens);
        }

        /// <summary>
        /// ExprTail := nil | '+' Term ExprTail | '-' Term ExprTail
        /// </summary>
        void ParseExprTail(TokenQueue tokens)
        {
            Token current = tokens.PeekToken();

            if (current.TokenType == TokenType.Plus)
            {
                current = tokens.NextToken();

                ParseTerm(tokens);

                // Append operators postfix
                EmitBinaryOperator("+", current.Position);

                ParseExprTail(tokens);

            }
            else if (current.TokenType == TokenType.Minus)
            {
                current = tokens.NextToken();

                ParseTerm(tokens);

                // Append operators postfix
                EmitBinaryOperator("-", current.Position);
               
                ParseExprTail(tokens);

            }
            else
            {
                // nil - but this is ok.
            }
        }

        /// <summary>
        /// TermTail := nil | '*' Factor | '/' Factor
        /// </summary>
        void ParseTermTail(TokenQueue tokens)
        {
            Token current = tokens.PeekToken();

            if (current.TokenType == TokenType.Divide)
            {
                current = tokens.NextToken();

                ParseFactor(tokens);

                // Append operators postfix
                EmitBinaryOperator("/", current.Position);
               
                ParseTermTail(tokens);
            }
            else if (current.TokenType == TokenType.Multiply)
            {
                current = tokens.NextToken();

                ParseFactor(tokens);

                // Append operators postfix
                EmitBinaryOperator("*", current.Position);

                ParseTermTail(tokens);
            }
            else
            {
                // nil - but this is ok
            }
        }

        /// <summary>
        /// Factor := UnaryPrefixOp FactorTail | FactorTail
        /// </summary>
        void ParseFactor(TokenQueue tokens)
        {
            Token current = tokens.PeekToken();

            if (current.TokenType == TokenType.Minus)
            {
                current = tokens.NextToken();
                ParseFactorTail(tokens);

                // We negate by multiplying by -1.
                EmitNumeric(-1d, current.Position);
                EmitBinaryOperator("*", current.Position);
            }
            else
            {
                ParseFactorTail(tokens);
            }
        }

        /// <summary>
        /// FactorTail := Number | '(' Expr ')' | Function
        /// </summary>
        void ParseFactorTail(TokenQueue tokens)
        {
            Token current = tokens.PeekToken();

            if (current.TokenType == TokenType.OpenParenthesis)
            {
                current = tokens.NextToken();
                ParseExpressionR(tokens);

                // Skip the closing bracket
                if (tokens.PeekToken().TokenType != TokenType.CloseParenthesis)
                    throw new ParserExceptionWithPosistion(string.Format("Expected close bracket at {0}.", current.Position), current.Position);
                tokens.NextToken();
            }
            else if (current.TokenType == TokenType.Text && IsRegisteredFunction(current.Value))
            {
                ParseFunction(tokens);
            }
            else if (current.TokenType == TokenType.Numeric)
            {
                ParseNumber(tokens);
            }
            else
                throw new ParserExceptionWithPosistion(string.Format("Unrecognised factor at {0}.", current.Position), current.Position);
        }

        /// <summary>
        /// </summary>
        void ParseNumber(TokenQueue tokens)
        {
            Token current = tokens.PeekToken();

            if (current.TokenType == TokenType.Numeric)
            {
                // Append numbers as encountered.
                Double result = (Double.Parse(current.Value, NumberFormatInfo.InvariantInfo));

                EmitNumeric(result, current.Position);
                
                 current = tokens.NextToken();
            }
            else
            {
                throw new ParserExceptionWithPosistion(string.Format("Expected number at {0}.", current.Position), current.Position);
            }
        }

        /// Function := Name '(' ArgList ')'
        void ParseFunction(TokenQueue tokens)
        {
            var funcStartPosition = tokens.PeekToken().Position;
            RegisteredFunction function = ParseFunctionName(tokens);
            Token current = tokens.PeekToken();
            if (current.TokenType != TokenType.OpenParenthesis)
                throw new ParserExceptionWithPosistion(string.Format("Expected open bracket at {0}.", current.Position), current.Position);
            tokens.NextToken();
            int args = 0;
            ParseArgList(tokens, ref args);
            current = tokens.PeekToken();
            if (args != function.ArgCount)
                throw new ParserExceptionWithPosistion(string.Format("Invalid number of function parameters in function {0}.  Expected {1} at {2}.", function.Name, function.ArgCount, current.Position), current.Position);
            if (current.TokenType != TokenType.CloseParenthesis)
                throw new ParserExceptionWithPosistion(string.Format("Expected close bracket at {0}.", current.Position), current.Position);

            // Append function names after all their arguments.
            EmitFunction(function, funcStartPosition);

            tokens.NextToken();

        }

        /// Name := (One of a recognised/registered list of function Names)
        RegisteredFunction ParseFunctionName(TokenQueue tokens)
        {
            Token current = tokens.PeekToken();
            if (current.TokenType == TokenType.Text &&
                IsRegisteredFunction(current.Value))
            {
                tokens.NextToken();
                return _functions[current.Value.ToUpperInvariant()];
            }
            else
                throw new ParserExceptionWithPosistion(string.Format("Expected known function at {0}.", current.Position), current.Position);
        }

        /// ArgList :=nil  |  NonEmptyArgList
        void ParseArgList(TokenQueue tokens, ref int argc)
        {
            Token current = tokens.PeekToken();

            // If it is a close parenthesis then its the end of the arglist.
            if (current.TokenType != TokenType.CloseParenthesis)
                ParseNonEmptyArgList(tokens, ref argc);
        }

       /// NonEmptyArgList :=Expr ArgListTail
        void ParseNonEmptyArgList(TokenQueue tokens, ref int argc)
        {
            argc++;
            ParseExpressionR(tokens);
            ParseArgListTail(tokens, ref argc);
        }

        /// ArgListTail : = nil | ',' NonEmptyArgList
        void ParseArgListTail(TokenQueue tokens, ref int argc)
        {
            Token current = tokens.PeekToken();

            // Otherwise it's the end of the arg list
            if (current.TokenType != TokenType.CloseParenthesis)
            {
                if (current.TokenType == TokenType.Separator)
                {
                    tokens.NextToken();
                    ParseNonEmptyArgList(tokens, ref argc);

                }
                else
                    throw new ParserExceptionWithPosistion(string.Format("Expected comma at {0}.", current.Position), current.Position);
            }
        }

        /// <summary>
        /// Emits a binary operator to both rpn and tree
        /// </summary>
        /// <param name="op">Op.</param>
        /// <param name="position">Position.</param>
        void EmitBinaryOperator(string op, int position)
        {
            _rpn.Add(new BinaryOperatorItem(op));
            var r = _treeNodes.Pop();
            var l = _treeNodes.Pop();
            _treeNodes.Push(new TreeNode(op, position, new List<TreeNode> { l, r }));;
        }

        /// <summary>
        /// Emits numeric to both rpn and tree
        /// </summary>
        /// <param name="d">D.</param>
        /// <param name="position">Position.</param>
        void EmitNumeric(double d, int position)
        {
            _rpn.Add(new NumericItem(d));
            _treeNodes.Push(new TreeNode(d.ToString(CultureInfo.InvariantCulture), position, null));
        }

        /// <summary>
        /// Emits a function (variable number of args) to rpn and tree
        /// </summary>
        /// <param name="function">Function.</param>
        /// <param name="position">Position.</param>
        void EmitFunction(RegisteredFunction function, int position)
        {
            _rpn.Add(new FunctionOperatorItem(function.Name));
            List<TreeNode> argNodes = new List<TreeNode>();
            for (int i = 0; i < function.ArgCount; i++)
            {
                argNodes.Add(_treeNodes.Pop());
            }
            argNodes.Reverse();
            _treeNodes.Push(new TreeNode(function.Name, position, argNodes));
        }

        /// <summary>
        /// Get an RPN array (consisting of operators, function names, and invariant culture formatted numbers).
        /// </summary>
        /// <returns></returns>
        public RPNItem[] RPN()
        {
            return _rpn.ToArray();
        }

        /// <summary>
        /// Access the root of the tree
        /// </summary>
        /// <returns>The root.</returns>
        public TreeNode Root()
        {
            Debug.Assert(_treeNodes.Count == 1, "Nodes??");
            return _treeNodes.Pop();
        }

        /// <summary>
        /// Test whether a function is registered.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        bool IsRegisteredFunction(string functionName)
        {
            return _functions.ContainsKey(functionName.ToUpperInvariant());
        }

        /// <summary>
        /// Static constructor builds recognised functions.
        /// Better to do this from the consumer of the parser since that is what needs to interpret them
        /// </summary>
        static Parser()
        {
            _functions = new Dictionary<string, RegisteredFunction>();
            _functions.Add("POWER", new RegisteredFunction("POWER", 2));
            /*
            _functions.Add("MOD", new RegisteredFunction("MOD", 2)); 
            _functions.Add("IDIVIDE", new RegisteredFunction("IDIVIDE", 2)); 
            _functions.Add("SQRT", new RegisteredFunction("SQRT", 1)); 
            _functions.Add("SIN", new RegisteredFunction("SIN", 1)); 
            _functions.Add("COS", new RegisteredFunction("COS", 1)); 
            _functions.Add("TAN", new RegisteredFunction("TAN", 1)); 
            _functions.Add("ASIN", new RegisteredFunction("ASIN", 1)); 
            _functions.Add("ACOS", new RegisteredFunction("ACOS", 1)); 
            _functions.Add("ATAN", new RegisteredFunction("ATAN", 1)); 
            */
        }
        static Dictionary<string, RegisteredFunction> _functions = null;

        /// <summary>
        /// Represents a registered function.
        /// </summary>
        struct RegisteredFunction
        {
            public readonly string Name;
            public readonly int ArgCount;
            public RegisteredFunction(string name, int argCount)
            {
                Name = name.ToUpperInvariant();
                ArgCount = argCount;
            }
        }
    }
}

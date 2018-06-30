using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using ExpressionParser;
using ExpressionParser.Parser;
using ExpressionParser.Exceptions;

namespace ExpressionParser
{


	public static class ExpressionCalculator
	{
		public static double ProcessExpression(string expression)
		{
			var s = new Scanner.Scanner();
			s.Scan(expression);
            Console.WriteLine($"Tokens: {s.Tokens.ToShortString()}");
            Console.WriteLine();
			var p = new Parser.Parser();
			p.ParseExpression(s.Tokens);
            string rpn = string.Join(" ", p.RPN().Select(r => r.ToString()));
            Console.WriteLine();
            Console.WriteLine($"RPN: {rpn}");
            Console.WriteLine();
            Console.WriteLine("Tree:");
            p.Root().Dump();
            Console.WriteLine();

			double result = ProcessRPN(p.RPN());
			return result;
		}

		/// <summary>
		/// Process the RPN on a stack
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		private static double ProcessRPN(RPNItem[] rpnEntities)
		{
			Stack<Double> stack = new Stack<double>();
			foreach (RPNItem item in rpnEntities)
			{
				if (item is NumericItem)
				{
					NumericItem ni = item as NumericItem;
					stack.Push(ni.Value);
				}
				else if (item is BinaryOperatorItem)
				{
					BinaryOperatorItem boi = item as BinaryOperatorItem;
					double a = stack.Pop();
					double b = stack.Pop();
					double r = 0d;
					switch (boi.OperatorName)
					{
						case "ADD":
							r = b + a;
							break;
						case "SUBTRACT":
							r = b - a;
							break;
						case "MULTIPLY":
							r = b * a;
							break;
						case "DIVIDE":
							r = b / a;
							break;
						default:
                            Debug.Fail($"Invalid Binary Operator: {boi.OperatorName}");
							break;
					}
					stack.Push(r);
				}
                // Just one example function for now
                else if (item is FunctionOperatorItem)
                {
                    var foi = item as FunctionOperatorItem;
                    switch (foi.OperatorName)
                    {
                        case "POWER":
                            double a = stack.Pop();
                            double b = stack.Pop();
                            double r = Math.Pow(b, a);
                            stack.Push(r);
                            break;
                    }
                }
			}

			if (stack.Count == 1)
				return stack.Pop();
			else
				throw new ExpressionException("Invalid RPN Stack");
		}
	}
}


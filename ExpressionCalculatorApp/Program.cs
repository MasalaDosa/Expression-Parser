using System;
using ExpressionParser;

namespace ExpressionCalculatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Enter Infix Expression");
                string expression = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(expression))
                    break;
                try
                {
                    Console.WriteLine(ExpressionCalculator.ProcessExpression(expression));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}

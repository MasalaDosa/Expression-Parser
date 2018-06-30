namespace ExpressionParser.Scanner
{
    /// <summary>
    /// Enum of the kinds of token we understand.
    /// </summary>
    enum TokenType
    {
        EOF,
        OpenParenthesis,        // "("
        CloseParenthesis,       // ")"
        Plus,                   // "+"
        Minus,                  // "-"
        Multiply,               // "*"
        Divide,                 // "/"
        Separator,              // "," Most likely separating expressions.
        Numeric,                // "7", "3.143"
        Text,                   // "SomeText" Most likely a function name
    }
}

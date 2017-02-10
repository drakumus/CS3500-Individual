// Skeleton written by Joe Zachary for CS 3500, January 2017

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public class Formula
    {
        String[] OPERATOR_ARRAY = { "+", "-", "/", "*" };
        String formula;
        /// <summary>
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     v
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula)
        {
            //           formula.Replace(" ", string.Empty);
            this.formula = formula;
            isValidFormat();

        }

        public ISet<string> GetVariables()
        {
            return null;
        }

        /// <summary>
        /// examines the token passed to find out whether it is a valid variable or double.
        /// </summary>
        public bool isValidValue(string t, ref double value, ref Lookup lookup)
        {

            //checks if double
            if (Double.TryParse(t, out value))
            {
                return true;
            }
            else
            {
                //attempts to lookup the token to see if it matches any variables and returns the value if it does.
                try
                {
                    value = lookup(t);
                    return true;
                }
                catch (UndefinedVariableException e)
                {

                }
            }
            return false;
        }

        private bool isValidOperator(string s)
        {
            foreach (string o in OPERATOR_ARRAY)
                if (s == o)
                    return true;
            return false;
        }

        private bool isValidVar(string s)
        {
            return Regex.IsMatch(s, @"[a-zA-Z][0-9a-zA-Z]*");
        }

        public void isValidFormat()
        {
            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();
            String token = "";
            Stack<String> openParenthesisStack = new Stack<String>();
            int tokenCounter = 0;
            double result;
            bool openParenOrOperator = false;
            bool closeParenOrNumberOrVariable = false;

            while (eFormula.MoveNext())
            {
                token = eFormula.Current;
                tokenCounter++;
                // check first token
                if (tokenCounter == 1)
                {
                    if (!Double.TryParse(token, out result) && !token.Equals("(") && !isValidVar(token))
                        throw new FormulaFormatException("First token must start with a number, '(' or variable");
                }
                if (token.Equals("("))
                {
                    if (closeParenOrNumberOrVariable == true)
                        throw new FormulaFormatException("Invalid token '(' following close paren or number or variable");
                    openParenthesisStack.Push(token);
                    openParenOrOperator = true;
                }
                else if (token.Equals(")"))
                {
                    if (openParenthesisStack.Count == 0)
                        throw new FormulaFormatException("No matching opening paranthesis");
                    openParenthesisStack.Pop();
                    closeParenOrNumberOrVariable = true;
                    if (openParenOrOperator == true)
                        throw new FormulaFormatException("Invalid ')' following open paren or operator");
                }
                else if (Double.TryParse(token, out result))
                {
                    if (closeParenOrNumberOrVariable == true)
                        throw new FormulaFormatException("Invalid token (number) following close paren or number or variable");
                    closeParenOrNumberOrVariable = true;
                    openParenOrOperator = false;
                }
                else if (isValidVar(token))
                {
                    if (closeParenOrNumberOrVariable == true)
                        throw new FormulaFormatException("Invalid token (variable) following close paren or number or variable");
                    closeParenOrNumberOrVariable = true;
                    openParenOrOperator = false;
                }
                else if (isValidOperator(token))
                {
                    if (openParenOrOperator == true)
                        throw new FormulaFormatException("Invalid token (operator) following open paren or operator");
                    openParenOrOperator = true;
                    closeParenOrNumberOrVariable = false;
                }
                else
                {
                    throw new FormulaFormatException("Invalid token in formula");
                }
            }

            if (tokenCounter == 0)
                throw new FormulaFormatException("No tokens");
            if (openParenthesisStack.Count > 0)
                throw new FormulaFormatException("Matching closing paranthesis missing");
            // last token has to be a number, variable or closing paranthesis
            if (!Double.TryParse(token, out result) && !token.Equals(")") && !isValidVar(token))
            {
                throw new FormulaFormatException("Last token must end with a number, ')' or variable");
            }
        }

        /// <summary>
        /// Cycles through the tokens and finds invalid formatting in the string passed to the constructor.
        /// </summary>
        public void isValidFormat1()
        {
            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();
            string t = "";
            double value = 0;
            int numParanValues = 0;
            int openCounter = 0;
            int closeCounter = 0;
            int nonIntPassed = 0;

            List<string> tokens = new List<string>();
            List<string> set = new List<string>();
            bool openPassed = false;
            bool setOpen = false;

            //counts number of operators and values passed to make sure there aren't any in a row.
            int valuePassed = 0;
            int operatorPassed = 0;

            //cicle through the formula
            while (eFormula.MoveNext())
            {
                t = eFormula.Current;
                tokens.Add(t);
                if (tokens.Count == 0 && isValidOperator(t))
                {
                    throw new FormulaFormatException("You cannot start expression with an operator");
                }
                //counter for tokens. (used for making sure tokens > 0

                if (t == "(")
                {
                    setOpen = true;
                    openCounter++;
                    openPassed = true;
                }
                if (t == ")")
                {
                    openPassed = false;
                    setOpen = false;
                    set.Clear();
                    numParanValues = 0;
                    closeCounter++;
                }

                if (setOpen)
                {
                    set.Add(t);
                }

                //checks for two numbers in a row
                if (Double.TryParse(t, out value))
                {
                    valuePassed++;
                    operatorPassed = 0;
                    if (openPassed)
                    {
                        numParanValues++;
                    }
                }
                else
                {
                    nonIntPassed++;
                    //checks for two operators in a row using a static operator list
                    int opPassedNew = operatorPassed;
                    foreach (var i in OPERATOR_ARRAY)
                    {
                        if (t == i)
                            opPassedNew++;
                    }

                    if (opPassedNew > operatorPassed)
                    {
                        operatorPassed = opPassedNew;
                    }
                    else
                    {
                        operatorPassed = 0;
                    }
                    //resets num variables passed in a row
                    valuePassed = 0;

                }

                if (set.Count > 1)
                {
                    if (isValidOperator(set[1]))
                        throw new FormulaFormatException("you cannot have an operator after an open parethese");
                }
                //throws an exception if more than 1 variable is passed in a row
                if (valuePassed > 1)
                {
                    throw new FormulaFormatException("2 or more variables were passed in a row.");
                }
                //throws an exception if more than 1 operator is passed in a row
                if (operatorPassed > 1)
                {
                    throw new FormulaFormatException("2 or more operators were passed in a row.");
                }
                //throws an exception if the number of tokens is 0
                if (tokens.Count == 0)
                    throw new FormulaFormatException("No tokens were passed");
                //if (!isValidVar(t))
                //    throw new FormulaFormatException("Invalid Var");
            }
            if (isValidOperator(t))
                throw new FormulaFormatException("Expressions cannot end with a valid operator.");

            if (t == "")
                throw new FormulaFormatException("Empty Expression Passed");

            if (openCounter != closeCounter)
                throw new FormulaFormatException("Invalid Parentheses formatting.");

            //if (openCounter == closeCounter && valuePassed == 0)
            //  throw new FieldAccessException("There must be an item between Parentheses");
        }

        /// <summary>
        /// Passes value token 1 (vt1), value token 2 (vt2), and operation token (opt)
        /// to calculate 
        /// </summary>

        private static double Calculate(double vt1, string opt, double vt2)
        {
            double solution = 0;

            switch (opt)
            {
                case "*":
                    solution = vt1 * vt2;
                    break;
                case "/":
                    if (vt2 == 0)
                        throw new FormulaEvaluationException("Divison by 0");
                    solution = vt1 / vt2;
                    break;
                case "-":
                    solution = vt1 - vt2;
                    break;
                case "+":
                    solution = vt1 + vt2;
                    break;
            }

            return solution;
        }

        private String peekOperatorStack(Stack<String> s)
        {
            if (s.Count > 0)
                return s.Peek();
            return "";
        }

        public double Evaluate(Lookup lookup)
        {
            Stack<double> valueStack = new Stack<double>();
            Stack<String> operatorStack = new Stack<String>();

            String token;
            double sval, tval, result;
            String op;

            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();

            while (eFormula.MoveNext())
            {
                token = eFormula.Current;
                if (Double.TryParse(token, out tval))
                {
                    op = peekOperatorStack(operatorStack);
                    if (op.Equals("*") || op.Equals("/"))
                    {
                        sval = valueStack.Pop();
                        op = operatorStack.Pop();
                        result = Calculate(sval, op, tval);
                        valueStack.Push(result);
                    }
                    else
                    {
                        valueStack.Push(tval);
                    }
                }
                else if (isValidVar(token))
                {
                    try
                    {
                        tval = lookup(token);
                    }
                    catch (UndefinedVariableException ex)
                    {
                        throw new FormulaEvaluationException("Variable not defined");
                    }
                    op = peekOperatorStack(operatorStack);
                    if (op.Equals("*") || op.Equals("/"))
                    {
                        sval = valueStack.Pop();
                        op = operatorStack.Pop();
                        result = Calculate(sval, op, tval);
                        valueStack.Push(result);
                    }
                    else
                    {
                        valueStack.Push(tval);
                    }
                }
                else if (token.Equals("+") || token.Equals("-"))
                {
                    op = peekOperatorStack(operatorStack);
                    if (op.Equals("+") || op.Equals("-"))
                    {
                        tval = valueStack.Pop();
                        sval = valueStack.Pop();
                        op = operatorStack.Pop();
                        result = Calculate(sval, op, tval);
                        valueStack.Push(result);
                    }
                    operatorStack.Push(token);
                }
                else if (token.Equals("*") || token.Equals("/"))
                {
                    operatorStack.Push(token);
                }
                else if (token.Equals("("))
                {
                    operatorStack.Push(token);
                }
                else if (token.Equals(")"))
                {
                    op = peekOperatorStack(operatorStack);
                    if (op.Equals("+") || op.Equals("-"))
                    {
                        tval = valueStack.Pop();
                        sval = valueStack.Pop();
                        op = operatorStack.Pop();
                        result = Calculate(sval, op, tval);
                        valueStack.Push(result);
                    }

                    operatorStack.Pop();

                    op = peekOperatorStack(operatorStack);
                    if (op.Equals("*") || op.Equals("/"))
                    {
                        tval = valueStack.Pop();
                        sval = valueStack.Pop();
                        op = operatorStack.Pop();
                        result = Calculate(sval, op, tval);
                        valueStack.Push(result);
                    }
                }
            }
            if (operatorStack.Count == 0)
                return valueStack.Pop();

            tval = valueStack.Pop();
            sval = valueStack.Pop();
            op = operatorStack.Pop();
            return Calculate(sval, op, tval);

        }

        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate1(Lookup lookup)
        {
            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();


            String t = "";  //temp token
            double value = 0;

            //respective stacks
            Stack<double> valueStack = new Stack<double>();
            Stack<String> operatorStack = new Stack<String>();

            while (eFormula.MoveNext())
            {
                t = eFormula.Current;
                if (isValidValue(t, ref value, ref lookup))
                {
                    if (operatorStack.Count != 0)
                    {

                        switch (operatorStack.Peek())
                        {
                            //case for multiply at top of operator stack when a valid variable or value is found
                            case "*":
                                operatorStack.Pop();
                                valueStack.Push(valueStack.Pop() * value);
                                break;
                            //case for divide at top of operator stack when a valid variable or value is found
                            case "/":
                                try
                                {
                                    operatorStack.Pop();
                                    valueStack.Push(valueStack.Pop() / value);
                                }
                                catch (DivideByZeroException e)
                                {
                                    throw new System.DivideByZeroException("Your formula attempts to divide by 0");
                                }
                                break;
                            default:
                                valueStack.Push(value);
                                break;
                        }
                    }
                    else
                    {
                        valueStack.Push(value);
                    }
                }
                else
                {
                    if (operatorStack.Count != 0)
                    {
                        switch (t)
                        {
                            //case for t when + is found
                            case "+":
                                switch (operatorStack.Peek())
                                {
                                    case "-":
                                        operatorStack.Pop();
                                        valueStack.Push(-valueStack.Pop() + valueStack.Pop());
                                        break;
                                    case "+":
                                        operatorStack.Pop();
                                        valueStack.Push(valueStack.Pop() + valueStack.Pop());
                                        break;
                                }
                                operatorStack.Push(t);
                                break;
                            //case for t when it is found to be the "-" operator
                            case "-":
                                switch (operatorStack.Peek())
                                {
                                    case "-":
                                        operatorStack.Pop();
                                        valueStack.Push(-valueStack.Pop() + valueStack.Pop());
                                        break;
                                    case "+":
                                        operatorStack.Pop();
                                        valueStack.Push(valueStack.Pop() + valueStack.Pop());
                                        break;
                                }
                                operatorStack.Push(t);
                                break;
                            //case for t when "*" is the current operator
                            case "*":
                                operatorStack.Push(t);
                                break;
                            //case for t when "/" is the current operator
                            case "/":
                                operatorStack.Push(t);
                                break;
                            //case for t when "(" is the current operator
                            case "(":
                                operatorStack.Push(t);
                                break;
                            //case for t when ")" is the current operator
                            case ")":
                                switch (operatorStack.Peek())
                                {
                                    case "+":
                                        operatorStack.Pop();
                                        valueStack.Push(valueStack.Pop() + valueStack.Pop());
                                        break;
                                    case "-":
                                        operatorStack.Pop();
                                        valueStack.Push(-valueStack.Pop() + valueStack.Pop());
                                        break;
                                }
                                operatorStack.Pop();
                                if (operatorStack.Count != 0)
                                {
                                    switch (operatorStack.Peek())
                                    {
                                        case "*":
                                            operatorStack.Pop();
                                            valueStack.Push(valueStack.Pop() * valueStack.Pop());
                                            break;
                                        case "/":
                                            double temp = valueStack.Pop();
                                            operatorStack.Pop();
                                            valueStack.Push(valueStack.Pop() / temp);
                                            break;
                                    }
                                }
                                break;
                            //if all operators fall through and no valid lookup variable is found then an exception is thrown for Formula Evaluation
                            default:
                                throw new FormulaEvaluationException("Variable " + t + " is undefined.");

                        }
                    }
                    else
                    {
                        operatorStack.Push(t);
                    }

                }
            }

            //case for when all tokens are cycled through
            if (operatorStack.Count != 0)
            {
                switch (operatorStack.Pop())
                {
                    case "-":
                        valueStack.Push(-valueStack.Pop() + valueStack.Pop());
                        break;
                    case "+":
                        valueStack.Push(valueStack.Pop() + valueStack.Pop());
                        break;
                }
            }

            //final evaluated value
            var output = valueStack.Pop();
            return output;
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            // PLEASE NOTE:  I have added white space to this regex to make it more readable.
            // When the regex is used, it is necessary to include a parameter that says
            // embedded white space should be ignored.  See below for an example of this.
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern.  It contains embedded white space that must be ignored when
            // it is used.  See below for an example of this.
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            // PLEASE NOTE:  Notice the second parameter to Split, which says to ignore embedded white space
            /// in the pattern.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string var);

    public delegate string Normalizer(string s);
    public delegate bool Validator(string s);

    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    [Serializable]
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    [Serializable]
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    [Serializable]
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }
}
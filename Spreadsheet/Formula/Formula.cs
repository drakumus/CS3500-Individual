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
        ///     
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
            this.formula = formula;
            isValidFormat();
        }

        public bool isValidValue(string t, ref double value, ref Lookup lookup)
        {
            if (Double.TryParse(t, out value))
            {
                return true;
            }
            else
            {
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

        public void isValidFormat()
        {
            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();
            string t;
            double value = 0;
            int numTokens = 0;

            int valuePassed = 0;
            int operatorPassed = 0;

            while (eFormula.MoveNext())
            {
                numTokens++;
                t = eFormula.Current;
                if (Double.TryParse(t, out value))
                {
                    valuePassed++;
                    operatorPassed = 0;
                }else
                {
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
                    
                    valuePassed = 0;
                }
                if (valuePassed > 1)
                {
                    throw new FormulaFormatException("2 or more variables were passed in a row.");
                }
                if (operatorPassed > 1)
                {
                    throw new FormulaFormatException("2 or more operators were passed in a row.");
                }
                if (numTokens == 0)
                    throw new FormulaFormatException("No tokens were passed");
                
            }
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
        public double Evaluate(Lookup lookup)
        {
            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();


            String t = "";  //temp token
            double value = 0;

            Stack<double> valueStack = new Stack<double>();
            Stack<String> operatorStack = new Stack<String>();

            while(eFormula.MoveNext())
            {
                t = eFormula.Current;
                if (isValidValue(t, ref value, ref lookup))
                {
                    if (operatorStack.Count != 0)
                    {
                        switch (operatorStack.Peek())
                        {
                            case "*":
                                operatorStack.Pop();
                                valueStack.Push(valueStack.Pop() * value);
                                break;
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
                    } else
                    {
                        valueStack.Push(value);
                    }
                } else
                {
                    if (operatorStack.Count != 0)
                    {
                        switch (t)
                        {
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
                            case "*":
                                operatorStack.Push(t);
                                break;
                            case "/":
                                operatorStack.Push(t);
                                break;
                            case "(":
                                operatorStack.Push(t);
                                break;
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
                                if(operatorStack.Count != 0) { 
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
                            default:
                                throw new FormulaEvaluationException("Variable " + t + " is undefined.");
                                
                        }
                    }else
                    {
                        operatorStack.Push(t);
                    }

                }
            }

            if(operatorStack.Count != 0)
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
            var output = valueStack.Pop();
            return output;
        }

        private double Operate(double var1, string op, double var2)
        {
            //var1 is the popped value, var2 is the current t
            double dVar1 = Convert.ToDouble(var1);
            double dVar2 = Convert.ToDouble(var2);
            double output = 0;

            switch (op)
            {
                case "*":
                    output = dVar1 * dVar2;
                    break;
                case "/":
                    try
                    {
                        output = dVar1 / dVar2;
                    }catch(DivideByZeroException e)
                    {
                        throw new System.DivideByZeroException("Your formula attempts to divide by 0");
                    }
                    break;
                case "+":
                    output = dVar1 + dVar2;
                    break;
                case "-":
                    output = dVar1 - dVar2;
                    break;
                default: throw new System.InvalidOperationException("The operator you are attempting to use is invalid (programmer fucked up)");
            }
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

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
    public struct Formula
    {

        String[] OPERATOR_ARRAY;
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
        /// 

        /*

        public Formula()
        {
            OPERATOR_ARRAY = new String[]{ "+", "-", "/", "*" };
            formula = "0";
            isValidFormat();
        }
        */

        public ISet<String> GetVariables()
        {
            IEnumerable<String> tokens = GetTokens(formula);
            ISet<String> variables = new HashSet<String>();
            foreach (string s in tokens)
                if (isValidVar(s))
                    variables.Add(s);

            return variables;
        }
        public Formula(String formula)
        {
            //           formula.Replace(" ", string.Empty);
            OPERATOR_ARRAY = new String[] { "+", "-", "/", "*" };
            this.formula = formula;
            isValidFormat();

        }

        public Formula(String formula, Normalizer normalizer, Validator validator) :this(formula)
        {
            if (normalizer == null || validator == null)
                throw new ArgumentNullException("You are missing a parameter");

            NormalizedValidate(normalizer);
            if(ValidaterValidate(normalizer, validator))
            {
                UpdateVariables(normalizer);
            }
            //write a variable check and make a variant that can use normalize and thorw FormulaFormatException
            //write variable check that also passes through validator 
            //if the above conditions pass then pass N(x) as formula and evaluate accordingly. 

            //after normalizing the method pass the normalized string into ToString (this is probably already achieved when u assign the
            //normalized string to formula.

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

        /// <summary>
        /// checks if variable passed is formatted correctly via Regex.
        /// </summary>
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
       
        private void NormalizedValidate(Normalizer normalize)
        {
            if (normalize == null)
                throw new ArgumentNullException("Parameter Expected");
            ISet<String> vars = GetVariables();
            bool isValid = true;
            if(vars.Count > 0)
                foreach (string s in vars)
                    if (!isValidVar(normalize(s)))
                        isValid = false;
            if (!isValid)
                throw new FormulaFormatException("The normalized variables are not valid");
        }

        private bool ValidaterValidate(Normalizer normalize, Validator validate)
        {
            if (normalize == null || validate == null)
                throw new ArgumentNullException("Parameter Expected");
            ISet<String> vars = GetVariables();
            bool isValid = true;
            if (vars.Count > 0)
                foreach (string s in vars)
                    if (!validate(normalize(s)))
                        isValid = false;
            if (!isValid)
                throw new FormulaFormatException("The normalized variable did not pass the validator's requirements");
            return isValid;
        }

        private void UpdateVariables(Normalizer normalize)
        {
            if (normalize == null)
                throw new ArgumentNullException("Parameter Expected");
            ISet<String> set = new HashSet<String>();
            foreach (string s in GetVariables())
            {
                formula.Replace(s,normalize(s));
                set.Add(normalize(s));
            }
        }

        /// <summary>
        /// Passes value token 1 (vt1), value token 2 (vt2), and operation token (opt)
        /// to calculate 
        /// </summary>

        private static double Calculate(double vt1, string opt, double vt2)
        {
            if (opt == null)
                throw new ArgumentNullException("Parameter Expected");

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
            if (lookup == null)
                throw new ArgumentNullException("Parameter Expected");

            Stack<double> valueStack = new Stack<double>();
            Stack<String> operatorStack = new Stack<String>();

            String token;
            double sval, tval, result;
            String op;

            IEnumerator<String> eFormula = GetTokens(formula).GetEnumerator();

            if (formula == null)
            {
                formula = "0";
            }

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
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            if (formula == null)
            {
                formula = "0";
            }
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

        /// <summary>
        /// Returns formula normalized or not depending on the used constructor.
        /// Normalization is handled in the constructor and independent of the return for toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return formula;
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
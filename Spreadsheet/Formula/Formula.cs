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

        ///Get Variables acts as a tool that cycles through tokens
        ///within the formula string. If these tokens are validated as variables
        ///then they are added to a set
        ///the returned item is an ISet of variables
        public ISet<String> GetVariables()
        {
            //sets up an IEnumerable data set of tokens using the getTokens method
            IEnumerable<String> tokens = GetTokens(formula);
            //creates a hashset to store tokens to
            ISet<String> variables = new HashSet<String>();
            //cycles through all tokens in the formula string and checks if they follow the normal definition
            //if they do they're stored in the variables Set
            foreach (string s in tokens)
                if (isValidVar(s))
                    variables.Add(s);

            return variables;
        }

        ///<summary>
        ///base constructor for formula. It also store base values for various variables
        ///and checks for valid format of the formula
        ///</summary>
        public Formula(String formula) :this("0", n=> n, v => true)
        {
            
            OPERATOR_ARRAY = new String[] { "+", "-", "/", "*" };
            this.formula = formula;
            isValidFormat();

        }
        
        /// <summary>
        /// Takes inputs for normalizer and validator for a refined formula
        /// see instantiation of the delegates normalizer and validator for details.
        /// This constructor also acts to check if Normalizer and Validator meet their 
        /// conditions or break the code.
        /// </summary>
        public Formula(String formula, Normalizer normalizer, Validator validator)
        {
            OPERATOR_ARRAY = new String[] { "+", "-", "/", "*" };
            this.formula = formula;
            isValidFormat();
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
        private bool isValidValue(string t, ref double value, ref Lookup lookup)
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

        /// <summary>
        /// checks if a passed string is contained in the OPERATOR_ARRAY
        /// which holds all valid operators.
        /// Returns true if it does and false if it does not.
        /// </summary>
        private bool isValidOperator(string s)
        {
            foreach (string o in OPERATOR_ARRAY)
                if (s == o)
                    return true;
            return false;
        }

        /// <summary>
        /// checks if variable passed is formatted correctly via Regex and follows the normal
        /// defenition of a variable.
        /// </summary>
        private bool isValidVar(string s)
        {
            return Regex.IsMatch(s, @"[a-zA-Z][0-9a-zA-Z]*");
        }

        /// <summary>
        /// This array checks for all cases of error for formatting that can occur when passing an array.
        /// </summary>
        private void isValidFormat()
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
                // check first token to make sure it is not an opperator
                if (tokenCounter == 1)
                {
                    if (!Double.TryParse(token, out result) && !token.Equals("(") && !isValidVar(token))
                        throw new FormulaFormatException("First token must start with a number, '(' or variable");
                }
                if (token.Equals("("))
                {
                    //starts tracking parenthesis and checks to make sure that a closed parenthesis, number, or variable isn't immediately preceding it
                    if (closeParenOrNumberOrVariable == true)
                        throw new FormulaFormatException("Invalid token '(' following close paren or number or variable");
                    openParenthesisStack.Push(token);
                    openParenOrOperator = true;
                }
                // throws an error if there is no open parenthesis to match
                else if (token.Equals(")"))
                {
                    if (openParenthesisStack.Count == 0)
                        throw new FormulaFormatException("No matching opening paranthesis");
                    //pops an open Parenthesis off its stack since a set is complete with close
                    openParenthesisStack.Pop();
                    //sets case for close paren true to make sure an open parenthesis doesn't appear right after
                    closeParenOrNumberOrVariable = true;
                    //checks if an open operator is immediately preceeding a closed operator
                    if (openParenOrOperator == true)
                        throw new FormulaFormatException("Invalid ')' following open paren or operator");
                }
                //double casses
                else if (Double.TryParse(token, out result))
                {
                    //case for if this value is immediately next to a matching type or paren or variable
                    if (closeParenOrNumberOrVariable == true)
                        throw new FormulaFormatException("Invalid token (number) following close paren or number or variable");
                    closeParenOrNumberOrVariable = true;
                    openParenOrOperator = false;
                }
                //checks if the variable is valid by normal definition and supplies cases for it
                else if (isValidVar(token))
                {
                    //case for fail type if paren, number or variable is next to it
                    if (closeParenOrNumberOrVariable == true)
                        throw new FormulaFormatException("Invalid token (variable) following close paren or number or variable");
                    //sets up for close paren case
                    closeParenOrNumberOrVariable = true;
                    //sets up for open paren case
                    openParenOrOperator = false;
                }
                //cases for all values in VALID_OPERATOR array
                else if (isValidOperator(token))
                {
                    //makes sure an open paren is not immediately before these strings
                    if (openParenOrOperator == true)
                        throw new FormulaFormatException("Invalid token (operator) following open paren or operator");
                    openParenOrOperator = true;
                    closeParenOrNumberOrVariable = false;
                }
                //if all other cases fall through throw an error for invalid token
                else
                {
                    throw new FormulaFormatException("Invalid token in formula");
                }
            }

            //makes sure the formula isn't empty
            if (tokenCounter == 0)
                throw new FormulaFormatException("No tokens");
            //case for unresolved open paren
            if (openParenthesisStack.Count > 0)
                throw new FormulaFormatException("Matching closing paranthesis missing");
            // last token has to be a number, variable or closing paranthesis
            if (!Double.TryParse(token, out result) && !token.Equals(")") && !isValidVar(token))
            {
                throw new FormulaFormatException("Last token must end with a number, ')' or variable");
            }
        }

       /// <summary>
       /// checks to make sure that normalize does not invalidate the variables it is changing
       /// </summary>
        private void NormalizedValidate(Normalizer normalize)
        {
            //checks for empty parameter
            if (normalize == null)
                throw new ArgumentNullException("Parameter Expected");
            //gets all variables
            ISet<String> vars = GetVariables();
            //base case
            bool isValid = true;
            //cycles through each variable in variables and passes normalize then passes the normalized token into isValidVar
            //this checks to make sure it follows standard form.
            if (vars.Count > 0)
                foreach (string s in vars)
                    if (!isValidVar(normalize(s)))
                        isValid = false;
            //throws an exception if the var is not valid
            if (!isValid)
                throw new FormulaFormatException("The normalized variables are not valid");
        }

        /// <summary>
        /// checks to make sure the validate deligate passes as true for all normalized variables
        /// </summary>
        private bool ValidaterValidate(Normalizer normalize, Validator validate)
        {
            //checks for null parameters
            if (normalize == null || validate == null)
                throw new ArgumentNullException("Parameter Expected");
            //makes a set for all variables
            ISet<String> vars = GetVariables();
            //base case
            bool isValid = true;
            //checks to make sure vars exist
            if (vars.Count > 0)
                //cycles through vars and checks to make sure the normalized variables pass the validate deligate
                foreach (string s in vars)
                    if (!validate(normalize(s)))
                        isValid = false;
            //throws an exception if the normalized variables dont validate.
            if (!isValid)
                throw new FormulaFormatException("The normalized variable did not pass the validator's requirements");
            //returns the result
            return isValid;
        }
        /// <summary>
        /// replaces all variables with their normalized counterparts
        /// </summary>
        private void UpdateVariables(Normalizer normalize)
        {
            //checks for null parameters
            if (normalize == null)
                throw new ArgumentNullException("Parameter Expected");
            //replaces variables with normalized variables 
            ISet<string> set = GetVariables();
            string v;
            foreach (string s in set)
            {
                v = normalize(s);
                formula = formula.Replace(s,normalize(s));
            }
        }

        /// <summary>
        /// Passes value token 1 (vt1), value token 2 (vt2), and operation token (opt)
        /// to calculate result
        /// </summary>

        private static double Calculate(double vt1, string opt, double vt2)
        {
            //checks for null inputs. doubles cannot be null by nature
            if (opt == null)
                throw new ArgumentNullException("Parameter Expected");

            double solution = 0;

            //cases for various operators
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
            //returns result
            return solution;
        }

        /// <summary>
        /// peeks at the top of the operator stack
        /// </summary>
        /// <param name="s"></param>
        /// <returns>returns top of operator stack or "" if stack is empty</returns>
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

            //case for null formula
            
            if (formula == null)
            {
                formula = "0";
            }

            //cycles through tokens
            while (eFormula.MoveNext())
            {
                token = eFormula.Current;
                //case for token being a double
                if (Double.TryParse(token, out tval))
                {
                    op = peekOperatorStack(operatorStack);
                    //case for * and /
                    if (op.Equals("*") || op.Equals("/"))
                    {
                        sval = valueStack.Pop();
                        op = operatorStack.Pop();
                        result = Calculate(sval, op, tval);
                        valueStack.Push(result);
                    }
                    //pushes to operator stack if value stack empty
                    else
                    {
                        valueStack.Push(tval);
                    }
                }
                //case for valid variable
                else if (isValidVar(token))
                {
                    //case for looking up the variable
                    try
                    {
                        tval = lookup(token);
                    }
                    catch (UndefinedVariableException ex)
                    {
                        //throws exception if no variable could be found
                        throw new FormulaEvaluationException("Variable not defined");
                    }
                    //peeks operator stack and builds cases for it incase var is valid
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
                //case for + - operators
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
                //case for * / tokens. Simply pushes to operator stack. same for (
                else if (token.Equals("*") || token.Equals("/"))
                {
                    operatorStack.Push(token);
                }
                else if (token.Equals("("))
                {
                    operatorStack.Push(token);
                }
                //case for closing parentheses. Attempts to resolve paren set
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
            //case for empty operator stack
            if (operatorStack.Count == 0)
                return valueStack.Pop();
            //if operator stack isn't empty there must be 2 val and 1 operator which is resolved
            //and returned with calculate
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
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
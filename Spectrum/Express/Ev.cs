//Copyright (c) 2015 Giorgi Dalakishvili. All rights reserved.
//The original, unmodified source files can be located at
//    https://github.com/Giorgi/Math-Expression-Evaluator

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Spectrum.ExpressTest
{
    public class ExpressionEvaluator : DynamicObject
    {
        public CultureInfo Culture { get; set; }
        private readonly Stack<Expression> expressionStack = new();
        private readonly Stack<Symbol> operatorStack = new();
        private readonly List<string> parameters = new();
        private readonly Func<long, long> ReadRam;

        /// <summary>
        /// Initializes new instance of <see cref="ExpressionEvaluator"></see> using <see cref="CultureInfo.InvariantCulture" />
        /// </summary>
        public ExpressionEvaluator(Func<long, long> t) : this(t, CultureInfo.InvariantCulture)
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="ExpressionEvaluator"></see> using specified culture info
        /// </summary>
        /// <param name="culture">Culture to use for parsing long numbers</param>
        public ExpressionEvaluator(Func<long, long> t, CultureInfo culture)
        {
            ReadRam = t;
            Culture = culture;
        }

        //public Func<object, long> Compile(string expression)
        //{
        //    var compiled = Parse(expression);

        //    Func<List<string>, Func<object, long>> curriedResult = list => argument =>
        //    {
        //        var arguments = ParseArguments(argument);
        //        return Execute(compiled, arguments, list);
        //    };

        //    var result = curriedResult(parameters.ToList());

        //    return result;
        //}

        public long Evaluate(string expression, object argument = null)
        {
            var arguments = ParseArguments(argument);

            return Evaluate(expression, arguments);
        }

        private long Evaluate(string expression, Dictionary<string, long> arguments)
        {
            var compiled = Parse(expression);

            return Execute(compiled, arguments, parameters);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if ("Evaluate" != binder.Name)
            {
                return base.TryInvokeMember(binder, args, out result);
            }

            if (args[0] is not string)
            {
                throw new ArgumentException("No expression specified for parsing");
            }

            //args will contain expression and arguments,
            //ArgumentNames will contain only named arguments
            if (args.Length != binder.CallInfo.ArgumentNames.Count + 1)
            {
                throw new ArgumentException("Argument names missing.");
            }

            var arguments = new Dictionary<string, long>();

            for (int i = 0; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                if (IsNumeric(args[i + 1].GetType()))
                {
                    arguments.Add(binder.CallInfo.ArgumentNames[i], Convert.ToInt64(args[i + 1]));
                }
            }

            result = Evaluate((string)args[0], arguments);

            return true;
        }


        private Func<long[], long> Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return s => 0;
            }

            var arrayParameter = Expression.Parameter(typeof(long[]), "args");

            parameters.Clear();
            operatorStack.Clear();
            expressionStack.Clear();

            using (var reader = new StringReader(expression))
            {
                int peek;
                while ((peek = reader.Peek()) > -1)
                {
                    var next = (char)peek;

                    if (char.IsLetterOrDigit(next))
                    {
                        expressionStack.Push(ReadOperand(reader));
                        continue;
                    }

                    if (char.IsLetter(next))
                    {
                        expressionStack.Push(ReadParameter(reader, arrayParameter));
                        continue;
                    }

                    if (Operation.IsDefined(next))
                    {
                        if (next == '-' && expressionStack.Count == 0)
                        {
                            reader.Read();
                            operatorStack.Push(Operation.UnaryMinus);
                            continue;
                        }

                        var currentOperation = ReadOperation(reader);

                        EvaluateWhile(() => operatorStack.Count > 0 && operatorStack.Peek() != Parentheses.Left
                            && operatorStack.Peek() != Brackets.Left &&
                            currentOperation.Precedence <= ((Operation)operatorStack.Peek()).Precedence);

                        operatorStack.Push(currentOperation);
                        continue;
                    }

                    if (next == '(')
                    {
                        reader.Read();
                        operatorStack.Push(Parentheses.Left);

                        if (reader.Peek() == '-')
                        {
                            reader.Read();
                            operatorStack.Push(Operation.UnaryMinus);
                        }

                        continue;
                    }

                    if (next == ')')
                    {
                        reader.Read();
                        EvaluateWhile(() => operatorStack.Count > 0 && operatorStack.Peek() != Parentheses.Left);
                        operatorStack.Pop();
                        continue;
                    }

                    if (next == '[')
                    {
                        reader.Read();

                        operatorStack.Push(Brackets.Left); if (reader.Peek() == '-')
                        {
                            reader.Read();
                            operatorStack.Push(Operation.UnaryMinus);
                        }
                        continue;
                    }

                    if (next == ']')
                    {
                        reader.Read();
                        EvaluateWhile(() => operatorStack.Count > 0 && operatorStack.Peek() != Brackets.Left);

                        ReadRamPrivate(arrayParameter);

                        operatorStack.Pop();
                        continue;
                    }

                    if (next == ' ')
                    {
                        reader.Read();
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("Encountered invalid character {0}", next),
                            nameof(expression));
                    }
                }
            }

            EvaluateWhile(() => operatorStack.Count > 0);

            var lambda = Expression.Lambda<Func<long[], long>>(expressionStack.Pop(), arrayParameter);
            var compiled = lambda.Compile();
            return compiled;
        }

        private void ReadRamPrivate(ParameterExpression arrayParameter)
        {
            var ramExpression = expressionStack.Pop();


            var lambda = Expression.Lambda<Func<long[], long>>(ramExpression, arrayParameter);
            var compiled = lambda.Compile();

            var value = compiled(Array.Empty<long>());
            long converted = ReadRam(value);
            var expression = Expression.Constant(converted);

            expressionStack.Push(expression);
        }

        private static Dictionary<string, long> ParseArguments(object argument)
        {
            if (argument == null)
            {
                return new Dictionary<string, long>();
            }

            var argumentType = argument.GetType();

            var properties = argumentType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && IsNumeric(p.PropertyType));

            var arguments = properties.ToDictionary(property => property.Name,
                property => Convert.ToInt64(property.GetValue(argument, null)));

            return arguments;
        }

        private static long Execute(Func<long[], long> compiled, Dictionary<string, long> arguments, List<string> parameters)
        {
            arguments ??= new Dictionary<string, long>();

            if (parameters.Count != arguments.Count)
            {
                throw new ArgumentException(string.Format("Expression contains {0} parameters but got only {1}",
                    parameters.Count, arguments.Count));
            }

            var missingParameters = parameters.Where(p => !arguments.ContainsKey(p)).ToList();

            if (missingParameters.Any())
            {
                throw new ArgumentException("No values provided for parameters: " + string.Join(",", missingParameters));
            }

            var values = parameters.Select(parameter => arguments[parameter]).ToArray();

            return compiled(values);
        }


        private void EvaluateWhile(Func<bool> condition)
        {
            while (condition())
            {
                var operation = (Operation)operatorStack.Pop();

                var expressions = new Expression[operation.NumberOfOperands];
                for (var i = operation.NumberOfOperands - 1; i >= 0; i--)
                {
                    expressions[i] = expressionStack.Pop();
                }

                expressionStack.Push(operation.Apply(expressions));
            }
        }

        private Expression ReadOperand(TextReader reader)
        {
            var decimalSeparator = Culture.NumberFormat.NumberDecimalSeparator[0];
            var groupSeparator = Culture.NumberFormat.NumberGroupSeparator[0];

            var operand = string.Empty;

            int peek;

            while ((peek = reader.Peek()) > -1)
            {
                var next = (char)peek;

                if (char.IsLetterOrDigit(next) || next == decimalSeparator || next == groupSeparator)
                {
                    reader.Read();
                    operand += next;
                }
                else
                {
                    break;
                }
            }
            long.TryParse(operand, NumberStyles.HexNumber, Culture, out long v);

            return Expression.Constant(v);
        }

        private static Operation ReadOperation(TextReader reader)
        {
            var operation = (char)reader.Read();
            return (Operation)operation.ToString();
        }

        private Expression ReadParameter(TextReader reader, Expression arrayParameter)
        {
            var parameter = string.Empty;

            int peek;

            while ((peek = reader.Peek()) > -1)
            {
                var next = (char)peek;

                if (char.IsLetter(next))
                {
                    reader.Read();
                    parameter += next;
                }
                else
                {
                    break;
                }
            }

            if (!parameters.Contains(parameter))
            {
                parameters.Add(parameter);
            }

            return Expression.ArrayIndex(arrayParameter, Expression.Constant(parameters.IndexOf(parameter)));
        }


        private static bool IsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    break;
            }
            return false;
        }
    }

    internal sealed class Operation : Symbol
    {
        private readonly Func<Expression, Expression, Expression> operation;
        private readonly Func<Expression, Expression> unaryOperation;

        public static readonly Operation Addition = new(8, Expression.Add, "Addition");
        public static readonly Operation Subtraction = new(8, Expression.Subtract, "Subtraction");
        public static readonly Operation Multiplication = new(9, Expression.Multiply, "Multiplication");
        public static readonly Operation Division = new(9, Expression.Divide, "Division");
        public static readonly Operation Modulus = new(9, Expression.Modulo, "Modulo");
        public static readonly Operation UnaryMinus = new(10, Expression.Negate, "Negation");
        public static readonly Operation And = new(4, Expression.And, "And");
        public static readonly Operation XOr = new(3, Expression.ExclusiveOr, "XOr");
        public static readonly Operation Or = new(2, Expression.Or, "Or");

        //public static readonly Operation Test = new Operation(0, , "test");

        private static readonly Dictionary<string, Operation> Operations = new()
        {
            { "+", Addition },
            { "-", Subtraction },
            { "*", Multiplication},
            { "/", Division },
            { "&", And },
            { "^", XOr },
            { "|", Or },
            { "%", Modulus }
        };

        private Operation(int precedence, string name)
        {
            Name = name;
            Precedence = precedence;

            //UnaryExpression body = Expression.MakeUnary(ExpressionType.Lambda, 
            //    Expression.Lambda<Func<long>>(x=> x +=1))

            //ParameterExpression pI = Expression.Parameter(typeof(long));

        }

        private Operation(int precedence, Func<Expression, Expression> unaryOperation, string name) : this(precedence, name)
        {
            this.unaryOperation = unaryOperation;
            NumberOfOperands = 1;
        }

        private Operation(int precedence, Func<Expression, Expression, Expression> operation, string name) : this(precedence, name)
        {
            this.operation = operation;
            NumberOfOperands = 2;
        }

        public string Name { get; private set; }

        public int NumberOfOperands { get; private set; }

        public int Precedence { get; private set; }

        public static explicit operator Operation(string operation)
        {
            if (Operations.TryGetValue(operation, out Operation result))
            {
                return result;
            }
            else
            {
                throw new InvalidCastException();
            }
        }

        private Expression Apply(Expression expression)
        {
            return unaryOperation(expression);
        }

        private Expression Apply(Expression left, Expression right)
        {
            return operation(left, right);
        }

        public static bool IsDefined(char operation)
        {
            return Operations.ContainsKey(operation.ToString());
        }

        public Expression Apply(params Expression[] expressions)
        {
            if (expressions.Length == 1)
            {
                return Apply(expressions[0]);
            }

            if (expressions.Length == 2)
            {
                return Apply(expressions[0], expressions[1]);
            }

            throw new NotImplementedException();
        }
    }

    internal class Parentheses : Symbol
    {
        public static readonly Parentheses Left = new();
        public static readonly Parentheses Right = new();

        private Parentheses()
        {

        }
    }
    internal class Brackets : Symbol
    {
        public static readonly Brackets Left = new();
        public static readonly Brackets Right = new();
    }

    internal class Symbol
    {
    }
}
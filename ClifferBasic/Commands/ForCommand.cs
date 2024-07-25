using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

using Cliffer;

using ClifferBasic.Model;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("for", "Repeat a section of code for a number of times")]
[Argument(typeof(IEnumerable<string>), "args", "The calculation and conditional expression", Arity = Cliffer.ArgumentArity.OneOrMore)]
internal class ForCommand {
    public int Execute(
        IEnumerable<string> args,
        ProgramService programService,
        InvocationContext context,
        ExpressionBuilder expressionBuilder,
        VariableStore variableStore) {

        var expression = expressionBuilder.BuildExpression(args);

        if (expression is not null) {
            string identifier = string.Empty;

            if (expression is BinaryExpression binaryExpression && binaryExpression.Operator.Lexeme == "=") {
                if (binaryExpression.Left is VariableExpression variable) {
                    identifier = variable.Name;
                }

                bool isActive = programService.IsForLoopActive(identifier);

                if (!isActive) {
                    if (binaryExpression.Left is DoubleVariableExpression doubleVariable) {
                        var variableValue = binaryExpression.Right.Evaluate(variableStore);
                        variableStore.SetVariable(doubleVariable.Name, new DoubleVariable(variableValue));
                    }
                    else if (binaryExpression.Left is IntegerVariableExpression integerVariable) {
                        var variableValue = binaryExpression.Right.Evaluate(variableStore);
                        variableStore.SetVariable(integerVariable.Name, new IntegerVariable(variableValue));
                    }
                    else {
                        Console.Error.WriteLine($"Error: Left-hand side of assignment must be a numeric variable");
                        return Result.Error;
                    }

                    programService.EnterForLoop(identifier);
                    return Result.Success;
                }
            }
            else {
                Console.Error.WriteLine($"Error: Invalid assignment expression");
                return Result.Error;
            }

            expression = expressionBuilder.BuildExpression();

            if (expression is ToExpression) {
                expression = expressionBuilder.BuildExpression();

                if (expression is not null) {
                    double incrementAmount = 1;
                    double incrementValue = 0;

                    var loopVarValue = variableStore.GetVariable(identifier);

                    if (loopVarValue is DoubleVariable doubleVariable) {
                        incrementValue = doubleVariable.ToDouble();
                    }
                    else if (loopVarValue is IntegerVariable integerVariable) {
                        incrementValue = integerVariable.ToInt();
                    }
                    else {
                        Console.Error.WriteLine($"Error: Invalid variable type");
                        return Result.Error;
                    }

                    var comparisonResult = expression.Evaluate(variableStore);
                    double comparisonValue = (double)comparisonResult;

                    expression = expressionBuilder.BuildExpression();

                    if (expression is StepExpression) {
                        expression = expressionBuilder.BuildExpression();

                        if (expression is not null) {
                            var step = expression.Evaluate(variableStore);
                            
                            if (step is not null) {
                                incrementAmount = (double)step;
                            }
                        }
                        else {
                            Console.Error.WriteLine($"Error: Missing step value");
                            return Result.Error;
                        }
                    }

                    incrementValue += incrementAmount;
                    variableStore.SetVariable(identifier, new DoubleVariable(incrementValue));

                    if (incrementAmount > 0) {
                        if (incrementValue.CompareTo(comparisonValue) >= 0) {
                            programService.ExitForLoop(identifier);
                        }
                    }
                    else if (incrementAmount < 0) {
                        if (incrementValue.CompareTo(comparisonValue) <= 0) {
                            programService.ExitForLoop(identifier);
                        }
                    }
                }
            }
        }

        return Result.Success;
    }
}


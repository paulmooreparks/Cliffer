using Cliffer;

using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("if", "Take an action conditionally based on a Boolean evaluation")]
[Argument(typeof(IEnumerable<string>), "args", "The condition and action to take", Arity = ArgumentArity.OneOrMore)]
internal class IfCommand {
    public void Execute(
        IEnumerable<string> args,
        ExpressionBuilder expressionBuilder,
        VariableStore variableStore
        ) 
    {
        var expression = expressionBuilder.BuildExpression(args);

        while (expression is not null) {
            var result = expression.Evaluate(variableStore);
            Console.WriteLine(result);
            expression = expressionBuilder.BuildExpression();
        }
    }
}

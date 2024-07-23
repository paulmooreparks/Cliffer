using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic.Commands;

[Command("print", "Print text to the screen")]
[Argument(typeof(IEnumerable<string>), "args", "The text to print or expression to evaluate", Cliffer.ArgumentArity.ZeroOrMore)]
internal class PrintCommand {
    public int Execute(
        IEnumerable<string> args,
        Tokenizer tokenizer,
        ExpressionParser expressionParser,
        VariableStore variableStore
        ) {
        if (!args.Any()) {
            Console.WriteLine();
            return Result.Success;
        }

        var tokens = tokenizer.Tokenize(args);
        var expression = expressionParser.Parse(tokens);
        var result = expression.Evaluate(variableStore);
        Console.WriteLine(result);

        return Result.Success;
    }
}

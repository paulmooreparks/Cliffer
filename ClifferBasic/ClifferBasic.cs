using System.CommandLine.Invocation;
using System.CommandLine;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Text.RegularExpressions;

using Cliffer;

using ClifferBasic.Model;
using ClifferBasic.Services;

namespace ClifferBasic;

internal class ClifferBasic {
    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureServices(services => {
                services.AddSingleton<VariableStore>();
                services.AddSingleton<Tokenizer>();
                services.AddSingleton<ExpressionParser>();
                services.AddSingleton<PersistenceService>();
                services.AddSingleton<CommandSplitter>();
                services.AddSingleton<Dictionary<int, string>>(provider => { 
                    var persistenceService = provider.GetService<PersistenceService>()!;
                    return persistenceService.Load();
                });
            })
            .Build();

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferExitHandler.OnExit += () => {
        };

        return await cli.RunAsync(args);
    }
}

internal static class Utility {
    private static IServiceProvider? _serviceProvider;

    internal static void SetServiceProvider(IServiceProvider provider) {
        _serviceProvider = provider;
    }

    internal static IServiceProvider GetServiceProvider() {
        if (_serviceProvider is null) {
            throw new InvalidOperationException("Service provider is not set.");
        }

        return _serviceProvider;
    }

    internal static T? GetService<T>() {
        if (_serviceProvider is null) {
            throw new InvalidOperationException("Service provider is not set.");
        }

        return _serviceProvider.GetService<T>();
    }
}

internal class BasicReplContext : Cliffer.DefaultReplContext {
    private readonly CommandSplitter _splitter = Utility.GetService<CommandSplitter>()!;
    public override string GetTitleMessage() => "Cliffer Basic";

    public override string GetPrompt(Command command, InvocationContext context) => "> ";

    public override string[] GetPopCommands() => [];

    public override void OnEntry() {
        base.OnEntry();
    }

    public override string[] SplitCommandLine(string input) {
        return _splitter.Split(input).ToArray();
    }

    public override string[] PreprocessArgs(string[] args, Command command, InvocationContext context) {
        return base.PreprocessArgs(args, command, context);
    }
}

[RootCommand("Cliffer Basic")]
internal class RootCommand {
    public async Task<int> Execute(int lineNumber, Command command, IServiceProvider serviceProvider, InvocationContext context) {
        return await command.Repl(serviceProvider, context, new BasicReplContext());
    }
}

[Command("load", "Load stack from persistent storage")]
internal class LoadCommand {
    public int Execute(Dictionary<int, string> program, PersistenceService persistenceService) {
        var newProgram = persistenceService.Load();
        return Result.Success;
    }
}

[Command("save", "Save stack to persistent storage")]
internal class SaveCommand {
    public int Execute(Dictionary<int, string> program, PersistenceService persistenceService) {
        persistenceService.Save(program);
        return Result.Success;
    }
}

[Command("print", "Print text to the screen")]
[Argument(typeof(IEnumerable<string>), "args", "The text to print or expression to evaluate", Cliffer.ArgumentArity.ZeroOrMore)]
internal class PrintCommand {
    public int Execute(
        IEnumerable<string> args,
        Tokenizer tokenizer,
        ExpressionParser expressionParser,
        VariableStore variableStore
        ) 
    {
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

[Command("let", "Assign a value to a variable")]
[Argument(typeof(IEnumerable<string>), "args", "The assignment expression", Cliffer.ArgumentArity.ZeroOrMore)]
internal class LetCommand {
    public int Execute(
        IEnumerable<string> args, 
        Tokenizer tokenizer, 
        ExpressionParser expressionParser, 
        VariableStore variableStore
        ) 
    {
        if (!args.Any()) {
            Console.Error.WriteLine("Error: No parameters");
            return Result.Error;
        }

        var tokens = tokenizer.Tokenize(args);
        var expression = expressionParser.Parse(tokens);

        if (expression is BinaryExpression binaryExpression && binaryExpression.Operator.Lexeme == "=") {
            if (binaryExpression.Left is VariableExpression variableNameExpression) {
                var variableValue = binaryExpression.Right.Evaluate(variableStore);
                variableStore.SetVariable(variableNameExpression.Name, new Variable(variableValue, typeof(double)));
                return Result.Success;
            }

            Console.Error.WriteLine($"Error: Left-hand side of assignment must be a variable");
            return Result.Error;
        }

        Console.Error.WriteLine($"Error: Invalid assignment expression");
        return Result.Error;
    }
}

[Command("rem", "Starts a comment line (remark)", aliases: ["'"])]
[Argument(typeof(IEnumerable<string>), "args", "The comment line", Cliffer.ArgumentArity.ZeroOrMore)]
internal class RemCommand {
    public int Execute(IEnumerable<string> args) {
        return Result.Success;
    }
}

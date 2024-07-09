using System.CommandLine.Invocation;
using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Cliffer;

namespace CliCalc;

internal class CliCalcProgram {
    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureServices(services => {
                services.AddSingleton<StackPersistenceService>();
                services.AddSingleton<Stack<double>>(provider => {
                    var persistenceService = provider.GetService<StackPersistenceService>()!;
                    return persistenceService.LoadStack();
                });
            })
            .Build();

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferExitHandler.OnExit += () => {
            var stack = Utility.GetService<Stack<double>>()!;
            var persistenceService = Utility.GetService<StackPersistenceService>()!;
            persistenceService.SaveStack(stack);
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

internal static class Macros {
    [Macro("cube", "Cube the top number on the stack")]
    private static string cube => "pow 3";

    [Macro("neg", "Negate the top item on the stack")]
    private static string negate => "* -1";

    [Macro("peek", "See the top item on the stack")]
    private static string peek => "dup;pop";

    [Macro("rec", "Calculate the reciprocal of the top item on the stack")]
    private static string rec => "push 1;swap;/";

    [Macro("square", "Square the top number on the stack")]
    private static string square => "pow 2";
}

internal class CalcReplContext : Cliffer.DefaultReplContext {
    public override string GetTitleMessage() => "CLI Stack Calculator";

    public override void OnEntry() {
        base.OnEntry();
        var stack = Utility.GetService<Stack<double>>()!;

        if (stack.Any()) {
            Console.WriteLine("Starting stack:");
            foreach (var item in stack) {
                Console.WriteLine(item);
            }
        }
        else {
            Console.WriteLine("Stack is empty");
        }

        Console.WriteLine();
    }
}

[RootCommand("CLI Stack Calculator")]
internal class RootCommand {
    public async Task<int> Execute(Command command, IServiceProvider serviceProvider, InvocationContext context) {
        return await command.Repl(serviceProvider, context, new CalcReplContext());
    }
}

[Command("load", "Load stack from persistent storage")]
internal class LoadCommand {
    public int Execute(Stack<double> stack, StackPersistenceService persistenceService) {
        var newStack = persistenceService.LoadStack();
        stack.Clear();
        stack.PushAll(newStack);
        return Result.Success;
    }
}

[Command("save", "Save stack to persistent storage")]
internal class SaveCommand {
    public int Execute(Stack<double> stack, StackPersistenceService persistenceService) {
        persistenceService.SaveStack(stack);
        return Result.Success;
    }
}

[Command("push", "Push one or more numbers onto the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers to push onto the stack", Cliffer.ArgumentArity.OneOrMore)]
internal class PushCommand {
    public int Execute(IEnumerable<double> values, Stack<double> stack) {
        stack.PushAll(values);
        return Result.Success;
    }
}

[Command("pop", "Pop a number from the stack")]
internal class PopCommand {
    public int Execute(Stack<double> stack) {
        if (stack.TryPop(out var value)) {
            Console.WriteLine(value);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("swap", "Swap the top two numbers on the stack")]
internal class SwapCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Count > 1) {
            var a = stack.Pop();
            var b = stack.Pop();
            stack.Push(a);
            stack.Push(b);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Not enough items on stack");
        return Result.Error;
    }
}

[Command("dup", "Duplicate the top number on the stack")]
internal class DupCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Peek();
            stack.Push(value);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack is empty");
        return Result.Error;
    }
}

[Command("clear", "Clear the stack")]
internal class ClearCommand {
    public int Execute(Stack<double> stack) {
        stack.Clear();
        return Result.Success;
    }
}

[Command("list", "List all items on the stack")]
internal class ListCommand {
    public int Execute(Stack<double> stack) {
        foreach (var item in stack) {
            Console.WriteLine(item);
        }

        return Result.Success;
    }
}

internal abstract class StackOperation {
    protected abstract double Operation(double a, double b);

    public int Execute(IEnumerable<double> values, Stack<double> stack) {
        if (values.Count() > 2) {
            Console.Error.WriteLine("Error: Too many arguments");
            return Result.Error;
        }

        stack.PushAll(values);

        if (stack.Count > 1) {
            var term2 = stack.Pop();
            var term1 = stack.Pop();
            var result = Operation(term1, term2);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Not enough items on stack");
        return Result.Error;
    }
}

[Command("+", "Add the top two numbers on the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers to add", Cliffer.ArgumentArity.ZeroOrMore)]
internal class AddCommand : StackOperation {
    protected override double Operation(double a, double b) => a + b;
}

[Command("-", "Subtract the top number on the stack from the second number on the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers to subtract", Cliffer.ArgumentArity.ZeroOrMore)]
internal class SubtractCommand : StackOperation {
    protected override double Operation(double a, double b) => a - b;
}

[Command("*", "Multiply the top two numbers on the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers to multiply", Cliffer.ArgumentArity.ZeroOrMore)]
internal class MultiplyCommand : StackOperation {
    protected override double Operation(double a, double b) => a * b;
}

[Command("/", "Divide the second number on the stack by the top number on the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers to divide", Cliffer.ArgumentArity.ZeroOrMore)]
internal class DivideCommand : StackOperation {
    protected override double Operation(double a, double b) => a / b;
}

[Command("mod", "Calculate the result of the second number on the stack modulo the top number on the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers for which to calculate modulus", Cliffer.ArgumentArity.ZeroOrMore)]
internal class ModulusCommand : StackOperation {
    protected override double Operation(double a, double b) => a % b;
}

[Command("pow", "Raise the second number on the stack to the power of the top number on the stack")]
[Argument(typeof(IEnumerable<double>), "values", "Numbers for which to calculate power", Cliffer.ArgumentArity.ZeroOrMore)]
internal class PowerCommand : StackOperation {
    protected override double Operation(double a, double b) => Math.Pow(a, b);
}

[Command("sqrt", "Calculate the square root of the top number on the stack")]
internal class SquareRootCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Pop();
            var result = Math.Sqrt(value);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("log", "Calculate the natural logarithm of the top number on the stack")]
internal class LogarithmCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Pop();
            var result = Math.Log(value);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("abs", "Replace the top number on the stack with its absolute value")]
internal class AbsoluteValueCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Pop();
            var result = Math.Abs(value);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("sin", "Calculate the sine of the top number on the stack")]
internal class SineCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Pop();
            var result = Math.Sin(value);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("cos", "Calculate the cosine of the top number on the stack")]
internal class CosineCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Pop();
            var result = Math.Cos(value);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("tan", "Calculate the tangent of the top number on the stack")]
internal class TangentCommand {
    public int Execute(Stack<double> stack) {
        if (stack.Any()) {
            var value = stack.Pop();
            var result = Math.Tan(value);
            stack.Push(result);
            Console.WriteLine(result);
            return Result.Success;
        }

        Console.Error.WriteLine("Error: Stack empty");
        return Result.Error;
    }
}

[Command("pi", "Push the value of pi onto the stack")]
internal class PiCommand {
    public int Execute(Stack<double> stack) {
        stack.Push(Math.PI);
        Console.WriteLine(Math.PI);
        return Result.Success;
    }
}

internal static class StackExtensions {
    public static void PushAll<T>(this Stack<T> stack, IEnumerable<T> values) {
        foreach (var value in values) {
            stack.Push(value);
        }
    }
}

public class StackPersistenceService {
    private readonly string _filePath;
    private readonly Mutex _mutex;
    private readonly string _mutexName = "Global\\CLICalcMutex"; // Global mutex name for cross-process synchronization

    public StackPersistenceService() {
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var clicalcDirectory = Path.Combine(homeDirectory, ".clicalc");

        if (!Directory.Exists(clicalcDirectory)) {
            Directory.CreateDirectory(clicalcDirectory);
        }

        _filePath = Path.Combine(clicalcDirectory, "stack.json");
        _mutex = new Mutex(false, _mutexName);
    }

    public Stack<double> LoadStack() {
        try {
            _mutex.WaitOne(); 

            if (File.Exists(_filePath)) {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<Stack<double>>(json) ?? new Stack<double>();
            }

            return new Stack<double>();
        }
        finally {
            _mutex.ReleaseMutex();
        }
    }

    public void SaveStack(Stack<double> stack) {
        try {
            _mutex.WaitOne();

            var json = JsonSerializer.Serialize(stack);
            File.WriteAllText(_filePath, json);
        }
        finally {
            _mutex.ReleaseMutex();
        }
    }
}

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Cliffer;

namespace ClifferDemo;

internal class ClifferDemo {
    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureAppConfiguration(configBuilder => {
                // Get the directory of the currently executing assembly
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);

                if (assemblyDirectory != null) {
                    var appSettingsPath = Path.Combine(assemblyDirectory, "appSettings.json");
                    configBuilder.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
                }
            })
            .ConfigureServices(services => {
                services.AddSingleton<IReplContext, ReplContext>();
            })
            .Build();

        return await cli.RunAsync(args);
    }
}

internal class ReplContext(Command currentCommand) : DefaultReplContext(currentCommand) {
    public override string EntryMessage => "Entering interactive mode...";

    public override string LoopMessage => "Do it again!";
}

[RootCommand("Cliffer demo")]
internal class ReplCommand {
    public ReplCommand(IServiceProvider serviceProvider) {
    }

    public async Task<int> Execute(Command command, InvocationContext context, IServiceProvider serviceProvider) {
        return await command.Repl(serviceProvider, context);
    }
}

[Command("hello", "Say hello")]
[Argument(typeof(string), "target", "State whom you are greeting.")]
internal class HelloCommand {
    [Macro("sunshine", "Greet the morning sun.")]
    private static string sunshine => "hello Sunshine";

    [Macro("greet", "Greet someone.")]
    private static string greet => "hello {{[arg]::0}}";

    public int Execute(string target, IServiceProvider serviceProvider) {
        if (string.IsNullOrEmpty(target)) {
            target = "World";
        }

        Console.WriteLine($"Hello, {target}!");
        return 0;
    }
}

[Command("foo", "Repl sub-command")]
internal class FooCommand {

    public FooCommand(IServiceProvider serviceProvider) {
    }

    public void Configure(Command command) {
    }

    public async Task<int> Execute(Command command, InvocationContext context, IServiceProvider serviceProvider) {
        return await command.Repl(serviceProvider, context);
    }
}

[Command("complex", "Complex command")]
[Option(typeof(string), "--foo", "Foo option")]
[Option(typeof(string), "--bar", "Bar option")]
internal class ComplexCommand {
    public ComplexCommand(IServiceProvider serviceProvider) {
    }

    public int Execute(
        [OptionParam("--foo")] string foo,
        [OptionParam("--bar")] string bar,
        IServiceProvider serviceProvider
        )
    {
        var replContext = serviceProvider.GetService<IReplContext>();
        System.Console.WriteLine($"Complex command: --foo = {foo}, --bar = {bar}");
        return 0;
    }
}

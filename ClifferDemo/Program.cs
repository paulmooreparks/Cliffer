using System;
using System.CommandLine;
using System.CommandLine.Invocation;

using Cliffer;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;

namespace ClifferDemo;

internal class Program {
    static Task<int> Main(string[] args) {
        var cli = ClifferCli.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configurationBuilder) => {
                configurationBuilder.AddJsonFile("appsettings.json");
            })
            .ConfigureServices(services => {
                services.AddSingleton<IReplContext, ReplContext>();
            })
            .BuildCommands((configuration, rootCommand) => {
                var macroSection = configuration.GetSection("Settings").GetSection("Macros");
                var macros = macroSection.Get<Macro[]>();

                if (macros is not null) {
                    foreach (var macro in macros) {
                        rootCommand.AddCommand(macro);
                    }
                }
            })
            .ConfigureCommands((configuration, rootCommand) => {
            })
            .Build();

        return cli.RunAsync(args); 
    }
}

internal class ReplContext : IReplContext {
    public string GetEntryMessage() => "Entering interactive mode...";

    public string GetLoopMessage() => "Do it again!";

    public string[] GetExitCommands() {
        return new string[] { "exit" };
    }

    public string[] GetPopCommands() {
        return new string[] { "pop" };
    }

    public string[] GetHelpCommands() {
        return new string[] { "help", "?" };
    }

    public string GetPrompt(Command command, InvocationContext context) {
        return $"{command.Name}> ";
    }

    public string[] PreprocessArgs(string[] args, Command command, InvocationContext context) {
        return args;
    }
}

[RootCommand("Cliffer demo")]
internal class ReplCommand {
    public ReplCommand(IServiceProvider serviceProvider) {
    }

    public async Task<int> Execute(Command command, InvocationContext context, IServiceProvider serviceProvider) {
        // return await command.Repl(serviceProvider, context, serviceProvider.GetService<IReplContext>());
        return await command.Repl(serviceProvider, context);
    }
}

[Command("hello", "Say hello")]
internal class HelloCommand {

    public HelloCommand(IServiceProvider serviceProvider) {
    }

    public void Configure(RootCommand command) {
        command.Description = "Say hello";
    }

    public int Execute(IServiceProvider serviceProvider) {
        System.Console.WriteLine("Hello, World!");
        return 0;
    }
}

[Command("foo", "Say foo")]
internal class FooCommand {

    public FooCommand(IServiceProvider serviceProvider) {
    }

    public void Configure(Command command) {
    }

    public async Task<int> Execute(Command command, InvocationContext context, IServiceProvider serviceProvider) {
        // return await command.Repl(serviceProvider, context, serviceProvider.GetService<IReplContext>());
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
        [OptionParam("--bar")] string bar
        )
    {
        System.Console.WriteLine($"Complex command: --foo = {foo}, --bar = {bar}");
        return 0;
    }
}

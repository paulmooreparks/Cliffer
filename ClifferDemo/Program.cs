using System;
using System.CommandLine;

using Cliffer;

using Microsoft.Extensions.DependencyInjection;

namespace ClifferDemo;

internal class Program {
    static Task<int> Main(string[] args) {
        var cli = ClifferCli.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configurationBuilder) => {
                // ConfigurationBuilder.AddJsonFile("appsettings.json");
            })
            .ConfigureServices(services => {
                // services.AddSingleton<MyService>();
            })
            .BuildCommands((configuration, rootCommand) => {
                var macro = new Macro("simple", "Macro command", "complex --foo bar --bar foo");
                rootCommand.AddCommand(macro);
            })
            .ConfigureCommands((configuration, rootCommand) => {
                // configuration.GetSection("Commands").Bind(context.Commands);
            })
            .Build();

        return cli.RunAsync(args); 
    }
}

[RootCommand("Say hello")]
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
        Console.WriteLine("FooCommand.FooCommand");
    }

    public void Configure(Command command) {
        Console.WriteLine("FooCommand.Configure");
    }

    public int Execute(IServiceProvider serviceProvider) {
        System.Console.WriteLine("Foo");
        return 0;
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



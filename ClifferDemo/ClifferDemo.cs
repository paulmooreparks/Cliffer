﻿using System.CommandLine;
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

internal class ReplContext : DefaultReplContext {
    public override string GetEntryMessage() => "Entering interactive mode...";

    public override string GetLoopMessage() => "Do it again!";
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

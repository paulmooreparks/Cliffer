using System.CommandLine;

using Cliffer;

using Microsoft.Extensions.DependencyInjection;

namespace ClifferDemo;

internal class Program {
    static void Main(string[] args) {
        ClifferCli.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, configurationBuilder) => {
                // ConfigurationBuilder.AddJsonFile("appsettings.json");
                Console.WriteLine("ConfigureAppConfiguration");
            })
            .ConfigureServices(services => {
                // services.AddSingleton<HelloCommand>();
                Console.WriteLine("ConfigureServices");
            })
            .BuildCommands(configuration => {
                // configuration.GetSection("Commands").Bind(context.Commands);
                Console.WriteLine("BuildCommands");
            })
            .ConfigureCommands(configuration => {
                // configuration.GetSection("Commands").Bind(context.Commands);
                Console.WriteLine("ConfigureCommands");
            })
            .Build()
            .RunAsync(args)
            .Wait();
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


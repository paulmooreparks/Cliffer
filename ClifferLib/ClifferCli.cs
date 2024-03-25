using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public class ClifferCli : IClifferCli {
    public IServiceProvider ServiceProvider { get; set; }

    public ClifferCli(IServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;
    }

    public static IClifferBuilder CreateDefaultBuilder(string[] args) {
        return new ClifferBuilder();
    }

    public async Task<int> RunAsync(string[] args) {
        var rootCommand = ServiceProvider.GetService<RootCommand>();

        if (rootCommand is null) {
            throw new InvalidOperationException("Root command not found.");
        }

        var macros = rootCommand.Children.OfType<Macro>().ToDictionary(macro => macro.Name, macro => macro);

        if (macros is not null) {
            args = Macro.PreprocessArgs(args, macros);
        }

        return await rootCommand.InvokeAsync(args);
    }
}
using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

public class ClifferCli : IClifferCli {
    private readonly IServiceProvider serviceProvider;

    public ClifferCli(IServiceProvider serviceProvider) {
        this.serviceProvider = serviceProvider;
    }

    public static IClifferBuilder CreateDefaultBuilder(string[] args) {
        return new ClifferBuilder();
    }

    public async Task<int> RunAsync(string[] args) {
        var rootCommand = serviceProvider.GetService<RootCommand>();

        if (rootCommand is null) {
            throw new InvalidOperationException("Root command not found.");
        }

        return await rootCommand.InvokeAsync(args);
    }
}
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
                services.AddSingleton<ExpressionBuilder>();
                services.AddSingleton<PersistenceService>();
                services.AddSingleton<CommandSplitter>();
                services.AddSingleton<Dictionary<int, string[]>>(provider => { 
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


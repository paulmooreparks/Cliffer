using Microsoft.Extensions.DependencyInjection;
using Cliffer;
using ClifferBasic.Services;

namespace ClifferBasic;

internal class ClifferBasic {
    static async Task<int> Main(string[] args) {
        var cli = new ClifferBuilder()
            .ConfigureServices(services => {
                services.AddTransient<Tokenizer>();
                services.AddTransient<ExpressionParser>();
                services.AddTransient<ExpressionBuilder>();
                services.AddSingleton<CommandSplitter>();
                services.AddSingleton<VariableStore>();
                services.AddSingleton<ProgramService>();
            })
            .Build();

        Utility.SetServiceProvider(cli.ServiceProvider);

        ClifferExitHandler.OnExit += () => {
        };

        return await cli.RunAsync(args);
    }
}


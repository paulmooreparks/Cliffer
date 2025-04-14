using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cliffer;

internal class ClifferCli : IClifferCli {
    public IServiceProvider ServiceProvider { get; set; }
    public IServiceCollection Services;
    private RootCommand _rootCommand;
    public Parser Parser { get; }

    public IDictionary<string, Object> Commands { get; set; }

    public ClifferCli(
        IServiceProvider serviceProvider, 
        IServiceCollection serviceCollection,
        RootCommand rootCommand,
        Parser parser,
        IDictionary<string, Object> commands
        )
    {
        ServiceProvider = serviceProvider;
        Services = serviceCollection;
        Parser = parser;
        _rootCommand = rootCommand;
        Commands = commands;

        if (_rootCommand is null) {
            throw new InvalidOperationException("Root command not found.");
        }
    }

    public async Task<int> RunAsync(string[] args) {
        int result = Result.Error;

        try {
            // result = await _rootCommand.RunAsync(args);
            result = await Parser.InvokeAsync(args);
            return result;
        }
        finally {
            ClifferEventHandler.Exit(result);
        }
    }
}

public static class StringExtensions {
    public static string ReplaceVariablePlaceholders(this string template, IConfiguration config, string[] args, Dictionary<string, string>? extraVars = null) {
        // Regex pattern matches placeholders like "{{[env]::VariableName}}", "{{[cfg]::VariableName}}", or "{{VariableName::DefaultValue}}"
        string placeholderPattern = @"\{\{(\[env\]::|\[cfg\]::|\[arg\]::)?([^:{}]+(?::[^:{}]+)*)(?:::([^}]+))?\}\}";

        // Find all matches in the template
        var matches = Regex.Matches(template, placeholderPattern);

        foreach (Match match in matches) {
            string namespacePrefix = match.Groups[1].Value;
            var variable = match.Groups[2].Value;
            var defaultValue = match.Groups[3].Success ? match.Groups[3].Value : null;
            string? value;

            if (namespacePrefix == "[env]::") {
                value = Environment.GetEnvironmentVariable(variable);
            }
            else if (namespacePrefix == "[cfg]::") {
                value = config[variable];
            }
            else if (namespacePrefix == "[arg]::") {
                if (int.TryParse(variable, out int index)) {
                    value = args[++index];
                }
                else {
                    value = null;
                }
            }
            else {
                // Attempt to get the corresponding value from configuration
                value = config.GetSection("Variables")?[variable];
            }

            // Use extraVars if available and the value is not found in configuration
            if (value is null && extraVars != null && extraVars.TryGetValue(variable, out var extraValue)) {
                value = extraValue;
            }

            // Use defaultValue if value is still null
            value = value ?? defaultValue;

            // If a value (including a default value) is found, replace the placeholder in the template with the actual value
            if (value != null) {
                template = template.Replace(match.Value, value);
            }
        }

        return template;
    }
}
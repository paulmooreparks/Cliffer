namespace Cliffer;

public interface IClifferCli {
    IServiceProvider ServiceProvider { get; }
    IDictionary<string, Object> Commands { get; }
    Task<int> RunAsync(string[] args);
}

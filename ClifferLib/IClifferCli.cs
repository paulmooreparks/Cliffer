namespace Cliffer;

public interface IClifferCli {
    IServiceProvider ServiceProvider { get; }
    Task<int> RunAsync(string[] args);
}

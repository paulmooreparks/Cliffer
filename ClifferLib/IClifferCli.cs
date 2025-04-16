namespace Cliffer;

public interface IClifferCli {
    IServiceProvider ServiceProvider { get; }
    object RootCommandInstance { get; }
    Task<int> RunAsync(string[] args);
}

namespace Cliffer;

public static class Result {
    // Standard success/failure
    public const int Success = 0;
    public const int Error = 1;

    // Usage or command-line issues
    public const int InvalidArguments = 2;
    public const int UnknownCommand = 3;
    public const int MissingWorkspace = 4;
    public const int InvalidCommandSyntax = 5;

    // File and IO
    public const int FileNotFound = 10;
    public const int PermissionDenied = 11;
    public const int ReadError = 12;
    public const int WriteError = 13;
    public const int FileAlreadyExists = 14;

    // Networking
    public const int NetworkError = 20;
    public const int ConnectionTimeout = 21;
    public const int ConnectionRefused = 22;
    public const int HostUnreachable = 23;

    // Script and macro handling
    public const int MacroError = 30;
    public const int ScriptRuntimeError = 31;
    public const int ScriptCompileError = 32;

    // REPL or CLI-specific
    public const int ReplExit = 50;
    public const int ReplPop = 51;
    public const int ReplToRoot = 52;
    public const int ReplToParent = 53;

    // Reserved range for future or custom application-specific exit codes
    public const int CustomBase = 100;
}

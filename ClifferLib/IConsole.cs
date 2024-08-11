using System;
using System.IO;
using System.Text;

public interface IConsole
{
    TextReader In { get; }
    Encoding InputEncoding { get; set; }
    Encoding OutputEncoding { get; set; }
    bool KeyAvailable { get; }
    TextWriter Out { get; }
    TextWriter Error { get; }
    bool IsInputRedirected { get; }
    bool IsOutputRedirected { get; }
    bool IsErrorRedirected { get; }
    int CursorSize { get; set; }
    bool NumberLock { get; }
    bool CapsLock { get; }
    ConsoleColor BackgroundColor { get; set; }
    ConsoleColor ForegroundColor { get; set; }
    int BufferWidth { get; set; }
    int BufferHeight { get; set; }
    int WindowLeft { get; set; }
    int WindowTop { get; set; }
    int WindowWidth { get; set; }
    int WindowHeight { get; set; }
    int LargestWindowWidth { get; }
    int LargestWindowHeight { get; }
    bool CursorVisible { get; set; }
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
    string Title { get; set; }
    bool TreatControlCAsInput { get; set; }
    ConsoleKeyInfo ReadKey();
    ConsoleKeyInfo ReadKey(bool intercept);
    void ResetColor();
    void SetBufferSize(int width, int height);
    void SetWindowPosition(int left, int top);
    void SetWindowSize(int width, int height);
    (int Left, int Top) GetCursorPosition();
    void Beep();
    void Beep(int frequency, int duration);
    void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop);
    void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor);
    void Clear();
    void SetCursorPosition(int left, int top);
    Stream OpenStandardInput();
    Stream OpenStandardInput(int bufferSize);
    Stream OpenStandardOutput();
    Stream OpenStandardOutput(int bufferSize);
    Stream OpenStandardError();
    Stream OpenStandardError(int bufferSize);
    void SetIn(TextReader newIn);
    void SetOut(TextWriter newOut);
    void SetError(TextWriter newError);
    int Read();
    string? ReadLine();
    void WriteLine();
    void WriteLine(bool value);
    void WriteLine(char value);
    void WriteLine(char[]? buffer);
    void WriteLine(char[] buffer, int index, int count);
    void WriteLine(decimal value);
    void WriteLine(double value);
    void WriteLine(float value);
    void WriteLine(int value);
    void WriteLine(uint value);
    void WriteLine(long value);
    void WriteLine(ulong value);
    void WriteLine(object? value);
    void WriteLine(string? value);
    void WriteLine(string format, object? arg0);
    void WriteLine(string format, object? arg0, object? arg1);
    void WriteLine(string format, object? arg0, object? arg1, object? arg2);
    void WriteLine(string format, params object?[]? arg);
    void Write(string format, object? arg0);
    void Write(string format, object? arg0, object? arg1);
    void Write(string format, object? arg0, object? arg1, object? arg2);
    void Write(string format, params object?[]? arg);
    void Write(bool value);
    void Write(char value);
    void Write(char[]? buffer);
    void Write(char[] buffer, int index, int count);
    void Write(double value);
    void Write(decimal value);
    void Write(float value);
    void Write(int value);
    void Write(uint value);
    void Write(long value);
    void Write(ulong value);
    void Write(object? value);
    void Write(string? value);
}

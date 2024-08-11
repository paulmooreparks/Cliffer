using System;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

public class ConsoleIOHandler : IConsole
{
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public TextReader In => Console.In;

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public Encoding InputEncoding
    {
        get => Console.InputEncoding;
        set => Console.InputEncoding = value;
    }

    public Encoding OutputEncoding
    {
        get => Console.OutputEncoding;
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set => Console.OutputEncoding = value;
    }

    public bool KeyAvailable => Console.KeyAvailable;

    public TextWriter Out => Console.Out;

    public TextWriter Error => Console.Error;

    public bool IsInputRedirected => Console.IsInputRedirected;

    public bool IsOutputRedirected => Console.IsOutputRedirected;

    public bool IsErrorRedirected => Console.IsErrorRedirected;

    public int CursorSize {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.CursorSize;
        [SupportedOSPlatform("windows")]
        set => Console.CursorSize = value; 
    }

    [SupportedOSPlatform("windows")]
    public bool NumberLock => Console.NumberLock;

    [SupportedOSPlatform("windows")]
    public bool CapsLock => Console.CapsLock;

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public ConsoleColor BackgroundColor
    {
        get => Console.BackgroundColor;
        set => Console.BackgroundColor = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    public int BufferWidth
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.BufferWidth;
        [SupportedOSPlatform("windows")]
        set => Console.BufferWidth = value;
    }

    public int BufferHeight
    {
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        get => Console.BufferHeight;
        [SupportedOSPlatform("windows")]
        set => Console.BufferHeight = value;
    }

    public int WindowLeft
    {
        get => Console.WindowLeft;
        [SupportedOSPlatform("windows")]
        set => Console.WindowLeft = value;
    }

    public int WindowTop
    {
        get => Console.WindowTop;
        [SupportedOSPlatform("windows")]
        set => Console.WindowTop = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public int WindowWidth
    {
        get => Console.WindowWidth;
        set => Console.WindowWidth = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public int WindowHeight
    {
        get => Console.WindowHeight;
        set => Console.WindowHeight = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public int LargestWindowWidth => Console.LargestWindowWidth;

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public int LargestWindowHeight => Console.LargestWindowHeight;

    public bool CursorVisible
    {
        [SupportedOSPlatform("windows")]
        get => Console.CursorVisible;
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set => Console.CursorVisible = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public int CursorLeft
    {
        get => Console.CursorLeft;
        set => Console.CursorLeft = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public int CursorTop
    {
        get => Console.CursorTop;
        set => Console.CursorTop = value;
    }

    public string Title
    {
        [SupportedOSPlatform("windows")]
        get => Console.Title;
        [UnsupportedOSPlatform("android")]
        [UnsupportedOSPlatform("browser")]
        [UnsupportedOSPlatform("ios")]
        [UnsupportedOSPlatform("tvos")]
        set => Console.Title = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public bool TreatControlCAsInput
    {
        get => Console.TreatControlCAsInput;
        set => Console.TreatControlCAsInput = value;
    }

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public ConsoleKeyInfo ReadKey() => Console.ReadKey();

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public ConsoleKeyInfo ReadKey(bool intercept) => Console.ReadKey(intercept);

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public void ResetColor() => Console.ResetColor();

    [SupportedOSPlatform("windows")]
    public void SetBufferSize(int width, int height) => Console.SetBufferSize(width, height);

    [SupportedOSPlatform("windows")]
    public void SetWindowPosition(int left, int top) => Console.SetWindowPosition(left, top);

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public void SetWindowSize(int width, int height) => Console.SetWindowSize(width, height);

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public (int Left, int Top) GetCursorPosition() => Console.GetCursorPosition();

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public void Beep() => Console.Beep();

    [SupportedOSPlatform("windows")]
    public void Beep(int frequency, int duration) => Console.Beep(frequency, duration);

    [SupportedOSPlatform("windows")]
    public void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop) => Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);

    [SupportedOSPlatform("windows")]
    public void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor) => Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop, sourceChar, sourceForeColor, sourceBackColor);

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public void Clear() => Console.Clear();

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public Stream OpenStandardInput() => Console.OpenStandardInput();

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public Stream OpenStandardInput(int bufferSize) => Console.OpenStandardInput(bufferSize);

    public Stream OpenStandardOutput() => Console.OpenStandardOutput();

    public Stream OpenStandardOutput(int bufferSize) => Console.OpenStandardOutput(bufferSize);

    public Stream OpenStandardError() => Console.OpenStandardError();

    public Stream OpenStandardError(int bufferSize) => Console.OpenStandardError(bufferSize);

    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    [UnsupportedOSPlatform("ios")]
    [UnsupportedOSPlatform("tvos")]
    public void SetIn(TextReader newIn) => Console.SetIn(newIn);

    public void SetOut(TextWriter newOut) => Console.SetOut(newOut);

    public void SetError(TextWriter newError) => Console.SetError(newError);

    [MethodImplAttribute(MethodImplOptions.NoInlining)]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public int Read() => Console.Read();

    [MethodImplAttribute(MethodImplOptions.NoInlining)]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("browser")]
    public string? ReadLine() => Console.ReadLine();

    public void WriteLine() => Console.WriteLine();

    public void WriteLine(bool value) => Console.WriteLine(value);

    public void WriteLine(char value) => Console.WriteLine(value);

    public void WriteLine(char[]? buffer) => Console.WriteLine(buffer);

    public void WriteLine(char[] buffer, int index, int count) => Console.WriteLine(buffer, index, count);

    public void WriteLine(decimal value) => Console.WriteLine(value);

    public void WriteLine(double value) => Console.WriteLine(value);

    public void WriteLine(float value) => Console.WriteLine(value);

    public void WriteLine(int value) => Console.WriteLine(value);

    public void WriteLine(uint value) => Console.WriteLine(value);

    public void WriteLine(long value) => Console.WriteLine(value);

    public void WriteLine(ulong value) => Console.WriteLine(value);

    public void WriteLine(object? value) => Console.WriteLine(value);

    public void WriteLine(string? value) => Console.WriteLine(value);

    [MethodImplAttribute(MethodImplOptions.NoInlining)]
    public void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0) => Console.WriteLine(format, arg0);

    public void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1) => Console.WriteLine(format, arg0, arg1);

    public void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2) => Console.WriteLine(format, arg0, arg1, arg2);

    public void WriteLine([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[]? arg) => Console.WriteLine(format, arg);

    public void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0) => Console.Write(format, arg0);

    public void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1) => Console.Write(format, arg0, arg1);

    public void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2) => Console.Write(format, arg0, arg1, arg2);

    public void Write([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[]? arg) => Console.Write(format, arg);

    public void Write(bool value) => Console.Write(value);

    public void Write(char value) => Console.Write(value);

    public void Write(char[]? buffer) => Console.Write(buffer);

    public void Write(char[] buffer, int index, int count) => Console.Write(buffer, index, count);

    public void Write(double value) => Console.Write(value);

    public void Write(decimal value) => Console.Write(value);

    public void Write(float value) => Console.Write(value);

    public void Write(int value) => Console.Write(value);

    public void Write(uint value) => Console.Write(value);

    public void Write(long value) => Console.Write(value);

    public void Write(ulong value) => Console.Write(value);

    public void Write(object? value) => Console.Write(value);

    public void Write(string? value) => Console.Write(value);
}

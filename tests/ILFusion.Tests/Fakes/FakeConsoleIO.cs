namespace ILFusion.Tests.Fakes;

using ILFusion.UI;

sealed class FakeConsoleIO : IConsoleIO
{
    private readonly Queue<ConsoleKeyInfo> _keys;
    private int _cursorLeft;
    private int _cursorTop;

    public FakeConsoleIO(params ConsoleKeyInfo[] keys)
    {
        _keys = new Queue<ConsoleKeyInfo>(keys);
    }

    public List<string> Output { get; } = [];
    public int WindowWidth { get; init; } = 80;
    public bool CursorVisible { set { } }
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

    public void Write(string text) => Output.Add(text);
    public void WriteLine(string? text = null) => Output.Add(text ?? string.Empty);

    public ConsoleKeyInfo ReadKey(bool intercept = true) =>
        _keys.Count > 0
            ? _keys.Dequeue()
            : new ConsoleKeyInfo('\x1B', ConsoleKey.Escape, false, false, false);

    public string? ReadLine() => string.Empty;
    public (int Left, int Top) GetCursorPosition() => (_cursorLeft, _cursorTop);
    public void SetCursorPosition(int left, int top) { _cursorLeft = left; _cursorTop = top; }
    public void Clear() { }
    public void ResetColor() { ForegroundColor = ConsoleColor.Gray; }
}

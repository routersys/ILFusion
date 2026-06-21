namespace ILFusion.UI;

sealed class ConsoleIO : IConsoleIO
{
    public int WindowWidth
    {
        get
        {
            try { return Console.WindowWidth; }
            catch { return 80; }
        }
    }

    public bool CursorVisible
    {
        set
        {
            try { Console.CursorVisible = value; }
            catch { }
        }
    }

    public ConsoleColor ForegroundColor
    {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    public void Write(string text) => Console.Write(text);
    public void WriteLine(string? text = null) => Console.WriteLine(text);
    public ConsoleKeyInfo ReadKey(bool intercept = true) => Console.ReadKey(intercept);
    public string? ReadLine() => Console.ReadLine();
    public (int Left, int Top) GetCursorPosition() => Console.GetCursorPosition();
    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);
    public void Clear() => Console.Clear();
    public void ResetColor() => Console.ResetColor();
}

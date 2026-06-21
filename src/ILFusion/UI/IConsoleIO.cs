namespace ILFusion.UI;

interface IConsoleIO
{
    int WindowWidth { get; }
    bool CursorVisible { set; }
    ConsoleColor ForegroundColor { get; set; }

    void Write(string text);
    void WriteLine(string? text = null);
    ConsoleKeyInfo ReadKey(bool intercept = true);
    string? ReadLine();
    (int Left, int Top) GetCursorPosition();
    void SetCursorPosition(int left, int top);
    void Clear();
    void ResetColor();
}

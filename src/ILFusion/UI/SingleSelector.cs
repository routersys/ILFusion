namespace ILFusion.UI;

using ILFusion.Models;

sealed class SingleSelector(IConsoleIO console)
{
    public AssemblyEntry? Select(IReadOnlyList<AssemblyEntry> assemblies)
    {
        if (assemblies.Count == 0)
            return null;

        var currentIndex = 0;
        console.CursorVisible = false;

        try
        {
            WriteHint();
            var startTop = console.GetCursorPosition().Top;
            RenderList(assemblies, currentIndex, startTop);

            while (true)
            {
                var key = console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        currentIndex = (currentIndex - 1 + assemblies.Count) % assemblies.Count;
                        break;

                    case ConsoleKey.DownArrow:
                        currentIndex = (currentIndex + 1) % assemblies.Count;
                        break;

                    case ConsoleKey.Enter:
                        console.SetCursorPosition(0, startTop + assemblies.Count);
                        return assemblies[currentIndex];

                    case ConsoleKey.Escape:
                        console.SetCursorPosition(0, startTop + assemblies.Count);
                        return null;
                }

                RenderList(assemblies, currentIndex, startTop);
            }
        }
        finally
        {
            console.CursorVisible = true;
        }
    }

    private void WriteHint()
    {
        console.ForegroundColor = ConsoleColor.DarkGray;
        console.WriteLine("[↑↓] 移動  [Enter] 決定  [Esc] キャンセル");
        console.ResetColor();
        console.WriteLine();
    }

    private void RenderList(
        IReadOnlyList<AssemblyEntry> assemblies,
        int currentIndex,
        int startTop)
    {
        var maxWidth = Math.Max(20, console.WindowWidth - 1);

        for (var i = 0; i < assemblies.Count; i++)
        {
            console.SetCursorPosition(0, startTop + i);

            var assembly = assemblies[i];
            var isCurrent = i == currentIndex;

            console.ForegroundColor = isCurrent ? ConsoleColor.Cyan : ConsoleColor.DarkGray;

            var cursor = isCurrent ? "▶" : " ";
            var sizeStr = $"({assembly.FormattedSize})";
            var prefix = $" {cursor} ";
            var suffix = $"  {sizeStr}";
            var nameWidth = Math.Max(1, maxWidth - prefix.Length - suffix.Length);
            var name = assembly.Name.Length > nameWidth
                ? $"{assembly.Name[..(nameWidth - 3)]}..."
                : assembly.Name.PadRight(nameWidth);

            var line = $"{prefix}{name}{suffix}";
            var padding = Math.Max(0, maxWidth - line.Length);
            console.Write($"{line}{new string(' ', padding)}");
            console.ResetColor();
        }
    }
}

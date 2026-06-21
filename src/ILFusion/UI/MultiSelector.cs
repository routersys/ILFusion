namespace ILFusion.UI;

using ILFusion.Models;

sealed class MultiSelector(IConsoleIO console)
{
    public IReadOnlyList<AssemblyEntry> Select(
        IReadOnlyList<AssemblyEntry> assemblies,
        AssemblyEntry? preselectedPrimary = null)
    {
        if (assemblies.Count == 0)
            return [];

        var selected = new HashSet<int>();
        var preselectedIdx = -1;

        if (preselectedPrimary is not null)
        {
            for (var i = 0; i < assemblies.Count; i++)
            {
                if (string.Equals(assemblies[i].Path, preselectedPrimary.Path, StringComparison.OrdinalIgnoreCase))
                {
                    preselectedIdx = i;
                    selected.Add(i);
                    break;
                }
            }
        }

        var currentIndex = 0;
        console.CursorVisible = false;

        try
        {
            WriteHint();
            var startTop = console.GetCursorPosition().Top;
            RenderList(assemblies, selected, preselectedIdx, currentIndex, startTop);

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

                    case ConsoleKey.Spacebar:
                        if (!selected.Remove(currentIndex))
                            selected.Add(currentIndex);
                        break;

                    case ConsoleKey.A when key.Modifiers.HasFlag(ConsoleModifiers.Control):
                        if (selected.Count == assemblies.Count)
                            selected.Clear();
                        else
                            for (var i = 0; i < assemblies.Count; i++)
                                selected.Add(i);
                        break;

                    case ConsoleKey.Enter when selected.Count >= 1:
                        console.SetCursorPosition(0, startTop + assemblies.Count);
                        return BuildResult(assemblies, selected, preselectedIdx);

                    case ConsoleKey.Escape:
                        console.SetCursorPosition(0, startTop + assemblies.Count);
                        return [];
                }

                RenderList(assemblies, selected, preselectedIdx, currentIndex, startTop);
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
        console.WriteLine("[Space] 選択/解除  [↑↓] 移動  [Ctrl+A] 全選択/解除  [Enter] 確定  [Esc] キャンセル");
        console.ResetColor();
        console.WriteLine();
    }

    private void RenderList(
        IReadOnlyList<AssemblyEntry> assemblies,
        HashSet<int> selected,
        int preselectedIdx,
        int currentIndex,
        int startTop)
    {
        var maxWidth = Math.Max(20, console.WindowWidth - 1);
        var primaryIndex = GetPrimaryIndex(selected, preselectedIdx);

        for (var i = 0; i < assemblies.Count; i++)
        {
            console.SetCursorPosition(0, startTop + i);

            var assembly = assemblies[i];
            var isSelected = selected.Contains(i);
            var isCurrent = i == currentIndex;
            var isPrimary = i == primaryIndex;

            console.ForegroundColor = (isCurrent, isPrimary, isSelected) switch
            {
                (true, _, _) => ConsoleColor.Cyan,
                (_, true, _) => ConsoleColor.Yellow,
                (_, _, true) => ConsoleColor.Green,
                _ => ConsoleColor.DarkGray
            };

            var cursor = isCurrent ? "▶" : " ";
            var check = isSelected ? "●" : "○";
            var tag = isPrimary ? " [主]" : "     ";
            var sizeStr = $"({assembly.FormattedSize})";
            var prefix = $" {cursor} [{check}]{tag} ";
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

    private static int GetPrimaryIndex(HashSet<int> selected, int preselectedIdx)
    {
        if (preselectedIdx >= 0 && selected.Contains(preselectedIdx))
            return preselectedIdx;
        return selected.Count > 0 ? selected.Min() : -1;
    }

    private static IReadOnlyList<AssemblyEntry> BuildResult(
        IReadOnlyList<AssemblyEntry> assemblies,
        HashSet<int> selected,
        int preselectedIdx)
    {
        if (selected.Count == 0)
            return [];

        var primaryIndex = GetPrimaryIndex(selected, preselectedIdx);
        return [assemblies[primaryIndex], .. selected
            .Where(i => i != primaryIndex)
            .OrderBy(i => i)
            .Select(i => assemblies[i])];
    }
}

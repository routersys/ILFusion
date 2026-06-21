namespace ILFusion.Tests.UI;

using ILFusion.Models;
using ILFusion.Tests.Fakes;
using ILFusion.UI;

public sealed class SingleSelectorTests
{
    private static ConsoleKeyInfo Key(ConsoleKey key, ConsoleModifiers modifiers = default) =>
        new('\0', key,
            (modifiers & ConsoleModifiers.Shift) != 0,
            (modifiers & ConsoleModifiers.Alt) != 0,
            (modifiers & ConsoleModifiers.Control) != 0);

    private static AssemblyEntry Assembly(string name) =>
        new($"/{name}", name, 1024);

    [Fact]
    public void Select_EmptyAssemblies_ReturnsNull()
    {
        var selector = new SingleSelector(new FakeConsoleIO());

        var result = selector.Select([]);

        Assert.Null(result);
    }

    [Fact]
    public void Select_EnterKey_ReturnsFirstItem()
    {
        var selector = new SingleSelector(new FakeConsoleIO(Key(ConsoleKey.Enter)));

        var result = selector.Select([Assembly("a.dll"), Assembly("b.dll")]);

        Assert.NotNull(result);
        Assert.Equal("a.dll", result.Name);
    }

    [Fact]
    public void Select_DownArrowThenEnter_ReturnsSecondItem()
    {
        var selector = new SingleSelector(new FakeConsoleIO(
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Enter)));

        var result = selector.Select([Assembly("a.dll"), Assembly("b.dll")]);

        Assert.NotNull(result);
        Assert.Equal("b.dll", result.Name);
    }

    [Fact]
    public void Select_EscapeKey_ReturnsNull()
    {
        var selector = new SingleSelector(new FakeConsoleIO(Key(ConsoleKey.Escape)));

        var result = selector.Select([Assembly("a.dll")]);

        Assert.Null(result);
    }

    [Fact]
    public void Select_UpArrowWrapsToLastItem()
    {
        var selector = new SingleSelector(new FakeConsoleIO(
            Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll"), Assembly("c.dll") };

        var result = selector.Select(assemblies);

        Assert.Equal("c.dll", result?.Name);
    }

    [Fact]
    public void Select_MultipleDownArrows_NavigatesCorrectly()
    {
        var selector = new SingleSelector(new FakeConsoleIO(
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll"), Assembly("c.dll") };

        var result = selector.Select(assemblies);

        Assert.Equal("c.dll", result?.Name);
    }

    [Fact]
    public void Select_DownArrowWrapsToFirst()
    {
        var selector = new SingleSelector(new FakeConsoleIO(
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll") };

        var result = selector.Select(assemblies);

        Assert.Equal("a.dll", result?.Name);
    }

    [Fact]
    public void Select_UpThenDown_ReturnsToOrigin()
    {
        var selector = new SingleSelector(new FakeConsoleIO(
            Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll") };

        var result = selector.Select(assemblies);

        Assert.Equal("a.dll", result?.Name);
    }
}

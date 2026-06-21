namespace ILFusion.Tests.UI;

using ILFusion.Models;
using ILFusion.Tests.Fakes;
using ILFusion.UI;

public sealed class MultiSelectorTests
{
    private static ConsoleKeyInfo Key(ConsoleKey key, ConsoleModifiers modifiers = default) =>
        new('\0', key,
            (modifiers & ConsoleModifiers.Shift) != 0,
            (modifiers & ConsoleModifiers.Alt) != 0,
            (modifiers & ConsoleModifiers.Control) != 0);

    private static AssemblyEntry Assembly(string name) =>
        new($"/{name}", name, 1024);

    [Fact]
    public void Select_EmptyAssemblies_ReturnsEmpty()
    {
        var selector = new MultiSelector(new FakeConsoleIO());

        var result = selector.Select([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Select_EscapeKey_ReturnsEmpty()
    {
        var selector = new MultiSelector(new FakeConsoleIO(Key(ConsoleKey.Escape)));

        var result = selector.Select([Assembly("a.dll")]);

        Assert.Empty(result);
    }

    [Fact]
    public void Select_SpaceThenEnter_SelectsFirstItem()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Enter)));

        var result = selector.Select([Assembly("a.dll"), Assembly("b.dll")]);

        Assert.Single(result);
        Assert.Equal("a.dll", result[0].Name);
    }

    [Fact]
    public void Select_DownArrowSpaceEnter_SelectsSecondItem()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Enter)));

        var result = selector.Select([Assembly("a.dll"), Assembly("b.dll")]);

        Assert.Single(result);
        Assert.Equal("b.dll", result[0].Name);
    }

    [Fact]
    public void Select_UpArrowWrapsToLast()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll"), Assembly("c.dll") };

        var result = selector.Select(assemblies);

        Assert.Single(result);
        Assert.Equal("c.dll", result[0].Name);
    }

    [Fact]
    public void Select_CtrlA_SelectsAll()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.A, ConsoleModifiers.Control),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll"), Assembly("c.dll") };

        var result = selector.Select(assemblies);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void Select_CtrlATwice_DeselectsAll()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.A, ConsoleModifiers.Control),
            Key(ConsoleKey.A, ConsoleModifiers.Control),
            Key(ConsoleKey.Escape)));

        var result = selector.Select([Assembly("a.dll"), Assembly("b.dll")]);

        Assert.Empty(result);
    }

    [Fact]
    public void Select_SpaceToggle_RemovesSelection()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Escape)));

        var result = selector.Select([Assembly("a.dll")]);

        Assert.Empty(result);
    }

    [Fact]
    public void Select_EnterWithNoSelection_DoesNotExit()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.Enter),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Enter)));

        var result = selector.Select([Assembly("a.dll")]);

        Assert.Single(result);
    }

    [Fact]
    public void Select_MultipleItems_ResultOrderedByIndex()
    {
        var selector = new MultiSelector(new FakeConsoleIO(
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.DownArrow),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.UpArrow),
            Key(ConsoleKey.Spacebar),
            Key(ConsoleKey.Enter)));
        var assemblies = new[] { Assembly("a.dll"), Assembly("b.dll"), Assembly("c.dll") };

        var result = selector.Select(assemblies);

        Assert.Equal(3, result.Count);
        Assert.Equal(["a.dll", "b.dll", "c.dll"], result.Select(r => r.Name));
    }
}

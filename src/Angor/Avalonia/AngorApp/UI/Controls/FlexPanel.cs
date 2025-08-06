using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using CSharpFunctionalExtensions;

namespace AngorApp.UI.Controls;

public enum FlexJustify
{
    Start,
    Center,
    End,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum FlexAlign
{
    Start,
    Center,
    End,
    Stretch
}

public class FlexPanel : Panel
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<FlexPanel, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly StyledProperty<bool> WrapProperty =
        AvaloniaProperty.Register<FlexPanel, bool>(nameof(Wrap), true);

    public bool Wrap
    {
        get => GetValue(WrapProperty);
        set => SetValue(WrapProperty, value);
    }

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<FlexPanel, double>(nameof(Spacing));

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly StyledProperty<FlexJustify> JustifyContentProperty =
        AvaloniaProperty.Register<FlexPanel, FlexJustify>(nameof(JustifyContent), FlexJustify.Start);

    public FlexJustify JustifyContent
    {
        get => GetValue(JustifyContentProperty);
        set => SetValue(JustifyContentProperty, value);
    }

    public static readonly StyledProperty<FlexAlign> AlignItemsProperty =
        AvaloniaProperty.Register<FlexPanel, FlexAlign>(nameof(AlignItems), FlexAlign.Stretch);

    public FlexAlign AlignItems
    {
        get => GetValue(AlignItemsProperty);
        set => SetValue(AlignItemsProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children.Where(c => c.IsVisible).ToList();
        foreach (var child in children)
        {
            child.Measure(availableSize);
        }

        var lines = BuildLines(children, MainValue(availableSize));

        var main = lines.Any() ? lines.Max(l => l.Main) : 0;
        var cross = lines.Sum(l => l.Cross) + Spacing * Math.Max(0, lines.Count - 1);

        return Orientation == Orientation.Horizontal
            ? new Size(main, cross)
            : new Size(cross, main);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children.Where(c => c.IsVisible).ToList();
        var lines = BuildLines(children, MainValue(finalSize));

        var crossOffset = 0.0;

        foreach (var line in lines)
        {
            var (start, spacing) = CalculateJustify(line, MainValue(finalSize));
            var mainOffset = start;

            foreach (var item in line.Items)
            {
                var (alignOffset, alignSize) = CalculateAlign(line.Cross, CrossValue(item.Size));
                var rect = Orientation == Orientation.Horizontal
                    ? new Rect(mainOffset, crossOffset + alignOffset, item.Size.Width, alignSize)
                    : new Rect(crossOffset + alignOffset, mainOffset, alignSize, item.Size.Height);

                item.Control.Arrange(rect);
                mainOffset += MainValue(item.Size) + spacing;
            }

            crossOffset += line.Cross + Spacing;
        }

        return finalSize;
    }

    private (double start, double spacing) CalculateJustify(Line line, double availableMain)
    {
        var leftover = Math.Max(0, availableMain - line.Main);
        var itemCount = line.Items.Count;
        var spacing = Spacing;
        var start = 0.0;

        var leftoverMaybe = leftover > 0 ? Maybe<double>.From(leftover) : Maybe<double>.None;

        switch (JustifyContent)
        {
            case FlexJustify.Center:
                leftoverMaybe.Execute(l => start = l / 2);
                break;
            case FlexJustify.End:
                leftoverMaybe.Execute(l => start = l);
                break;
            case FlexJustify.SpaceBetween:
                leftoverMaybe.Execute(l => spacing += itemCount > 1 ? l / (itemCount - 1) : 0);
                break;
            case FlexJustify.SpaceAround:
                leftoverMaybe.Execute(l =>
                {
                    spacing += l / itemCount;
                    start = spacing / 2;
                });
                break;
            case FlexJustify.SpaceEvenly:
                leftoverMaybe.Execute(l =>
                {
                    spacing += l / (itemCount + 1);
                    start = spacing;
                });
                break;
        }

        return (start, spacing);
    }

    private (double offset, double size) CalculateAlign(double lineCross, double itemCross)
    {
        return AlignItems switch
        {
            FlexAlign.Start => (0, itemCross),
            FlexAlign.Center => ((lineCross - itemCross) / 2, itemCross),
            FlexAlign.End => (lineCross - itemCross, itemCross),
            FlexAlign.Stretch => (0, lineCross),
            _ => (0, itemCross)
        };
    }

    private IReadOnlyList<Line> BuildLines(IReadOnlyList<Control> children, double availableMain)
    {
        var lines = new List<Line>();
        var current = new Line();

        foreach (var child in children)
        {
            var size = child.DesiredSize;
            var main = MainValue(size);
            var cross = CrossValue(size);
            var gap = current.Items.Count > 0 ? Spacing : 0;

            var exceeds = Wrap && current.Items.Count > 0 && current.Main + main + gap > availableMain;
            if (exceeds)
            {
                lines.Add(current);
                current = new Line();
                gap = 0;
            }

            current.Items.Add(new Item(child, size));
            current.Main += main + gap;
            current.Cross = Math.Max(current.Cross, cross);
        }

        if (current.Items.Count > 0)
        {
            lines.Add(current);
        }

        return lines;
    }

    private double MainValue(Size size) => Orientation == Orientation.Horizontal ? size.Width : size.Height;
    private double CrossValue(Size size) => Orientation == Orientation.Horizontal ? size.Height : size.Width;

    private record Item(Control Control, Size Size);

    private class Line
    {
        public List<Item> Items { get; } = new();
        public double Main { get; set; }
        public double Cross { get; set; }
    }
}


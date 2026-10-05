using System.Windows;
using System.Windows.Controls;

namespace AIControlTower.Services;

/// <summary>One or two card columns, retaining the actual ListBox containers and keyboard selection.</summary>
public sealed class ResponsiveCardPanel : Panel
{
    public double MinimumCardWidth { get; set; } = 260;
    public double Gap { get; set; } = 12;
    private int _columns;
    private double _cardWidth;
    private readonly List<double> _rowHeights = [];
    protected override Size MeasureOverride(Size available)
    {
        var width = double.IsFinite(available.Width) ? Math.Max(0, available.Width) : MinimumCardWidth;
        _columns = width >= MinimumCardWidth * 2 + Gap ? 2 : 1;
        _cardWidth = Math.Max(0, (width - Gap * (_columns - 1)) / _columns);
        _rowHeights.Clear();
        for (var i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];
            child.Measure(new Size(_cardWidth, double.PositiveInfinity));
            var row = i / _columns;
            if (_rowHeights.Count <= row) _rowHeights.Add(0);
            _rowHeights[row] = Math.Max(_rowHeights[row], child.DesiredSize.Height);
        }
        return new Size(width, _rowHeights.Sum() + Math.Max(0, _rowHeights.Count - 1) * Gap);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        var y = 0.0;
        for (var i = 0; i < InternalChildren.Count; i++)
        {
            var row = i / Math.Max(1, _columns);
            var col = i % Math.Max(1, _columns);
            InternalChildren[i].Arrange(new Rect(col * (_cardWidth + Gap), y, _cardWidth, _rowHeights[row]));
            if (col == _columns - 1 || i == InternalChildren.Count - 1) y += _rowHeights[row] + Gap;
        }
        return finalSize;
    }
}

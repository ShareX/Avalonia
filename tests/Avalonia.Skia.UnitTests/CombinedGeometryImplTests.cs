using SkiaSharp;
using Xunit;

namespace Avalonia.Skia.UnitTests;

public class CombinedGeometryImplTests
{
    [Fact]
    public void Combining_Fill_With_Empty_Stroke_Returns_Fill_Bounds()
    {
        using var fillBuilder = new SKPathBuilder();
        fillBuilder.LineTo(100, 0);
        fillBuilder.LineTo(100, 100);
        fillBuilder.LineTo(0, 100);
        fillBuilder.Close();
        var fill = fillBuilder.Detach();

        var stroke = new SKPath();

        var result = new CombinedGeometryImpl(stroke, fill);

        Assert.Equal(new Rect(0, 0, 100, 100), result.Bounds);
    }
}

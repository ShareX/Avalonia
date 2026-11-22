using Avalonia.Media;
using Avalonia.Platform;
using SkiaSharp;

namespace Avalonia.Skia
{
    /// <summary>
    /// A Skia implementation of a <see cref="IStreamGeometryImpl"/>.
    /// </summary>
    internal class StreamGeometryImpl : GeometryImpl, IStreamGeometryImpl
    {
        private Rect _bounds;
        private SKPath _strokePath;
        private SKPath? _fillPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamGeometryImpl"/> class.
        /// </summary>
        /// <param name="stroke">An existing Skia <see cref="SKPath"/> for the stroke.</param>
        /// <param name="fill">An existing Skia <see cref="SKPath"/> for the fill, can also be null or the same as the stroke</param>
        /// <param name="bounds">Precomputed path bounds.</param>
        public StreamGeometryImpl(SKPath stroke, SKPath? fill, Rect? bounds = null)
        {
            _strokePath = stroke;
            _fillPath = fill;
            _bounds = bounds ?? stroke.TightBounds.ToAvaloniaRect();
        }

        private StreamGeometryImpl(SKPath path) : this(path, path, default(Rect))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamGeometryImpl"/> class.
        /// </summary>
        public StreamGeometryImpl() : this(CreateEmptyPath())
        {
        }

        /// <inheritdoc />
        public override SKPath? StrokePath => _strokePath;

        /// <inheritdoc />
        public override SKPath? FillPath => _fillPath;

        /// <inheritdoc />
        public override Rect Bounds => _bounds;

        /// <inheritdoc />
        public IStreamGeometryImpl Clone()
        {
            var stroke = _strokePath.Clone();
            var fill = _fillPath == _strokePath ? stroke : _fillPath.Clone();
            return new StreamGeometryImpl(stroke, fill, Bounds);
        }

        /// <inheritdoc />
        public IStreamGeometryContextImpl Open()
        {
            return new StreamContext(this);
        }

        /// <summary>
        /// Create new empty <see cref="SKPath"/>.
        /// </summary>
        /// <returns>Empty <see cref="SKPath"/></returns>
        private static SKPath CreateEmptyPath()
        {
            return new SKPath
            {
                FillType = SKPathFillType.EvenOdd
            };
        }

        /// <summary>
        /// A Skia implementation of a <see cref="IStreamGeometryContextImpl"/>.
        /// </summary>
        private class StreamContext : IStreamGeometryContextImpl
        {
            private readonly StreamGeometryImpl _geometryImpl;
            private readonly SKPathBuilder _strokeBuilder;
            private SKPathBuilder? _fillBuilder;
            private bool _hasSeparateFill;
            private bool _disposed;
            private bool _isFilled;
            private Point _startPoint;
            private bool _isFigureBroken;

            private SKPathBuilder FillBuilder
            {
                get
                {
                    if (_hasSeparateFill)
                        return _fillBuilder!;

                    if (_geometryImpl._fillPath is null)
                    {
                        _hasSeparateFill = true;
                        return _fillBuilder = new SKPathBuilder();
                    }

                    return _strokeBuilder;
                }
            }

            private bool Duplicate => _isFilled && (_hasSeparateFill || _geometryImpl._fillPath is null);

            private void EnsureSeparateFillPath()
            {
                if (!_hasSeparateFill)
                {
                    _hasSeparateFill = true;
                    if (_geometryImpl._fillPath is not null)
                    {
                        using var snapshot = _strokeBuilder.Snapshot();
                        _fillBuilder = new SKPathBuilder(snapshot);
                    }
                    else
                    {
                        _fillBuilder = new SKPathBuilder();
                    }
                }
            }
            
            private void BreakFigure()
            {
                if (!_isFigureBroken)
                {
                    _isFigureBroken = true;
                    EnsureSeparateFillPath();
                }
                    
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="StreamContext"/> class.
            /// <param name="geometryImpl">Geometry to operate on.</param>
            /// </summary>
            public StreamContext(StreamGeometryImpl geometryImpl)
            {
                _geometryImpl = geometryImpl;
                _strokeBuilder = new SKPathBuilder(geometryImpl._strokePath);
                if (geometryImpl._fillPath is { } fillPath && !ReferenceEquals(fillPath, geometryImpl._strokePath))
                {
                    _fillBuilder = new SKPathBuilder(fillPath);
                    _hasSeparateFill = true;
                }
            }

            /// <inheritdoc />
            /// <remarks>Will update bounds of passed geometry.</remarks>
            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                var stroke = _strokeBuilder.Detach();
                _strokeBuilder.Dispose();
                _geometryImpl._strokePath = stroke;

                if (_hasSeparateFill)
                {
                    _geometryImpl._fillPath = _fillBuilder!.Detach();
                    _fillBuilder.Dispose();
                }
                else if (_geometryImpl._fillPath is not null)
                {
                    _geometryImpl._fillPath = stroke;
                }

                _geometryImpl._bounds = stroke.TightBounds.ToAvaloniaRect();
                _geometryImpl.InvalidateCaches();
            }

            /// <inheritdoc />
            public void BeginFigure(Point startPoint, bool isFilled = true)
            {
                if (!isFilled) 
                    EnsureSeparateFillPath();

                _isFilled = isFilled;
                _startPoint = startPoint;
                _isFigureBroken = false;
                _strokeBuilder.MoveTo((float)startPoint.X, (float)startPoint.Y);
                if (Duplicate)
                    FillBuilder.MoveTo((float)startPoint.X, (float)startPoint.Y);
            }

            /// <inheritdoc />
            public void EndFigure(bool isClosed)
            {
                if (isClosed)
                {
                    if (_isFigureBroken)
                    {
                        _strokeBuilder.LineTo(_startPoint.ToSKPoint());
                        _isFigureBroken = false;
                    }
                    else
                        _strokeBuilder.Close();
                    if (Duplicate)
                        FillBuilder.Close();
                }
            }

            /// <inheritdoc />
            public void SetFillRule(FillRule fillRule)
            {
                FillBuilder.FillType = fillRule == FillRule.EvenOdd ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
            }

            /// <inheritdoc />
            public void LineTo(Point point, bool isStroked = true)
            {
                if (isStroked)
                {
                    _strokeBuilder.LineTo((float)point.X, (float)point.Y);
                }
                else
                {
                    BreakFigure();
                    _strokeBuilder.MoveTo((float)point.X, (float)point.Y);
                }
                if (Duplicate)
                    FillBuilder.LineTo((float)point.X, (float)point.Y);
            }

            /// <inheritdoc />
            public void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked = true)
            {
                var arc = isLargeArc ? SKPathArcSize.Large : SKPathArcSize.Small;
                var sweep = sweepDirection == SweepDirection.Clockwise
                    ? SKPathDirection.Clockwise
                    : SKPathDirection.CounterClockwise;

                if (isStroked)
                {
                    _strokeBuilder.ArcTo(
                        (float)size.Width,
                        (float)size.Height,
                        (float)rotationAngle,
                        arc,
                        sweep,
                        (float)point.X,
                        (float)point.Y);
                }
                else
                {
                    BreakFigure();
                    _strokeBuilder.MoveTo((float)point.X, (float)point.Y);
                }
                if (Duplicate)
                    FillBuilder.ArcTo(
                        (float)size.Width,
                        (float)size.Height,
                        (float)rotationAngle,
                        arc,
                        sweep,
                        (float)point.X,
                        (float)point.Y);
            }

            /// <inheritdoc />
            public void CubicBezierTo(Point point1, Point point2, Point point3, bool isStroked = true)
            {
                if (isStroked)
                {
                    _strokeBuilder.CubicTo((float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y, (float)point3.X, (float)point3.Y);
                }
                else
                {
                    BreakFigure();
                    _strokeBuilder.MoveTo((float)point3.X, (float)point3.Y);
                }
                if (Duplicate)
                    FillBuilder.CubicTo((float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y, (float)point3.X, (float)point3.Y);
            }

            /// <inheritdoc />
            public void QuadraticBezierTo(Point point1, Point point2, bool isStroked = true)
            {
                if (isStroked)
                {
                    _strokeBuilder.QuadTo((float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y);
                }
                else
                {
                    BreakFigure();
                    _strokeBuilder.MoveTo((float)point2.X, (float)point2.Y);
                }
                if (Duplicate)
                    FillBuilder.QuadTo((float)point1.X, (float)point1.Y, (float)point2.X, (float)point2.Y);
            }
        }
    }
}

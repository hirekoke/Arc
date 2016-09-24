using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Arc
{
    public class Arc : Shape
	{
		#region field
		protected Rect _rect = Rect.Empty;
		protected Pen _pen = null;
		protected Geometry _geometryCache = null;
		#endregion

		static Arc()
		{
			StretchProperty.OverrideMetadata(typeof(Arc), new FrameworkPropertyMetadata(Stretch.Fill));
		}

		#region Dependency Properties

		#region StartAngle
		public static readonly DependencyProperty StartAngleProperty =
			DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc),
			new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

		public double StartAngle
		{
			get { return (double)GetValue(StartAngleProperty); }
			set { SetValue(StartAngleProperty, value); }
		}
		#endregion

		#region Angle
		public static readonly DependencyProperty AngleProperty =
			DependencyProperty.Register("Angle", typeof(double), typeof(Arc),
			new FrameworkPropertyMetadata(90.0, FrameworkPropertyMetadataOptions.AffectsRender));
		public double Angle
		{
			get { return (double)GetValue(AngleProperty); }
			set { SetValue(AngleProperty, value); }
		}
		#endregion

		#region IsPie
		public static readonly DependencyProperty IsPieProperty =
			DependencyProperty.Register("IsPie", typeof(bool), typeof(Arc),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
		public bool IsPie
		{
			get { return (bool)GetValue(IsPieProperty); }
			set { SetValue(IsPieProperty, value); }
		}
		#endregion

		#region IsClockwise
		public static readonly DependencyProperty IsClockwiseProperty =
			DependencyProperty.Register("IsClockwise", typeof(bool), typeof(Arc),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
		public bool IsClockwise
		{
			get { return (bool)GetValue(IsClockwiseProperty); }
			set { SetValue(IsClockwiseProperty, value); }
		}
		#endregion

		#endregion Dependency Properties

		#region Layout Methods (cf. Ellipse)
		// For an Arc, RenderedGeometry = defining geometry and GeometryTransform = Identity
		// cf. Ellipse

		public override Geometry RenderedGeometry
		{
			get
			{
				Console.WriteLine("hoge");
				return DefiningGeometry;
			}
		}
		public override Transform GeometryTransform
		{
			get { return Transform.Identity; }
		}

		protected override Size MeasureOverride(Size constraint)
		{
			if (Stretch == Stretch.UniformToFill)
			{
				double width = constraint.Width;
				double height = constraint.Height;

				if (Double.IsInfinity(width) && Double.IsInfinity(height))
				{
					return GetNaturalSize();
				}
				else if (Double.IsInfinity(width) || Double.IsInfinity(height))
				{
					width = Math.Min(width, height);
				}
				else
				{
					width = Math.Max(width, height);
				}

				return new Size(width, width);
			}

			return GetNaturalSize();
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			// We construct the rectangle to fit finalSize with the appropriate Stretch mode.  The rendering
			// transformation will thus be the identity.

			double penThickness = ValidStrokeThickness;
			double margin = penThickness / 2;

			_rect = new Rect(
				margin, // X
				margin, // Y
				Math.Max(0, finalSize.Width - penThickness),    // Width
				Math.Max(0, finalSize.Height - penThickness));  // Height

			switch (Stretch)
			{
				case Stretch.None:
					// A 0 Rect.Width and Rect.Height rectangle
					_rect.Width = _rect.Height = 0;
					break;

				case Stretch.Fill:
					// The most common case: a rectangle that fills the box.
					// _rect has already been initialized for that.
					break;

				case Stretch.Uniform:
					// The maximal square that fits in the final box
					if (_rect.Width > _rect.Height)
					{
						_rect.Width = _rect.Height;
					}
					else  // _rect.Width <= _rect.Height
					{
						_rect.Height = _rect.Width;
					}
					break;

				case Stretch.UniformToFill:

					// The minimal square that fills the final box
					if (_rect.Width < _rect.Height)
					{
						_rect.Width = _rect.Height;
					}
					else  // _rect.Width >= _rect.Height
					{
						_rect.Height = _rect.Width;
					}
					break;
			}

			_geometryCache = null;
			return finalSize;
		}

		protected override Geometry DefiningGeometry
		{
			get
			{
				if (_rect.IsEmpty)
					return Geometry.Empty;

				Geometry geometry = CreateGeometry();
				CombinedGeometry geo = new CombinedGeometry(GeometryCombineMode.Intersect,
					new EllipseGeometry(_rect), geometry);
				return geo;
			}
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (!_rect.IsEmpty)
			{
				drawingContext.DrawGeometry(Fill, Pen, CreateGeometry());
			}
		}

		private Size GetNaturalSize()
		{
			double strokeThickness = ValidStrokeThickness;
			return new Size(strokeThickness, strokeThickness);
		}
		#endregion

		protected virtual Geometry CreateGeometry()
		{
			if (_geometryCache == null)
			{
				double startAngle = StartAngle;
				double angle = Angle;
				bool isClockwise = IsClockwise;
				bool isPie = IsPie;

				if (isClockwise) angle = -angle;

				if (angle <= -360)
					angle = -angle;
				if (angle < 0)
				{
					startAngle = startAngle + angle;
					angle = -angle;
				}

				bool sep = angle >= 360;

				double startRad = startAngle * Math.PI / 180.0;
				double angleRad = (sep ? 359 : angle) * Math.PI / 180.0;

				bool largeAngle = angle > 180;
				double endRad = startRad + angleRad;

				double radiusX = _rect.Width / 2.0;
				double radiusY = _rect.Height / 2.0;
				double cx = _rect.X + _rect.Width / 2.0;
				double cy = _rect.Y + _rect.Height / 2.0;

				double spX = cx + radiusX * Math.Cos(startRad);
				double spY = cy - radiusY * Math.Sin(startRad);
				double epX = cx + radiusX * Math.Cos(endRad);
				double epY = cy - radiusY * Math.Sin(endRad);

				if (sep)
				{
					return new EllipseGeometry(_rect);
				}

				PathFigure pathFigure = null;
				if (isPie)
				{
					pathFigure = new PathFigure(
						new Point(cx, cy),
						new List<PathSegment>() 
							{
								new LineSegment(new Point(spX, spY), true),
								new ArcSegment(new Point(epX, epY), new Size(radiusX, radiusY), 0.0, largeAngle,
									SweepDirection.Counterclockwise, true)
							},
						true);
				}
				else
				{
					pathFigure = new PathFigure(
						new Point(spX, spY),
						new List<PathSegment>()
							{
								new ArcSegment(new Point(epX, epY), new Size(radiusX, radiusY), 0.0, largeAngle, 
									SweepDirection.Counterclockwise, true),
							},
						false);
				}
				_geometryCache = new PathGeometry(new List<PathFigure>() { pathFigure });
			}
			return _geometryCache;
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			switch (e.Property.Name)
			{
				case "Stroke":
				case "StrokeThickness":
				case "StrokeStartLineCap":
				case "StrokeEndLineCap":
				case "StrokeDashCap":
				case "StrokeLineJoin":
				case "StrokeMiterLimit":
				case "StrokeDashOffset":
				case "StrokeDashArray":
					_pen = null;
					break;
			}
		}

		protected bool IsNoPen
		{
			get
			{
				double th = StrokeThickness;
				return (Stroke == null) || Double.IsNaN(th) || Math.Abs(th) < 10 * Double.MinValue;
			}
		}

		protected double ValidStrokeThickness
		{
			get
			{
				if (IsNoPen) return 0.0;
				return Math.Abs(StrokeThickness);
			}
		}

		protected Pen Pen
		{
			get
			{
				if (IsNoPen) return null;

				if (_pen == null)
				{
					double th = ValidStrokeThickness;

					_pen = new Pen()
					{
						Thickness = th,
						Brush = Stroke,
						StartLineCap = StrokeStartLineCap,
						EndLineCap = StrokeEndLineCap,
						DashCap = StrokeDashCap,
						LineJoin = StrokeLineJoin,
						MiterLimit = StrokeMiterLimit,
					};

					DoubleCollection strokeDashArray = StrokeDashArray;
					double strokeDashOffset = StrokeDashOffset;
					if (strokeDashArray != null || strokeDashOffset != 0.0)
					{
						_pen.DashStyle = new DashStyle(strokeDashArray, strokeDashOffset);
					}
				}
				return _pen;
			}
		}
	}
}

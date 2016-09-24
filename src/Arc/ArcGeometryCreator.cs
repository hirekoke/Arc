using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;


namespace Arc
{
	public class ArcGeometryCreator : DependencyObject
	{
		#region StartAngle
		public static readonly DependencyProperty StartAngleProperty =
			DependencyProperty.Register("StartAngle", typeof(double), typeof(ArcGeometryCreator),
			new PropertyMetadata(0.0, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));

		public double StartAngle
		{
			get { return (double)GetValue(StartAngleProperty); }
			set { SetValue(StartAngleProperty, value); }
		}
		#endregion

		#region Angle
		public static readonly DependencyProperty AngleProperty =
			DependencyProperty.Register("Angle", typeof(double), typeof(ArcGeometryCreator),
			new PropertyMetadata(90.0, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));
		public double Angle
		{
			get { return (double)GetValue(AngleProperty); }
			set { SetValue(AngleProperty, value); }
		}
		#endregion

		#region IsPie
		public static readonly DependencyProperty IsPieProperty =
			DependencyProperty.Register("IsPie", typeof(bool), typeof(ArcGeometryCreator),
			new PropertyMetadata(false, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));
		public bool IsPie
		{
			get { return (bool)GetValue(IsPieProperty); }
			set { SetValue(IsPieProperty, value); }
		}
		#endregion

		#region IsClockwise
		public static readonly DependencyProperty IsClockwiseProperty =
			DependencyProperty.Register("IsClockwise", typeof(bool), typeof(ArcGeometryCreator),
			new PropertyMetadata(false, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));
		public bool IsClockwise
		{
			get { return (bool)GetValue(IsClockwiseProperty); }
			set { SetValue(IsClockwiseProperty, value); }
		}
		#endregion

		#region RadiusX
		public static readonly DependencyProperty RadiusXProperty =
			DependencyProperty.Register("RadiusX", typeof(double), typeof(ArcGeometryCreator),
			new PropertyMetadata(100.0, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));

		public double RadiusX
		{
			get { return (double)GetValue(RadiusXProperty); }
			set { SetValue(RadiusXProperty, value); }
		}
		#endregion

		#region RadiusY
		public static readonly DependencyProperty RadiusYProperty =
			DependencyProperty.Register("RadiusY", typeof(double), typeof(ArcGeometryCreator),
			new PropertyMetadata(100.0, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));

		public double RadiusY
		{
			get { return (double)GetValue(RadiusYProperty); }
			set { SetValue(RadiusYProperty, value); }
		}
		#endregion

		#region Center
		public static readonly DependencyProperty CenterProperty =
			DependencyProperty.Register("Center", typeof(Point), typeof(ArcGeometryCreator),
			new PropertyMetadata(new Point(50, 50), (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));
		public Point Center
		{
			get { return (Point)GetValue(CenterProperty); }
			set { SetValue(CenterProperty, value); }
		}
		#endregion

		#region Thickness
		public static readonly DependencyProperty ThicknessProperty =
			DependencyProperty.Register("Thickness", typeof(double), typeof(ArcGeometryCreator),
			new PropertyMetadata(1.0, (o, e) =>
			{
				(o as ArcGeometryCreator).UpdateGeometry();
			}));
		public double Thickness
		{
			get { return (double)GetValue(ThicknessProperty); }
			set { SetValue(ThicknessProperty, value); }
		}
		#endregion

		#region Geometry
		private static readonly DependencyPropertyKey _geometryPropertyKey =
			DependencyProperty.RegisterReadOnly("Geometry", typeof(Geometry), typeof(ArcGeometryCreator),
			new PropertyMetadata(new PathGeometry()));
		public static readonly DependencyProperty GeometryProperty = _geometryPropertyKey.DependencyProperty;
		public Geometry Geometry
		{
			get { return (Geometry)GetValue(GeometryProperty); }
			private set { SetValue(_geometryPropertyKey, value); }
		}
		#endregion

		#region Path
		private static readonly DependencyPropertyKey _pathPropertyKey =
			DependencyProperty.RegisterReadOnly("PathGeometry", typeof(Geometry), typeof(ArcGeometryCreator),
			new PropertyMetadata(new PathGeometry()));
		public static readonly DependencyProperty PathProperty = _pathPropertyKey.DependencyProperty;
		public Geometry Path
		{
			get { return (Geometry)GetValue(PathProperty); }
			private set { SetValue(_pathPropertyKey, value); }
		}
		#endregion

		private Geometry CreateArcGeometry()
		{
			double startAngle = StartAngle;
			double angle = Angle;
			bool isClockwise = IsClockwise;
			bool isPie = IsPie;
			double radiusX = RadiusX;
			double radiusY = RadiusY;
			double cx = Center.X;
			double cy = Center.Y;

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

			double spX = cx + radiusX * Math.Cos(startRad);
			double spY = cy - radiusY * Math.Sin(startRad);
			double epX = cx + radiusX * Math.Cos(endRad);
			double epY = cy - radiusY * Math.Sin(endRad);

			if (sep)
			{
				return new EllipseGeometry(Center, RadiusX, RadiusY);
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

			return new PathGeometry(new List<PathFigure>() { pathFigure });
		}

		private Geometry CreateThickArcGeometry()
		{
			double startAngle = StartAngle;
			double angle = Angle;
			bool isClockwise = IsClockwise;
			bool isPie = IsPie;
			double outerRadiusX = RadiusX;
			double outerRadiusY = RadiusY;
			double cx = Center.X;
			double cy = Center.Y;
			double thickness = Thickness;

			if (isClockwise) angle = -angle;

			if (angle <= -360)
				angle = Math.Abs(angle);
			if (angle < 0)
			{
				startAngle = startAngle + angle;
				angle = Math.Abs(angle);
			}

			bool sep = angle >= 360;

			double innerRadiusX = outerRadiusX - thickness;
			double innerRadiusY = outerRadiusY - thickness;
			if (innerRadiusX < 0) innerRadiusX = 0;
			if (innerRadiusY < 0) innerRadiusY = 0;

			if (sep)
			{
				if (isPie)
				{
					return new EllipseGeometry(new Point(cx, cy), outerRadiusX, outerRadiusY);
				}
				else
				{
					Geometry outerGeometry = new EllipseGeometry(new Point(cx, cy), outerRadiusX, outerRadiusY);
					Geometry innerGeometry = new EllipseGeometry(new Point(cx, cy), innerRadiusX, innerRadiusY);
					return new CombinedGeometry(GeometryCombineMode.Exclude, outerGeometry, innerGeometry);
				}
			}
			else
			{
				double startRad = startAngle * Math.PI / 180.0;
				double angleRad = (sep ? 359 : angle) * Math.PI / 180.0;

				bool largeAngle = angle > 180;
				double endRad = startRad + angleRad;

				double cosStart = Math.Cos(startRad);
				double sinStart = Math.Sin(startRad);
				double cosEnd = Math.Cos(endRad);
				double sinEnd = Math.Sin(endRad);

				double outerSpX = cx + outerRadiusX * cosStart;
				double outerSpY = cy - outerRadiusY * sinStart;
				double outerEpX = cx + outerRadiusX * cosEnd;
				double outerEpY = cy - outerRadiusY * sinEnd;

				if (isPie)
				{

					PathFigure pathFigure = new PathFigure(
						new Point(cx, cy),
						new List<PathSegment>() 
							{
								new LineSegment(new Point(outerEpX, outerEpY), true),
								new ArcSegment(
									new Point(outerSpX, outerSpY), 
									new Size(outerRadiusX, outerRadiusY), 0.0, largeAngle, SweepDirection.Clockwise, true)
							},
						true);
					return new PathGeometry(new List<PathFigure>() { pathFigure });
				}
				else
				{
					double innerSpX = cx + innerRadiusX * cosStart;
					double innerSpY = cy - innerRadiusY * sinStart;
					double innerEpX = cx + innerRadiusX * cosEnd;
					double innerEpY = cy - innerRadiusY * sinEnd;
					
					PathFigure pathFigure = new PathFigure(
						new Point(outerSpX, outerSpY),
						new List<PathSegment>()
							{
								new ArcSegment(new Point(outerEpX, outerEpY), 
									new Size(outerRadiusX, outerRadiusY),
									0.0, largeAngle, 
									SweepDirection.Counterclockwise, true),
								new LineSegment(new Point(innerEpX, innerEpY), true),
								new ArcSegment(new Point(innerSpX, innerSpY),
									new Size(innerRadiusX, innerRadiusY),
									0.0, largeAngle,
									SweepDirection.Clockwise, true),
								new LineSegment(new Point(outerSpX, outerSpY), true)
							},
						true);
					return new PathGeometry(new List<PathFigure>() { pathFigure });
				}
			}
		}

		private void UpdateGeometry()
		{
			Geometry = CreateThickArcGeometry();
			Path = CreateArcGeometry();
			return;
		}
	}
}

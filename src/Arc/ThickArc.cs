using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Arc
{
	public class ThickArc : Arc
	{
		static ThickArc()
		{
			StretchProperty.OverrideMetadata(typeof(ThickArc), new FrameworkPropertyMetadata(Stretch.Fill));
		}

		#region Dependency Properties

		#region Thickness
		public static readonly DependencyProperty ThicknessProperty =
			DependencyProperty.Register("Thickness", typeof(double), typeof(ThickArc),
			new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));
		public double Thickness
		{
			get { return (double)GetValue(ThicknessProperty); }
			set { SetValue(ThicknessProperty, value); }
		}
		#endregion

		#endregion Dependency Properties

		protected override Geometry CreateGeometry()
		{
			if (_geometryCache == null)
			{
				double startAngle = StartAngle;
				double thickness = Thickness;
				double angle = Angle;
				bool isClockwise = IsClockwise;
				bool isPie = IsPie;

				if (isClockwise) angle = -angle;

				if (angle <= -360)
					angle = Math.Abs(angle);
				if (angle < 0)
				{
					startAngle = startAngle + angle;
					angle = Math.Abs(angle);
				}

				bool sep = angle >= 360;

				double outerRadiusX = _rect.Width / 2.0;
				double outerRadiusY = _rect.Height / 2.0;
				double innerRadiusX = outerRadiusX - thickness;
				double innerRadiusY = outerRadiusY - thickness;
				if (innerRadiusX < 0) innerRadiusX = 0;
				if (innerRadiusY < 0) innerRadiusY = 0;

				double cx = _rect.X + _rect.Width / 2.0;
				double cy = _rect.Y + _rect.Height / 2.0;

				if (sep)
				{
					if (isPie)
					{
						_geometryCache = new EllipseGeometry(new Point(cx, cy), outerRadiusX, outerRadiusY);
					}
					else
					{
						Geometry outerGeometry = new EllipseGeometry(new Point(cx, cy), outerRadiusX, outerRadiusY);
						Geometry innerGeometry = new EllipseGeometry(new Point(cx, cy), innerRadiusX, innerRadiusY);
						_geometryCache = new CombinedGeometry(GeometryCombineMode.Exclude, outerGeometry, innerGeometry);
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

					double innerSpX = cx + innerRadiusX * cosStart;
					double innerSpY = cy - innerRadiusY * sinStart;
					double innerEpX = cx + innerRadiusX * cosEnd;
					double innerEpY = cy - innerRadiusY * sinEnd;

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
						_geometryCache = new PathGeometry(new List<PathFigure>() { pathFigure });
					}
					else
					{
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
						_geometryCache = new PathGeometry(new List<PathFigure>() { pathFigure });
					}
				}
			}
			return _geometryCache;
		}
	}
}

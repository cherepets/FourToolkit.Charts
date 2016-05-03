﻿using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FourToolkit.Charts.Shapes
{
    internal class Arc : UserControl
    {
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), new PropertyMetadata(default(double), PropertyChangedDelegate));

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty SweepAngleProperty =
            DependencyProperty.Register("SweepAngle", typeof(double), typeof(Arc), new PropertyMetadata(default(double), PropertyChangedDelegate));

        public double SweepAngle
        {
            get { return (double)GetValue(SweepAngleProperty); }
            set { SetValue(SweepAngleProperty, value); }
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(Arc), new PropertyMetadata(default(double), PropertyChangedDelegate));

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(SolidColorBrush), typeof(Arc), new PropertyMetadata(new SolidColorBrush(), PropertyChangedDelegate));

        public SolidColorBrush Fill
        {
            get { return (SolidColorBrush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        private CanvasControl _canvas;

        public Arc()
        {
            _canvas = new CanvasControl();
            Content = _canvas;
            _canvas.Draw += Canvas_Draw;
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var startAngle = (float)(StartAngle - Math.PI / 2);

            var center = new Vector2((float)(ActualWidth / 2), (float)(ActualHeight / 2));
            var radius = center.X > center.Y
                ? new Vector2(center.Y - (float)Thickness / 2)
                : new Vector2(center.X - (float)Thickness / 2);
            var startPoint = center + Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(startAngle)) * radius;

            using (var builder = new CanvasPathBuilder(sender))
            {
                builder.BeginFigure(startPoint);
                builder.AddArc(center, radius.X, radius.Y, startAngle, (float)SweepAngle);
                builder.EndFigure(CanvasFigureLoop.Open);
                using (var geometry = CanvasGeometry.CreatePath(builder))
                    args.DrawingSession.DrawGeometry(geometry, Fill.Color, (float)Thickness);
            }
        }

        private void Refresh() => _canvas.Invalidate();

        private static PropertyChangedCallback PropertyChangedDelegate = (s, a) => (s as Arc)?.Refresh();
    }
}
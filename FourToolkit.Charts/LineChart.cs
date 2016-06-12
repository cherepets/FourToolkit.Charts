﻿using FourToolkit.Charts.Extensions;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FourToolkit.Charts
{
    public class LineChart : UserControl
    {
        public DataTemplate ItemTemplate { get; set; }

        public IList ItemsSource
        {
            get
            {
                return _itemsSource;
            }
            set
            {
                _itemsSource = value;
                var observable = _itemsSource as INotifyCollectionChanged;
                if (observable != null)
                    observable.CollectionChanged += (s, e) => _canvas.Invalidate();
                _canvas.Invalidate();
            }
        }

        private IList _itemsSource;

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(LineChart), new PropertyMetadata(3d, PropertyChangedDelegate));

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(SolidColorBrush), typeof(LineChart), new PropertyMetadata(null, PropertyChangedDelegate));

        public SolidColorBrush Fill
        {
            get { return (SolidColorBrush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public YMode YAxis { get; set; }

        // Root of markup
        private Grid _root;
        // X axis captions
        private Grid _xCap;
        // Y axis captions
        private Grid _yCap;
        // Canvas for line drawing
        private CanvasControl _canvas;

        public LineChart()
        {
            _root = new Grid();
            _root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Auto) });
            _root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            _xCap = new Grid();
            _root.Children.Add(_xCap);
            Grid.SetColumn(_xCap, 1);
            Grid.SetRow(_xCap, 1);

            _yCap = new Grid { VerticalAlignment = VerticalAlignment.Stretch };
            _root.Children.Add(_yCap);

            _canvas = new CanvasControl();
            _root.Children.Add(_canvas);
            Grid.SetColumn(_canvas, 1);

            Content = _root;
            _canvas.Draw += Canvas_Draw;
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (ItemTemplate == null) return;
            if (ItemsSource == null) return;
            var items = ItemsSource.Select(o =>
            {
                var item = ItemTemplate.LoadContent() as LineChartItem;
                item.DataContext = o;
                return item;
            }).ToList();
            if (!items.Any()) return;

            var availableWidth = (float)_canvas.ActualWidth;
            var availableHeight = (float)_canvas.ActualHeight;
            var fill = Fill ?? new SolidColorBrush(DefaultColors.GetRandom());
            var elementWidth = availableWidth / (items.Count - 1);
            var radius = (float)(Thickness * 2);

            var min = items.Min(i => i.Value);
            var max = items.Max(i => i.Value);
            var diff = max - min;
            var d = diff * 0.01;

            #region Add X captions
            _xCap.Children.Clear();
            _xCap.Children.Add(new TextBlock
            {
                Text = items.First().Key.ToString(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            _xCap.Children.Add(new TextBlock
            {
                Text = items.Last().Key.ToString(),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            #endregion

            #region Add Y captions
            _yCap.Children.Clear();
            if (YAxis == YMode.FromMin)
            {
                _yCap.Children.Add(new TextBlock
                {
                    Text = min.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
            }
            else
            {
                _yCap.Children.Add(new TextBlock
                {
                    Text = "0",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
                _yCap.Children.Add(new TextBlock
                {
                    Text = min.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, -10 + min * availableHeight / max)
                });
            }
            _yCap.Children.Add(new TextBlock
            {
                Text = max.ToString(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            });
            #endregion

            // Draw lines
            using (var builder = new CanvasPathBuilder(sender))
            {
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var x = i * elementWidth;
                    var y = availableHeight -
                        (YAxis == YMode.FromMin
                            ? (float)((item.Value - min) * availableHeight / diff)
                            : (float)(item.Value * availableHeight / max));
                    // Fixes for edge points
                    if (i == 0) x += radius;
                    if (i == items.Count - 1) x -= radius;
                    if (max - item.Value < d) y += radius;
                    if (item.Value - min < d) y -= radius;
                    // Main drawing
                    args.DrawingSession.FillCircle(x, y, radius, fill.Color);
                    if (i == 0)
                        builder.BeginFigure(x, y);
                    else
                        builder.AddLine(x, y);
                }
                builder.EndFigure(CanvasFigureLoop.Open);
                using (var geometry = CanvasGeometry.CreatePath(builder))
                    args.DrawingSession.DrawGeometry(geometry, fill.Color, (float)Thickness);
                // Draw axis
                var color = ForegroundColor;
                args.DrawingSession.DrawLine(0, 0, 0, availableHeight, Colors.Black, 1);
                args.DrawingSession.DrawLine(0, availableHeight, availableWidth, availableHeight, Colors.Black, 1);
            }
        }

        private Color ForegroundColor => GetBrush("SystemControlForegroundAltMediumHighBrush").Color;

        private SolidColorBrush GetBrush(string s)
            => Application.Current.Resources[s] as SolidColorBrush;

        private void Refresh() => _canvas.Invalidate();

        private static PropertyChangedCallback PropertyChangedDelegate = (s, a) => (s as LineChart)?.Refresh();
    }

    public enum YMode
    {
        FromZero, FromMin
    }
}
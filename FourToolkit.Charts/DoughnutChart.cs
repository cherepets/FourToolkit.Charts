﻿using FourToolkit.Charts.Shapes;
using System.Collections;
using System.Collections.Specialized;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FourToolkit.Charts
{
    public class DoughnutChart : UserControl
    {
        public DataTemplate ItemTemplate { get; set; }
        
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(DoughnutChart), new PropertyMetadata(null, OnItemsSourceChanged));

        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = d as DoughnutChart;
            var value = e.NewValue as IList;
            if (chart == null || value == null) return;
            var observable = value as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged += (s, a) => chart.Redraw();
            chart.Redraw();
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(DoughnutChart), new PropertyMetadata(10d, PropertyChangedDelegate));

        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }

        private Grid _root;

        public DoughnutChart()
        {
            _root = new Grid();
            Content = _root;
        }

        private void Redraw()
        {
            if (ItemTemplate == null) return;
            if (ItemsSource == null) return;
            _root.Children.Clear();
            for (var i = 0; i < ItemsSource.Count; i++)
            {
                var c = i;
                var chartItem = ItemTemplate.LoadContent() as DoughnutChartItem;
                if (chartItem == null) return;
                chartItem.DataContext = ItemsSource[c];
                if (chartItem.Color == default(Color))
                    chartItem.Color = DefaultColors.GetRandom();
                var arc = new Arc { Thickness = Thickness };
                arc.DataContext = chartItem;
                // Angle
                var angleBinding = new Binding
                { Path = new PropertyPath(nameof(DoughnutChartItem.Angle)) };
                arc.SetBinding(Arc.SweepAngleProperty, angleBinding);
                // Fill
                var fillBinding = new Binding
                { Path = new PropertyPath(nameof(DoughnutChartItem.Fill)) };
                arc.SetBinding(Arc.FillProperty, fillBinding);
                _root.Children.Add(arc);
            }
            UpdateStartAngles();
        }

        private void UpdateStartAngles()
        {
            var d = 0d;
            foreach (Arc arc in _root.Children)
            {
                arc.StartAngle = d;
                d += arc.SweepAngle;
            }
        }

        private static PropertyChangedCallback PropertyChangedDelegate = (s, a) => (s as DoughnutChart)?.Redraw();
    }
}

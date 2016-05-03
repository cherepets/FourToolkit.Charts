using FourToolkit.Charts.Shapes;
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
                    observable.CollectionChanged += (s, e) => Redraw();
                Redraw();
            }
        }

        private IList _itemsSource;

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register("Thickness", typeof(double), typeof(DoughnutChart), null);

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

            #region Defaults
            Thickness = 10;
            #endregion
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
    }
}

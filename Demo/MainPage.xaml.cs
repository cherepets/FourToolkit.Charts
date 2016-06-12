using FourToolkit.Charts.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace Demo
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            Init();
        }

        public async void Init()
        {
            var rnd = new Random();
            while (true)
            {
                var i = 0;
                var chartData = ChartData.PrepareChartData(new List<int> { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) }
                #region Comment these lines to set random colors
                , q =>
                {
                    i++;
                    return
                        i == 1 ? Colors.PaleGoldenrod :
                        i == 2 ? Colors.PaleGreen :
                        i == 3 ? Colors.PaleTurquoise
                        : Colors.PaleVioletRed;
                }
                #endregion
                );
                DoughnutChart.ItemsSource = chartData;
                LineChart.ItemsSource = chartData;
                await Task.Delay(3000);
            }
        }
    }
}

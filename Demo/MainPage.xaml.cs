using FourToolkit.Charts.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI;

namespace Demo
{
    public sealed partial class MainPage : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ChartData ChartData
        {
            get { return _chartData; }
            set
            {
                _chartData = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChartData)));
            }
        }
        private ChartData _chartData;

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            Init();
        }

        public async void Init()
        {
            var rnd = new Random();
            while (true)
            {
                var i = 0;
                ChartData = ChartData.PrepareChartData(new List<int> { rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10), rnd.Next(1, 10) }
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
                await Task.Delay(3000);
            }
        }
    }
}

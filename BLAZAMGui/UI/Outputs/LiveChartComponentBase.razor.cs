
using MudBlazor;

namespace BLAZAM.Gui.UI.Outputs
{
    public class LiveChartComponentBase : AppComponentBase
    {
        public InterpolationOption LineInterpolation = InterpolationOption.Straight;
        [Parameter]
        public Func<double> PollFunc { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public int YAxisTicks { get; set; } = 20;

        public ChartOptions ChartOptions => new()
        {
            InterpolationOption = Data.Count > 3 ? InterpolationOption.NaturalSpline : InterpolationOption.Straight,
            YAxisTicks = YAxisTicks,
            YAxisLines = true
        };

        [Parameter]
        public string SeriesName { get; set; }

        /// <summary>
        /// How many points of data to keep in the series
        /// </summary>
        [Parameter]
        public int History { get; set; } = 120;
        private Timer _pollingTimer;
        protected List<DataPoint> Data = new();
        protected List<ChartSeries> DataSeries
        {
            get
            {
                var series = new List<ChartSeries>();
                series.Add(new ChartSeries { Data = Data.OrderBy(d => d.TimeStamp).Select(g => g.Value).ToArray(), Name = "CPU Usage" });
                return series;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _pollingTimer = new Timer(Tick, null, 500, 5000);
        }

        protected virtual void PollData()
        {
            throw new NotImplementedException("PollData must be overriden in a live chart component");
        }



        protected void Tick(object state)
        {
            if (Data.Count >= History)
                Data.Remove(Data.First());
            Task.Run(() =>
            {
                PollData();
            });
        }

        public class DataPoint
        {
            public DateTime TimeStamp { get; set; }

            public double Value { get; set; }
        }
    }
}
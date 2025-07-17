using System.Diagnostics;

namespace Neptune.NsPay.CefTransfer.Common.Classes
{
    public class MyStopWatch
    {
        #region Property
        public Stopwatch Stopwatch { get; set; }
        #endregion

        public MyStopWatch(bool start = false)
        {
            if (start)
                Stopwatch = Stopwatch.StartNew();
        }

        public void Start()
        {
            Stopwatch = Stopwatch.StartNew();
        }

        public void Stop()
        {
            Stopwatch?.Stop();
        }

        public TimeSpan? Elapsed(bool stop = false)
        {
            if (stop)
                Stopwatch?.Stop();

            return Stopwatch?.Elapsed;
        }
    }
}

using System;
using System.Diagnostics;
using Prometheus;
namespace PrometheusClient {
    public class MetricsClient : IDisposable {
        Uri       _uri;
        long      _frameCounter;
        Stopwatch _timer;

        Gauge _frameRate;
        Gauge _frameTime;
        Gauge _systemMemory;
        Gauge _gcMemory;

        readonly MetricPusher _pusher;

        public MetricsClient(Uri uri, string identifier = "unity_default") {
            _timer          = new Stopwatch();
            _uri            = uri;
            _frameCounter   = 0;
            _frameRate = Metrics.CreateGauge("frame_rate", "The current frame rate in FPS");
            _frameTime = Metrics.CreateGauge("frame_time", "The current frame time in milliseconds");
            _gcMemory = Metrics.CreateGauge("gc_memory", "GC Reserved Memory");
            _systemMemory = Metrics.CreateGauge("system_memory", "System Used Memory");
            
            _pusher = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = _uri.AbsoluteUri,
                Job      = identifier
            });

            _pusher.Start();
        }
        
        public MetricsClient(string uri) : this(new Uri(uri)) { }
        
        public double ElapsedMilliseconds => _timer.ElapsedMilliseconds;
        public long FrameCounter => _frameCounter;
        
        public void Dispose() {
            // Set all metrics to zero to avoid stale values
            _frameRate.Set(0);
            _frameTime.Set(0);
            _gcMemory.Set(0);
            _systemMemory.Set(0);
            
            _timer.Stop();
            _pusher.Stop();
        }
        
        /// <summary>
        /// Call this method every time a frame has been processed, but before the wait for the next frame.
        /// </summary>
        /// <param name="time"></param>
        public void SetFrameTime(double time) {
            // Get time since last frame and call SetFrameRate
            SetFrameRate(1.0f/_timer.ElapsedMilliseconds);
            _timer.Restart();
            _frameTime.Set(time);
        }
        
        void SetFrameRate(float fps) {
            _frameCounter++;
            _frameRate.Set(fps);
        }
        
        public void SetGCMemory(long bytes) {
            _gcMemory.Set(bytes);
        }
        
        public void SetSystemMemory(long bytes) {
            _systemMemory.Set(bytes);
        }
    }
}

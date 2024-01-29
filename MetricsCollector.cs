using System.Collections;
using System.Collections.Generic;
using System.Text;
using PrometheusClient;
using Unity.Profiling;
using UnityEngine;

namespace Aaron.Metrics {
    public class MetricsCollector : MonoBehaviour {
        string           _statsText;
        ProfilerRecorder _systemMemoryRecorder;
        ProfilerRecorder _gcMemoryRecorder;
        ProfilerRecorder _mainThreadTimeRecorder;
        MetricsClient    _metricsClient;

        static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0)
                return 0;

            double r = 0;
            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[samplesCount];
                recorder.CopyTo(samples, samplesCount);
                for (var i = 0; i < samplesCount; ++i)
                    r += samples[i].Value;
                r /= samplesCount;
            }

            return r;
        }

        void OnEnable()
        {
            _systemMemoryRecorder   = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory https://docs.unity3d.com/Manual/ProfilerMemory.html#markers.html");
            _gcMemoryRecorder       = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory https://docs.unity3d.com/Manual/ProfilerMemory.html#markers.html");
            _mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            _metricsClient = new MetricsClient("http://localhost:9091/metrics");
            Debug.Log($"Starting Metrics Client on http://localhost:9091/metrics");
        }

        void OnDisable()
        {
            _systemMemoryRecorder.Dispose();
            _gcMemoryRecorder.Dispose();
            _mainThreadTimeRecorder.Dispose();
            _metricsClient.Dispose();
            Debug.Log($"Stopping Metrics Client");
        }

        void Update()
        {
            _metricsClient.SetFrameTime((float)(GetRecorderFrameAverage(_mainThreadTimeRecorder) * 1e-6f));
            _metricsClient.SetGCMemory(_gcMemoryRecorder.LastValue);
            _metricsClient.SetSystemMemory(_systemMemoryRecorder.LastValue);
        }
    }
}

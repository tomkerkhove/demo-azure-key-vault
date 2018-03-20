using System;

namespace TomKerkhove.Samples.KeyVault.API.Providers.Interfaces
{
    public interface ITelemetryProvider
    {
        void IncreaseGauge(string gaugeName);
        void LogException(Exception exception);
        void LogTrace(string traceMessage);
    }
}
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.Verification.Core.Telemetry;

/// <summary>
/// Used for creating traces and metrics for the application.
/// </summary>
/// <remarks>
/// This class is heavily inspired by the OpenTelemetry implementation in App.Lib.Core.
/// Holds all labels, metrics and trace datastructures for OTel based instrumentation.
/// </remarks>
public sealed partial class Telemetry : IDisposable
{
    /// <summary>
    /// A unique name of the application for use in the telemetry context.
    /// </summary>
    public const string AppName = "platform-verification";

    private const string _metricPrefix = "verification";

    private bool _isDisposed;
    private bool _isInitialized;

    private readonly object _lock = new();

    /// <summary>
    /// Gets the ActivitySource for the app.
    /// Using this, you can create traces that are transported to the OpenTelemetry collector.
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Gets the Meter for the app.
    /// Using this, you can create metrics that are transported to the OpenTelemetry collector.
    /// </summary>
    public Meter Meter { get; }

    private FrozenDictionary<string, Counter<long>> _counters;

    /// <summary>
    /// Initializes a new instance of the <see cref="Telemetry"/> class.
    /// </summary>
    public Telemetry()
    {
        ActivitySource = new ActivitySource(AppName);
        Meter = new Meter(AppName);

        _counters = FrozenDictionary<string, Counter<long>>.Empty;

        Init();
    }

    /// <summary>
    /// Initializes the telemetry object.
    /// </summary>
    internal void Init()
    {
        lock (_lock)
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            var counters = new Dictionary<string, Counter<long>>();
            var context = new InitContext(counters);

            InitAddressVerification(context);

            _counters = counters.ToFrozenDictionary();
        }
    }

    private readonly record struct InitContext(
        Dictionary<string, Counter<long>> Counters);

    /// <summary>
    /// Utility methods for creating metrics.
    /// </summary>
    public static class Metrics
    {
        /// <summary>
        /// Creates a name for a metric.
        /// </summary>
        /// <param name="name">Name of the metric, separate words with dot.</param>
        /// <returns>Full metric name</returns>
        public static string CreateName(string name) => $"{_metricPrefix}.{name}";
    }

    private void InitMetricCounter(InitContext context, string name, Action<Counter<long>> init)
    {
        var counter = Meter.CreateCounter<long>(name, unit: null, description: null);
        context.Counters.Add(name, counter);
        init(counter);
    }

    /// <summary>
    /// Disposes the Telemetry object.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ActivitySource?.Dispose();
            Meter?.Dispose();
        }
    }
}

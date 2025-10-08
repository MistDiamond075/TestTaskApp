using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TestTaskApp.sensors;
using TestTaskApp.services;

namespace TestTaskApp.ViewModels
{
    public class SensorsGraph : INotifyPropertyChanged
    {
        private readonly Dictionary<string, List<(DateTime, double)>> _sensorData = new();
        private readonly Timer _cleanupTimer;

        public TimeSpan TimeRange { get; private set; } = TimeSpan.FromMinutes(15);

        public event PropertyChangedEventHandler? PropertyChanged;

        public SensorsGraph(ServiceSensors serviceSensors)
        {
            foreach (var s in serviceSensors.GetSensors())
            {
                _sensorData[s.GetType().Name] = new();
                s.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(ISensor.Value))
                    {
                        lock (_sensorData)
                            _sensorData[s.GetType().Name].Add((DateTime.Now, s.Value));
                        PropertyChanged?.Invoke(this, new(nameof(_sensorData)));
                    }
                };
            }

            _cleanupTimer = new Timer(30_000);
            _cleanupTimer.Elapsed += (_, _) => Cleanup();
            _cleanupTimer.Start();
        }

        private void Cleanup()
        {
            var threshold = DateTime.Now - TimeRange;
            lock (_sensorData)
            {
                foreach (var key in _sensorData.Keys.ToList())
                    _sensorData[key] = _sensorData[key].Where(p => p.Item1 > threshold).ToList();
            }
        }

        public IReadOnlyDictionary<string, List<(DateTime, double)>> GetData() => _sensorData;

        public void SetTimeRange(TimeSpan range)
        {
            TimeRange = range;
            Cleanup();
            PropertyChanged?.Invoke(this, new(nameof(TimeRange)));
        }
    }
}

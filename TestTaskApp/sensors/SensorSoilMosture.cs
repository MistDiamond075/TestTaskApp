using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTaskApp.sensors
{
    internal class SensorSoilMosture : ISensor
    {
        private int value;
        private int trend;
        private DateTime lastUpdated;
        private static uint _globalId = 0;
        private uint id;
        public int Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {
                    updateTrend(this.value, value);
                    this.value = value;
                    LastUpdated = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }
        public int Trend
        {
            get => trend;
            set
            {
                if (this.trend != value)
                {
                    this.trend = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime LastUpdated
        {
            get => lastUpdated; private set
            {
                if (this.lastUpdated != value)
                {
                    lastUpdated = value;
                    OnPropertyChanged();
                }
            }
        }
        public uint Id { get => id;
            private set
            {
                id= value;
                OnPropertyChanged();
            }
        }
        public const string Units = "%";
        public event PropertyChangedEventHandler? PropertyChanged;
        public SensorSoilMosture()
        {
            value = 0;
            trend = 0;
            id = Interlocked.Increment(ref _globalId);
            lastUpdated = DateTime.MinValue;
        }

        private void updateTrend(int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                Trend = newValue > oldValue ? 1 : 0;
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;
            if (obj is ISensor other) return Value.CompareTo(other.Value);
            return Value.CompareTo(obj);
        }
    }
}

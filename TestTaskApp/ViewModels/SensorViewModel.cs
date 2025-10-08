using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.sensors;
using TestTaskApp.utils;

namespace TestTaskApp.ViewModels
{
    public class SensorViewModel : INotifyPropertyChanged
    {
        private ISensor sensor;
        public event PropertyChangedEventHandler? PropertyChanged;
        public SensorViewModel(ISensor sensor)
        {
            this.sensor = sensor;
            if (sensor is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ISensor.Value))
                        OnPropertyChanged(nameof(Value));
                    else if (e.PropertyName == nameof(ISensor.Trend))
                        OnPropertyChanged(nameof(Trend));
                    else if (e.PropertyName == nameof(ISensor.LastUpdated))
                        OnPropertyChanged(nameof(LastUpdated));
                };
            }
        }

        public string DisplayName
        {
            get
            {
                return Parser.ParseSensorName(sensor, true);
            }
        }
        public string Value
        {
            get
            {
                return "Значение: "+ sensor.Value;
            }
        }

        public string Trend
        {
            get
            {
                return "Тренд: "+ (sensor.Trend==1 ? '↑' : '↓');
            }
        }

        public string LastUpdated
        {
            get
            {
                string time = sensor.LastUpdated.CompareTo(DateTime.MinValue)==0 ? "никогда" : sensor.LastUpdated.ToString("dd.MM HH:mm:ss");
                return "Обновилось: "+ sensor.LastUpdated.ToString("dd.MM HH:mm:ss");
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

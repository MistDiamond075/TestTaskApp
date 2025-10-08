using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.sensors;

namespace TestTaskApp.services
{
    public class ServiceSensors
    {
        private readonly List<ISensor> Sensors = [];

        public ServiceSensors() {
            Sensors.Add(new SensorCA());
            Sensors.Add(new SensorHumidify());
            Sensors.Add(new SensorIlluminance());
            Sensors.Add(new SensorSoilMosture());
            Sensors.Add(new SensorSoilMosture());
            Sensors.Add(new SensorTemperature());
        }
        public List<ISensor> GetSensors()
        {
            return Sensors;
        }

        public ISensor? GetSensorByName(string name)
        {
            foreach (var sensor in Sensors)
            {
                var idProp = sensor.GetType().GetProperty("Id");
                object? idValue = idProp?.GetValue(sensor);
                string fullname = idValue != null
                    ? $"{sensor.GetType().Name} {idValue}"
                    : sensor.GetType().Name;
                if (fullname.Equals(name))
                {
                    return sensor;
                }
            }
            return null;
        }

        public List<ISensor>? GetSensorByType(string type,uint? id=null)
        {
            List<ISensor>? list = [];
            foreach (var sensor in Sensors)
            {
                if (id != null) { 
                    if(sensor.GetType().Name.Equals(type) && (uint)sensor.GetType().GetProperty("Id").GetValue(sensor) == id)
                    {
                        list.Add(sensor);
                        return list;
                    }
                }
                else if (sensor.GetType().Name.Equals(type)) list.Add(sensor);
            }
            return list.Count == 0 ? null : list;
        }
    }
}

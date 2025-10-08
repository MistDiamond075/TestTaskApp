using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.sensors;

namespace TestTaskApp.services
{
    public class ServiceSensorSimulation : BackgroundService
    {
        private readonly ServiceSensors srvSensors;
        private readonly ServiceActors srvActors;
        private readonly Random rnd = new();
        private bool isDayTime;
        private readonly TimeSpan dayTimeBound= new(20, 0, 0);
        private readonly ServiceUserRules srvUserRules;

        public ServiceSensorSimulation(ServiceSensors serviceSensors, ServiceActors srvActors, ServiceUserRules srvUserRules)
        {
            this.srvSensors = serviceSensors;
            this.srvActors = srvActors;
            this.srvUserRules = srvUserRules;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                isDayTime = DateTime.Now.TimeOfDay < dayTimeBound;
                foreach (var sensor in srvSensors.GetSensors())
                {
                    sensor.Value=GenerateValue(sensor, SetBounds(sensor));
                   // Console.WriteLine(sensor.GetType().Name+'\t'+sensor.Value);
                }
                SaveCurrentValues();
                await Task.Delay(3000, stoppingToken);
            }
        }

        private KeyValuePair<int, int> SetBounds(ISensor sensor)
        {
            switch (sensor.GetType().Name)
            {
                case "SensorCA": return new KeyValuePair<int, int>(436, 1903);
                case "SensorTemperature": return new KeyValuePair<int, int>(18, 27);
                case "SensorIlluminance": return new KeyValuePair<int, int>(1000, 1940);
                case "SensorHumidify": return new KeyValuePair<int, int>(29, 70);
                case "SensorSoilMosture": return new KeyValuePair<int, int>(20, 57);
                default: return new KeyValuePair<int, int>(0, 100);
            }
        }

        private int GenerateValue(ISensor sensor, KeyValuePair<int, int> bounds)
        {
            int result=sensor.Value;
            int lowerBound=bounds.Key;
            int upperBound=bounds.Value;

            void generate(string ActorType,int[] lowerAdd,int[] upperAdd,int randLower=-2,int randUpper=2,bool checkDayTime=false) {
                uint lowerAddIterator = 0;
                uint upperAddIterator = 0;
                List<IActor>? actors = srvActors.GetActorByType(ActorType);
                if (actors == null)
                {
                    return;
                }
                if (checkDayTime && !isDayTime)
                {
                    if (lowerAddIterator < lowerAdd.Length && upperAddIterator < upperAdd.Length)
                    {
                        lowerBound += lowerAdd[lowerAddIterator];
                        upperBound += upperAdd[upperAddIterator];
                        lowerAddIterator++;
                        upperAddIterator++;
                    }
                }
                foreach (var actor in actors)
                {
                    if (actor is ActorLamp lamp) {
                        result +=actor.State ? lowerAdd[0] : -lowerAdd[0];
                    } else
                    if (actor.State)
                    {
                        if (lowerAddIterator < lowerAdd.Length && upperAddIterator < upperAdd.Length)
                        {
                            lowerBound += lowerAdd[lowerAddIterator];
                            upperBound += upperAdd[upperAddIterator];
                        }
                    }
                }
               // Console.WriteLine(actor.GetType().Name + '\t' + lowerBound+'\t'+upperBound);
                result += rnd.Next(randLower, randUpper);
            }

            switch (sensor.GetType().Name)
            {
                case "SensorTemperature":
                    {
                        generate("ActorVentilation",[2, 3], [-3,-4],-2,2,true);
                        break;
                    }
                case "SensorHumidify":
                    {
                        generate("ActorVentilation", [14], [10],-3,3);
                        break;
                    }
                case "SensorSoilMosture":
                    {
                        generate("ActorPump", [18], [6]);
                        break;
                    }
                case "SensorCA":
                    {
                        generate("ActorVentilation", [-150, -100], [-250, -200], -30, 50, false);
                        break;
                    }
                case "SensorIlluminance":
                    {
                        generate("ActorLamp", [600], [900], -50, 50, true);
                        break;
                    }
            }
            result = Math.Max(result, lowerBound);
            result = Math.Min(result, upperBound);
            return result;
        }

        public void SaveCurrentValues()
        {
            var values = srvSensors.GetSensors()
                .Select(s => new { s.GetType().Name, s.Value, s.Trend, s.LastUpdated })
                .ToList();
            File.WriteAllText("sensor_values.json", JsonSerializer.Serialize(values));
        }
    }
}

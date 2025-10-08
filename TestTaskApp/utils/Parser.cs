using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.sensors;

namespace TestTaskApp.utils
{
    public class Parser
    {

        public static bool ParseExpressionResult(string operation, int compareResult) {
            switch (operation)
            {
                case "=":
                    {
                        return compareResult == 0;
                    }
                case ">":
                    {
                        return compareResult > 0;
                    }
                case "<":
                    {
                        return compareResult < 0;
                    }
                case ">=":
                    {
                        return compareResult >= 0;
                    }
                case "<=":
                    {
                        return compareResult <= 0;
                    }
                default:
                    {
                        return false;
                    }
            }
        }
        public static bool ParseTwoExpressionResult(string operationFirst,int compareResultFirst,string operationSecond,int compareResultSecond,string midOperation="&")
        {
            bool first=ParseExpressionResult(operationFirst,compareResultFirst);
            bool second=ParseExpressionResult(operationSecond,compareResultSecond);
            if (midOperation == "&")
            {
                return first && second;
            }else if(midOperation == "||")
            {
                return first || second;
            }else return false;
        }

        public static string ParseSensorName(ISensor sensor,bool withUnits=false)
        {
            try
            {
                switch (sensor.GetType().Name)
                {
                    case "SensorCA":
                        {
                            return withUnits ? "Углекислый газ (" + SensorCA.Units + ")" : "Углекислый газ";
                        }
                    case "SensorHumidify":
                        {
                            return withUnits ? "Влажность (" + SensorHumidify.Units + ")" : "Влажность";
                        }
                    case "SensorIlluminance":
                        {
                            return withUnits ? "Освещённость (" + SensorIlluminance.Units + ")" : "Освещённость";
                        }
                    case "SensorSoilMosture":
                        {
                            return withUnits ? "Влажность почвы " + ((SensorSoilMosture)sensor).Id + " (" + SensorSoilMosture.Units + ")" :
                                "Влажность почвы " + ((SensorSoilMosture)sensor).Id;
                        }
                    case "SensorTemperature":
                        {
                            return withUnits ? "Температура (" + SensorTemperature.Units + ")" : "Температура";
                        }
                    default:
                        {
                            return "undefined";
                        }
                }
            }
            catch (NullReferenceException e)
            {
                return "";
            }
        }

        public static string ParseSensorType(string name)
        {
            try
            {
                if(name.Contains("Углекислый газ"))
                        {
                            return "SensorCA";
                        }
                if (name.Contains("Освещённость"))
                {
                    return "SensorIlluminance";
                }
                if (name.Contains("Влажность почвы"))
                {
                    return "SensorSoilMosture " + name.Substring(name.LastIndexOf(" " )+1);
                }
                if (name.Contains("Влажность"))
                {
                    return "SensorHumidify";
                }
                if (name.Contains("Температура"))
                {
                    return "SensorTemperature";
                }
                return "undefined";
            }
            catch (NullReferenceException e)
            {
                return "";
            }
        }

        public static string ParseActorName(IActor actor)
        {
            try
            {
                switch (actor.GetType().Name)
                {
                    case "ActorLamp":
                        {
                            return "Лампа " + ((ActorLamp)actor).Id;
                        }
                    case "ActorPump":
                        {
                            return "Насос";
                        }
                    case "ActorVentilation":
                        {
                            return "Вентиляция";
                        }
                    default:
                        {
                            return "undefined";
                        }
                }
            }
            catch (NullReferenceException e)
            {
                return "";
            }
        }
        public static string ParseActorType(string name)
        {
            try
            {
                if (name.Contains("Насос"))
                {
                    return "ActorPump";
                }
                if (name.Contains("Лампа"))
                {
                    return "ActorLamp "+name.Substring(name.IndexOf(" ")+1);
                }
                if (name.Contains("Вентиляция"))
                {
                    return "ActorVentilation";
                }
                return "undefined";
            }
            catch (NullReferenceException e)
            {
                return "";
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.actors;
using TestTaskApp.sensors;

namespace TestTaskApp.services
{
    public class ServiceActors
    {
        private readonly List<IActor> Actors = [];
        public ServiceActors() {
            Actors.Add(new ActorPump());
            Actors.Add(new ActorLamp());
            Actors.Add(new ActorLamp());
            Actors.Add(new ActorVentilation());
        }

        public List<IActor> GetActors()
        {
            return Actors;
        }

        public List<IActor>? GetActorByType(string type, uint? id = null)
        {
            List<IActor>? list = [];
            foreach (var actor in Actors)
            {
                if (id != null)
                {
                    if (actor.GetType().Name.Equals(type) && (uint)actor.GetType().GetProperty("Id").GetValue(actor) == id)
                    {
                        list.Add(actor);
                        return list;
                    }
                }
                else if (actor.GetType().Name.Equals(type)) list.Add(actor);
            }
            return list.Count == 0 ? null : list;
        }

        public IActor? GetActorByName(string name)
        {
            foreach (var actor in Actors)
            {
                var idProp = actor.GetType().GetProperty("Id");
                object? idValue = idProp?.GetValue(actor);
                string fullname = idValue != null
                    ? $"{actor.GetType().Name} {idValue}"
                    : actor.GetType().Name;
                if (fullname.Equals(name))
                {
                    return actor;
                }
            }
            return null;
        }
    }
}

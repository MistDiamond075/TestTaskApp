using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestTaskApp.actors
{
    internal class ActorLamp:IActor, INotifyPropertyChanged
    {
        private bool state;
        private DateTime lastUpdated;
        private uint id;
        public bool State { get => state; 
            set {
                if (state != value)
                {
                    state = value;
                    LastUpdated = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime LastUpdated { get => lastUpdated;
            private set {
                if (lastUpdated != value)
                {
                    lastUpdated = value;
                    OnPropertyChanged();
                }
            } 
        }
        public uint Id { get => id;}
        private static uint _globalId=0;

        public event PropertyChangedEventHandler? PropertyChanged;


        public ActorLamp()
        {
            state = false;
            lastUpdated = DateTime.MinValue;
            id= Interlocked.Increment(ref _globalId);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

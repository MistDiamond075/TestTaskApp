using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TestTaskApp.actors
{
    internal class ActorVentilation : IActor, INotifyPropertyChanged
    {
        private bool state;
        private DateTime lastUpdated;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    LastUpdated = DateTime.Now;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime LastUpdated
        {
            get => lastUpdated;
            private set
            {
                if (lastUpdated != value)
                {
                    lastUpdated = value;
                    OnPropertyChanged();
                }
            }
        }
        public ActorVentilation()
        {
            State = false;
            LastUpdated = DateTime.MinValue;
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

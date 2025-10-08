using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using TestTaskApp.actors;
using TestTaskApp.sensors;
using TestTaskApp.utils;


namespace TestTaskApp.ViewModels
{
    public class ActorViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand ToggleStateCommand { get; }
        private IActor actor;
        public ActorViewModel(IActor actor)
        {
            this.actor = actor;
            ToggleStateCommand = new RelayCommand(ToggleState);
            if (actor is INotifyPropertyChanged npc)
            {
                npc.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(IActor.State))
                        OnPropertyChanged(nameof(State));
                    else if (e.PropertyName == nameof(IActor.LastUpdated))
                        OnPropertyChanged(nameof(LastUpdated));
                };
            }
        }

        public string DisplayName
        {
            get
            {
                return Parser.ParseActorName(actor);
            }
        }

        public string LastUpdated
        {
            get
            {
                string time = actor.LastUpdated.CompareTo(DateTime.MinValue) == 0 ? "никогда" : actor.LastUpdated.ToString("dd.MM HH:mm:ss");
                return "Обновилось: " + actor.LastUpdated.ToString("dd.MM HH:mm:ss");
            }
        }
        public bool State
        {
            get => actor.State; 
            set
            {
                if (actor.State != value)
                {
                    actor.State=value; 
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                }
            }
        }
        private void ToggleState()
        {
            State = !State;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

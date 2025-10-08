using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;
using TestTaskApp.actors;
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
        }

        public string DisplayName
        {
            get
            {
                return Parser.ParseActorName(actor);
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
    }
}

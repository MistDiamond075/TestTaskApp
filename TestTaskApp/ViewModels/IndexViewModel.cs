using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.services;

namespace TestTaskApp.ViewModels
{
    public class IndexViewModel: ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment => "index";
        public ObservableCollection<ActorViewModel> Actors { get; } = [];
        public ObservableCollection<SensorViewModel> Sensors { get; } = [];
        public IScreen HostScreen { get; }

        public ReactiveCommand<Unit, IRoutableViewModel> ButtonRules { get; }

        public IndexViewModel(IScreen screen, ServiceActors srvActors, ServiceSensors srvSensors,ServiceUserRules serviceUserRules,ServiceRulesScheduler srvRulesScheduler)
        {
            HostScreen = screen;
            foreach (var actor in srvActors.GetActors())
            {
                Actors.Add(new ActorViewModel(actor));
            }
            foreach (var sensor in srvSensors.GetSensors())
            {
                Sensors.Add(new SensorViewModel(sensor));
            }
            ButtonRules = ReactiveCommand.CreateFromObservable(
               () => HostScreen.Router.Navigate.Execute(new RulesViewModel(HostScreen,serviceUserRules,srvActors,srvSensors,srvRulesScheduler))
           );
        }
    }
}

using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using TestTaskApp.actors;
using TestTaskApp.services;

namespace TestTaskApp.ViewModels;

public class MainViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new();

    public MainViewModel(ServiceActors srvActors, ServiceSensors srvSensors,ServiceUserRules srvUserRules,ServiceRulesScheduler srvRuleScheduler)
    {
        Router.Navigate.Execute(new IndexViewModel(this, srvActors, srvSensors,srvUserRules,srvRuleScheduler))
          .Subscribe();
    }
}

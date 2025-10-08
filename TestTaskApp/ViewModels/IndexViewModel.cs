using Avalonia.Styling;
using Avalonia.Threading;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using TestTaskApp.sensors;
using TestTaskApp.services;

namespace TestTaskApp.ViewModels
{
    public class IndexViewModel: ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment => "index";
        public ObservableCollection<ActorViewModel> Actors { get; } = [];
        public ObservableCollection<SensorViewModel> Sensors { get; } = [];
        private TimeSpan selectedRange = TimeSpan.FromMinutes(15);
        private string _selectedRangeLabel = "15 минут";
        public ReactiveCommand<Unit, Unit> SwitchThemeCommand { get; }
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
                var svm = new SensorViewModel(sensor);
                sensor.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(ISensor.Value))
                    {
                        Dispatcher.UIThread.Post(() => UpdatePlot(svm));
                    }
                };

                svm.OnPlotAssigned = () => Dispatcher.UIThread.Post(() => UpdatePlot(svm));

                Sensors.Add(svm);
            }
            SwitchThemeCommand = ReactiveCommand.Create(ToggleTheme);
            ButtonRules = ReactiveCommand.CreateFromObservable(
               () => HostScreen.Router.Navigate.Execute(new RulesViewModel(HostScreen,serviceUserRules,srvActors,srvSensors,srvRulesScheduler))
           );
        }

        public string SelectedRangeLabel
        {
            get => _selectedRangeLabel;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedRangeLabel, value);

                SelectedRange = value switch
                {
                    "15 минут" => TimeSpan.FromMinutes(15),
                    "1 час" => TimeSpan.FromHours(1),
                    "24 часа" => TimeSpan.FromHours(24),
                    _ => TimeSpan.FromMinutes(15)
                };

                UpdateAllPlots();
            }
        }

        public TimeSpan SelectedRange
        {
            get => selectedRange;
            private set => this.RaiseAndSetIfChanged(ref selectedRange, value);
        }

        private void ToggleTheme()
        {
            if (Avalonia.Application.Current is App app)
            {
                app.RequestedThemeVariant =
                    app.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Dark
                    ? Avalonia.Styling.ThemeVariant.Light
                    : Avalonia.Styling.ThemeVariant.Dark;
            }
        }

        private void UpdateAllPlots()
        {
            foreach (var vm in Sensors)
                UpdatePlot(vm);
        }

        private void UpdatePlot(SensorViewModel vm)
        {
            if (vm.PlotControl == null)
                return;

            var plt = vm.PlotControl.Plot;
            plt.Clear();

            var data = vm.sensor.History
              .Where(d => d.Item1 >= DateTime.Now - SelectedRange)
              .ToList();
            if (data?.Count > 1)
            {
                double[] xs = data.Select(d => d.Item1.ToOADate()).ToArray();
                int[] ys = data.Select(d => d.Item2).ToArray();
                plt.Add.Scatter(xs, ys, color: Colors.Lime);
                plt.Axes.DateTimeTicksBottom();
            }

            vm.PlotControl.Refresh();
        }
    }
}

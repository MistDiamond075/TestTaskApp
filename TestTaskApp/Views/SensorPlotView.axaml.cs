using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using TestTaskApp.ViewModels;

namespace TestTaskApp.Views;

public partial class SensorPlotView :  ReactiveUserControl<SensorsGraph>
{
    public SensorPlotView()
    {
        InitializeComponent();

        this.AttachedToVisualTree += (_, _) =>
        {
            if (DataContext is SensorViewModel vm)
            {
                vm.PlotControl = Plot;
                Plot.SizeChanged += (_, _) =>
                {
                    if (vm.PlotControl != null)
                        vm.PlotControl.Refresh();
                };
                vm.OnPlotAssigned?.Invoke();
            }
        };

        this.DetachedFromVisualTree += (_, _) =>
        {
            if (DataContext is SensorViewModel vm)
            {
                vm.PlotControl = null;
            }
        };
    }
}

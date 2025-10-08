using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ScottPlot.Avalonia;
using TestTaskApp.ViewModels;

namespace TestTaskApp.Views;

public partial class IndexView :  ReactiveUserControl<IndexViewModel>
{
    public IndexView()
    {
        InitializeComponent();
    }
}

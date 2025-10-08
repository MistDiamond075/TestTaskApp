using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;
using ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;
using TestTaskApp.utils;
using TestTaskApp.ViewModels;
using static TestTaskApp.utils.FilePickerRequests;

namespace TestTaskApp.Views;
public partial class RulesView : ReactiveUserControl<RulesViewModel>
{
    private IDisposable? _openHandlerDisposable;
    private IDisposable? _saveHandlerDisposable;

    public RulesView()
    {
        InitializeComponent();
        DataContextChanged += RulesView_DataContextChanged;
        Unloaded += RulesView_Unloaded;
    }

    private void RulesView_Unloaded(object? sender, RoutedEventArgs e)
    {
        _openHandlerDisposable?.Dispose();
        _saveHandlerDisposable?.Dispose();
        _openHandlerDisposable = null;
        _saveHandlerDisposable = null;
    }

    private void RulesView_DataContextChanged(object? sender, EventArgs e)
    {
        _openHandlerDisposable?.Dispose();
        _saveHandlerDisposable?.Dispose();
        _openHandlerDisposable = null;
        _saveHandlerDisposable = null;

        if (DataContext is RulesViewModel vm)
        {
            _openHandlerDisposable = vm.OpenFileInteraction.RegisterHandler(HandleOpenFilePickerAsync);
            _saveHandlerDisposable = vm.SaveFileInteraction.RegisterHandler(HandleSaveFilePickerAsync);
        }
    }

    private async Task HandleOpenFilePickerAsync(IInteractionContext<OpenFileRequest, string?> interaction)
    {
        var top = this.GetVisualRoot() as TopLevel;
        if (top == null)
        {
            interaction.SetOutput(null);
            return;
        }

        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = interaction.Input.Title,
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON") { Patterns = interaction.Input.Extensions }
            }
        });

        if (files == null || files.Count == 0)
        {
            interaction.SetOutput(null);
            return;
        }

        var localPath = files[0].TryGetLocalPath();
        if (!string.IsNullOrEmpty(localPath))
        {
            interaction.SetOutput(localPath);
            return;
        }

        using var stream = await files[0].OpenReadAsync();
        var tmp = Path.GetTempFileName();
        using (var fs = File.OpenWrite(tmp))
        {
            await stream.CopyToAsync(fs);
        }
        interaction.SetOutput(tmp);
    }

    private async Task HandleSaveFilePickerAsync(IInteractionContext<SaveFileRequest, string?> interaction)
    {
        var top = this.GetVisualRoot() as TopLevel;
        if (top == null)
        {
            interaction.SetOutput(null);
            return;
        }

        var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = interaction.Input.Title,
            SuggestedFileName = interaction.Input.SuggestedFileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON") { Patterns = interaction.Input.Extensions }
            }
        });

        if (file == null)
        {
            interaction.SetOutput(null);
            return;
        }

        var localPath = file.TryGetLocalPath();
        if (!string.IsNullOrEmpty(localPath))
        {
            interaction.SetOutput(localPath);
            return;
        }
        var tmpPath = Path.GetTempFileName();
        interaction.SetOutput(tmpPath);
    }
}

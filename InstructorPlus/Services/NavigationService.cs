using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace InstructorPlus.Services;

public class NavigationService
{
    public async Task ShowError(string title, string message)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var owner = desktop.Windows.LastOrDefault(w => w.IsVisible && w.IsActive)
                        ?? desktop.MainWindow;
        
            if (owner != null)
            {
                var dialog = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.Ok);
                await dialog.ShowWindowDialogAsync(owner);
            }
        }
    }

    public async Task<ButtonResult> ShowConfirm(string title, string message)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var owner = desktop.Windows.LastOrDefault(w => w.IsVisible && w.IsActive)
                        ?? desktop.MainWindow;
        
            if (owner != null)
            {
                var dialog = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNo);
                return await dialog.ShowWindowDialogAsync(owner);
            }
        }
        return ButtonResult.No;
    }
    public Window GetWindow()
    {
        Window? owner = null;
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            owner = desktop.MainWindow;
        }
        return owner;
    }
    
    
    public void CloseWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var windows = desktop.Windows;
            if (windows.Count > 1)
            {
                windows[^1].Close();
            }
        }
    }
    
    public void CloseApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
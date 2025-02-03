using System.Threading.Tasks;
using AngorApp.Core;
using AngorApp.Sections.Shell;
using Avalonia;
using Avalonia.Markup.Xaml;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Serilog;
using Zafiro.Avalonia.Mixins;

namespace AngorApp;

public partial class App : Application
{
    public override void Initialize()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger();
        
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        _ = InitializeAsync();
        base.OnFrameworkInitializationCompleted();
    }
    
    
    private async Task InitializeAsync()
    {
        try 
        {
            this.Connect(() => new MainView(), control => CompositionRoot.CreateMainViewModel(control), () => new MainWindow());
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Failed to create MainViewModel");
        }
    }
}
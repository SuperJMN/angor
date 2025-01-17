using Zafiro.Avalonia.Controls.Navigation;

namespace AngorApp.Sections;

public class NavigationViewModel : ReactiveObject
{
    public NavigationViewModel(Func<INavigator, object> viewModel)
    {
        Navigator = new Navigator();
        Create = ReactiveCommand.Create(() => Navigator.Go(() => viewModel(Navigator)));
    }

    public Navigator Navigator { get; set; }
   
    public ReactiveCommand<Unit, Unit> Create { get; }
}
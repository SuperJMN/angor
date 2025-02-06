using System.Windows.Input;

namespace AngorApp.Sections.Home;

public interface IHomeSectionViewModel
{
    public ReactiveCommand<Unit, Unit> GoToWalletSection { get; }
    public ICommand OpenHub { get; }
}
using System.Windows.Input;

namespace AngorApp.Sections.Home;

public class HomeSectionViewModelDesign : IHomeSectionViewModel
{
    public bool IsWalletSetup { get; set; }
    public ReactiveCommand<Unit, Unit> GoToWalletSection { get; }
    public ICommand OpenHub { get; }
}
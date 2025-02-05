using System.Windows.Input;

namespace AngorApp.Sections.Home;

public class HomeSectionViewModelDesign : IHomeSectionViewModel
{
    public bool IsWalletSetup { get; set; }
    public ICommand GoToWalletSection { get; }
    public ICommand OpenHub { get; }
}
using System.Windows.Input;

namespace AngorApp.Sections.Home;

public interface IHomeSectionViewModel
{
    public bool IsWalletSetup { get; }
    public ICommand GoToWalletSection { get; }
    public ICommand OpenHub { get; }
}
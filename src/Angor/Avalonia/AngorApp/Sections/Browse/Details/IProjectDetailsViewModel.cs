using System.Windows.Input;
using Angor.UI.Model;
using Angor.UI.Model.Projects;

namespace AngorApp.Sections.Browse.Details;

public interface IProjectDetailsViewModel
{
    object Icon { get; }
    object Picture { get; }
    public ICommand Invest { get; }
    public IEnumerable<INostrRelay> Relays { get; }
    public double TotalDays { get; }
    public double TotalInvestment { get; }
    public double CurrentDays { get; }
    public double CurrentInvestment { get; }
    public IProject Project { get; }
}
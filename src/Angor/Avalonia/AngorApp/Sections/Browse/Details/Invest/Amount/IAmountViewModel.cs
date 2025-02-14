using Angor.UI.Model;
using Angor.UI.Model.Projects;
using Zafiro.Avalonia.Controls.Wizards.Builder;

namespace AngorApp.Sections.Browse.Details.Invest.Amount;

public interface IAmountViewModel : IStep
{
    public long? Amount { get; set; }
    IProject Project { get; }
}
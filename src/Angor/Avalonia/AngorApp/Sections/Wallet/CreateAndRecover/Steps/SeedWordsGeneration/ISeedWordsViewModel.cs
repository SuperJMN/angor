using Angor.UI.Model;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia.Controls.Wizards.Builder;

namespace AngorApp.Sections.Wallet.CreateAndRecover.Steps.SeedWordsGeneration;

public interface ISeedWordsViewModel : IStep
{
    SeedWords? Words { get; }
    ReactiveCommand<Unit, Maybe<SeedWords>> GenerateWords { get; }
    bool AreWordsWrittenDown { get; set; }
}
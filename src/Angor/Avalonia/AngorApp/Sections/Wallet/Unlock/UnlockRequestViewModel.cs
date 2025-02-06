using ReactiveUI.SourceGenerators;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace AngorApp.Sections.Wallet.Unlock;

public partial class UnlockRequestViewModel : ReactiveValidationObject
{
    public UnlockRequestViewModel()
    {
        this.ValidationRule(x => x.Password, x => !string.IsNullOrWhiteSpace(x), "Can't be empty");
    }

    [Reactive] private string? password;
}
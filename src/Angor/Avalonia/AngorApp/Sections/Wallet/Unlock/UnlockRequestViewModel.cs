using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace AngorApp.Sections.Wallet.Unlock;

public class UnlockRequestViewModel : ReactiveValidationObject
{
    public UnlockRequestViewModel()
    {
        this.ValidationRule(x => x.Password, x => !string.IsNullOrWhiteSpace(x), "Can't be empty");
    }

    public string? Password { get; set; }
}
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace AngorApp.Sections.Wallet.Unlock;

public class PassphraseRequestViewModel : ReactiveValidationObject
{
    public PassphraseRequestViewModel()
    {
        this.ValidationRule(x => x.Passphrase, x => !string.IsNullOrWhiteSpace(x), "Passphrase is required");
    }

    public string? Passphrase { get; set; }
}
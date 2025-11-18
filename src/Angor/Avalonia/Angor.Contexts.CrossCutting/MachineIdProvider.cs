using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Angor.Contexts.CrossCutting;

public class MachineIdProvider : IMachineIdProvider
{
    private readonly string machineId;

    public MachineIdProvider()
    {
        machineId = GenerateMachineId();
    }

    public string GetMachineId()
    {
        return machineId;
    }

    private static string GenerateMachineId()
    {
        var fingerprintParts = new[]
        {
            Environment.MachineName,
            Environment.UserName,
            Environment.OSVersion.Platform.ToString(),
            RuntimeInformation.OSDescription,
        };

        var fingerprint = string.Join("-", fingerprintParts.Where(part => string.IsNullOrWhiteSpace(part) == false));

        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            return string.Empty;
        }

        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(fingerprint);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}

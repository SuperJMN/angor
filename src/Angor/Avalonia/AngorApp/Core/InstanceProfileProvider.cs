using CSharpFunctionalExtensions;

namespace AngorApp.Core;

public sealed record InstanceProfile
{
    public InstanceProfile(string value)
    {
        Value = Maybe<string>.From(value, string.IsNullOrWhiteSpace)
            .GetValueOrDefault(DefaultPrefix);
    }

    public string Value { get; }

    private const string DefaultPrefix = "Default";
}

public static class InstanceProfileProvider
{
    private static InstanceProfile current = DefaultProfile;

    public static InstanceProfile DefaultProfile { get; } = new InstanceProfile("Default");

    public static InstanceProfile Current => current;

    public static void Configure(string prefix)
    {
        current = new InstanceProfile(prefix);
    }
}

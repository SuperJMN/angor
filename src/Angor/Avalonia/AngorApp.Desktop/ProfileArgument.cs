using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using AngorApp.Core;

namespace AngorApp.Desktop;

public static class ProfileArgument
{
    private const string ProfileOption = "--profile";

    public static string GetProfile(string[] args)
    {
        foreach (var (value, index) in args.Select((value, index) => (value, index)))
        {
            var profile = ParseProfile(args, value, index);
            if (profile.HasValue)
            {
                return profile.Value;
            }
        }

        return InstanceProfileProvider.DefaultProfile.Value;
    }

    private static Maybe<string> ParseProfile(IReadOnlyList<string> args, string value, int index)
    {
        if (!value.StartsWith(ProfileOption, StringComparison.OrdinalIgnoreCase))
        {
            return Maybe<string>.None;
        }

        if (value.Equals(ProfileOption, StringComparison.OrdinalIgnoreCase))
        {
            return NextArgument(args, index);
        }

        var parts = value.Split('=', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return Maybe<string>.None;
        }

        return Maybe<string>.From(parts[1], string.IsNullOrWhiteSpace);
    }

    private static Maybe<string> NextArgument(IReadOnlyList<string> args, int index)
    {
        var nextIndex = index + 1;
        if (nextIndex >= args.Count)
        {
            return Maybe<string>.None;
        }

        var nextValue = args[nextIndex];
        if (nextValue.StartsWith("--", StringComparison.OrdinalIgnoreCase))
        {
            return Maybe<string>.None;
        }

        return Maybe<string>.From(nextValue, string.IsNullOrWhiteSpace);
    }
}

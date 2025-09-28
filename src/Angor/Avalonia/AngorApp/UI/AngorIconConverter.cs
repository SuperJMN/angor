using System;
using System.IO;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Styling;
using Zafiro.Avalonia;

namespace AngorApp.UI;

public class AngorIconConverter : IIconConverter
{
    public static AngorIconConverter Instance { get; } = new();
    
    public Control? Convert(Zafiro.UI.IIcon icon)
    {
        return TryCreateSvgIcon(icon.Source)
            .Match<Control?>(
                themedIcon => themedIcon,
                () => new Projektanker.Icons.Avalonia.Icon { Value = icon.Source });
    }

    private static Maybe<ThemedSvgIcon> TryCreateSvgIcon(string source)
    {
        return Parse(source).Map(resource => new ThemedSvgIcon(resource));
    }

    private static Maybe<SvgResource> Parse(string source)
    {
        var parts = source.Split(new[] { ':' }, 2);
        if (parts.Length != 2 || parts[0] != "svg")
        {
            return Maybe<SvgResource>.None;
        }

        var remainder = parts[1];

        return remainder.StartsWith("/")
            ? FromImplicitPath(remainder)
            : FromExplicitPath(remainder);
    }

    private static Maybe<SvgResource> FromImplicitPath(string remainder)
    {
        var assemblyName = Application.Current?.GetType().Assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return Maybe<SvgResource>.None;
        }

        var resourcePath = remainder.TrimStart('/');
        return SvgResource.Create(assemblyName, resourcePath);
    }

    private static Maybe<SvgResource> FromExplicitPath(string remainder)
    {
        var index = remainder.IndexOf('/');
        if (index <= 0)
        {
            return Maybe<SvgResource>.None;
        }

        var assemblyName = remainder[..index];
        var resourcePath = remainder[(index + 1)..];
        return SvgResource.Create(assemblyName, resourcePath);
    }
}

public class AngorSvgTransformer(string color)
{
    public string Transform(string svgContent)
    {
        var svg = XElement.Parse(svgContent);

        foreach (var element in svg.Descendants())
        {
            var strokeAttribute = element.Attribute("stroke");
            if (strokeAttribute != null)
            {
                strokeAttribute.Value = color;
            }

            var fillAttribute = element.Attribute("fill");
            if (fillAttribute != null)
            {
                fillAttribute.Value = color;
            }
        }

        return svg.ToString();
    }
}

public sealed class ThemedSvgIcon : global::Avalonia.Svg.Skia.Svg
{
    private readonly string svgContent;
    private IDisposable? themeSubscription;

    public ThemedSvgIcon(SvgResource resource)
    {
        svgContent = LoadSvg(resource.AssetUri);
        UpdateIcon(GetCurrentThemeVariant());
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateIcon(GetCurrentThemeVariant());

        themeSubscription = this
            .GetObservable(ThemeVariantScope.ActualThemeVariantProperty)
            .Subscribe(_ => UpdateIcon(GetCurrentThemeVariant()));
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        themeSubscription?.Dispose();
        themeSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateIcon(ThemeVariant themeVariant)
    {
        var color = themeVariant == ThemeVariant.Light ? "Black" : "White";
        var transformer = new AngorSvgTransformer(color);
        Source = transformer.Transform(svgContent);
    }

    private ThemeVariant GetCurrentThemeVariant()
    {
        return ActualThemeVariant ?? Application.Current?.ActualThemeVariant ?? ThemeVariant.Light;
    }

    private static string LoadSvg(Uri uri)
    {
        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

public sealed record SvgResource(Uri BaseUri, string ResourcePath)
{
    public Uri AssetUri => new(BaseUri, ResourcePath);

    public static Maybe<SvgResource> Create(string assemblyName, string resourcePath)
    {
        if (string.IsNullOrWhiteSpace(assemblyName) || string.IsNullOrWhiteSpace(resourcePath))
        {
            return Maybe<SvgResource>.None;
        }

        var baseUri = new Uri($"avares://{assemblyName}");
        return Maybe.From(new SvgResource(baseUri, resourcePath));
    }
}

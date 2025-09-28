using System;
using System.IO;
using System.Reactive.Linq;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using CSharpFunctionalExtensions;
using Zafiro.Avalonia;

namespace AngorApp.UI;

public class AngorIconConverter : IIconConverter
{
    public static AngorIconConverter Instance { get; } = new();
    
    public Control? Convert(Zafiro.UI.IIcon icon)
    {
        return Parse(icon.Source)
            .Map(resource => (Control)new ThemeAwareSvgIcon(resource))
            .GetValueOrDefault(new Projektanker.Icons.Avalonia.Icon { Value = icon.Source });
    }

    private static Result<SvgResource> Parse(string source)
    {
        return Result.Success(source)
            .Ensure(s => s.StartsWith("svg:"), "Not an svg resource")
            .Map(s => s.Split(new[] { ':' }, 2)[1])
            .Bind(CreateDescriptor)
            .Bind(BuildResource);
    }

    private static Result<ResourceDescriptor> CreateDescriptor(string remainder)
    {
        if (remainder.StartsWith("/"))
        {
            var assemblyName = Application.Current!.GetType().Assembly.GetName().Name!;
            return Result.Success(new ResourceDescriptor(assemblyName, remainder.TrimStart('/')));
        }

        var separatorIndex = remainder.IndexOf('/');

        return separatorIndex > 0
            ? Result.Success(new ResourceDescriptor(remainder[..separatorIndex], remainder[(separatorIndex + 1)..]))
            : Result.Failure<ResourceDescriptor>("Invalid svg resource path");
    }

    private static Result<SvgResource> BuildResource(ResourceDescriptor descriptor)
    {
        var baseUri = new Uri($"avares://{descriptor.Assembly}");

        return LoadSvg(baseUri, descriptor.Path)
            .Map(content => new SvgResource(baseUri, content));
    }

    private static Result<string> LoadSvg(Uri baseUri, string resourcePath)
    {
        return Result.Try(() =>
        {
            using var stream = AssetLoader.Open(new Uri(baseUri, resourcePath));
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
    }

    private sealed record ResourceDescriptor(string Assembly, string Path);

    private sealed record SvgResource(Uri BaseUri, string Content);

    private sealed class ThemeAwareSvgIcon : global::Avalonia.Svg.Skia.Svg
    {
        private readonly SvgResource resource;
        private IDisposable? subscription;

        public ThemeAwareSvgIcon(SvgResource resource) : base(resource.BaseUri)
        {
            this.resource = resource;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            UpdateSource();
            subscription ??= this.GetObservable(ThemeVariantScope.ActualThemeVariantProperty)
                .Subscribe(_ => UpdateSource());
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            subscription?.Dispose();
            subscription = null;
        }

        private void UpdateSource()
        {
            var theme = ActualThemeVariant ?? Application.Current?.ActualThemeVariant;
            var color = theme == ThemeVariant.Light ? "Black" : "White";
            Source = AngorSvgTransformer.Transform(resource.Content, color);
        }
    }
}

public static class AngorSvgTransformer
{
    public static string Transform(string svgContent, string color)
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
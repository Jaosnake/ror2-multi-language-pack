using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class RuntimeShape
{
    private static readonly Regex Tags = new Regex(
        @"</?(?:color|b|i|u|s|nobr|size|link|mark|mspace)(?:=[^>]+)?>|<pele-sprite=[A-Za-z0-9+/=]+>|<#[0-9a-fA-F]{6,8}>|<br\s*/?>",
        RegexOptions.IgnoreCase);
    private static readonly Regex Unsupported = new Regex(@"</?[a-zA-Z][\w-]*(?:[=\s>])");

    private struct Span
    {
        internal string Name;
        internal string Value;
        internal int Start;
        internal int End;
    }

    internal static int Run(FontFamily[] available)
    {
        var families = new Dictionary<string, FontFamily>
        {
            { "ar", available.Single(f => f.Source.EndsWith("#Noto Sans Arabic", StringComparison.Ordinal)) },
            { "hi", available.Single(f => f.Source.EndsWith("#Noto Sans Devanagari", StringComparison.Ordinal)) },
            { "th", available.Single(f => f.Source.EndsWith("#Noto Sans Thai Looped", StringComparison.Ordinal)) }
        };
        string request;
        while ((request = Console.ReadLine()) != null)
        {
            var columns = request.Split('\t');
            var id = columns[0];
            try
            {
                var markup = Encoding.UTF8.GetString(Convert.FromBase64String(columns[8]));
                var png = Render(families[columns[1]], columns[1],
                    int.Parse(columns[2], CultureInfo.InvariantCulture),
                    int.Parse(columns[3], CultureInfo.InvariantCulture),
                    int.Parse(columns[4], CultureInfo.InvariantCulture),
                    columns[5] == "1", int.Parse(columns[6], CultureInfo.InvariantCulture), columns[7], markup);
                Console.WriteLine(id + "\t" + (png == null ? "-" : Convert.ToBase64String(png)));
            }
            catch (Exception error)
            {
                Console.WriteLine(id + "\t-");
                Console.Error.WriteLine(error.GetType().Name + ": " + error.Message);
            }
        }
        return 0;
    }

    private static byte[] Render(FontFamily family, string lang, int width, int maxHeight,
        int size, bool wrap, int alignment, string color, string markup)
    {
        if (width < 16 || width > 2048 || maxHeight < 8 || maxHeight > 8192 ||
            size < 8 || size > 220 || markup.Length > 262144 || Unsupported.IsMatch(Tags.Replace(markup, "")))
            return null;

        var plain = new StringBuilder();
        var open = new List<Span>();
        var spans = new List<Span>();
        int offset = 0;
        foreach (Match tag in Tags.Matches(markup))
        {
            plain.Append(markup, offset, tag.Index - offset);
            var raw = tag.Value.Trim('<', '>').TrimEnd('/');
            var value = raw.ToLowerInvariant();
            if (value.StartsWith("pele-sprite=", StringComparison.Ordinal))
            {
                spans.Add(new Span { Name = "pele-sprite", Value = raw.Substring("pele-sprite=".Length), Start = plain.Length, End = plain.Length + 1 });
                plain.Append('\u25a1');
                offset = tag.Index + tag.Length;
                continue;
            }
            if (value.Trim() == "br")
                plain.Append('\n');
            else if (value.StartsWith("/", StringComparison.Ordinal))
            {
                var name = value.Substring(1);
                int index = open.FindLastIndex(s => s.Name == name);
                if (index >= 0)
                {
                    var span = open[index];
                    span.End = plain.Length;
                    spans.Add(span);
                    open.RemoveAt(index);
                }
            }
            else
            {
                var parts = value.Split(new[]{ '=' }, 2);
                var name = value[0] == '#' ? "color" : parts[0];
                var argument = value[0] == '#' ? value : parts.Length == 2 ? parts[1].Trim('"', '\'') : "";
                open.Add(new Span { Name = name, Value = argument, Start = plain.Length });
            }
            offset = tag.Index + tag.Length;
        }
        plain.Append(markup, offset, markup.Length - offset);
        foreach (var entry in open)
        {
            var span = entry;
            span.End = plain.Length;
            spans.Add(span);
        }
        var source = plain.ToString();
        if (string.IsNullOrWhiteSpace(source)) return null;

        var culture = CultureInfo.GetCultureInfo(lang == "ar" ? "ar-SA" : lang == "hi" ? "hi-IN" : "th-TH");
        var face = new Typeface(family, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var breaks = wrap && lang == "th"
            ? ShapeText.WrapThai(source, face, culture, size, width - 8)
            : new List<int>();
        breaks.RemoveAll(c => spans.Any(s => s.Name == "nobr" && s.Start < c && c < s.End));
        var wrapped = new StringBuilder(source);
        for (int i = breaks.Count - 1; i >= 0; --i) wrapped.Insert(breaks[i], '\n');

        var text = new FormattedText(wrapped.ToString(), culture,
            lang == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            face, size, MakeBrush(color, Brushes.White), 1);
        text.TextAlignment = alignment == 1 ? TextAlignment.Center :
            alignment == 2 ? (lang == "ar" ? TextAlignment.Left : TextAlignment.Right) :
                (lang == "ar" ? TextAlignment.Right : TextAlignment.Left);
        text.MaxTextWidth = wrap ? width - 8 : 2040;
        foreach (var span in spans.OrderBy(s => s.Start).ThenByDescending(s => s.End))
        {
            int start = span.Start + breaks.Count(c => c <= span.Start);
            int length = span.End + breaks.Count(c => c < span.End) - start;
            if (length <= 0) continue;
            switch (span.Name)
            {
                case "color": text.SetForegroundBrush(MakeBrush(span.Value, Brushes.White), start, length); break;
                case "b": text.SetFontWeight(FontWeights.Bold, start, length); break;
                case "i": text.SetFontStyle(FontStyles.Italic, start, length); break;
                case "u": text.SetTextDecorations(TextDecorations.Underline, start, length); break;
                case "s": text.SetTextDecorations(TextDecorations.Strikethrough, start, length); break;
                case "pele-sprite": text.SetForegroundBrush(Brushes.Transparent, start, length); break;
                case "size":
                    double factor;
                    if (span.Value.EndsWith("%") && double.TryParse(span.Value.TrimEnd('%'), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out factor) && factor > 0 && factor <= 400)
                        text.SetFontSize(size * factor / 100.0, start, length);
                    break;
            }
        }

        // Compact empty paragraph lines only when a small control would otherwise
        // shrink its entire text. Keep a line break between paragraphs and parse
        // the markup again so spans retain correct offsets.
        if (wrap && maxHeight <= 1000 && text.Height > maxHeight - 6)
        {
            var compact = Regex.Replace(markup, @"(?:\r\n|\r|\n){2,}", "\n");
            if (compact != markup)
                return Render(family, lang, width, maxHeight, size, wrap, alignment, color, compact);
        }

        // When text needs to shrink, reduce its font and reflow at the original
        // width. Scaling a finished multiline bitmap also shrinks its width and
        // leaves half of a skill panel unused. Thai uses precomputed break points
        // and retains the original path until it can be resegmented per size.
        if (wrap && lang != "th")
        {
            double fittedSize = size;
            for (int attempt = 0; attempt < 8; ++attempt)
            {
                double fit = Math.Min(1, Math.Min((width - 8) / Math.Max(1, text.Width),
                    (maxHeight - 6) / Math.Max(1, text.Height)));
                if (fit >= 0.995) break;
                fittedSize = Math.Max(4, fittedSize * Math.Max(0.65, fit));
                text.SetFontSize(fittedSize);
                foreach (var span in spans.Where(s => s.Name == "size"))
                {
                    double factor;
                    int start = span.Start + breaks.Count(c => c <= span.Start);
                    int length = span.End + breaks.Count(c => c < span.End) - start;
                    if (length > 0 && span.Value.EndsWith("%") &&
                        double.TryParse(span.Value.TrimEnd('%'), NumberStyles.Float,
                            CultureInfo.InvariantCulture, out factor) && factor > 0 && factor <= 400)
                        text.SetFontSize(fittedSize * factor / 100.0, start, length);
                }
                if (fittedSize <= 4) break;
            }
        }
        double scale = Math.Min(1, Math.Min((width - 8) / Math.Max(1, text.Width),
            (maxHeight - 6) / Math.Max(1, text.Height)));
        // Alignment needs the same width used for line breaking. Padding keeps marks at the edges visible.
        double canvasWidth = wrap ? width - 8 : Math.Ceiling(text.WidthIncludingTrailingWhitespace);
        if (!wrap) text.MaxTextWidth = Math.Max(1, canvasWidth);
        int height = (int)Math.Ceiling(text.Height * scale) + 6;
        int bitmapWidth = Math.Min(width, (int)Math.Ceiling(canvasWidth * scale) + 8);
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.PushTransform(new TranslateTransform(4, 3));
            context.PushTransform(new ScaleTransform(scale, scale));
            foreach (var span in spans.Where(s => s.Name == "mark"))
            {
                int start = span.Start + breaks.Count(c => c <= span.Start);
                int length = span.End + breaks.Count(c => c < span.End) - start;
                if (length <= 0) continue;
                var geometry = text.BuildHighlightGeometry(new Point(0, 0), start, length);
                if (geometry != null) context.DrawGeometry(MakeBrush(span.Value, Brushes.Transparent), null, geometry);
            }
            context.DrawText(text, new Point(0, 0));
            foreach (var span in spans.Where(s => s.Name == "pele-sprite"))
            {
                int start = span.Start + breaks.Count(c => c <= span.Start);
                var geometry = text.BuildHighlightGeometry(new Point(0, 0), start, 1);
                if (geometry == null) continue;
                var bitmapImage = new BitmapImage();
                using (var input = new MemoryStream(Convert.FromBase64String(span.Value)))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = input;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
                var bounds = geometry.Bounds;
                double side = Math.Min(bounds.Width, bounds.Height);
                context.DrawImage(bitmapImage, new Rect(bounds.X + (bounds.Width - side) / 2, bounds.Y + (bounds.Height - side) / 2, side, side));
            }
        }
        var bitmap = new RenderTargetBitmap(bitmapWidth, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var png = new PngBitmapEncoder();
        png.Frames.Add(BitmapFrame.Create(bitmap));
        using (var stream = new MemoryStream())
        {
            png.Save(stream);
            return stream.ToArray();
        }
    }

    private static Brush MakeBrush(string value, Brush fallback)
    {
        if (Regex.IsMatch(value, @"^#[0-9a-fA-F]{8}$"))
            value = "#" + value.Substring(7, 2) + value.Substring(1, 6); // TMP RGBA, WPF ARGB
        try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value)); }
        catch (FormatException) { return fallback; }
        catch (NotSupportedException) { return fallback; }
    }
}

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class ShapeText
{
    private static bool warnedThaiFallback;
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        try
        {
            string root = Path.GetDirectoryName(typeof(ShapeText).Assembly.Location);
            var fonts = Path.Combine(root, "fonts") + Path.DirectorySeparatorChar;
            var baseUri = new Uri(fonts);
            var available = Fonts.GetFontFamilies(baseUri, "./").ToArray();
            if (args.Length == 1 && args[0] == "--worker")
                return RuntimeShape.Run(available);
            foreach (string row in File.ReadAllLines(args[0], Encoding.UTF8))
            {
                if (row.Length == 0) continue;
                string[] cols = row.Split('\t');
                var name = cols[0] == "hi" ? "Noto Sans Devanagari" :
                    cols[0] == "ar" ? "Noto Sans Arabic" : "Noto Sans Thai Looped";
                var font = available.Single(f => f.Source.EndsWith("#" + name, StringComparison.Ordinal));
                Draw(new Typeface(font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                     cols[0], int.Parse(cols[1]), int.Parse(cols[2]),
                     Encoding.UTF8.GetString(Convert.FromBase64String(cols[3])), cols[4]);
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ubrk_open(int type, [MarshalAs(UnmanagedType.LPStr)] string locale,
        IntPtr text, int textLength, ref int status);

    [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern int ubrk_next(IntPtr iterator);

    [DllImport("icu.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern void ubrk_close(IntPtr iterator);

    internal static List<int> WrapThai(string plain, Typeface font, CultureInfo culture, int size, int width)
    {
        var candidates = new List<int>();
        var protectedBreaks = new HashSet<int>();
        foreach (Match term in Regex.Matches(plain, @"ความเสียหาย(?:\u00A0[0-9]+%)?|เสียหาย|เพิ่มขึ้น|เดสก์ท็อป|Steamworks|VOL-T|[0-9]+%"))
            for (int i = term.Index+1; i < term.Index+term.Length; ++i) protectedBreaks.Add(i);
        var buffer = Marshal.StringToHGlobalUni(plain);
        IntPtr iterator = IntPtr.Zero;
        try
        {
            int status = 0;
            iterator = ubrk_open(2, "th", buffer, plain.Length, ref status);
            if (status > 0 || iterator == IntPtr.Zero)
                AddFallbackThaiBreaks(plain, candidates, protectedBreaks);
            else for (int at = ubrk_next(iterator); at >= 0; at = ubrk_next(iterator))
            {
                if (at != 0 && at < plain.Length &&
                    (protectedBreaks.Contains(at) || plain[at] == '%' || plain[at] == '-' || plain[at - 1] == '-'))
                    continue;
                candidates.Add(at);
            }
        }
        catch (DllNotFoundException)
        {
            AddFallbackThaiBreaks(plain, candidates, protectedBreaks);
        }
        catch (EntryPointNotFoundException)
        {
            AddFallbackThaiBreaks(plain, candidates, protectedBreaks);
        }
        finally
        {
            if (iterator != IntPtr.Zero) ubrk_close(iterator);
            Marshal.FreeHGlobal(buffer);
        }
        var breaks = new List<int>();
        int start = 0, previous = 0;
        foreach (int at in candidates)
        {
            var part = new FormattedText(plain.Substring(start, at-start).Trim(), culture,
                FlowDirection.LeftToRight, font, size, Brushes.White, 1.0);
            if (part.Width > width && previous > start)
            {
                breaks.Add(previous);
                start = previous;
            }
            previous = at;
        }
        return breaks;
    }

    // ICU provides dictionary-based Thai line boundaries. If the Windows ICU
    // library is unavailable, retain readable text and avoid splitting Unicode
    // text elements; word boundaries may be less natural in this fallback.
    private static void AddFallbackThaiBreaks(string plain, List<int> candidates, HashSet<int> protectedBreaks)
    {
        if (!warnedThaiFallback)
        {
            Console.Error.WriteLine("ICU indisponivel: usando quebra alternativa para texto tailandes.");
            warnedThaiFallback = true;
        }
        candidates.Clear();
        foreach (int at in StringInfo.ParseCombiningCharacters(plain))
            if (at > 0 && !protectedBreaks.Contains(at) && plain[at] != '%' &&
                plain[at] != '-' && plain[at - 1] != '-')
                candidates.Add(at);
        candidates.Add(plain.Length);
    }

    private static void Draw(Typeface typeface, string lang, int width, int size,
                             string markup, string output)
    {
        var regex = new Regex("<color=(#[0-9A-Fa-f]{6})>(.*?)</color>");
        var plain = regex.Replace(markup, "$2");
        string locale = lang == "ar" ? "ar-SA" : lang == "hi" ? "hi-IN" : "th-TH";
        var culture = CultureInfo.GetCultureInfo(locale);
        var breaks = lang == "th" ? WrapThai(plain, typeface, culture, size, width - 28) : new List<int>();
        var wrapped = new StringBuilder(plain);
        for (int i = breaks.Count - 1; i >= 0; --i) wrapped.Insert(breaks[i], '\n');
        if (lang == "th") Console.WriteLine("Thai line boundaries: " + wrapped.ToString().Replace("\n"," | "));
        var text = new FormattedText(wrapped.ToString(), culture,
            lang == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
            typeface, size, Brushes.White, 1.0);
        text.MaxTextWidth = width - 20;
        text.Trimming = TextTrimming.None;
        if (output.EndsWith("-0.png") || output.EndsWith("-1.png") || output.EndsWith("-2.png"))
            text.TextAlignment = TextAlignment.Center;
        int removed = 0;
        foreach (Match match in regex.Matches(markup))
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(match.Groups[1].Value));
            int first = match.Index - removed;
            int last = first + match.Groups[2].Length;
            int start = first + breaks.Count(x => x <= first);
            int length = last + breaks.Count(x => x < last) - start;
            text.SetForegroundBrush(brush, start, length);
            removed += match.Length - match.Groups[2].Length;
        }
        int height = (int)Math.Ceiling(text.Height) + 12;
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
            context.DrawText(text, new Point(10, 4));
        var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(visual);
        var png = new PngBitmapEncoder();
        png.Frames.Add(BitmapFrame.Create(bmp));
        using (var stream = File.Create(output)) png.Save(stream);
        Console.WriteLine("{0} {1}x{2} {3}",lang,width,height,typeface.FontFamily.Source);
    }
}

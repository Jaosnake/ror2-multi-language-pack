using System.IO;
using Path = System.IO.Path;
using R2API;
using UnityEngine;

public sealed partial class LanguagePreview
{
    private void LoadSelectorFlags(string assets)
    {
        LoadFlag(assets, "th");
        LoadFlag(assets, "hi");
        LoadFlag(assets, "ar");
        LoadFlag(assets, "vi");
        LoadFlag(assets, "el");
    }

    private void LoadFlag(string assets, string lang)
    {
        var file = Path.Combine(assets, "flag-" + lang + ".png");
        if (!File.Exists(file))
        {
            Logger.LogWarning("Bandeira ausente: " + lang);
            return;
        }

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(file), true))
        {
            Logger.LogWarning("Bandeira invalida: " + lang);
            Destroy(texture);
            return;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        CustomLanguageRegistration.RegisterSelectorFlag(lang, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect));
    }
}

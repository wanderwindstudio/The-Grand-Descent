using UnityEditor;

// Every image dropped into Assets/Art gets pixel-art import settings automatically:
// point filtering, no compression, no mipmaps, alpha as transparency. The source files themselves are never changed.
// - Player hands: max 512. FX and HUD icons: max 256.
// - Menu and story art: UI sprites. Backgrounds max 2048, portraits and UI pieces max 1024.
//   The trimmed, 9-sliced menu pieces in Art/Generated/UI are made by Grand Descent > Prepare Menu Art.
// - World textures: max 64, tiling (Repeat). Props and decals: max 256.
public class ArtImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Art/")) return;

        var importer = (TextureImporter)assetImporter;
        importer.filterMode = UnityEngine.FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;

        string file = System.IO.Path.GetFileName(assetPath);
        bool menu = assetPath.StartsWith("Assets/Art/UI/Menu/") || assetPath.StartsWith("Assets/Art/Generated/UI/");
        bool story = assetPath.StartsWith("Assets/Art/Story/");

        if (menu || story)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            bool background = file.StartsWith("bg_") || file.StartsWith("ui_menu_bg");
            importer.maxTextureSize = background ? 2048 : 1024;
            return;
        }

        importer.textureType = TextureImporterType.Default;
        if (assetPath.StartsWith("Assets/Art/Player/")) importer.maxTextureSize = 512;
        else if (assetPath.StartsWith("Assets/Art/FX/") || assetPath.StartsWith("Assets/Art/UI/")) importer.maxTextureSize = 256;
        else if (assetPath.StartsWith("Assets/Art/World/"))
        {
            importer.maxTextureSize = 64;
            importer.wrapMode = UnityEngine.TextureWrapMode.Repeat;
        }
        else if (assetPath.StartsWith("Assets/Art/Props/") || assetPath.StartsWith("Assets/Art/Decals/")) importer.maxTextureSize = 256;
    }
}

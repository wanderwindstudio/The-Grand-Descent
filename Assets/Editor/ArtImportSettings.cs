using UnityEditor;

// Every image dropped into Assets/Art gets pixel-art import settings automatically:
// point filtering, no compression, no mipmaps, clamped edges, alpha as transparency.
// Hands are capped at 512 px and effects at 256 px; the source files themselves are never changed.
public class ArtImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Art/")) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.filterMode = UnityEngine.FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.npotScale = TextureImporterNPOTScale.None;

        if (assetPath.StartsWith("Assets/Art/Player/")) importer.maxTextureSize = 512;
        else if (assetPath.StartsWith("Assets/Art/FX/")) importer.maxTextureSize = 256;
    }
}

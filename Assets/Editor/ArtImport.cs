using UnityEditor;
using UnityEngine;

// 像素资产导入设置：点过滤、不压缩、按目录分配轴心
class ArtImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/Art/")) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.spritePixelsPerUnit = 100f;
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        if (assetPath.Contains("/Rooms/")) pivot = new Vector2(0f, 0f);
        else if (assetPath.Contains("/Sprites/")) pivot = new Vector2(0.5f, 0f);
        else if (assetPath.EndsWith("beam.png")) pivot = new Vector2(0f, 0.5f);
        else if (assetPath.EndsWith("cone.png")) pivot = new Vector2(0.5f, 1f);
        ti.spritePivot = pivot;
    }
}

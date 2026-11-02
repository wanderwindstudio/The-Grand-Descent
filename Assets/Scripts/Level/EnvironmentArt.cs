using UnityEngine;

// Dungeon textures, prop sprites and decals in one place (Assets/Settings/EnvironmentArt.asset).
// Filled in by Grand Descent > Create Prototype Scene. Anything missing falls back to the gray-box look.
[CreateAssetMenu(fileName = "EnvironmentArt", menuName = "Grand Descent/Environment Art")]
public class EnvironmentArt : ScriptableObject
{
    public static EnvironmentArt Current;

    [Header("Textures (64 x 64, one repeat per metre)")]
    public Texture2D stoneFloor;
    public Texture2D stoneWall;
    public Texture2D brickWall;
    public Texture2D woodPlanks;
    public Texture2D stoneCeiling;

    [Header("Props (billboards, bottom-centre pivot)")]
    public Texture2D torchWall;
    public Texture2D barrel;
    public Texture2D barrelExplosive;
    public Texture2D crate;
    public Texture2D bonePile;
    public Texture2D candles;
    public Texture2D mopBucket;
    public Texture2D wetFloorSign;
    public Texture2D pipeShrine;
    public Texture2D portalExtract;
    public Texture2D portalDescend;

    [Header("Decals")]
    public Texture2D bannerCrown;
    public Texture2D posterSponsor;
    public Texture2D crack;
    public Texture2D scorch;
}

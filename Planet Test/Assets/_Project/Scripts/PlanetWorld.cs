using UnityEngine;

namespace TestPlanet
{
    public interface IPlanetFeature { void Generate(PlanetWorld world); }

    [DefaultExecutionOrder(-100)]
    public sealed class PlanetWorld : MonoBehaviour
    {
        [SerializeField, Tooltip("Completed feature stage (1–6). Also used for staged screenshot review.")] int stage = 1;
        [SerializeField, Tooltip("Procedural terrain settings.")] PlanetTerrain terrain;
        public PlanetTerrain Terrain => terrain;
        public int Stage { get => stage; set => stage = value; }
        public void Configure(PlanetTerrain value) { terrain = value; }
        void Awake()
        {
            Application.targetFrameRate = 60;
            Application.runInBackground = true;
            terrain.Generate();
            foreach (var feature in GetComponents<IPlanetFeature>()) feature.Generate(this);
        }
    }
}

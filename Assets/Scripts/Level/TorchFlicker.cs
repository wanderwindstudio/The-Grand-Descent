using UnityEngine;

// A torch light that flickers around the tuned torch intensity.
[RequireComponent(typeof(Light))]
public class TorchFlicker : MonoBehaviour
{
    Light torch;
    float seed;

    void Awake()
    {
        torch = GetComponent<Light>();
        seed = Random.value * 100f;
    }

    void Update()
    {
        var t = Game.Tuning;
        float noise = Mathf.PerlinNoise(seed, Time.time * t.torchFlickerSpeed) - 0.5f; // -0.5 .. 0.5
        torch.intensity = t.torchIntensity * (1f + noise * 2f * t.torchFlicker);
    }
}

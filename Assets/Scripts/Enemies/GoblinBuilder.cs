using UnityEngine;
using UnityEngine.AI;

// Assembles a goblin intern out of primitives.
public static class GoblinBuilder
{
    public static GoblinEnemy Create()
    {
        var t = Game.Tuning;
        var root = new GameObject("Goblin");
        root.SetActive(false); // the pool activates it once it's placed on the NavMesh

        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.up * (t.goblinHeight / 2f);
        body.transform.localScale = new Vector3(t.goblinWidth, t.goblinHeight / 2f, t.goblinWidth);
        body.GetComponent<Renderer>().sharedMaterial = Mats.New(Palette.Goblin, emissive: true);

        // Eyes show which way it faces.
        var eyes = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eyes.name = "Eyes";
        Object.DestroyImmediate(eyes.GetComponent<Collider>());
        eyes.transform.SetParent(body.transform, false);
        eyes.transform.localPosition = new Vector3(0f, 0.55f, 0.45f);
        eyes.transform.localScale = new Vector3(0.7f, 0.15f, 0.2f);
        eyes.GetComponent<Renderer>().sharedMaterial = Mats.Solid(Palette.GoblinEyes);

        var agent = root.AddComponent<NavMeshAgent>();
        agent.height = t.goblinHeight;
        agent.radius = t.goblinWidth / 2f;
        agent.angularSpeed = 720f;
        agent.autoBraking = true;

        var audio = root.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 1f;
        audio.rolloffMode = AudioRolloffMode.Linear;
        audio.minDistance = 3f;
        audio.spread = 0f;       // sharp left/right panning
        audio.dopplerLevel = 0f; // no pitch smear when you turn

        root.AddComponent<GoblinTraversal>();
        root.AddComponent<GoblinAirborne>();
        var goblin = root.AddComponent<GoblinEnemy>();
        root.AddComponent<GoblinSprite>();
        return goblin;
    }
}

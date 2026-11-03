using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TestPlanet.Editor
{
    // This runner lives only in the Editor. A shipped game never teleports or auto-exits.
    [InitializeOnLoad]
    public static class PlanetReview
    {
        static PlanetReview() { EditorApplication.playModeStateChanged += Changed; }
        static void Changed(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || SessionState.GetInt("PlanetReviewStage", 0) == 0) return;
            new GameObject("Temporary screenshot review").AddComponent<ReviewRunner>();
        }
    }
    public sealed class ReviewRunner : MonoBehaviour
    {
        IEnumerator Start()
        {
            int stage = SessionState.GetInt("PlanetReviewStage", 1);
            yield return new WaitForSeconds(4);
            var camera = Camera.main;
            var player = FindFirstObjectByType<PlanetPlayer>();
            if (player != null) { player.ReviewControl = true; camera.transform.SetParent(null); PlanetPlayer.LockCursor(false); }
            camera.transform.position = new Vector3(-430, 340, -430); camera.transform.LookAt(new Vector3(0, 18, 0));
            yield return new WaitForSeconds(1); yield return Capture(camera, $"stage{stage}_aerial");
            if (stage >= 2) { yield return new WaitForSeconds(1); yield return Capture(camera, $"stage{stage}_ocean_motion"); }
            var terrain = FindFirstObjectByType<PlanetTerrain>();
            camera.enabled = false;
            var fresh = new GameObject("Shore review camera").AddComponent<Camera>(); fresh.CopyFrom(camera);
            camera = fresh; camera.enabled = true; camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            camera.transform.position = new Vector3(-135, terrain.Height(-135, -105) + 1.8f, -105);
            if (Physics.Raycast(new Vector3(-135, 250, -105), Vector3.down, out RaycastHit shore, 400)) camera.transform.position = shore.point + Vector3.up * 1.8f;
            camera.transform.LookAt(new Vector3(65, 65, 52));
            Debug.Log($"REVIEW CAMERA {camera.transform.position} forward={camera.transform.forward} summit={terrain.Height(65,52)}");
            yield return new WaitForSeconds(1); yield return Capture(camera, $"stage{stage}_shoreline");
            if (stage >= 4)
            {
                yield return WalkReview(player, terrain);
                camera.transform.position = player.Position + Vector3.up * 1.8f;
                camera.transform.LookAt(new Vector3(-80,35,-140));
                yield return Capture(camera, $"stage{stage}_summit");
            }
            Debug.Log($"PLANET_REVIEW_PASS stage={stage} triangles={terrain.TriangleCount} colliders={FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Length}");
            SessionState.SetInt("PlanetReviewStage", 0);
            EditorApplication.ExitPlaymode();
        }
        static IEnumerator WalkReview(PlanetPlayer player, PlanetTerrain terrain)
        {
            player.Respawn();
            for(int i=0;i<20;i++) { player.Move(Vector3.zero,false,false,.02f); yield return null; }
            float startY=player.Position.y; bool jumped=false;
            for(int i=0;i<35;i++)
            {
                player.Move(Vector3.zero,false,i==0,.02f);
                if(player.Position.y>startY+.5f) jumped=true;
                yield return null;
            }
            Vector3 target=new Vector3(terrain.VolcanoCentre.x-24,0,terrain.VolcanoCentre.y-12);
            float elapsed=0; int rescues=player.Respawns;
            while(elapsed<65)
            {
                Vector3 delta=target-player.Position;delta.y=0;
                if(delta.magnitude<4) break;
                player.Move(delta.normalized,true,false,.02f);elapsed+=.02f;yield return null;
            }
            bool climbed=player.Position.y>100 && player.Respawns==rescues;
            Debug.Log($"PLAYER_REVIEW jump={jumped} craterReached={climbed} position={player.Position} simulatedSeconds={elapsed:F1}");
            if(!jumped || !climbed) Debug.LogError("PLAYER_REVIEW_FAIL: jump or shore-to-crater climb");
        }
        public static IEnumerator Capture(Camera camera, string name)
        {
            // Render explicitly at 1080p; the editor's docked Game view size does not change the evidence.
            var target = new RenderTexture(1920, 1080, 24);
            var previous = camera.targetTexture; camera.targetTexture = target; camera.aspect = 1920f / 1080f;
            camera.ResetWorldToCameraMatrix(); camera.ResetProjectionMatrix();
            // Let URP render normal frames; manually calling Camera.Render can reuse stale view data.
            yield return null; yield return new WaitForEndOfFrame();
            var old = RenderTexture.active; RenderTexture.active = target;
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); image.Apply();
            File.WriteAllBytes($"Assets/_Project/Screenshots/{name}.png", image.EncodeToPNG());
            camera.targetTexture = previous; RenderTexture.active = old;
            Destroy(image); target.Release(); Destroy(target);
        }
    }
}

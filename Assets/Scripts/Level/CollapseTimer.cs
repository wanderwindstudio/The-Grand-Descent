using UnityEngine;

// On deep floors, opening the portals starts a countdown. Reach a portal before it hits zero or die.
// The rumble and camera shake grow as time runs out.
public class CollapseTimer : MonoBehaviour
{
    public bool Active { get; private set; }
    public float TimeLeft { get; private set; }
    public float Duration { get; private set; }

    AudioSource rumble;
    Vector3 headRest;

    void Awake()
    {
        rumble = gameObject.AddComponent<AudioSource>();
        rumble.clip = ToneGenerator.Rumble("CollapseRumble");
        rumble.loop = true;
        rumble.playOnAwake = false;
        rumble.spatialBlend = 0f;
    }

    // Seconds the player gets on the current floor (only meaningful on collapsing floors).
    public static float TimeForFloor(int floor)
    {
        var t = Game.Tuning;
        float time = t.collapseTime - t.collapseTimeReductionPerFloor * (floor - t.collapseStartFloor);
        return Mathf.Max(t.collapseTimeMin, time);
    }

    public void Begin()
    {
        Duration = TimeForFloor(Game.Floor);
        TimeLeft = Duration;
        Active = true;
        headRest = Game.Camera.transform.localPosition;
        rumble.volume = 0f;
        rumble.Play();
    }

    public void Stop()
    {
        if (!Active) return;
        Active = false;
        rumble.Stop();
        Game.Camera.transform.localPosition = headRest;
    }

    // 0 when it starts, 1 at the moment of collapse.
    public float Danger01 => Active ? 1f - TimeLeft / Duration : 0f;

    void Update()
    {
        if (!Active) return;
        TimeLeft -= Time.deltaTime;
        rumble.volume = Game.Tuning.collapseRumbleVolume * Mathf.Lerp(0.2f, 1f, Danger01);
        rumble.pitch = Time.timeScale == 0f ? 0f : 1f; // silent pause during level-ups

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            Stop();
            if (Game.GodMode) Hud.ShowMessage("God mode held the ceiling up");
            else
            {
                RunManager.Instance.MarkCollapseDeath(); // before Kill, so the run ends as "collapsed"
                Game.PlayerHealth.Kill();
            }
        }
    }

    void LateUpdate()
    {
        if (!Active || Time.timeScale == 0f) return;
        float k = Danger01 * Danger01; // shake mostly near the end
        Game.Camera.transform.localPosition = headRest + Random.insideUnitSphere * (Game.Tuning.collapseShakeMax * k);
    }
}

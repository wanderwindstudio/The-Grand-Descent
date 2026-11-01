using UnityEngine;

// The "bip-BIP" you hear when a goblin you can't see starts its attack.
// Closer goblins sound higher and louder; a global cooldown stops big waves from spamming it.
public static class BehindYouCue
{
    static AudioClip clip;
    static float clipPitchHz;
    static float lastPlayTime = -999f;

    public static void TryPlay(AudioSource source, Vector3 enemyPosition)
    {
        var t = Game.Tuning;
        float distance = Vector3.Distance(enemyPosition, Game.Player.position);
        if (distance > t.behindCueMaxDistance) return;
        if (Time.time - lastPlayTime < t.behindCueMinInterval) return;
        lastPlayTime = Time.time;

        if (clip == null || clipPitchHz != t.behindCuePitchHz)
        {
            clip = ToneGenerator.WarningChirp("BehindYouCue", t.behindCuePitchHz);
            clipPitchHz = t.behindCuePitchHz;
        }

        float closeness = 1f - distance / t.behindCueMaxDistance;
        source.maxDistance = t.behindCueMaxDistance;
        source.pitch = 1f + t.behindCueClosePitchBoost * closeness;
        source.PlayOneShot(clip, t.behindCueVolume);
    }
}

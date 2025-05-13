// portions of this file were generated using GitHub Copilot
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Main { get; private set; }

    [field: Header("FMOD EVENTS")]
    [field: SerializeField] public List<EventReference> PLAYER_SFX { get; private set; }
    [field: SerializeField] public List<EventReference> ENEMY_SFX { get; private set; }
    public EnemyAudio enemyAudio;
    [SerializeField] float sfxAudio = 0.5f;
    private List<EventInstance> allSFX = new List<EventInstance>();

    private void Awake()
    {
        if (Main != null && Main != this) { Destroy(this); return; }
        else Main = this;
        enemyAudio = new();
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos = default)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference sound)
    {
        EventInstance instance = RuntimeManager.CreateInstance(sound);
        instance.setVolume(sfxAudio);
        return instance;
    }

    public static bool IsAudioEventPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != PLAYBACK_STATE.STOPPED;
    }
}

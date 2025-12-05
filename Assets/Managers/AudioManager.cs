// portions of this file were generated using GitHub Copilot
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Singleton { get; private set; }


    [FMODUnity.BankRef]
    public List<string> FMOD_Banks = new List<string>();

    [field: Header("FMOD EVENTS")]
    [field: SerializeField] public List<EventReference> PLAYER_SFX { get; private set; }
    [field: SerializeField] public List<EventReference> ENEMY_SFX { get; private set; }
    [field: SerializeField] public List<EventReference> MUSIC { get; private set; }

    public EnemyAudio enemyAudio;
    [SerializeField] float sfxAudio = 1;
    private List<EventInstance> allSFX = new List<EventInstance>();
    internal MusicHandler _MusicHandler;

    private void Awake()
    {
        if (Singleton != null && Singleton != this) { Destroy(this); return; }
        else Singleton = this;
        enemyAudio = new();
        _MusicHandler = new(this);
        if (SettingsManager.Singleton != null && SettingsManager.Singleton.settingsData != null && SettingsManager.Singleton.SfxVolume != sfxAudio)
        {
            sfxAudio = SettingsManager.Singleton.SfxVolume;
        }
        else
        {
            sfxAudio = 0.5f;
        }
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos = default)
    {
        EventInstance instance = CreateInstance(sound);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
        instance.start();
        instance.release();
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

    public static void UpdateMusicVolume()
    {
        Singleton._MusicHandler.UpdateMusicVolume();
    }

    public static void UpdateSfxVolume()
    {
        Singleton.sfxAudio = SettingsManager.Singleton.SfxVolume;
    }
}

using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio
{
    List<EventInstance> swingSpoon;
    public PlayerAudio()
    {
        // Create event pools
        swingSpoon = new List<EventInstance>
        {
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[0]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[0]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[0])
        };
        PlayerInventory.UsedSpoon += SwingSpoon;
    }

    public void SwingSpoon()
    {
        EventInstance GetSwingSpoon()
        {
            for (int i = 0; i < swingSpoon.Count; i++)
            {
                if (!AudioManager.IsAudioEventPlaying(swingSpoon[i]))
                {
                    return swingSpoon[i];
                }
            }

            EventInstance instance = AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[0]);
            swingSpoon.Add(instance);
            return instance;
        }

        EventInstance instance = GetSwingSpoon();
        instance.start();
    }
    
}

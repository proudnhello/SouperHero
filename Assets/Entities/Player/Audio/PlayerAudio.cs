using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio
{
    List<EventInstance> swingSpoon;
    EventInstance cookSoup;
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

        cookSoup = AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[1]);
        CookingManager.CookSoup += CookSoup;
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

    public void CookSoup()
    {
        cookSoup.start();
    }
    
}

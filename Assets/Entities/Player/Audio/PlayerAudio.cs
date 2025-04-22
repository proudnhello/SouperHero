// portions of this file were generated using GitHub Copilot
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio
{
    List<EventInstance> swingSpoon;
    List<EventInstance> dash;
    List<EventInstance> breakBreakable;
    List<EventInstance> pickup;
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

        dash = new List<EventInstance>
        {
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[2]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[2]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[2])
        };
        PlayerMovement.dash += Dash;

        breakBreakable = new List<EventInstance>
        {
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[3]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[3]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[3])
        };
        Destroyables.Destroyed += BreakBreakable;

        pickup = new List<EventInstance>
        {
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[4]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[4]),
            AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[4])
        };
        CollectableObject.Collected += Pickup;

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

    public void Dash()
    {
        EventInstance GetDash()
        {
            for (int i = 0; i < dash.Count; i++)
            {
                if (!AudioManager.IsAudioEventPlaying(dash[i]))
                {
                    return dash[i];
                }
            }

            EventInstance instance = AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[2]);
            dash.Add(instance);
            return instance;
        }

        EventInstance instance = GetDash();
        instance.start();
    }

    public void BreakBreakable()
    {
        EventInstance GetBreakBreakable()
        {
            for (int i = 0; i < breakBreakable.Count; i++)
            {
                if (!AudioManager.IsAudioEventPlaying(breakBreakable[i]))
                {
                    return breakBreakable[i];
                }
            }

            EventInstance instance = AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[3]);
            breakBreakable.Add(instance);
            return instance;
        }

        EventInstance instance = GetBreakBreakable();
        instance.start();
    }

    public void Pickup()
    {
        EventInstance GetPickup()
        {
            for (int i = 0; i < pickup.Count; i++)
            {
                if (!AudioManager.IsAudioEventPlaying(pickup[i]))
                {
                    return pickup[i];
                }
            }

            EventInstance instance = AudioManager.Main.CreateInstance(AudioManager.Main.PLAYER_SFX[4]);
            pickup.Add(instance);
            return instance;
        }

        EventInstance instance = GetPickup();
        instance.start();
    }

    public void CookSoup()
    {
        cookSoup.start();
    }
    
}

using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class EnemyAudio

{
    List<EventInstance> enemyDamage;
    public EnemyAudio()
    {
        // Create event pools
        enemyDamage = new List<EventInstance>();
        EnemyBaseClass.EnemyDamageEvent += PlayDamage;
    }

    public void PlayDamage(EnemyBaseClass enemy)
    {
        Debug.Log("is this being alled");
        EventInstance PlayDamage()
        {
            for (int i = 0; i < enemyDamage.Count; i++)
            {
                if (!AudioManager.IsAudioEventPlaying(enemyDamage[i]))
                {
                    return enemyDamage[i];
                }
            }

            EventInstance instance = AudioManager.Singleton.CreateInstance(AudioManager.Singleton.ENEMY_SFX[0]);
            enemyDamage.Add(instance);
            return instance;
        }

        EventInstance instance = PlayDamage();
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(enemy.transform));
        instance.start();
    }
}

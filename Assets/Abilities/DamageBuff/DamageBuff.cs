using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/DamageBuff")]
public class DamageBuff : AbilityAbstractClass
{
    private PlayerManager player;
    private TimerHelper timerHelper;
    [SerializeField] public int buffMult = 3; // this will change to be based on soup value
    [SerializeField] public float buffDuration = 10.0f;
    int buffAmount = 10;
    
    public override void Initialize(int soupVal)
    {
        player = PlayerManager.Singleton;
        timerHelper = TimerHelper.instance;
        // TODO: make buffAmount based on soupValue
        // ex: buffAmount = Mathf.CeilToInt(PlayerManager.instance.soupVal / 25)

        int usageValue = Mathf.CeilToInt(soupVal / 2.0f);
        buffAmount = soupVal/buffMult;

        if (player != null)
        {
            // buff player's damage by buff amount
            player.SetDamage(buffAmount + PlayerManager.Singleton.GetDamage());
            Debug.Log("setting damage");
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }

        if(timerHelper != null)
        {
            // start the timer for the buff
            Debug.Log("starting buff timer");
            timerHelper.StartBuffTimer(this, buffDuration);
        }
        else{
            Debug.LogWarning("TimerHelper not found!");
        }

        return;
    }
    public override void Active(){
        // active won't do much now for buffs
        Debug.Log("using buff >:], buffed DMG = " + PlayerManager.Singleton.GetDamage());
        
    }
    public override void End()
    {
        // decrease player damage by buff amount
        // gets called at the end of the buff timer
        if (player != null)
        {
            player.SetDamage(PlayerManager.Singleton.GetDamage() - buffAmount);
            //PlayerManager.instance.RemoveAbility(this);
            Debug.Log("Ending buff");
            //Debug.Log("DMG after debuff: " + PlayerManager.instance.GetDamage());
        }
        else
        {
            Debug.LogWarning("Player not found!");
        }
    }
}

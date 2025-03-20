//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using static SoupSpoon;

//public class SpoonUses : MonoBehaviour
//{

//    [SerializeField] TMP_Text text;

//    // Start is called before the first frame update
//    void Start()
//    {
//        PlayerInventory.ChangedSpoon += ChangeSpoon;
//    }

//    private void OnDisable()
//    {
//        PlayerInventory.ChangedSpoon -= ChangeSpoon;
//    }

//    // Update is called once per frame
//    void ChangeSpoon(int spoonNum)
//    {
//        SoupSpoon spoon = PlayerInventory.Singleton.GetSpoons()[spoonNum];

//        int maxUses = 0;
//        foreach(SpoonAbility ability in spoon.spoonAbilities)
//        {
//            if (ability.uses > maxUses)
//            {
//                maxUses = ability.uses;
//            }
//        }

//        if (maxUses >= 0)
//        {
//            text.text = "Uses = " + maxUses;
//        } else if (maxUses == -1){
//            text.text = "Uses = ∞";
//        }
//        else
//        {
//            Debug.LogError("Invalid Uses!");
//        }
//    }

//}

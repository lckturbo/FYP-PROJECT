using UnityEngine;

public static class InteractionLock
{
    public static bool IsLocked => ShopManager.Instance?.IsShopActive == true
                                   || ShopManager.Instance?.isSellOpen == true
                                   || DialogueManager.Instance?.IsDialogueActive == true;
}

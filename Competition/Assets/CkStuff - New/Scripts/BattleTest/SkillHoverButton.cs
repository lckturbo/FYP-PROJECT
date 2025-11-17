using UnityEngine;
using UnityEngine.EventSystems;

public class SkillHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BattleActionUI battleUI;
    public BattleActionUI.SkillSlot slot;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (battleUI)
            battleUI.BeginHover(slot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (battleUI)
            battleUI.EndHover(slot);
    }
}

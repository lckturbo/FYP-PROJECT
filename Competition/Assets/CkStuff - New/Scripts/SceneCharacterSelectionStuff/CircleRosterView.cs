using UnityEngine;

public class CircleRosterView : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private NewCharacterDefinition[] roster;
    [SerializeField] private SelectedCharacter selectedStore;

    [Header("Refs")]
    [SerializeField] private Transform circleList;         // VerticalLayout parent
    [SerializeField] private CharacterSelectView mainView; // left+center binder

    private CircleRosterItemView[] items;
    private int current = 0;

    void Awake()
    {
        if (roster == null || roster.Length == 0 || circleList == null || mainView == null)
        {
            Debug.LogWarning("CircleRosterView: missing refs/roster.");
            return;
        }

        int n = Mathf.Min(roster.Length, circleList.childCount);
        items = new CircleRosterItemView[n];

        for (int i = 0; i < n; i++)
        {
            var t = circleList.GetChild(i);
            var item = t.GetComponent<CircleRosterItemView>();
            if (!item) item = t.gameObject.AddComponent<CircleRosterItemView>();
            items[i] = item;

            int cap = i;
            item.Bind(roster[i], () => Select(cap));
        }

        Select(0);
    }

    public void Select(int index)
    {
        if (items == null || items.Length == 0) return;

        current = Mathf.Clamp(index, 0, items.Length - 1);
        var def = roster[current];

        selectedStore.Set(def, current);
        mainView.Bind(current); // refresh left+center

        for (int i = 0; i < items.Length; i++)
            items[i].SetSelected(i == current);
    }
}

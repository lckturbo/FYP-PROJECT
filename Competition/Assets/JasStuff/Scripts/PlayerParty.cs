using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParty : MonoBehaviour
{
    public static PlayerParty instance;

    [Header("Party Members")]
    [SerializeField] private GameObject leader; // main character
    [SerializeField] private List<GameObject> partyMembers;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    //public List<GameObject> GetAllMembers()
    //{
    //    List<GameObject> result = new List<GameObject>();
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPuzzle : MonoBehaviour
{
    [SerializeField] private ToggleableBlock doorBlock;
    [SerializeField] private List<int> correctPath; 
    private int currentStep = 0;
    private bool canStep = false;

    private List<PuzzleZone> zones;

    private void Start()
    {
        zones = new List<PuzzleZone>(FindObjectsOfType<PuzzleZone>());
        StartCoroutine(ShowCorrectPath());
    }

    public void OnZoneStepped(PuzzleZone zone)
    {
        if (!canStep) return;

        if (zone.ZoneID == correctPath[currentStep])
        {
            currentStep++;
            Debug.Log($"Correct step {currentStep}/{correctPath.Count}");

            if (currentStep >= correctPath.Count)
            {
                Debug.Log("Puzzle Completed!");
                canStep = false;

            }
        }
        else
        {
            Debug.Log("Wrong step — Resetting...");
            currentStep = 0;
            canStep = false;
            StartCoroutine(ShowCorrectPath());
        }
    }

    private IEnumerator ShowCorrectPath()
    {
        Debug.Log("Showing correct path...");
        foreach (int id in correctPath)
        {
            PuzzleZone zone = zones.Find(z => z.ZoneID == id);
            if (zone != null)
            {
                zone.Highlight(0.5f);
                yield return new WaitForSeconds(0.6f);
            }
        }

        canStep = true; // player can start moving
        Debug.Log("Player may now step.");
    }
}

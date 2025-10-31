using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class creditfly : MonoBehaviour
{
    [System.Serializable]
    public class Section
    {
        [TextArea(1, 3)] public string title;
        [TextArea(3, 10)] public string body;
        public float holdSeconds = 3f;
    }

    [Header("Wiring")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Timing")]
    [SerializeField] private float fadeInSeconds = 1.0f;
    [SerializeField] private float fadeOutSeconds = 1.0f;
    [SerializeField] private float betweenSectionsDelay = 0.3f;

    [Header("Flow")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = false;

    [Header("Sections")]
    [SerializeField] private List<Section> sections = new List<Section>();

    private Coroutine _runner;

    private void Start()
    {
        if (sections == null || sections.Count == 0)
            BuildDefaultSections();

        if (canvasGroup) canvasGroup.alpha = 0f;

        if (playOnStart)
            _runner = StartCoroutine(RunCredits());
    }

    public void Play()
    {
        if (_runner != null) StopCoroutine(_runner);
        _runner = StartCoroutine(RunCredits());
    }

    private IEnumerator RunCredits()
    {
        do
        {
            for (int i = 0; i < sections.Count; i++)
            {
                var s = sections[i];
                ApplySection(s);

                // Fade in
                yield return Fade(0f, 1f, fadeInSeconds);

                // Hold fully visible
                yield return new WaitForSeconds(Mathf.Max(0f, s.holdSeconds));

                // Fade out
                yield return Fade(1f, 0f, fadeOutSeconds);

                // Small gap between sections
                yield return new WaitForSeconds(betweenSectionsDelay);
            }
        }
        while (loop);

        // Ensure hidden at the end
        if (canvasGroup) canvasGroup.alpha = 0f;
    }

    private void ApplySection(Section s)
    {
        if (titleText) titleText.text = s.title ?? "";
        if (bodyText) bodyText.text = s.body ?? "";
    }

    private IEnumerator Fade(float from, float to, float seconds)
    {
        if (!canvasGroup || seconds <= 0f)
        {
            if (canvasGroup) canvasGroup.alpha = to;
            yield break;
        }

        float t = 0f;
        canvasGroup.alpha = from;
        while (t < seconds)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    // Your exact credits, same as before
    private void BuildDefaultSections()
    {
        sections = new List<Section>
        {
            new Section{
                title = "Game Title",
                body =
@"I GOT SUCKED INTO THIS DUNGEON AND NOW IM FORCED TO PLAY THIS
A game by JUMONKEY",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Director / Lead Developer",
                body = "Lam Cheng Kel",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Game Designer(s)",
                body =
@"Clayton Tan Wei Yu
Jaslin Joo Chui Hoon
Lam Cheng Kel
Yuina
Bonotan Faith Ann
Chua Ke En Chloe
Matias XinXin Rose Ye",
            holdSeconds = 4.0f
            },
            new Section{
                title = "Programmer(s)",
                body =
@"Clayton Tan Wei Yu
Jaslin Joo Chui Hoon
Lam Cheng Kel
Yuina",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Artist(s) / Animator(s)",
                body =
@"Bonotan Faith Ann
Chua Ke En Chloe
Matias XinXin Rose Ye",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Sound Designer",
                body =
@"Jaslin Joo Chui Hoon
Lam Cheng Kel",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Writer / Narrative Designer",
                body =
@"Bonotan Faith Ann
Chua Ke En Chloe
Matias XinXin Rose Ye
Lam Cheng Kel
Jaslin Joo Chui Hoon",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Additional Contributors",
                body =
@"GNDLF The Maker",
                holdSeconds = 1.0f
            },
            new Section{
                title = "QA / Playtesting",
                body =
@"Tan Kai Yang & his MSI Stealth 16 Studio A13V
Foong Wen Wei 
Huang XiaoZe 
Leong Yu Hui, Kayden",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Localization / Translation",
                body =
@"Jaslin Joo Chui Hoon
Lam Cheng Kel",
                holdSeconds = 3.0f
            },
            new Section{
                title = "Tools & Technology",
                body = @"Unity",
                holdSeconds = 2.0f
            },
            new Section{
                title = "Music & Sound Credits",
                body =
@"Steven Melin - Across The Distance
Steven Melin - Battle For Peace,
Steven Melin - Warrior's Pride
Steven Melin - New Adventure",
                holdSeconds = 2.5f
            },
            new Section{
                title = "Publishing & Legal",
                body = @"",
                holdSeconds = 1.0f
            },
            new Section{
                title = "Publisher",
                body =
@"JUNMONKEY",
                holdSeconds = 2.5f
            },
            new Section{
                title = "Legal / Copyright Info",
                body =
@"(C) JUMONKEY. All Rights Reserved.
I GOT SUCKED INTO THIS DUNGEON AND NOW IM FORCED TO PLAY THIS is a trademark of JUNMONKEY LCK.",
                holdSeconds = 4.0f
            },
            new Section{
                title = "Platform Partners",
                body = @"",
                holdSeconds = 2.0f
            },
            new Section{
                title = "Special Thanks",
                body =
@"Choo Chia Fong
Lewis Sang

Kris Lee
NicholasTey

Eng Kai Suen
Tan Kang Soon
Toh Da Jun

Nanyang Polytechnic (NYP)",
                holdSeconds = 4.0f
            },
            new Section{
                title = "Thanks for playing!",
                body = @"",
                holdSeconds = 3.0f
            }
        };
    }
}

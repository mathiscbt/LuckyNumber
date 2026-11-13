using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Ami.BroAudio;
using TMPro;
using UnityEngine;

public class StatsPanel : UIPanel
{
    [SerializeField] private TMP_Text playerStats;
    [SerializeField] private TMP_Text opponentStats;
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private SoundID typeSound;

    private static readonly string[] StatLabels =
    {
        "Weight",
        "Size",
        "Aura",
        "Force",
        "Stamina",
        "Cooldown",
        "Awkwardness",
        "Fatigue",
        "Inertia",
        "Delay",
    };

    private Action onFight;
    private Coroutine typingRoutine;

    public void Display(SumoSetup player, List<SumoSetup> opponents, bool showOpponentCount, Action onFightCallback)
    {
        onFight = onFightCallback;

        StringBuilder playerText = new StringBuilder(Describe(player));
        if (showOpponentCount)
        {
            playerText.AppendLine($"<color=red><i>Number of opponents: {opponents.Count}</i></color>");
        }

        StringBuilder opponentText = new StringBuilder();
        int lastShownNumber = -1;
        foreach (SumoSetup opponent in opponents)
        {
            if (opponent.luckyNumber == lastShownNumber) continue;
            lastShownNumber = opponent.luckyNumber;
            opponentText.Append(Describe(opponent));
        }

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(Type(playerStats, playerText.ToString(), opponentStats, opponentText.ToString()));
    }

    public void OnClickFight() => onFight?.Invoke();

    private IEnumerator Type(TMP_Text first, string firstText, TMP_Text second, string secondText)
    {
        first.text = firstText;
        second.text = secondText;

        first.maxVisibleCharacters = int.MaxValue;
        second.maxVisibleCharacters = int.MaxValue;
        first.ForceMeshUpdate();
        second.ForceMeshUpdate();
        int playerTotal = first.textInfo.characterCount;
        int total = Mathf.Max(playerTotal, second.textInfo.characterCount);

        int visible = 0;
        first.maxVisibleCharacters = visible;
        second.maxVisibleCharacters = visible;

        while (visible < total)
        {
            yield return new WaitForSeconds(1f / charactersPerSecond);
            visible++;
            first.maxVisibleCharacters = visible;
            second.maxVisibleCharacters = visible;
            if (visible % 2 == 0 && visible <= playerTotal) BroAudio.Play(typeSound);
        }
    }

    private static string Describe(SumoSetup setup)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string label in StatLabels)
        {
            if (string.IsNullOrEmpty(label))
            {
                sb.AppendLine();
                continue;
            }
            sb.AppendLine($"{label}: {Red(setup.luckyNumber)}");
        }
        return sb.ToString();
    }

    private static string Red(int value) => $"<color=red>{value}</color>";
}

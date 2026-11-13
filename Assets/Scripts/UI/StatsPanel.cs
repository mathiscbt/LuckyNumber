using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class StatsPanel : UIPanel
{
    [SerializeField] private TMP_Text playerStats;
    [SerializeField] private TMP_Text opponentStats;

    private static readonly string[] StatLabels =
    {
        "Weight",
        "Speed",
        "Dash cooldown",
    };

    private Action onFight;

    public void Display(SumoSetup player, List<SumoSetup> opponents, Action onFightCallback)
    {
        onFight = onFightCallback;
        playerStats.text = Describe(player);

        StringBuilder opponentText = new StringBuilder();
        foreach (SumoSetup opponent in opponents)
        {
            opponentText.Append(Describe(opponent)).AppendLine();
        }
        opponentStats.text = opponentText.ToString();
    }

    public void OnClickFight() => onFight?.Invoke();

    private static string Describe(SumoSetup setup)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Lucky number: {setup.luckyNumber}");
        foreach (string label in StatLabels)
        {
            sb.AppendLine($"{label}: {setup.luckyNumber}");
        }
        return sb.ToString();
    }
}

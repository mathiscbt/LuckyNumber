using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class StatsPanel : UIPanel
{
    [SerializeField] private TMP_Text playerStats;
    [SerializeField] private TMP_Text opponentStats;

    private static readonly List<(string label, Func<SumoData, float> getter)> StatDefs = new List<(string, Func<SumoData, float>)>
    {
        ("Masse", d => d.mass),
        ("Vitesse", d => d.maxSpeed),
        ("Dash cooldown", d => d.cooldown),
        // TODO: liste des stats affichées
    };

    private Action onFight;

    public void Display(SumoSetup player, SumoData playerData, List<SumoSetup> opponents, List<SumoData> opponentsData, Action onFightCallback)
    {
        onFight = onFightCallback;
        playerStats.text = Describe(player, playerData);

        StringBuilder opponentText = new StringBuilder();
        for (int i = 0; i < opponents.Count; i++)
        {
            opponentText.Append(Describe(opponents[i], opponentsData[i])).AppendLine();
        }
        opponentStats.text = opponentText.ToString();
    }

    public void OnClickFight() => onFight?.Invoke();

    private static string Describe(SumoSetup setup, SumoData data)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Lucky number : {setup.luckyNumber}");
        foreach (var (label, getter) in StatDefs)
        {
            sb.AppendLine($"{label} : {getter(data):0.##}");
        }
        return sb.ToString();
    }
}

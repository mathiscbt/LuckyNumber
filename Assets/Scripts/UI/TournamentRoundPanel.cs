using TMPro;
using UnityEngine;

public class TournamentRoundPanel : UIPanel
{
    [SerializeField] private TMP_Text nextRoundLabel;

    public void Display(string nextRoundName)
    {
        nextRoundLabel.text = $"Next round: {nextRoundName}";
    }

    public void OnClickContinue() => GameFlowManager.Instance.ContinueTournament();
    public void OnClickQuit() => GameFlowManager.Instance.BackToMenu();
}

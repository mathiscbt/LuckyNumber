using TMPro;
using UnityEngine;

public class MatchResultPanel : UIPanel
{
    [SerializeField] private TMP_Text winnerLabel;

    public void Display(string winnerText)
    {
        winnerLabel.text = winnerText;
    }

    public void OnClickReplay() => GameFlowManager.Instance.Replay();
    public void OnClickMenu() => GameFlowManager.Instance.BackToMenu();
}

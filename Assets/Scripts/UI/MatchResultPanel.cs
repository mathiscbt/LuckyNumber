using TMPro;
using UnityEngine;

public class MatchResultPanel : UIPanel
{
    [SerializeField] private TMP_Text winnerLabel;
    [SerializeField] private float fadeDuration = 0.6f;

    private void OnEnable() => FadeIn(fadeDuration);

    public void Display(string winnerText)
    {
        winnerLabel.text = winnerText;
    }

    public void OnClickReplay() => GameFlowManager.Instance.Replay();
    public void OnClickMenu() => GameFlowManager.Instance.BackToMenu();
}

using TMPro;
using UnityEngine;

public class TournamentEndPanel : UIPanel
{
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private float fadeDuration = 0.6f;

    private void OnEnable() => FadeIn(fadeDuration);

    public void Display(bool victory)
    {
        titleLabel.text = victory ? "VICTORY" : "DEFEAT";
    }

    public void OnClickMenu() => GameFlowManager.Instance.BackToMenu();
}

using TMPro;
using UnityEngine;

public class TournamentEndPanel : UIPanel
{
    [SerializeField] private TMP_Text titleLabel;

    public void Display(bool victory)
    {
        titleLabel.text = victory ? "VICTORY" : "DEFEAT";
    }

    public void OnClickMenu() => GameFlowManager.Instance.BackToMenu();
}

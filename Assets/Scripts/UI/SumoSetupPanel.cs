using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SumoSetupPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text luckyNumberLabel;
    [SerializeField] private Image colorPreview;

    private int luckyNumber;
    private int colorIndex;

    public void SelectLuckyNumber(int number)
    {
        luckyNumber = number;
        Refresh();
    }

    public void RandomizeLuckyNumber() => SelectLuckyNumber(Random.Range(1, 10));

    public void SelectColor(int index)
    {
        colorIndex = index;
        Refresh();
    }

    public SumoSetup Build(ControlScheme controls)
    {
        if (luckyNumber == 0) return null;
        return new SumoSetup(luckyNumber, GameFlowManager.Instance.Palette[colorIndex], false, controls);
    }

    private void Refresh()
    {
        if (luckyNumberLabel != null) luckyNumberLabel.text = luckyNumber == 0 ? "?" : luckyNumber.ToString();
        if (colorPreview != null) colorPreview.color = GameFlowManager.Instance.Palette[colorIndex];
    }
}

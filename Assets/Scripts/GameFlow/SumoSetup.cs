using UnityEngine;

public class SumoSetup
{
    public int luckyNumber;
    public Color color;
    public bool isAi;
    public ControlScheme controls;

    public SumoSetup(int luckyNumber, Color color, bool isAi, ControlScheme controls)
    {
        this.luckyNumber = luckyNumber;
        this.color = color;
        this.isAi = isAi;
        this.controls = controls;
    }
}

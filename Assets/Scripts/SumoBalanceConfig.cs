using UnityEngine;

[CreateAssetMenu(fileName = "SumoBalanceConfig", menuName = "ScriptableObjects/SumoBalanceConfig")]
public class SumoBalanceConfig : ScriptableObject
{
    [Header("For number 1")]
    public float minMass = 1f;
    public float minScale = 0.5f;
    public float minDrag = 1f;
    public float highMoveForce = 20f;
    public float highMaxSpeed = 10f;
    public float shortCooldown = 0.2f;
    public float minDashWindupTime = 0.1f;
    public float minDashDuration = 0.2f;

    [Header("For number 9")]
    public float maxMass = 9f;
    public float maxScale = 2.5f;
    public float maxDrag = 5f;
    public float lowMoveForce = 5f;
    public float lowMaxSpeed = 3f;
    public float longCooldown = 2.0f;
    public float maxDashWindupTime = 1f;
    public float maxDashDuration = 0.5f;

    [Header("Global")]
    public float dashForce = 15f;

    public SumoData GenerateDataForNumber(int number)
    {
        number = Mathf.Clamp(number, 1, 9);


        float t = (number - 1f) / 8f;

        SumoData data = new SumoData();
        data.luckyNumber = number;
        
        data.mass = Mathf.Lerp(minMass, maxMass, t);
        data.scale = Mathf.Lerp(minScale, maxScale, t);
        data.drag = Mathf.Lerp(minDrag, maxDrag, t);
        data.dashWindupTime = Mathf.Lerp(minDashWindupTime, maxDashWindupTime, t);
        data.dashDuration = Mathf.Lerp(minDashDuration, maxDashDuration, t);

        data.moveForce = Mathf.Lerp(highMoveForce, lowMoveForce, t);
        data.maxSpeed = Mathf.Lerp(highMaxSpeed, lowMaxSpeed, t);
        data.cooldown = Mathf.Lerp(shortCooldown, longCooldown, t);

        data.dashForce = dashForce; 

        return data;
    }
}
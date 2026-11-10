using UnityEngine;

[CreateAssetMenu(fileName = "SumoBalanceConfig", menuName = "ScriptableObjects/SumoBalanceConfig")]
public class SumoBalanceConfig : ScriptableObject
{
    [Header("Data 1")]
    public float _1mass = 1f;
    public float _1scale = 0.7f;
    public float _1drag = 2f;
    public float _1moveForce = 25f;
    public float _1maxSpeed = 9f;
    public float _1cooldown = 0.5f;
    public float _1dashWindupTime = 0.05f;
    public float _1dashDuration = 0.15f;
    public float _1dashForce = 12f;
    public float _1stunDuration = 1f;

    [Header("Data 9")]
    public float _9mass = 5f;
    public float _9scale = 1.8f;
    public float _9drag = 1f;
    public float _9moveForce = 60f;
    public float _9maxSpeed = 4f;
    public float _9cooldown = 2.0f;
    public float _9dashWindupTime = 0.35f;
    public float _9dashDuration = 0.35f;
    public float _9dashForce = 75f;
    public float _9stunDuration = 3f;

    public SumoData GenerateDataForNumber(int number)
    {
        number = Mathf.Clamp(number, 1, 9);
        float t = (number - 1f) / 8f;

        SumoData data = new SumoData();
        data.luckyNumber = number;
        
        data.mass = Mathf.Lerp(_1mass, _9mass, t);
        data.scale = Mathf.Lerp(_1scale, _9scale, t);
        data.drag = Mathf.Lerp(_1drag, _9drag, t);
        
        data.moveForce = Mathf.Lerp(_1moveForce, _9moveForce, t);
        data.maxSpeed = Mathf.Lerp(_1maxSpeed, _9maxSpeed, t);
        data.cooldown = Mathf.Lerp(_1cooldown, _9cooldown, t);

        data.dashWindupTime = Mathf.Lerp(_1dashWindupTime, _9dashWindupTime, t);
        data.dashDuration = Mathf.Lerp(_1dashDuration, _9dashDuration, t);
        data.dashForce = Mathf.Lerp(_1dashForce, _9dashForce, t);

        data.stunDuration = Mathf.Lerp(_1stunDuration,_9stunDuration,t);

        return data;
    }
}
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SumoMovement), typeof(SumoHealth))]
public class SumoDashUI : MonoBehaviour
{
    private SumoMovement sumoMovement;
    private SumoHealth sumoHealth;
    [SerializeField] private Image dashCooldownFill;
    [SerializeField] private Image healthFill;

    void Start()
    {
        sumoMovement = GetComponent<SumoMovement>();
        sumoHealth = GetComponent<SumoHealth>();
        sumoHealth.OnHealthPercentChange += UpdateHealthFill;
    }
    void Update()
    {
        if (sumoMovement == null || dashCooldownFill == null) return;

        float cooldownNormalized = sumoMovement.GetDashCooldownNormalized();
        dashCooldownFill.fillAmount = 1f - cooldownNormalized;
    }

    private void UpdateHealthFill(float value) => healthFill.fillAmount = value;

}
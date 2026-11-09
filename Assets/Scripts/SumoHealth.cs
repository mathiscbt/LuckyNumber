using System;
using UnityEngine;

[RequireComponent(typeof(SumoMovement))]
public class SumoHealth : MonoBehaviour
{
    [Header("Health & Stun Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float stunDuration = 2.0f;
    [SerializeField] private float baseDashDamage = 25f;

    private float currentHealth;
    private float stunTimer = 0f;
    private bool isStunned = false;

    private SumoMovement sumoMovement;

    public event Action<GameObject> OnStunned; // Pour plus tard
    public event Action<GameObject> OnRecovered;  // Pour plus tard

    public bool IsStunned => isStunned;
    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                RecoverFromStun();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStunned) return;

        SumoMovement attacker = collision.gameObject.GetComponent<SumoMovement>();
        if (attacker == null) return;

        if (attacker.IsDashing)
        {
            float attackerMass = attacker.SumoData.mass;
            float victimMass = sumoMovement.SumoData.mass;

            float massRatio = attackerMass / victimMass;
            float finalDamage = baseDashDamage * massRatio;

            TakeDamage(finalDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isStunned) return;

        currentHealth -= amount;
        Debug.Log(currentHealth);

        if (currentHealth <= 0f)
        {
            Stun();
        }
    }

    private void Stun()
    {
        isStunned = true;
        stunTimer = stunDuration;

        sumoMovement.SetStunned(true);
        OnStunned?.Invoke(gameObject);
    }

    private void RecoverFromStun()
    {
        isStunned = false;
        currentHealth = maxHealth;

        sumoMovement.SetStunned(false);
        OnRecovered?.Invoke(gameObject);
    }
}
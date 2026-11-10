using System;
using UnityEngine;

[RequireComponent(typeof(SumoMovement))]
public class SumoHealth : MonoBehaviour
{
    [Header("Health & Stun Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float stunDuration;

    private float currentHealth;
    public float CurrentHealth {
        get => currentHealth; 
        private set
        {
            currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            OnHealthPercentChange?.Invoke(Mathf.Clamp01(value/maxHealth));
        }
    }

    public float MyValue { get; set; }
    private float stunTimer = 0f;
    private bool isStunned = false;

    private SumoMovement sumoMovement;

    public event Action<GameObject> OnStunned; // Pour plus tard
    public event Action<GameObject> OnRecovered;  // Pour plus tard
    public event Action<float> OnHealthPercentChange;

    public bool IsStunned => isStunned;
    

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        CurrentHealth = maxHealth;
    }
    
    void Start()
    {
        stunDuration = sumoMovement.SumoData.stunDuration;
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


    public void TakeDamage(float amount)
    {
        if (isStunned) return;

        CurrentHealth -= amount;
        Debug.Log(CurrentHealth);

        if (CurrentHealth <= 0f)
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
        CurrentHealth = maxHealth;

        sumoMovement.SetStunned(false);
        OnRecovered?.Invoke(gameObject);
    }
}
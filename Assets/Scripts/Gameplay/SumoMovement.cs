using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class SumoMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMovementInput = Vector2.up;
    public Vector2 Facing => lastMovementInput;
    private enum SumoState { Normal, Windup, Dashing };
    private SumoState currentState = SumoState.Normal;
    private float dashCooldown = 0f;
    public float GetDashCooldownNormalized() => (SumoData == null || SumoData.cooldown <= 0f)? 0f : Mathf.Clamp01(dashCooldown / SumoData.cooldown);
    private float windupTimer = 0f;
    private float dashTimer = 0f;
    private bool needsDashImpulse = false;
    private List<GameObject> hitByDash = new List<GameObject>();
    [SerializeField] private float baseDashDamage = 25f;
    private bool ready = false;
    private bool isStunned = false;
    [SerializeField] private float rotationSpeed = 10f;

    public event Action<GameObject> OnEliminated;
    public bool IsDashing => currentState == SumoState.Dashing;
    public event Action StartWindup;
    public event Action StartDashing;
    public event Action EndDashing;
    public event Action<float> OnScaleChange;
    public SumoData SumoData {get; private set;}

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetMovementInput(Vector2 input)
    {
        if (!ready || isStunned) return;
        movementInput = input;
        if (currentState == SumoState.Normal && input.sqrMagnitude > 0.01f)
        {
            lastMovementInput = input.normalized;
        }
    }

    public void TryDash()
    {
        if (!ready || isStunned) return;
        if (dashCooldown > 0f) return;
        if (currentState != SumoState.Normal) return;

        if (movementInput.sqrMagnitude > 0.01f)
        {
            lastMovementInput = movementInput.normalized;
        }

        currentState = SumoState.Windup;
        StartWindup?.Invoke();
        windupTimer = SumoData.dashWindupTime;
        dashCooldown = SumoData.cooldown;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void SetStunned(bool stunned)
    {
        isStunned = stunned;
        
        if (isStunned)
        {
            currentState = SumoState.Normal;
            rb.mass = 1f; 
            rb.linearDamping = 1f;
        }
        else
        {
            RefreshData();
        }
    }

    private void SetSumoData(SumoData data)
    {
        SumoData = data;
    }

    public void Initialize(SumoData data)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        SumoData = data;
        RefreshData();
        ready = true;
    }

    private void RefreshData()
    {
        rb.mass = SumoData.mass;
        rb.linearDamping = SumoData.drag;
        transform.localScale = Vector3.one * SumoData.scale;
        OnScaleChange?.Invoke(SumoData.scale);
    }

    private void Update()
    {
        if (!ready || isStunned) return;

        if (dashCooldown > 0f)
        {
            dashCooldown -= Time.deltaTime;
        }

        if (currentState != SumoState.Normal)
        {
            if (currentState == SumoState.Windup)
            {
                windupTimer -= Time.deltaTime;
                if (windupTimer <= 0f)
                {
                    currentState = SumoState.Dashing;
                    dashTimer = SumoData.dashDuration;
                    needsDashImpulse = true;
                    hitByDash.Clear();
                    StartDashing?.Invoke();
                }
            }
            else if (currentState == SumoState.Dashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    currentState = SumoState.Normal;
                    EndDashing?.Invoke();
                }
            }
        }
        
    }

    private void FixedUpdate()
{
    if (!ready || isStunned) return;

    if (currentState == SumoState.Normal)
    {
        Vector2 targetDirection = movementInput.sqrMagnitude > 0.01f ? movementInput : lastMovementInput;

        if (targetDirection.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f;
            float smoothedAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedAngle);
        }

        rb.AddForce(movementInput * SumoData.moveForce, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > SumoData.maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * SumoData.maxSpeed;
        }
    }
    else if (currentState == SumoState.Dashing)
    {
        if (needsDashImpulse)
        {
            rb.AddForce(lastMovementInput * SumoData.dashForce, ForceMode2D.Impulse);
            needsDashImpulse = false;
        }
    }

}

    private void TryAttackVictim(GameObject target)
    {
        if (!IsDashing || hitByDash.Contains(target)) return;

        SumoHealth victimHealth = target.GetComponent<SumoHealth>();
        SumoMovement victimMovement = target.GetComponent<SumoMovement>();

        if (victimHealth != null && victimMovement != null)
        {
            float massRatio = Mathf.Sqrt(SumoData.mass / victimMovement.SumoData.mass);
            float damage = baseDashDamage * massRatio;

            victimHealth.TakeDamage(damage);
            hitByDash.Add(target);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) => TryAttackVictim(collision.gameObject);
    private void OnCollisionStay2D(Collision2D collision) => TryAttackVictim(collision.gameObject);

    public void Eliminate()
    {
        OnEliminated?.Invoke(gameObject);
        rb.linearVelocity = Vector2.zero;
        ready = false;
        SumoCamera.Instance?.RemoveTarget(transform);
        //Destroy(gameObject);
    }

    public void ResetState()
    {
        currentState = SumoState.Normal;
        dashCooldown = 0f;
        windupTimer = 0f;
        dashTimer = 0f;
        needsDashImpulse = false;
        hitByDash.Clear();
        movementInput = Vector2.zero;
        lastMovementInput = Vector2.right;
        isStunned = false;
        ready = true;
        RefreshData();
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}

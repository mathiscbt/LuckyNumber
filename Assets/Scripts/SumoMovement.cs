using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class SumoMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMovementInput = Vector2.up;
    private enum SumoState { Normal, Windup, Dashing };
    private SumoState currentState = SumoState.Normal;
    private float dashCooldown = 0f;
    private float windupTimer = 0f;
    private float dashTimer = 0f;
    private bool needsDashImpulse = false;
    private SumoData sumoData;
    private bool ready = false;

    public event Action<GameObject> OnEliminated;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetMovementInput(Vector2 input)
    {
        if (!ready) return;
        movementInput = input;
        if (input.sqrMagnitude > 0.01f)
        {
            lastMovementInput = input.normalized;
        }
    }

    public void TryDash()
    {
        if (!ready) return;
        if (dashCooldown > 0f) return;
        if (currentState != SumoState.Normal) return;

        currentState = SumoState.Windup;
        windupTimer = sumoData.dashWindupTime;
        dashCooldown = sumoData.cooldown;

        rb.linearVelocity = Vector2.zero;
    }

    private void SetSumoData(SumoData data)
    {
        sumoData = data;
    }

    public void Initialize(SumoData data)
    {
        sumoData = data;
        RefreshData();
        ready = true;
    }

    private void RefreshData()
    {
        rb.mass = sumoData.mass;
        rb.linearDamping = sumoData.drag;
        transform.localScale = Vector3.one * sumoData.scale;
    }

    private void Update()
    {
        if (!ready) return;

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
                    dashTimer = sumoData.dashDuration;
                    needsDashImpulse = true;
                }
            }
            else if (currentState == SumoState.Dashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0f)
                {
                    currentState = SumoState.Normal;
                }
            }
        }
        
    }

    private void FixedUpdate()
    {
        if (!ready) return;

        if (currentState == SumoState.Dashing)
        {
            if (needsDashImpulse)
            {
                rb.AddForce(lastMovementInput * sumoData.dashForce, ForceMode2D.Impulse);
                needsDashImpulse = false;
            }
            return;
        }
        else if (currentState == SumoState.Windup)
        {
            return;
        }
        else if (currentState == SumoState.Normal)
        {
            rb.AddForce(movementInput * sumoData.moveForce, ForceMode2D.Force);

            if (rb.linearVelocity.magnitude > sumoData.maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * sumoData.maxSpeed;
            }
        }

    }

    public void Eliminate()
    {
        OnEliminated?.Invoke(gameObject);
        rb.linearVelocity = Vector2.zero;
        ready = false;
        //Destroy(gameObject);
    }
}

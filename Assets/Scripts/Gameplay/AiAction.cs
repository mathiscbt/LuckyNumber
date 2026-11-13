using UnityEngine;

[RequireComponent(typeof(SumoMovement))]
public class AiAction : MonoBehaviour
{
    [Header("Target & Arena Settings")]
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private float dangerZoneRatio = 0.15f;

    [Header("Dash Tactics")]
    [SerializeField] private float idealDistance = 3.5f;
    [SerializeField] private float maxDashDistance = 6f;
    [SerializeField] private float dashAlignmentThreshold = 0.8f;

    private SumoMovement sumoMovement;
    private SumoHealth sumoHealth;
    private SumoMovement targetMovement;

    private float areaRadius;
    private Vector2 areaCenter;
    private float dangerZoneMargin => areaRadius * dangerZoneRatio;

    private float wanderTimer;
    private Vector2 wanderDirection;

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        sumoHealth = GetComponent<SumoHealth>();
    }

    private void Update()
    {
        if (sumoHealth != null && sumoHealth.IsStunned)
        {
            sumoMovement.SetMovementInput(Vector2.zero);
            return;
        }

        if (targetPlayer == null)
        {
            Wander();
            return;
        }

        if (targetMovement == null)
        {
            targetMovement = targetPlayer.GetComponent<SumoMovement>();
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = targetPlayer.position;
        Vector2 dirToTarget = (targetPos - currentPos).normalized;
        float distanceToTarget = Vector2.Distance(currentPos, targetPos);

        bool playerIsThreat = targetMovement != null && targetMovement.IsDashing;

        Vector2 desiredDirection = CalculateMovementDirection(currentPos, dirToTarget, distanceToTarget, playerIsThreat);
        
        sumoMovement.SetMovementInput(desiredDirection);

        EvaluateDashOpportunity(currentPos, dirToTarget, distanceToTarget, playerIsThreat, desiredDirection);
    }

    private void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = Random.Range(1f, 2.5f);
            wanderDirection = Random.value < 0.35f ? Vector2.zero : Random.insideUnitCircle.normalized;
        }

        Vector2 direction = wanderDirection;
        if (areaRadius > 0f && Vector2.Distance(transform.position, areaCenter) > areaRadius * 0.8f)
        {
            direction = (areaCenter - (Vector2)transform.position).normalized;
        }

        sumoMovement.SetMovementInput(direction);
    }

    private Vector2 CalculateMovementDirection(Vector2 currentPos, Vector2 dirToTarget, float distanceToTarget, bool playerIsThreat)
    {
        if (areaRadius > 0f)
        {
            float distanceToCenter = Vector2.Distance(currentPos, areaCenter);
            float dangerLimit = areaRadius - dangerZoneMargin;

            if (distanceToCenter > dangerLimit)
            {
                Vector2 dirToCenter = (areaCenter - currentPos).normalized;
                float dangerFactor = Mathf.InverseLerp(dangerLimit, areaRadius, distanceToCenter);
                return Vector2.Lerp(dirToTarget, dirToCenter, dangerFactor).normalized;
            }
        }

        if (playerIsThreat)
        {
            Vector2 dodgeDir = new Vector2(-dirToTarget.y, dirToTarget.x); 
            return dodgeDir.normalized;
        }

        if (targetMovement != null && sumoMovement.SumoData != null && targetMovement.SumoData != null)
        {
            bool isLighter = sumoMovement.SumoData.mass < targetMovement.SumoData.mass;

            if (isLighter)
            {
                if (distanceToTarget < 1.5f)
                {
                    return -dirToTarget;
                }

                if (distanceToTarget < idealDistance)
                {
                    Vector2 strafeDir = new Vector2(-dirToTarget.y, dirToTarget.x);
                    return (strafeDir * 0.7f - dirToTarget * 0.3f).normalized;
                }
            }
        }

        return dirToTarget;
    }

    private void EvaluateDashOpportunity(Vector2 currentPos, Vector2 dirToTarget, float distanceToTarget, bool playerIsThreat, Vector2 dashDirection)
    {
        if (sumoMovement.GetDashCooldownNormalized() > 0f) return;
        if (!IsDashSafe(currentPos, dashDirection)) return;

        if (playerIsThreat)
        {
            sumoMovement.TryDash();
            return;
        }

        if (distanceToTarget > maxDashDistance) return;

        float alignment = Vector2.Dot(transform.up, dirToTarget);
        if (alignment < dashAlignmentThreshold) return;

        sumoMovement.TryDash();
    }

    private bool IsDashSafe(Vector2 currentPos, Vector2 dashDirection)
    {
        if (areaRadius <= 0f || sumoMovement.SumoData == null) return true;

        float dashSpeed = sumoMovement.SumoData.dashForce / sumoMovement.SumoData.mass;
        float estimatedDashDistance = dashSpeed * sumoMovement.SumoData.dashDuration;
        Vector2 predictedEndPos = currentPos + dashDirection.normalized * estimatedDashDistance;

        return Vector2.Distance(predictedEndPos, areaCenter) < areaRadius - 0.5f;
    }

    public void SetTarget(Transform target)
    {
        targetPlayer = target;
        if (targetPlayer != null)
        {
            targetMovement = targetPlayer.GetComponent<SumoMovement>();
        }
    }

    public void SetAreaData(Collider2D areaCollider)
    {
        if (areaCollider is CircleCollider2D circle)
        {
            float radius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
            Vector2 center = (Vector2)circle.transform.position + circle.offset;

            areaRadius = radius;
            areaCenter = center;
        }
    }

    private void OnDrawGizmosSelected()
    {
        float radius = areaRadius;
        Vector2 center = areaCenter;

        if (radius <= 0f)
        {
            CircleCollider2D circle = FindFirstObjectByType<CircleCollider2D>();
            if (circle != null)
            {
                radius = circle.radius * Mathf.Max(circle.transform.lossyScale.x, circle.transform.lossyScale.y);
                center = (Vector2)circle.transform.position + circle.offset;
            }
        }

        Vector2 currentPos = transform.position;

        if (radius > 0f)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawWireSphere(center, radius);

            float margin = radius * dangerZoneRatio;
            Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.6f);
            Gizmos.DrawWireSphere(center, radius - margin);

            if (Vector2.Distance(currentPos, center) > (radius - margin))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(currentPos, center);
            }
        }

        if (targetPlayer != null)
        {
            Vector2 targetPos = targetPlayer.position;
            Vector2 dirToTarget = (targetPos - currentPos).normalized;
            float distanceToTarget = Vector2.Distance(currentPos, targetPos);

            Gizmos.color = new Color(0f, 1f, 1f, 0.7f);
            Gizmos.DrawWireSphere(currentPos, maxDashDistance);

            bool inRange = distanceToTarget <= maxDashDistance;
            Gizmos.color = inRange ? Color.green : Color.gray;
            Gizmos.DrawLine(currentPos, targetPos);

            Vector2 currentFacing = transform.up;
            float alignment = Vector2.Dot(currentFacing, dirToTarget);
            bool isAligned = alignment >= dashAlignmentThreshold;

            Gizmos.color = isAligned ? Color.green : Color.red;
            Gizmos.DrawRay(currentPos, currentFacing * 1.5f);

            if (sumoMovement != null && sumoMovement.SumoData != null)
            {
                float dashSpeed = sumoMovement.SumoData.dashForce / sumoMovement.SumoData.mass;
                float estimatedDistance = dashSpeed * sumoMovement.SumoData.dashDuration;
                Vector2 predictedEndPos = currentPos + (dirToTarget * estimatedDistance);

                bool dangerousDash = (radius > 0f) && (Vector2.Distance(predictedEndPos, center) >= (radius - 0.5f));

                Gizmos.color = dangerousDash ? Color.red : Color.magenta;
                Gizmos.DrawLine(currentPos, predictedEndPos);
                Gizmos.DrawCube(predictedEndPos, Vector3.one * 0.3f);
            }
        }
    }
}
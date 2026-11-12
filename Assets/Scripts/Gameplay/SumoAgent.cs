using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SumoMovement), typeof(SumoHealth), typeof(Rigidbody2D))]
public class SumoAgent : Agent
{
    [Header("Setup")]
    [SerializeField] private SumoBalanceConfig balanceConfig;
    [SerializeField] private int luckyNumber = 1;
    [SerializeField] private SumoAgent opponent;
    [SerializeField] private CircleCollider2D ring;
    [SerializeField] private bool spawnOnLeft = true;

    [Header("Rewards")]
    [SerializeField] private float winReward = 1f;
    [SerializeField] private float loseReward = -1f;
    [SerializeField] private float damageRewardScale = 0.5f;
    [SerializeField] private float potentialScale = 0.05f;   // récompense de pression vers le bord
    [SerializeField, Range(0f, 1f)] private float spawnRadiusRatio = 0.5f;

    private SumoMovement movement;
    private SumoHealth health;
    private Rigidbody2D rb;
    private bool eliminated;
    private float lastSelfHp;
    private float lastOppHp;
    private float lastPotential;
    private bool potentialReady;

    private Vector2 ArenaCenter => ring.transform.TransformPoint(ring.offset);
    private float ArenaRadius => ring.radius * Mathf.Max(ring.transform.lossyScale.x, ring.transform.lossyScale.y);

    public override void Initialize()
    {
        movement = GetComponent<SumoMovement>();
        health = GetComponent<SumoHealth>();
        rb = GetComponent<Rigidbody2D>();

        movement.Initialize(balanceConfig.GenerateDataForNumber(luckyNumber));
        movement.OnEliminated += OnSelfEliminated;
    }

    private void OnDestroy()
    {
        if (movement != null) movement.OnEliminated -= OnSelfEliminated;
    }

    public override void OnEpisodeBegin()
    {
        eliminated = false;
        movement.ResetState();
        health.ResetState();

        // Placement : chaque agent part d'un côté de l'anneau, avec un léger aléa
        float side = spawnOnLeft ? -1f : 1f;
        float jitter = Random.Range(-0.3f, 0.3f);
        Vector2 dir = new Vector2(side * Mathf.Cos(jitter), Mathf.Sin(jitter)).normalized;
        rb.position = ArenaCenter + dir * (ArenaRadius * spawnRadiusRatio);
        rb.rotation = 0f;

        lastSelfHp = health.HealthPercent;
        lastOppHp = opponent.health.HealthPercent;
        potentialReady = false;   // le potentiel est initialisé au premier décision, quand les deux sumos sont replacés
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float radius = ArenaRadius;
        Vector2 center = ArenaCenter;

        // Moi (11 valeurs)
        sensor.AddObservation((rb.position - center) / radius);
        sensor.AddObservation(rb.linearVelocity / movement.SumoData.maxSpeed);
        sensor.AddObservation(movement.Facing);
        AddSumoState(sensor, movement, health);

        // Adversaire (11 valeurs)
        sensor.AddObservation((opponent.rb.position - rb.position) / radius);
        sensor.AddObservation(opponent.rb.linearVelocity / opponent.movement.SumoData.maxSpeed);
        sensor.AddObservation(opponent.movement.Facing);
        AddSumoState(sensor, opponent.movement, opponent.health);
    }

    private static void AddSumoState(VectorSensor sensor, SumoMovement m, SumoHealth h)
    {
        sensor.AddObservation(m.GetDashCooldownNormalized());
        sensor.AddObservation(m.IsDashing ? 1f : 0f);
        sensor.AddObservation(h.IsStunned ? 1f : 0f);
        sensor.AddObservation(h.HealthPercent);
        sensor.AddObservation(m.SumoData.mass / 5f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (eliminated) return;

        Vector2 move = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        movement.SetMovementInput(Vector2.ClampMagnitude(move, 1f));

        if (actions.DiscreteActions[0] == 1 && movement.GetDashCooldownNormalized() <= 0f)
        {
            // Pénalité seulement pour un dash inutile : adversaire hors de portée ou mal aligné
            Vector2 toOpp = opponent.rb.position - rb.position;
            float reach = movement.SumoData.dashForce / movement.SumoData.mass * movement.SumoData.dashDuration;
            bool useful = toOpp.magnitude <= reach * 1.2f
                       && Vector2.Dot(movement.Facing, toOpp.normalized) > 0.7f;

            if (!useful) AddReward(-0.02f);
            movement.TryDash();
        }

        // Récompense de mise en forme : dégâts infligés et subis
        float selfHp = health.HealthPercent;
        float oppHp = opponent.health.HealthPercent;
        if (oppHp < lastOppHp) AddReward(damageRewardScale * (lastOppHp - oppHp));
        if (selfHp < lastSelfHp) AddReward(-damageRewardScale * (lastSelfHp - selfHp));
        lastSelfHp = selfHp;
        lastOppHp = oppHp;

        // Pression vers le bord : récompense quand l'adversaire se rapproche du bord
        // (et quand je reste au centre). Potentiel différentiel, donc il ne change pas l'objectif.
        float potential = Potential();
        if (potentialReady) AddReward(potentialScale * (potential - lastPotential));
        lastPotential = potential;
        potentialReady = true;

        // Légère pression temporelle pour éviter le surplace
        AddReward(-0.0005f);
    }

    // Positif quand l'adversaire est plus près du bord que moi
    private float Potential()
    {
        float radius = ArenaRadius;
        float self = Vector2.Distance(rb.position, ArenaCenter) / radius;
        float opp = Vector2.Distance(opponent.rb.position, ArenaCenter) / radius;
        return opp - self;
    }

    private void OnSelfEliminated(GameObject _)
    {
        if (eliminated) return;
        eliminated = true;

        AddReward(loseReward);
        EndEpisode();

        if (opponent != null && !opponent.eliminated)
        {
            opponent.AddReward(winReward);
            opponent.EndEpisode();
        }
    }

    // Contrôle manuel pour tester l'agent (Behavior Type = Heuristic Only)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        var continuous = actionsOut.ContinuousActions;
        var discrete = actionsOut.DiscreteActions;
        continuous[0] = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        continuous[1] = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        discrete[0] = kb.spaceKey.isPressed ? 1 : 0;
    }
}
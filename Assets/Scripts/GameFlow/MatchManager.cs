using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ami.BroAudio;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private SumoBalanceConfig balanceConfig;
    [SerializeField] private SoundID fightStartSound;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPrefab;
    [SerializeField] private Collider2D areaCollider;
    [SerializeField] private float teamSpacing = 1f;
    [SerializeField] private float leftTeamX = -2f;
    [SerializeField] private float arcRadius = 4f;
    [SerializeField] private float arcAngle = 120f;
    [SerializeField] private Vector3 arcCenter = new Vector3(-2f, 0f, 0f);

    private readonly Dictionary<SumoMovement, bool> aliveSumos = new Dictionary<SumoMovement, bool>();
    private readonly List<SumoMovement> spawned = new List<SumoMovement>();
    private bool fightOver;

    public event Action<bool> OnFightEnded;

    public SumoData GetDataFor(int luckyNumber) => balanceConfig.GenerateDataForNumber(luckyNumber);

    public void StartFight(IList<SumoSetup> leftTeam, IList<SumoSetup> rightTeam)
    {
        EndGame();
        fightOver = false;

        List<SumoMovement> left = SpawnTeam(leftTeam, null, true);
        SpawnTeam(rightTeam, left[0].transform, false);

        SumoCamera.Instance?.SetTargets(spawned.Select(sumo => sumo.transform));
        BroAudio.Play(fightStartSound);
    }

    public void EndGame()
    {
        foreach (SumoMovement sumo in spawned)
        {
            if (sumo == null) continue;
            sumo.OnEliminated -= HandleElimination;
            Destroy(sumo.gameObject);
        }

        spawned.Clear();
        aliveSumos.Clear();
        SumoCamera.Instance?.SetTargets(Enumerable.Empty<Transform>());
    }

    private List<SumoMovement> SpawnTeam(IList<SumoSetup> team, Transform target, bool isLeftTeam)
    {
        List<SumoMovement> result = new List<SumoMovement>();

        for (int i = 0; i < team.Count; i++)
        {
            result.Add(AddSumo(team[i], GetSpawnPosition(i, team.Count, isLeftTeam), target, isLeftTeam));
        }

        return result;
    }

    private Vector3 GetSpawnPosition(int index, int count, bool isLeftTeam)
    {
        if (isLeftTeam)
        {
            float y = (index - (count - 1) / 2f) * teamSpacing;
            return new Vector3(leftTeamX, y, 0f);
        }

        float t = count == 1 ? 0.5f : index / (float)(count - 1);
        float angle = Mathf.Lerp(-arcAngle / 2f, arcAngle / 2f, t) * Mathf.Deg2Rad;
        return arcCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * arcRadius;
    }

    private SumoMovement AddSumo(SumoSetup setup, Vector3 spawnPosition, Transform target, bool isLeftTeam)
    {
        GameObject prefab = setup.isAi ? aiPrefab : playerPrefab;
        GameObject sumoInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        SumoMovement sumoMovement = sumoInstance.GetComponent<SumoMovement>();
        sumoMovement.Initialize(balanceConfig.GenerateDataForNumber(setup.luckyNumber));
        sumoMovement.OnEliminated += HandleElimination;
        spawned.Add(sumoMovement);
        aliveSumos[sumoMovement] = isLeftTeam;

        sumoInstance.GetComponent<SumoVisuals>()?.SetColor(setup.color);

        if (setup.isAi)
        {
            AiAction aiAction = sumoInstance.GetComponent<AiAction>();
            aiAction.SetTarget(target);
            aiAction.SetAreaData(areaCollider);
        }
        else
        {
            sumoInstance.GetComponent<PlayerAction>().SetControlScheme(setup.controls);
        }

        return sumoMovement;
    }

    private void HandleElimination(GameObject eliminatedSumo)
    {
        SumoMovement sumo = eliminatedSumo.GetComponent<SumoMovement>();
        if (sumo == null || !aliveSumos.Remove(sumo)) return;

        if (fightOver)
        {
            ResetSurvivorsTargets();
            return;
        }

        bool leftRemaining = aliveSumos.ContainsValue(true);
        bool rightRemaining = aliveSumos.ContainsValue(false);
        if (leftRemaining && rightRemaining) return;

        fightOver = true;
        ResetSurvivorsTargets();
        OnFightEnded?.Invoke(leftRemaining);
    }

    private void ResetSurvivorsTargets()
    {
        List<SumoMovement> survivors = aliveSumos.Keys.ToList();
        foreach (SumoMovement sumo in survivors)
        {
            AiAction ai = sumo.GetComponent<AiAction>();
            if (ai == null) continue;

            List<SumoMovement> others = survivors.Where(other => other != sumo && other.GetComponent<AiAction>() != null).ToList();
            ai.SetTarget(others.Count > 0 ? others[UnityEngine.Random.Range(0, others.Count)].transform : null);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private SumoBalanceConfig balanceConfig;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPrefab;
    [SerializeField] private Collider2D areaCollider;
    [SerializeField] private float teamSpacing = 1f;
    [SerializeField] private float leftTeamX = -2f;
    [SerializeField] private float rightTeamX = 2f;

    private readonly Dictionary<SumoMovement, bool> aliveSumos = new Dictionary<SumoMovement, bool>();
    private readonly List<SumoMovement> spawned = new List<SumoMovement>();
    private bool fightOver;

    public event Action<bool> OnFightEnded;

    public SumoData GetDataFor(int luckyNumber) => balanceConfig.GenerateDataForNumber(luckyNumber);

    public void StartFight(IList<SumoSetup> leftTeam, IList<SumoSetup> rightTeam)
    {
        EndGame();
        fightOver = false;

        List<SumoMovement> left = SpawnTeam(leftTeam, leftTeamX, null, true);
        SpawnTeam(rightTeam, rightTeamX, left[0].transform, false);

        SumoCamera.Instance?.SetTargets(spawned.Select(sumo => sumo.transform));
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

    private List<SumoMovement> SpawnTeam(IList<SumoSetup> team, float x, Transform target, bool isLeftTeam)
    {
        List<SumoMovement> result = new List<SumoMovement>();

        for (int i = 0; i < team.Count; i++)
        {
            float y = (i - (team.Count - 1) / 2f) * teamSpacing;
            result.Add(AddSumo(team[i], new Vector3(x, y, 0f), target, isLeftTeam));
        }

        return result;
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
        if (fightOver || sumo == null || !aliveSumos.Remove(sumo)) return;

        bool leftRemaining = aliveSumos.ContainsValue(true);
        bool rightRemaining = aliveSumos.ContainsValue(false);
        if (leftRemaining && rightRemaining) return;

        fightOver = true;
        OnFightEnded?.Invoke(leftRemaining);
    }
}

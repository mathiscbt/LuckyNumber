using UnityEngine;
using System.Collections.Generic;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private SumoBalanceConfig balanceConfig;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject aiPrefab;
    [SerializeField] private Collider2D areaCollider;
    

    private List<SumoMovement> followedSumos = new List<SumoMovement>();    


    public void StartGame(int playerNumber = 1, int aiNumber = 1)
    {
        SumoData sumoData = balanceConfig.GenerateDataForNumber(playerNumber);
        GameObject sumoInstancePlayer = Instantiate(playerPrefab, new Vector3(-2, 0, 0), Quaternion.identity);
        SumoMovement sumoMovement = sumoInstancePlayer.GetComponent<SumoMovement>();
        sumoMovement.Initialize(sumoData);
        sumoMovement.OnEliminated += HandleElimination;
        followedSumos.Add(sumoMovement);

        sumoData = balanceConfig.GenerateDataForNumber(aiNumber);
        GameObject sumoInstance = Instantiate(aiPrefab, new Vector3(2, 0, 0), Quaternion.identity);
        sumoMovement = sumoInstance.GetComponent<SumoMovement>();
        sumoMovement.Initialize(sumoData);
        sumoMovement.OnEliminated += HandleElimination;
        followedSumos.Add(sumoMovement);

        AiAction aiAction = sumoInstance.GetComponent<AiAction>();
        aiAction.SetTarget(sumoInstancePlayer.transform);
        aiAction.SetAreaData(areaCollider);
    }

    public void EndGame()
    {
        foreach (SumoMovement sumoMovement in followedSumos)
        {
            Destroy(sumoMovement.gameObject);
        }
    }

    private void HandleElimination(GameObject eliminatedSumo)
    {
        SumoMovement movement = eliminatedSumo.GetComponent<SumoMovement>();
        
        if (movement != null)
        {
            movement.OnEliminated -= HandleElimination;
        }

        if (eliminatedSumo.GetComponent<PlayerAction>() != null)
        {
            Debug.Log("Player has been eliminated!");
        }
        else
        {
            Debug.Log("AI has been eliminated!");
        }
    }

    void OnDestroy()
    {
        foreach (var sumo in followedSumos)
        {
            sumo.OnEliminated -= HandleElimination;
        }
    }
}
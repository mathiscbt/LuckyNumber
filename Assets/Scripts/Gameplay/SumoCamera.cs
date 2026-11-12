using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class SumoCamera : MonoBehaviour
{
    public static SumoCamera Instance { get; private set; }
    
    private CinemachineTargetGroup targetGroup;
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float arenaCenterWeight = 0.5f;

    private void Awake()
    {
        Instance = this;
        if (targetGroup == null) targetGroup = GetComponent<CinemachineTargetGroup>();
    }

    public void SetTargets(IEnumerable<Transform> sumos)
    {
        targetGroup.Targets.Clear();

        if (arenaCenter != null)
        {
            targetGroup.AddMember(arenaCenter, weight: 0.5f, radius: 4f);
        }

        foreach (Transform sumo in sumos)
        {
            if (sumo != null)
            {
                targetGroup.AddMember(sumo, weight: 1f, radius: 1.8f);
            }
        }
    }

    public void RemoveTarget(Transform sumo)
    {
        if (sumo != null) targetGroup.RemoveMember(sumo);
    }
}
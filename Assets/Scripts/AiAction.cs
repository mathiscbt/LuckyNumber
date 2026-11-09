using UnityEngine;

[RequireComponent(typeof(SumoMovement))]
public class AiAction : MonoBehaviour
{
    private SumoMovement sumoMovement;
    private Transform targetPlayer;
    
    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
    }


    private void Update()
    {
        if (targetPlayer != null)
        {
            // Calcule un vecteur mathématique qui pointe du bot vers le joueur
            Vector2 direction = (targetPlayer.position - transform.position).normalized;
            
            // Envoie ce vecteur à votre composant de mouvement, exactement comme OnMove()
            sumoMovement.SetMovementInput(direction);
        }
    }

    public void SetTarget(Transform target)
    {
        targetPlayer = target;
    }
}
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class AreaChecker : MonoBehaviour
{
    
    void Start()
    {
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Sumo"))
        {
            collision.GetComponent<SumoMovement>()?.Eliminate();
            
        }
    }
}
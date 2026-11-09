using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SumoMovement))]
public class PlayerAction : MonoBehaviour
{
    public enum ControlScheme { KeyboardOnly, Mouse }

    [Header("Control Configuration")]
    [Tooltip("Choose between KeyboardOnly (ZQSD + Space) or Mouse + Keybord (Mouse Movement + Space)")]
    [SerializeField] private ControlScheme controlScheme = ControlScheme.KeyboardOnly;

    private SumoMovement sumoMovement;
    private Camera mainCamera;

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        mainCamera = Camera.main;
    }

    private void OnMove(InputValue value)
    {
        if (controlScheme != ControlScheme.KeyboardOnly) return;
        sumoMovement.SetMovementInput(value.Get<Vector2>());
    }

    private void OnJump()
    {
        sumoMovement.TryDash();
    }

    private void Update()
    {
        if (controlScheme == ControlScheme.Mouse)
        {
            Vector2 mouseDirection = GetDirectionToMouse();
            sumoMovement.SetMovementInput(mouseDirection);
        }
    }

    private Vector2 GetDirectionToMouse()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        return ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
    }
}
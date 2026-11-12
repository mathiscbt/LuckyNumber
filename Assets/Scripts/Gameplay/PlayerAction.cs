using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlScheme
{
    MouseAndSpace,
    ZQSDAndSpace,
    ArrowsAndRShift
}

[RequireComponent(typeof(SumoMovement))]
public class PlayerAction : MonoBehaviour
{
    private SumoMovement sumoMovement;

    [SerializeField] private ControlScheme controlScheme = ControlScheme.MouseAndSpace;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        sumoMovement = GetComponent<SumoMovement>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (sumoMovement == null) return;
        UpdateInput();
    }

    public void SetControlScheme(ControlScheme newScheme)
    {
        controlScheme = newScheme;
    }

    private void UpdateInput()
    {
        Vector2 movement = Vector2.zero;
        bool dashRequested = false;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null) return;

        switch (controlScheme)
        {
            case ControlScheme.MouseAndSpace:
                if (mouse != null)
                {
                    Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
                    Vector2 directionToMouse = (Vector2)(mouseWorldPos - transform.position);

                    if (directionToMouse.sqrMagnitude > 0.01f)
                    {
                        movement = directionToMouse.normalized;
                    }
                }
                dashRequested = keyboard.spaceKey.wasPressedThisFrame;
                break;

            case ControlScheme.ZQSDAndSpace:
                if (keyboard.zKey.isPressed || keyboard.wKey.isPressed) movement.y += 1f;
                if (keyboard.sKey.isPressed) movement.y -= 1f;
                if (keyboard.qKey.isPressed || keyboard.aKey.isPressed) movement.x -= 1f;
                if (keyboard.dKey.isPressed) movement.x += 1f;

                dashRequested = keyboard.spaceKey.wasPressedThisFrame;
                break;

            case ControlScheme.ArrowsAndRShift:
                if (keyboard.upArrowKey.isPressed) movement.y += 1f;
                if (keyboard.downArrowKey.isPressed) movement.y -= 1f;
                if (keyboard.leftArrowKey.isPressed) movement.x -= 1f;
                if (keyboard.rightArrowKey.isPressed) movement.x += 1f;

                dashRequested = keyboard.rightShiftKey.wasPressedThisFrame;
                break;
        }

        sumoMovement.SetMovementInput(movement.normalized);

        if (dashRequested)
        {
            sumoMovement.TryDash();
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Player Input Values")] 
    public Vector2 Move;
    public bool ClickMove;
    public float Zoom;
    
    [Header("Mouse Cursor Settings")]
    public bool CursorInputForLook = true;
    public enum CursorLockState
    {
        Locked,
        Confined,
        Unlocked,
    }
    
    public Inputs Actions;
    [SerializeField] private InputActionAsset _inputActionAsset;
    public Inputs.PlayerActions PlayerActions => Actions.Player;
    
    void Awake()
    {
        Actions = new Inputs();
        Actions.Enable();
        Actions.Player.Move.started += OnMove;
        Actions.Player.Move.performed += OnMove;
        Actions.Player.Move.canceled += OnMove;
        Actions.Player.ClickMove.started += OnClickMove;
        Actions.Player.ClickMove.performed += OnClickMove;
        Actions.Player.ClickMove.canceled += OnClickMove;
        Actions.Player.Zoom.started += OnZoom;
        Actions.Player.Zoom.performed += OnZoom;
        Actions.Player.Zoom.canceled += OnZoom;
    }
    
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput(context.ReadValue<Vector2>());
    }
    
    public void OnClickMove(InputAction.CallbackContext context) {
        ClickMoveInput(context.ReadValueAsButton());
    }
    
    public void OnZoom(InputAction.CallbackContext context) {
        ZoomInput(context.ReadValue<Vector2>());
    }
    
    public void MoveInput(Vector2 newMoveDirection)
    {
        Move = newMoveDirection;
    }
    
    public void ClickMoveInput(bool newClickMoveState)
    {
        ClickMove = newClickMoveState;
    }
    
    public void ZoomInput(Vector2 newZoomDelta)
    {
        Zoom = newZoomDelta.y;
    }
}

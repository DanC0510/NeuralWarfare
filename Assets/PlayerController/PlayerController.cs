using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private Vector2 mousePos;
    [SerializeField] private Vector2 moveTo;
    [SerializeField] private bool doMove;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {        
        if(PlayerInputs.)
        if (doMove)
        {
            transform.position = Vector2.MoveTowards(transform.position, moveTo, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, moveTo) < 0.1f)
            {
                doMove = false;
            }
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
         = context.ReadValue<Vector2>();
        Debug.Log(mousePos);
    }

    void OnClickMove()
    {
        doMove = true;
        moveTo = (Vector2)playerCamera.ScreenToWorldPoint(mousePos);
        Debug.Log("moveTo: " + moveTo);
    }
}

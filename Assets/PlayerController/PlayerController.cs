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
    
    [SerializeField] private PlayerInputs playerInputs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        playerInputs = GetComponent<PlayerInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputs.Move != Vector2.zero)
        {
            mousePos = playerInputs.Move;
        }
        if (playerInputs.ClickMove)
        {
            moveTo = playerCamera.ScreenToWorldPoint(mousePos);
            doMove = true;
        }
        if (doMove)
        {
            transform.position = Vector2.MoveTowards(transform.position, moveTo, moveSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, moveTo) < 0.1f)
            {
                doMove = false;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float camHeight;
    [SerializeField] private float camAngle;
    [SerializeField] private Vector2 mousePos;
    [SerializeField] private Vector3 moveTo;
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
            Vector3 screenToWorldPoint = playerCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, playerCamera.nearClipPlane));
            moveTo = new Vector3(screenToWorldPoint.x, camHeight, screenToWorldPoint.z);
            doMove = true;
        }
        if (doMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTo, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(camAngle, 0, 0);
            if (Vector2.Distance(transform.position, moveTo) < 0.1f)
            {
                doMove = false;
            }
        }
    }
}

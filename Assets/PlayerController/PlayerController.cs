using System.Collections;
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
    [SerializeField] private float groundPlaneY;
    
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
        if (playerInputs.ClickMove)
        {
            mousePos = playerInputs.Move;
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            float t = (groundPlaneY - ray.origin.y) / ray.direction.y;
            Vector3 hitPoint = ray.GetPoint(t);
            moveTo = new Vector3(hitPoint.x, camHeight, hitPoint.z);

            if (!doMove)
            {
                StartCoroutine(MoveCam(moveTo, Quaternion.Euler(camAngle, 0, 0)));
            }
        }
    }

    IEnumerator MoveCam(Vector3 moveTowards, Quaternion rotateTowards)
    {
        doMove = true;
        while (Vector3.Distance(transform.position, moveTowards) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTowards, moveSpeed * Time.deltaTime);
            transform.rotation = rotateTowards;
            yield return null;
        }
        doMove = false;
        
    }
}

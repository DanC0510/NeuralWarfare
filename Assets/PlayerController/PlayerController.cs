using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveDelta;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float camHeight;
    [SerializeField] private float camAngle;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Vector2 mousePos;
    [SerializeField] private Vector3 moveTo;
    [SerializeField] private Vector2[] screenCorners = new Vector2[4];
    [SerializeField] private bool doMove;
    [SerializeField] private bool doRotate;
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
            //Calculate the offset from the center of the screen
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 offset = new Vector2((mousePos.x - screenCenter.x) / screenCenter.x, (mousePos.y - screenCenter.y) / screenCenter.y);

            //Calculate distance
            Vector3 targetOffset = new Vector3(offset.x, 0, offset.y) * moveDelta;
            

            moveTo = transform.position + targetOffset;
            moveTo.y = camHeight;
            
            if (moveTo.x < screenCorners[0].x && moveTo.z < screenCorners[0].y) 
            { 
                moveTo.x = screenCorners[0].x;
                moveTo.z = screenCorners[0].y;
            }
            else if (moveTo.x > screenCorners[1].x && moveTo.z < screenCorners[1].y) 
            { 
                moveTo.x = screenCorners[1].x;
                moveTo.z = screenCorners[1].y;
            }
            else if (moveTo.x < screenCorners[2].x && moveTo.z > screenCorners[2].y) 
            { 
                moveTo.x = screenCorners[2].x;
                moveTo.z = screenCorners[2].y;
            }
            else if (moveTo.x > screenCorners[3].x && moveTo.z > screenCorners[3].y) 
            { 
                moveTo.x = screenCorners[3].x;
                moveTo.z = screenCorners[3].y;
            }

            if (!doMove)
            {
                StartCoroutine(MoveCam(moveTo));
            }
            CalculateRotation();
        }

        if (playerInputs.Zoom != 0)
        {
            CalculateRotation();
        }
    }

    void CalculateRotation()
    {
        camAngle -= playerInputs.Zoom * rotateSpeed * Time.deltaTime;
        camAngle = Mathf.Clamp(camAngle, 45, 70);
            
        if (!doRotate)
        {
            StartCoroutine(RotateCam(Quaternion.Euler(camAngle, 0, 0)));
        }
        
        camHeight -= playerInputs.Zoom * zoomSpeed * Time.deltaTime;
        camHeight = Mathf.Clamp(camHeight, 10f, 20f);
        
        moveTo.y = camHeight;
        
        if (!doMove)
        {
            StartCoroutine(MoveCam(moveTo));
        }
    }

    IEnumerator MoveCam(Vector3 moveTowards)
    {
        doMove = true;
        while (Vector3.Distance(transform.position, moveTowards) > 0.1f)
        {
            moveTowards.x = moveTo.x;
            moveTowards.y = camHeight;
            moveTowards.z = moveTo.z;
            transform.position = Vector3.MoveTowards(transform.position, moveTowards, moveSpeed * Time.deltaTime);
            yield return null;
        }
        doMove = false;
    }
    
    IEnumerator RotateCam(Quaternion rotateTowards)
    {
        doRotate = true;
        while (transform.rotation != rotateTowards)
        {
            rotateTowards = Quaternion.Euler(camAngle, 0, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotateTowards, zoomSpeed * Time.deltaTime);
            yield return null;
        }
        doRotate = false;
    }
}

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
    [SerializeField] private Vector2 mousePos;
    [SerializeField] private Vector3 moveTo;
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

            if (!doMove)
            {
                StartCoroutine(MoveCam(moveTo));
            }
        }
    }

    IEnumerator MoveCam(Vector3 moveTowards)
    {
        doMove = true;
        while (Vector3.Distance(transform.position, moveTowards) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTowards, moveSpeed * Time.deltaTime);
            yield return null;
        }
        doMove = false;
    }
    
    IEnumerator RotateCam(Quaternion rotateTowards)
    {
        doMove = true;
        while (transform.rotation != rotateTowards)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotateTowards, zoomSpeed * Time.deltaTime);
            yield return null;
        }
        doMove = false;
    }
}

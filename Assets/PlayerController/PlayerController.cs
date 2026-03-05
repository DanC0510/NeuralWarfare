using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private GameObject selectedObject;

    [Header("UI Actions")] 
    [SerializeField] private GameObject genericUI;
    [SerializeField] private GameObject settlerUI;
    [SerializeField] private GameObject meleeUI;
    [SerializeField] private GameObject rangedUI;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button sleepButton;
    [SerializeField] private Button settleCityButton;
    [SerializeField] private Button meleeAttackButton;
    [SerializeField] private Button rangedAttackButton;
    
    [Header("Input Actions")]
    [SerializeField] private PlayerInputs playerInputs;

    private bool selectingTarget;
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
            
            
            for (int i = 0; i < screenCorners.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if (moveTo.x < screenCorners[i].x)
                    {
                        moveTo.x = screenCorners[i].x;
                    }
                }
                else
                {
                    if (moveTo.x > screenCorners[i].x)
                    {
                        moveTo.x = screenCorners[i].x;
                    }
                }
                if(i <= 1 && moveTo.z < screenCorners[i].y)
                {
                    moveTo.z = screenCorners[i].y;
                }
                else if(i >= 2 && moveTo.z > screenCorners[i].y)
                {
                    moveTo.z = screenCorners[i].y;
                }
                
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

        if (playerInputs.Interact && !selectingTarget)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return; //this checks if the mouse is over a UI element, if it is, we don't want to interact with the world
            }
            
            mousePos = playerInputs.Move;
            Ray ray = playerCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Interactable"))
                {
                    selectedObject = hit.collider.gameObject;
                    
                    genericUI.SetActive(true);
                    moveButton.onClick.AddListener(selectedObject.GetComponent<Unit>().Move);
                    skipButton.onClick.AddListener(selectedObject.GetComponent<Unit>().Skip);
                    sleepButton.onClick.AddListener(selectedObject.GetComponent<Unit>().Sleep);
                    switch(selectedObject.GetComponent<Unit>().Type)
                    {
                        case(Unit.UnitType.Settler):
                            settlerUI.SetActive(true);
                            meleeUI.SetActive(false);
                            rangedUI.SetActive(false);
                            //add listeners here
                            settleCityButton.onClick.AddListener(() => {
                                selectedObject.GetComponent<Settler>().PlaceCity();
                                selectedObject = null;
                                genericUI.SetActive(false);
                                settlerUI.SetActive(false);
                                moveButton.onClick.RemoveAllListeners();
                                skipButton.onClick.RemoveAllListeners();
                                sleepButton.onClick.RemoveAllListeners();
                                settleCityButton.onClick.RemoveAllListeners();
                            });
                            break;
                        case(Unit.UnitType.Melee):
                            meleeUI.SetActive(true);
                            settlerUI.SetActive(false);
                            rangedUI.SetActive(false);
                            //add listeners here
                            meleeAttackButton.onClick.RemoveAllListeners();
                            meleeAttackButton.onClick.AddListener(WaitForTarget);
                            break;
                        case(Unit.UnitType.Ranged):
                            rangedUI.SetActive(true);
                            settlerUI.SetActive(false);
                            meleeUI.SetActive(false);
                            //add listeners here
                            break;
                    }
                }
            }
            else 
            {
                selectedObject = null;
                genericUI.SetActive(false);
                meleeUI.SetActive(false);
                rangedUI.SetActive(false);
                settlerUI.SetActive(false);
                moveButton.onClick.RemoveAllListeners();
                skipButton.onClick.RemoveAllListeners();
                sleepButton.onClick.RemoveAllListeners();
                settleCityButton.onClick.RemoveAllListeners();
            }
            //bring up UI for selected object here
        }
    }

    void WaitForTarget()
    {
        StartCoroutine(SelectTarget());
    }

    IEnumerator SelectTarget()
    {
        selectingTarget = true;
        Unit selectedTarget = null;
        while (selectedTarget == null)
        {
            yield return null;
            Ray ray = playerCamera.ScreenPointToRay(playerInputs.Move);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Interactable") && hit.collider.gameObject != this.gameObject && playerInputs.Interact)
                {
                    selectedTarget = hit.collider.gameObject.GetComponent<Unit>();
                }
            }
        }
        selectedObject.GetComponent<Unit>().Attack(selectedTarget);
        selectedObject = null;
        genericUI.SetActive(false);
        settlerUI.SetActive(false);
        moveButton.onClick.RemoveAllListeners();
        skipButton.onClick.RemoveAllListeners();
        sleepButton.onClick.RemoveAllListeners();
        settleCityButton.onClick.RemoveAllListeners();
        selectingTarget = false;
    }

    IEnumerator SelectTile()
    {
        selectingTarget = true;
        Tile selectedTarget = null;
        while (selectedTarget == null)
        {
            yield return null;
            Ray ray = playerCamera.ScreenPointToRay(playerInputs.Move);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform && playerInputs.Interact)
                {
                    selectedTarget = Helpers.GetTileAtWorldPos(new Vector2(hit.transform.position.x, hit.transform.position.z));
                }
            }
        }
        selectedObject.GetComponent<Unit>().Move();
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

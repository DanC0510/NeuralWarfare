using System.Collections;
using TMPro;
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
    public Vector3 MoveTo { get => moveTo; set => moveTo = value; }
    public float CamHeight { get => camHeight; }

    [Header("Unit UI Actions")]
    [SerializeField] private GameObject genericUnitUI;
    [SerializeField] private GameObject settlerUI;
    [SerializeField] private GameObject meleeUI;
    [SerializeField] private GameObject rangedUI;
    [SerializeField] private GameObject builderUI;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button sleepButton;
    [SerializeField] private Button settleCityButton;
    [SerializeField] private Button meleeAttackButton;
    [SerializeField] private Button rangedAttackButton;
    [SerializeField] private Button buildButton;
    
    [Header("City UI Actions")]
    [SerializeField] private GameObject genericCityUI;
    [SerializeField] private Button produceButton;
    [SerializeField] private Button fortifyCityButton;
    [SerializeField] private Button closeCityButton;
    [SerializeField] private TMP_Dropdown productionDropdown;

    [Header("Generic UI Actions")] 
    [SerializeField] private GameObject genericUI;
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TMP_Text turnNumberText;
    
    [Header("Input Actions")]
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private bool selectingTarget;

    [Header("Player Settings")]
    [SerializeField] private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        playerCamera = GetComponent<Camera>();
        playerInputs = GetComponent<PlayerInputs>();
        
        moveButton.onClick.AddListener(() =>
        {
            StartCoroutine(SelectTile());
        });
        skipButton.onClick.AddListener(() =>
        {
            selectedObject.GetComponent<Unit>().Skip();
            ResetUI();
        });
        sleepButton.onClick.AddListener(() =>
        {
            selectedObject.GetComponent<Unit>().Sleep();
            ResetUI();
        });
        settleCityButton.onClick.AddListener(() => {
            selectedObject.GetComponent<Settler>().PlaceCity();
            ResetUI();
        });
        meleeAttackButton.onClick.AddListener(() =>
        {
            StartCoroutine(SelectTarget(selectedObject.GetComponent<Unit>()));
        });
        rangedAttackButton.onClick.AddListener(() =>
        {
            StartCoroutine(SelectTarget(selectedObject.GetComponent<Unit>()));
        });
        buildButton.onClick.AddListener(() =>
        {
            selectedObject.GetComponent<Builder>().Build();
        });
        produceButton.onClick.AddListener(() =>
        {
            selectedObject.GetComponent<City>().StartProduction(productionDropdown.value);
            ResetUI();
        });
        fortifyCityButton.onClick.AddListener(() =>
        {
            selectedObject.GetComponent<City>().FortifyCity();
            ResetUI();
        });
        closeCityButton.onClick.AddListener(() =>
        {
            selectedObject.GetComponent<City>().CloseCity();
            ResetUI();
        });
        endTurnButton.onClick.AddListener(() =>
        {
            endTurnButton.interactable = false;
            TurnController.Instance.SetPlayerReady(player.PlayerIndex, true);
            //StartCoroutine(TurnController.Instance.CallNextTurn());
        });
    }

    // Update is called once per frame
    void Update()
    {
        turnNumberText.text = "Turn " + TurnController.Instance.turnNumber;
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
                if (hit.collider.CompareTag("InteractableUnit"))
                {
                    selectedObject = hit.collider.gameObject;
                    Unit unit = selectedObject.GetComponent<Unit>();
                    if(unit.OwningPlayer != player) { return; } //can't select units that aren't yours
                    
                    genericUnitUI.SetActive(true);
                    switch(unit.Type)
                    {
                        case(Unit.UnitType.Settler):
                            settlerUI.SetActive(true);
                            meleeUI.SetActive(false);
                            rangedUI.SetActive(false);
                            builderUI.SetActive(false);
                            break;
                        case(Unit.UnitType.Melee):
                            meleeUI.SetActive(true);
                            settlerUI.SetActive(false);
                            rangedUI.SetActive(false);
                            builderUI.SetActive(false);
                            break;
                        case(Unit.UnitType.Ranged):
                            rangedUI.SetActive(true);
                            settlerUI.SetActive(false);
                            meleeUI.SetActive(false);
                            builderUI.SetActive(false);
                            break;
                        case(Unit.UnitType.Builder):
                            builderUI.SetActive(true);
                            settlerUI.SetActive(false);
                            meleeUI.SetActive(false);
                            rangedUI.SetActive(false);
                            break;
                    }
                }

                else if (hit.collider.CompareTag("InteractableCity"))
                {
                    selectedObject = hit.collider.transform.parent.gameObject;
                    City city = selectedObject.GetComponent<City>();
                    if(city.OwningPlayer != player) { return; } //can't select city that isn't yours
                    genericCityUI.SetActive(true);
                }
            }
            else 
            {
                ResetUI();
            }
            //bring up UI for selected object here
        }
    }

    public void BringUpUI(object obj)
    {
        if (obj.GetType().BaseType == typeof(Unit))
        {
            Unit unit = (Unit)obj;
            selectedObject = unit.gameObject;
            if(unit.OwningPlayer != player) { return; } //can't select units that aren't yours
                    
            genericUnitUI.SetActive(true);
            switch(unit.Type)
            {
                case(Unit.UnitType.Settler):
                    settlerUI.SetActive(true);
                    meleeUI.SetActive(false);
                    rangedUI.SetActive(false);
                    builderUI.SetActive(false);
                    break;
                case(Unit.UnitType.Melee):
                    meleeUI.SetActive(true);
                    settlerUI.SetActive(false);
                    rangedUI.SetActive(false);
                    builderUI.SetActive(false);
                    break;
                case(Unit.UnitType.Ranged):
                    rangedUI.SetActive(true);
                    settlerUI.SetActive(false);
                    meleeUI.SetActive(false);
                    builderUI.SetActive(false);
                    break;
                case(Unit.UnitType.Builder):
                    builderUI.SetActive(true);
                    settlerUI.SetActive(false);
                    meleeUI.SetActive(false);
                    rangedUI.SetActive(false);
                    break;
            }
        }
        else if (obj.GetType() == typeof(City))
        {
            City city = (City)obj;
            selectedObject = city.gameObject;   
            if(city.OwningPlayer != player) { return; } //can't select city that isn't yours
            genericCityUI.SetActive(true);
        }
    }

    void ResetUI()
    {
        selectedObject = null;
        genericUnitUI.SetActive(false);
        genericCityUI.SetActive(false);
        meleeUI.SetActive(false);
        rangedUI.SetActive(false);
        settlerUI.SetActive(false);
        builderUI.SetActive(false);
    }

    IEnumerator SelectTarget(Unit unitSource)
    {
        selectingTarget = true;
        object selectedTarget = null;
        while (selectedTarget == null)
        {
            yield return null;
            Ray ray = playerCamera.ScreenPointToRay(playerInputs.Move);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (playerInputs.Interact && (hit.collider.CompareTag("InteractableUnit") || hit.collider.CompareTag("InteractableCity")) && hit.collider.gameObject != this.gameObject)
                {
                    if (hit.collider.gameObject.TryGetComponent<Unit>(out Unit unit))
                    {
                        if (unit.OwningPlayer == player)
                        {
                            break; //can't attack units that are yours
                        }
                        selectedTarget = hit.collider.gameObject.GetComponent<Unit>();
                    }
                    else if (hit.collider.gameObject.TryGetComponent<City>(out City city))
                    {
                        if (city.OwningPlayer == player)
                        {
                            break; //can't attack cities that are yours
                        }
                        selectedTarget = hit.collider.gameObject.GetComponent<City>();
                    }
                }
            }

            if (selectedTarget != null)
            {
                
                if(selectedTarget.GetType().BaseType == typeof(Unit))
                {
                    unitSource.Attack((Unit)selectedTarget);
                }
                else if(selectedTarget.GetType() == typeof(City))
                {
                    unitSource.Attack((City)selectedTarget);
                }
            }

            selectingTarget = false;
            ResetUI();
        }
    }

    IEnumerator SelectTile()
    {
        selectingTarget = true;
        Tile selectedTarget = null;
        while (selectedTarget == null)
        {
            yield return null;
            Ray ray = playerCamera.ScreenPointToRay(playerInputs.Move);
            if (playerInputs.Interact && Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.transform)
                {
                    Vector2 hitPoint = new Vector2(hit.point.x, hit.point.z);
                    selectedTarget = Helpers.GetTileAtWorldPos(hitPoint);
                }
            }
        }
        selectedObject.GetComponent<Unit>().GetPath(selectedTarget.Position);
        selectingTarget = false;
        ResetUI();
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

    public IEnumerator MoveCam(Vector3 moveTowards)
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

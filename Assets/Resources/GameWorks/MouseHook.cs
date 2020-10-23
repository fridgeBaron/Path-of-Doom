using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseHook : MonoBehaviour
{
    [SerializeField]
    GameObject light;
    [SerializeField]
    Camera gameCam;
    Transform camHandle;
    [SerializeField]
    Camera overHeadCam;
    [SerializeField]
    GameObject overHeadLight;
    CurrentCamera currentCamera;

    Vector3 mousePos;
    Vector3 snapPos;

    LayerMask UIMask;
    GraphicRaycaster UICaster;
    List<RaycastResult> UIHits;
    PointerEventData UIEvent;
    EventSystem UIEventSystem;

    [SerializeField]
    float panSpeed;
    [SerializeField]
    float hexSize;
    float hexWidth;

    float zoomLevel;

    Sprite heldSprite;
    [SerializeField]
    SpriteRenderer heldSpriteRenderer;

    Sprite tileSprite;
    [SerializeField]
    SpriteRenderer tileSpriteRenderer;
    [SerializeField]
    GameObject hexHook;
    List<HexHighlighter> gridOverlays;
    string lastButtonHover;
    [SerializeField]
    GameObject highlightFab;

    [SerializeField]
    TriMenu triMenu;
    [SerializeField]
    GameObject towerFabBasic;
    [SerializeField]
    GameObject augmentFabBasic;
    [SerializeField]
    GameObject buildingFabBasic;
    [SerializeField]
    GameObject linkFab;

    [SerializeField]
    GameObject towerParent;
    [SerializeField]
    GameObject projectileParent;

    Player player;
    [SerializeField]
    BuildingBase selectedBuilding;
    MobSpawner selectedSpawner;
    bool showingRange;

    int[] mouseCheck;
    Vector3 lastMousePos;
    [SerializeField]
    float roationSpeed;
    [SerializeField]
    float scrollBorder;
    [SerializeField]
    float overBorder;
    float rightClickTime;
    bool didBack;
    bool dragging;
    bool overButton;
    bool keepDetails;
    
    [SerializeField]
    private int subSnaps;

    public static MouseHook mousehook;

    // Use this for initialization
    void Start()
    {
        mousehook = this;
        overButton = false;
        didBack = false;
        dragging = false;
        keepDetails = false;
        //gameCam = GetComponentInParent<Camera>();
        heldSpriteRenderer = GetComponent<SpriteRenderer>();
        UICaster = GetComponentInParent<GraphicRaycaster>();
        UIEventSystem = GetComponentInParent<EventSystem>();
        player = transform.parent.GetComponentInChildren<Player>();
        //transform.parent = null;
        camHandle = gameCam.transform.parent;
        gridOverlays = new List<HexHighlighter>();
        currentCamera = CurrentCamera.main;
        UIMask = 1 << 5;

        mouseCheck = new int[3];
        zoomLevel = 10;
        hexWidth = Mathf.Sqrt(3) * hexSize;

        UIHits = new List<RaycastResult>();
        UIEvent = new PointerEventData(UIEventSystem);
        UIEvent.position = Input.mousePosition;
        showingRange = false;
        overHeadLight.SetActive(false);

        Vector3 camPos = gameCam.transform.localPosition;
        camPos.z = Mathf.Clamp(camPos.z, -40, -2.5f);
        camPos.y = Mathf.Clamp(camPos.y, 5f, 20);
        gameCam.transform.localPosition = camPos;
    }

    private void Update()
    {
        CheckMouseInput();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        overButton = false;
        mousePos = GetMousePosition();
        mousePos.z = -2;
        snapPos = mousePos;
        snapPos.z = 0;
        snapPos = MyMath.SnapToSub(subSnaps, snapPos);
        transform.position = mousePos;


        if (UIHits.Count > 0)
        {
            TriButton hitButton;
            ButtonClick clickButton;
            foreach (RaycastResult hit in UIHits)
            {
                if (hit.gameObject != null)
                {
                    hitButton = hit.gameObject.GetComponent<TriButton>();
                    clickButton = hit.gameObject.GetComponent<ButtonClick>();
                    triMenu.ResetButtons();
                    if (hitButton != null)
                    {
                        overButton = true;
                        hitButton.SetState(1);
                    }
                    if (clickButton != null)
                    {
                        overButton = true;
                    }
                }

            }
        }


        UIHits = new List<RaycastResult>();
        UIEvent = new PointerEventData(UIEventSystem);
        UIEvent.position = Input.mousePosition;
        UICaster.Raycast(UIEvent, UIHits);


        if (selectedBuilding == null && selectedSpawner == null && !keepDetails)
        {
            triMenu.DestroyDetails();
        }

        if (UIHits.Count > 0)
        {
            //Debug.Log("UI hit");
            TriButton hitButton;
            LinkToggle linkButton;
            
            foreach (RaycastResult hit in UIHits)
            {
                //Check For TriMenu
                hitButton = hit.gameObject.GetComponent<TriButton>();
                if (hitButton != null)
                {
                    // Load details for whichever button hit.

                    Vector2 detailPos = GetScreenPosition(hexHook.transform.position);
                    detailPos.x -= 120;
                    string buttonVal = hitButton.getValue().ToLower();
                    switch (buttonVal)
                    {
                        case "build_speed_augment_button":
                        case "build_range_augment_button":
                        case "build_damage_augment_button":
                        case "build_chain_augment_button":
                        case "build_fork_augment_button":
                        case "build_multi_augment_button":
                        case "build_ethereal_augment_button":
                            triMenu.DestroyDetails();
                            triMenu.CreateDetails(detailPos);
                            triMenu.AddDetail(hitButton.getValue(), -1);
                            triMenu.LoadDetails(Towers.instance.GetAugment(buttonVal.Replace("build_", "").Replace("_button", "").Replace("_augment", "")));
                            triMenu.RefreshDetails();
                            keepDetails = true;
                            break;
                        case "build_fire_tower_button":
                        case "build_water_tower_button":
                        case "build_earth_tower_button":
                        case "build_air_tower_button":
                        case "build_basic_tower_button":
                            triMenu.DestroyDetails();
                            triMenu.CreateDetails(detailPos);
                            triMenu.AddDetail(Translator.Get(buttonVal), -1);
                            string tower = buttonVal.Replace("build_", "").Replace("_button", "").Replace("_tower", "");
                            triMenu.LoadDetails(Towers.instance.GetTower(tower));
                            triMenu.RefreshDetails();
                            keepDetails = true;
                            break;

                        default: break;
                    }
                    if (lastButtonHover != buttonVal && triMenu.CurrentMenu() == "towers_build")
                    {
                        PreviewTowerRange(buttonVal.Replace("build_","").Replace("_button","").Replace("_tower", ""));
                    }
                    lastButtonHover = buttonVal;

                }
               
                //Check For Link Toggle

                linkButton = hit.gameObject.GetComponent<LinkToggle>();
                if (linkButton != null)
                {
                    Vector2 detailPos = GetScreenPosition(hexHook.transform.position);
                    detailPos.x -= 96;
                }

            }
        }

        if (Input.GetAxis("Jump") == 1)
        {
            if (currentCamera == CurrentCamera.main)
                ChangeCamera(CurrentCamera.overhead);
        }
        else
        {
            ChangeCamera(CurrentCamera.main);
        }

        if (mouseCheck[0] == 1)
        {
            
            if (UIHits.Count > 0)
            {
                MenuPositioner dragHandle;
                foreach (RaycastResult UIhit in UIHits)
                {
                    dragHandle = UIhit.gameObject.GetComponent<MenuPositioner>();
                    if (dragHandle != null)
                    {
                        dragHandle.StartDrag();
                        dragging = true;
                    }
                }
            }
        }
        //Check left mouse button up;
        if (mouseCheck[0] == -1)
        {
            dragging = false;
            Ray ray;
            if (currentCamera == CurrentCamera.main)
                ray = gameCam.ScreenPointToRay(Input.mousePosition);
            else
                ray = overHeadCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (UIHits.Count > 0)
            {
                TriButton hitButton;
                ButtonClick clickButton;
                foreach (RaycastResult UIhit in UIHits)
                {
                    hitButton = UIhit.gameObject.GetComponent<TriButton>();
                    clickButton = UIhit.gameObject.GetComponent<ButtonClick>();
                    if (hitButton != null)
                    {
                        string buttonValue = hitButton.getValue().ToLower();

                        TowerTypes towerType = TowerTypes.none;
                        AugmentTypes augmentType = AugmentTypes.none;
                        BuildingType buildingType = BuildingType.none;

                        buttonValue = buttonValue.Replace("_button", "");
                        if (buttonValue.Contains("build_"))
                        {
                            buttonValue = buttonValue.Replace("build_", "");
                            if (buttonValue.Contains("_tower"))
                            {
                                buttonValue = buttonValue.Replace("_tower", "");
                                towerType = TowerBase.GetTowerType(buttonValue);
                            }
                            else if (buttonValue.Contains("_augment"))
                            {
                                buttonValue = buttonValue.Replace("_augment", "");
                                augmentType = Towers.GetAugmentType(buttonValue);
                            }
                        }
                        
                        MobSpawner spawner;
                        
                        switch (buttonValue)
                        {
                            case "path":
                                PathKeeper.indexes[PathType.normal]++;
                                PathKeeper.pathKeeper.FindGoal(PathType.normal, TerrainGen.GetGridPosition2D(PathKeeper.goal), TerrainGen.GetGridPosition2D(hexHook.transform.position)); break;
                            case "select":
                                TerrainGen.mapGenState = MapGen.pathing;
                                buildingType = BuildingType.Base;
                                break;
                            case "regen":
                                TerrainGen.instance.KillTerrain();
                                TerrainGen.mapGenState = MapGen.terrain;
                                break;
                            case "towers":
                                triMenu.DestroyMenu();
                                triMenu.CreateMenu("towers_build");
                                break;

                            case "augments":
                                triMenu.DestroyMenu();
                                triMenu.CreateMenu("augments");
                                break;

                            case "buildings":
                                triMenu.DestroyMenu();
                                triMenu.CreateMenu("buildings");
                                break;

                            case "sell":
                                if (selectedBuilding != null)
                                {
                                    float value = selectedBuilding.Sell();
                                    value *= 0.8f;

                                    player.GiveGold((int)value);
                                    TowerBase.UpdateTowerHexes();
                                    triMenu.DestroyMenu();
                                    hexHook.SetActive(false);
                                    HexHighlighter.ResetGrid();
                                }
                                break;
                            case "upgrade":
                                if (selectedBuilding != null)
                                {
                                    if (player.hasGold(100))
                                    {
                                        player.TakeGold(100);
                                        selectedBuilding.AddValue(100);
                                        selectedBuilding.GetComponent<TowerBase>().LevelUp();
                                        triMenu.CreateDetails();
                                        triMenu.LoadDetails(selectedBuilding.GetComponent<TowerBase>().GetDetails());
                                        triMenu.RefreshDetails();
                                    }

                                }
                                break;

                            case "points":
                                spawner = MobSpawner.GetSpawner(TerrainGen.GetGridPosition2D(hexHook.transform.position));
                                if (spawner != null)
                                {
                                    spawner.wavePoints *= 1.1f;
                                    CloseMenu();
                                    triMenu.gameObject.SetActive(true);
                                    selectedSpawner = spawner;
                                    triMenu.CreateMenu("spawner");
                                    triMenu.CreateDetails();
                                    triMenu.LoadDetails(spawner.GetDetails());
                                    triMenu.RefreshDetails();
                                }
                                break;
                            case "more":
                                spawner = MobSpawner.GetSpawner(TerrainGen.GetGridPosition2D(hexHook.transform.position));
                                if (spawner != null)
                                {
                                    spawner.spawnNumber += 1;
                                    CloseMenu();
                                    triMenu.gameObject.SetActive(true);
                                    selectedSpawner = spawner;
                                    triMenu.CreateMenu("spawner");
                                    triMenu.CreateDetails();
                                    triMenu.LoadDetails(spawner.GetDetails());
                                    triMenu.RefreshDetails();
                                }
                                break;
                            case "less":
                                spawner = MobSpawner.GetSpawner(TerrainGen.GetGridPosition2D(hexHook.transform.position));
                                if (spawner != null)
                                {
                                    spawner.spawnNumber -= 1;
                                    if (spawner.spawnNumber < 0)
                                        spawner.spawnNumber = 0;
                                    CloseMenu();
                                    triMenu.gameObject.SetActive(true);
                                    selectedSpawner = spawner;
                                    triMenu.CreateMenu("spawner");
                                    triMenu.CreateDetails();
                                    triMenu.LoadDetails(spawner.GetDetails());
                                    triMenu.RefreshDetails();
                                }
                                break;
                            case "faster":
                                spawner = MobSpawner.GetSpawner(TerrainGen.GetGridPosition2D(hexHook.transform.position));
                                if (spawner != null)
                                {
                                    spawner.coolDownTime -= 1;
                                    if (spawner.coolDownTime < 1)
                                        spawner.coolDownTime = 1;
                                    CloseMenu();
                                    triMenu.gameObject.SetActive(true);
                                    selectedSpawner = spawner;
                                    triMenu.CreateMenu("spawner");
                                    triMenu.CreateDetails();
                                    triMenu.LoadDetails(spawner.GetDetails());
                                    triMenu.RefreshDetails();

                                }
                                break;
                            case "slower":
                                spawner = MobSpawner.GetSpawner(TerrainGen.GetGridPosition2D(hexHook.transform.position));
                                if (spawner != null)
                                {
                                    spawner.coolDownTime += 1;
                                    CloseMenu();
                                    triMenu.gameObject.SetActive(true);
                                    selectedSpawner = spawner;
                                    triMenu.CreateMenu("spawner");
                                    triMenu.CreateDetails();
                                    triMenu.LoadDetails(spawner.GetDetails());
                                    triMenu.RefreshDetails();
                                }
                                break;

                            case "wall":
                                buildingType = BuildingType.Wall; break;
                                
                                



                            default: break;
                        }
                        int cost = Towers.instance.GetCost(buttonValue.Replace("build_", "").Replace("_button", ""));
                        if (towerType != TowerTypes.none)
                        {
                            if (player.hasGold(cost))
                            {
                                TowerBase towerBase = Instantiate(towerFabBasic, hexHook.transform.position, new Quaternion(), towerParent.transform)
                                                        .GetComponent<TowerBase>();
                                player.TakeGold(cost);
                                towerBase.SetProjectileParent(projectileParent);
                                string tower = buttonValue.Replace("build_", "").Replace("_button", "");
                                towerBase.SetTower(TowerBase.GetTowerType(tower));
                                towerBase.UpdateLinks();
                                UpdateGrid();
                                CloseMenu();
                                //
                                //
                                //
                            }
                        }
                        if (augmentType != AugmentTypes.none)
                        {
                            if (player.hasGold(cost))
                            {
                                AugmentBase augmentBase = Instantiate(augmentFabBasic, hexHook.transform.position, new Quaternion(), towerParent.transform)
                                                            .GetComponent<AugmentBase>();
                                player.TakeGold(cost);
                                augmentBase.SetAugmentType(buttonValue);
                                augmentBase.AddValue(cost);
                                UpdateGrid();
                                CloseMenu();
                            }
                        }
                        if (buildingType != BuildingType.none)
                        {
                            if (player.hasGold(cost))
                            {
                                if (buttonValue == "select")
                                    buttonValue = "base";
                                BuildingBase buildingBase = Instantiate(buildingFabBasic, hexHook.transform.position, new Quaternion(), towerParent.transform)
                                                            .GetComponent<BuildingBase>();
                                buildingBase.SetBuilding(buttonValue);
                                player.TakeGold(cost);
                                UpdateGrid(hexHook.transform.position);
                                CloseMenu();
                            }
                        }
                    }
                    if (clickButton != null)
                    {
                        switch (clickButton.buttonClick)
                        {
                            case "pathing":
                                if (HexHighlighter.showingType != GridType.path)
                                {
                                    PathKeeper.ShowOnGrid(PathType.normal);
                                    HexHighlighter.showingType = GridType.path;
                                }
                                else
                                {
                                    HexHighlighter.ResetGrid();
                                    HexHighlighter.showingType = GridType.grid;
                                }
                                break;
                            case "height":
                                if (HexHighlighter.showingType != GridType.height)
                                {
                                    HexHighlighter.ShowHeightMap();
                                    HexHighlighter.showingType = GridType.height;
                                }
                                else
                                {
                                    HexHighlighter.ResetGrid();
                                    HexHighlighter.showingType = GridType.grid;
                                }
                                break;
                            case "towerrange":
                                if (HexHighlighter.showingType != GridType.range)
                                {
                                    HexHighlighter.ResetGrid();
                                    HexHighlighter.ShowTowersRange(true);
                                    HexHighlighter.showingType = GridType.range;
                                }
                                else
                                {
                                    HexHighlighter.ResetGrid();
                                    HexHighlighter.showingType = GridType.grid;
                                }
                                break;
                            case "undock_details":
                                triMenu.DockDetails();
                                break;


                        }
                    }
                }
            }
            else if (Physics.Raycast(ray, out hit, 10, UIMask))
            {
                Debug.Log("Other Ui hits>");
                LinkToggle hitToggle;
                hitToggle = hit.transform.parent.GetComponent<LinkToggle>();
                if (hitToggle != null)
                {
                    hitToggle.Toggle();
                    UpdateLinks();
                    showingRange = true;
                    
                }

            }
            else
            {
                triMenu.DestroyMenu();
                //Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
                Debug.DrawRay(ray.origin, ray.direction, Color.blue, 1);
                if (Physics.Raycast(ray, out hit))
                {
                    hexHook.SetActive(true);
                    if (hit.transform.position != Vector3.zero)
                        PlaceHexHook(hit.transform.position);
                    else
                        PlaceHexHook(hit.point);
                }
                
                BuildingBase buildingBase = null;
                if (hit.collider != null)
                {
                    buildingBase = hit.transform.parent.GetComponent<BuildingBase>();
                    
                    if (buildingBase != null)
                    {
                        triMenu.DestroyMenu();
                        Vector2 detailPos = GetScreenPosition(hexHook.transform.position);
                        detailPos.x -= 120;
                        triMenu.CreateMenu("towerselect");
                        triMenu.CreateDetails(detailPos);
                        triMenu.LoadDetails(buildingBase.GetDetails());
                        triMenu.AddDetail("Upgrade Cost:", 100);
                        triMenu.RefreshDetails();
                        selectedBuilding = buildingBase;

                        selectedBuilding.UpdateLinks();
                      
                        UpdateLinks();
                        //triMenu.DestroyButtons();

                        if (!showingRange && buildingBase is TowerBase towerBase)
                        {
                            towerBase.ShowHexes();
                        }
                    }
                }

                Vector2 menuPos = GetScreenPosition(hexHook.transform.position);
                menuPos.x += 60;
                triMenu.transform.position = menuPos;
                //
                if (buildingBase == null)
                {
                    selectedBuilding = null;
                    HexHighlighter.ResetGrid();
                    hexHook.GetComponentInChildren<MeshRenderer>(true).gameObject.SetActive(false);
                    MobSpawner spawner = MobSpawner.GetSpawner(TerrainGen.GetGridPosition2D(hexHook.transform.position));
                    if (TerrainGen.mapGenState == MapGen.terrainCon)
                    {
                        triMenu.CreateMenu("startmenu");
                    }
                    else if (spawner == null)
                        triMenu.CreateMenu("base");
                    else
                    {
                            CloseMenu();
                            triMenu.gameObject.SetActive(true);
                            selectedSpawner = spawner;
                            triMenu.CreateMenu("spawner");
                            triMenu.CreateDetails();
                            triMenu.LoadDetails(spawner.GetDetails());
                            triMenu.RefreshDetails();
                    }
                }
                else
                {
                    hexHook.GetComponentInChildren<MeshRenderer>(true).gameObject.SetActive(true);
                }
            }
            mouseCheck[0] = 0;
        }

        if (mouseCheck[1] == -1 )
        {
            if (!overButton)
            {
                if (rightClickTime <= 0.33f)
                {
                    triMenu.DestroyMenu();
                    selectedBuilding = null;
                    selectedSpawner = null;
                    hexHook.SetActive(false);
                    mouseCheck[1] = 0;
                    lastButtonHover = null;
                    HexHighlighter.ResetGrid();
                    showingRange = false;
                }
                
            }
            else if (overButton && !didBack)
            {
                if (triMenu.CurrentMenu() != "startmenu")
                {
                    didBack = true;
                    triMenu.DestroyMenu();
                    triMenu.CreateMenu("base");
                }
                
            }
        }
        bool inX = !dragging && !overButton && Input.mousePosition.x > 0 - overBorder && Input.mousePosition.x < Screen.width + overBorder;
        bool inside = inX && Input.mousePosition.y > 0 - overBorder && Input.mousePosition.y < Screen.height + overBorder ;
        if (inside)
            MoveCamera();
        if (mouseCheck[1] == 1)
            RotateCamera();
        if (triMenu.HaveLinks())
            triMenu.ReDrawLinks();
    }

    //
    // ///////////////////////////////////////////End this big ass function
    //

    Vector3 GetMousePosition()
    {
        Vector3 retVec;
        if (currentCamera == CurrentCamera.main)
            retVec = gameCam.ScreenToWorldPoint(Input.mousePosition);
        else
            retVec = overHeadCam.ScreenToViewportPoint(Input.mousePosition);
        return retVec;
    }



    /// <summary>
    /// Updates the highlight grid to show where a tower could hit taking its name as a string.
    /// </summary>
    /// <param name="tower"></param>
    void PreviewTowerRange(string tower)
    {
        float? towerVal = Towers.instance.GetTowerValue(tower, "attack_range");
        int range = 0;
        float heightOffset = 0.05f;
        bool arc = false;
        if (towerVal.HasValue)
            range = (int)towerVal.Value;
        towerVal = Towers.instance.GetTowerValue(tower, "projectile_arc");
        if (towerVal.HasValue)
            arc = true;
        towerVal = Towers.instance.GetTowerValue(tower, "projectile_yoff");
        if (towerVal.HasValue)
            heightOffset = towerVal.Value;
        HexHighlighter.ResetGrid();
        Dictionary<string, List<Vector2Int>> hexes = TowerBase.GetHexesInRange(hexHook.transform.position, range, arc, heightOffset);
        if (hexes.ContainsKey("good") && hexes["good"] != null)
        {
            foreach (Vector2Int newPos in hexes["good"])
            {
                if (!MyMath.IsWithin(newPos.x, -1, TerrainGen.gridX) || !MyMath.IsWithin(newPos.y, -1, TerrainGen.gridZ))
                    continue;
                if (newPos != TerrainGen.GetGridPosition2D(hexHook.transform.position))
                    HexHighlighter.Set(newPos, HexHighlight.positive);
            }
        }
        if (hexes.ContainsKey("bad") && hexes["bad"] != null)
        {
            foreach (Vector2Int newPos in hexes["bad"])
            {
                if (!MyMath.IsWithin(newPos.x, -1, TerrainGen.gridX) || !MyMath.IsWithin(newPos.y, -1, TerrainGen.gridZ))
                    continue;
                HexHighlighter.Set(newPos, HexHighlight.negative);
            }
        }
    }


    void MoveCamera()
    {
        if (Input.GetAxisRaw("Horizontal") > 0 || (Input.mousePosition.x > Screen.width - scrollBorder && Input.mousePosition.x < Screen.width + overBorder))
        {
            camHandle.transform.Translate(new Vector3(panSpeed, 0, 0), Space.Self);
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 || (Input.mousePosition.x < scrollBorder && Input.mousePosition.x + overBorder > 0))
        {
            camHandle.transform.Translate(new Vector3(-panSpeed, 0, 0), Space.Self);
        }
        if (Input.GetAxisRaw("Vertical") > 0 || (Input.mousePosition.y > Screen.height - scrollBorder && Input.mousePosition.y < Screen.height + overBorder))
        {
            Quaternion saveRotation = camHandle.transform.rotation;
            Quaternion rotation = camHandle.transform.rotation;
            Vector3 eulerRotation = rotation.eulerAngles;
            eulerRotation.x = 0;
            if (eulerRotation.z != 0)
            {
                eulerRotation.y += eulerRotation.z;
                eulerRotation.z = 0;
            }
            rotation.eulerAngles = eulerRotation;

            camHandle.rotation = rotation;
            camHandle.transform.Translate(new Vector3(0, 0, panSpeed), Space.Self);
            camHandle.rotation = saveRotation;
        }
        else if (Input.GetAxisRaw("Vertical") < 0 || (Input.mousePosition.y < scrollBorder && Input.mousePosition.y + overBorder > 0))
        {
            Quaternion saveRotation = camHandle.transform.rotation;
            Quaternion rotation = camHandle.transform.rotation;
            Vector3 eulerRotation = rotation.eulerAngles;
            eulerRotation.x = 0;
            if (eulerRotation.z != 0)
            {
                eulerRotation.y += eulerRotation.z;
                eulerRotation.z = 0;
            }
            rotation.eulerAngles = eulerRotation;

            camHandle.rotation = rotation;
            camHandle.transform.Translate(new Vector3(0, 0, -panSpeed), Space.Self);
            camHandle.rotation = saveRotation;
        }
        Vector3 pos = camHandle.position;
        float lastY = pos.y;
        pos.y = TerrainGen.GetHexHeight(TerrainGen.GetGridPosition(pos));
        if (pos.y == -1)
            pos.y = lastY;
        camHandle.transform.position = pos;

        

        if (mouseCheck[2] != 0)
        {
            if (mouseCheck[2] > 0)
                gameCam.transform.Translate(new Vector3(0, 0, panSpeed), Space.Self);
            else
                gameCam.transform.Translate(new Vector3(0, 0, -panSpeed), Space.Self);
            //

            Vector3 camPos = gameCam.transform.localPosition;
            camPos.z = Mathf.Clamp(camPos.z, -40, -2.5f);
            camPos.y = Mathf.Clamp(camPos.y, 5f, 20);
            gameCam.transform.localPosition = camPos;
        }

        Vector3 menuPos = GetScreenPosition(hexHook.transform.position);
        menuPos.x += 60;
        triMenu.PlaceMenu(menuPos);
        Vector2 detailPos = GetScreenPosition(hexHook.transform.position);
        detailPos.x -= 120;
        triMenu.hexDetails.SetPosition(detailPos);
    }

    void RotateCamera()
    {

        Vector3 rotationAxis;
        rotationAxis = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;
        float rotationSpeed;

        rotationSpeed = rotationAxis.x + rotationAxis.z + rotationAxis.y;
        rotationAxis = rotationAxis * roationSpeed;
        Quaternion camRotation = camHandle.rotation;
        rotationAxis.z = rotationAxis.x;
        rotationAxis.x = Mathf.Clamp(camRotation.eulerAngles.x - rotationAxis.y, 0f, 90);
        rotationAxis.y = rotationAxis.z + camRotation.eulerAngles.y;
        rotationAxis.z = 0;
        camRotation = Quaternion.Euler(rotationAxis.x, rotationAxis.y, 0);
        camHandle.rotation = camRotation;
    }

    void ChangeCamera(CurrentCamera camera)
    {
        if (camera == CurrentCamera.overhead)
        {
            currentCamera = CurrentCamera.overhead;
            overHeadCam.enabled = true;
            gameCam.enabled = false;
            overHeadLight.SetActive(true);
            light.SetActive(false);
        }
        else
        {
            currentCamera = CurrentCamera.main;
            overHeadCam.enabled = false;
            gameCam.enabled = true;
            overHeadLight.SetActive(false);
            light.SetActive(true);
        }
    }
    public Camera GetCamera()
    {
        if (currentCamera == CurrentCamera.main)
            return gameCam;
        return overHeadCam;
    }

    Vector2 GetMouseUIPosition()
    {
        Vector2 retVec;

        retVec = Input.mousePosition;

        return retVec;
    }

    Vector2 GetScreenPosition(Vector3 pos)
    {
        if (currentCamera == CurrentCamera.main)
            return gameCam.WorldToScreenPoint(pos);
        return overHeadCam.WorldToScreenPoint(pos);
    }

    void CheckMouseInput()
    {
        rightClickTime += Time.deltaTime;
        if (Input.GetMouseButtonDown(1))
        {
            mouseCheck[1] = 1;
            lastMousePos = Input.mousePosition;
            rightClickTime = 0;
        }

        if (Input.GetMouseButtonUp(1))
        {
            mouseCheck[1] = -1;
            didBack = false;
        }
            
        if (Input.GetMouseButtonDown(0))
            mouseCheck[0] = 1;
        if (Input.GetMouseButtonUp(0))
            mouseCheck[0] = -1;
        if (Input.mouseScrollDelta.y > 0)
            mouseCheck[2] = 1;
        else if (Input.mouseScrollDelta.y < 0)
            mouseCheck[2] = -1;
        else
            mouseCheck[2] = 0;
    }

    void PlaceHexHook(Vector3 position)
    {
        Vector3 newPos;
        newPos = position;
        newPos.y += 0.001f;
        hexHook.transform.position = TerrainGen.GetAsGridPosition(newPos);
    }

    void CloseMenu()
    {
        triMenu.DestroyMenu();
        hexHook.GetComponentInChildren<MeshRenderer>(true).gameObject.SetActive(false);
        hexHook.SetActive(false);
        keepDetails = false;
        HexHighlighter.ResetGrid();
    }
    
    public void UpdateLinks()
    {
        triMenu.DestroyLinks();
        if (selectedBuilding != null)
        {
            Dictionary<GameObject, LinkType> links = selectedBuilding.GetLinks();
            if (links != null)
            {
                foreach (KeyValuePair<GameObject, LinkType> val in links)
                {
                    Vector3 linkPos = new Vector3();

                    linkPos = val.Key.transform.position;
                    linkPos = gameCam.WorldToScreenPoint(linkPos);
                    GameObject newObject;
                    newObject = Instantiate(linkFab);
                    newObject.transform.SetParent(this.transform.parent);
                    newObject.transform.position = linkPos;
                    newObject.GetComponent<LinkToggle>().SetToggle(val.Value);
                    newObject.GetComponent<LinkToggle>().linkingTower = selectedBuilding.gameObject;
                    newObject.GetComponent<LinkToggle>().linkingAugment = val.Key;
                    GameObject collider = newObject.GetComponentInChildren<MeshCollider>().gameObject;
                    collider.transform.position = val.Key.transform.position;
                    triMenu.AddLink(newObject);
                    triMenu.CreateDetails();
                    triMenu.LoadDetails(selectedBuilding.GetDetails());
                    triMenu.RefreshDetails();
                }
                if (selectedBuilding is TowerBase towerBase)
                {
                    towerBase.ShowHexes();
                }
            }
        }
    }
    void UpdateGrid(Vector3 gridPos = new Vector3())
    {
        if (TerrainGen.mapGenState == MapGen.done)
        {
            if (gridPos != Vector3.zero)
                PathKeeper.pathKeeper.RecalcArea(PathType.normal, TerrainGen.GetGridPosition(hexHook.transform.position), 5, true);
            TowerBase.UpdateTowerHexes();
        }
    }
}



public enum CurrentCamera
{
    main,
    overhead
}
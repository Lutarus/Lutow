using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMechanics.GlobalSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class FormationNodeUI : MonoBehaviour
{
    #region Singleton
    public static FormationNodeUI Instance;

    void Awake()
    {
        if (Instance != null)
        {
            //Debug.LogError("More than one FormationCreator in scene!");
            return;
        }
        Instance = this;
    }
    #endregion

    [SerializeField] GameObject ui = null;
    FormationNode targetNode;
    PrefabCollector prefabCollector;
    GameObject placedPrefab;
    int placedPrefabType;

    [Header("UI Button Elements")]
    [SerializeField] GameObject buttonGOPrefab = null;
    [SerializeField] Transform buttonGOPrefabParent = null;

    [SerializeField] GameObject n_removeBtn = null;

    class ButtonCollector
    {
        public Button unitButton;
        public int unitType;
    }

    List<ButtonCollector> buttonCollector = null;
    ButtonCollector buttonCollectorVar = null;

    private void Start()
    {
        prefabCollector = PrefabCollector.Instance;
        buttonCollector = new List<ButtonCollector>();

        for (int i = 0; i < prefabCollector.armyPools.Count; i++)
        {
            var charType = prefabCollector.armyPools[i].characterType;

            buttonCollectorVar = new ButtonCollector();
            GameObject iButtonGOPrefab = Instantiate(buttonGOPrefab, buttonGOPrefabParent);
            iButtonGOPrefab.GetComponentsInChildren<Image>()[0].sprite = prefabCollector.armyPools[i].characterIconBackground;
            iButtonGOPrefab.GetComponentsInChildren<Image>()[1].sprite = prefabCollector.armyPools[i].characterIcon;
            iButtonGOPrefab.GetComponentInChildren<Button>().onClick.AddListener(() => StartPlacingPrefab(charType));
            buttonCollectorVar.unitButton = iButtonGOPrefab.GetComponentInChildren<Button>();
            buttonCollectorVar.unitType = prefabCollector.armyPools[i].characterType;
            buttonCollector.Add(buttonCollectorVar);
        }

        Destroy(buttonGOPrefabParent.GetChild(0).gameObject);
        n_removeBtn.transform.SetAsLastSibling();
        ui.SetActive(false);
    }

    private void Update()
    {
        UILookAtCamera();
        if (GlobalVariables.readyForAttack && ui.activeSelf)
        {
            Hide();
        }
    }

    private void UILookAtCamera()
    {
        ui.transform.LookAt(2 * transform.position - Camera.main.transform.position);
    }
    public void SetTarget(FormationNode _target)
    {
        if (targetNode != null)
        {
            targetNode.SetNodeColor();
        }
        targetNode = _target;
        //transform.position = targetNode.GetBuildPosition();
        ui.SetActive(true);
        CanBeBuild();
    }

    public void Hide()
    {
        ui.SetActive(false);
        UIGame.Instance.UpdateUIVariables();
    }

    private void CanBeBuild()
    {
        for (int i = 0; i < buttonCollector.Count; i++)
        {
            if (GlobalArmyCounter.GetPlaceableArmyCounter(buttonCollector[i].unitType) == 0 || targetNode.GetNodeCompatibility())
            {
                buttonCollector[i].unitButton.interactable = false;
            }
            else
            {
                buttonCollector[i].unitButton.interactable = true;
            }
        }

        if (targetNode.GetNodeCompatibility())
        {
            n_removeBtn.GetComponent<Button>().interactable = true;
        }
        else
        {
            n_removeBtn.GetComponent<Button>().interactable = false;
        }
    }

    public void StartPlacingPrefab(int characterType)
    {
        placedPrefab = ArmyPooler.Instance.SpawnFromNodePool(characterType, targetNode.transform.position, targetNode.transform.rotation);
        if (placedPrefab == null)
        {
            return;
        }
        targetNode.SetNodeCompatibility(true);
        targetNode.SetNodeColor();
        targetNode.SetNodePlacedPrefab(placedPrefab, characterType);
        GlobalArmyCounter.CounterRemovePlaceableUnit(characterType);

        GlobalArmyCounter.TotalPlaceableArmy();

        Hide();
    }

    public void RemovingPlacedPrefab()
    {
            targetNode.GetNodePlacedPrefab(out placedPrefab, out placedPrefabType);
            ArmyPooler.Instance.ReturnToNodePool(placedPrefabType, placedPrefab);
            targetNode.SetNodeCompatibility(false);
            targetNode.SetNodeColor();
        GlobalArmyCounter.CounterAddPlaceableUnit(placedPrefabType);
        Hide();
    }
}

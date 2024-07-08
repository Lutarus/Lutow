using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Character.Control;
using System;
using GameMechanics.GlobalSystem;
using Character.Core;
using Character.Movement;

public class FormationNode : MonoBehaviour
{
    [HideInInspector]
    public Vector3 offset; //
    [HideInInspector]
    public GameObject formationChief;

    public Vector3 positionOffSet;

    [SerializeField] Color startColor = Color.white;
    [SerializeField] Color enterColor = Color.green;
    [SerializeField] Color filledColor = Color.red;

    [SerializeField] Material startMaterial = null;
    [SerializeField] Material enterMaterial = null;
    [SerializeField] Material filledMaterial = null;

    Renderer rend;
    GameObject nodePlacedPrefab;
    int nodePlacedPrefabType;

    [HideInInspector] public bool isNodeFilled = false;
    

    private void Start()
    {
        //Debug.Log("offSet :" + transform.name + ": " + offset);
        
        rend = GetComponent<Renderer>();
        //rend.material.color = startColor;
        rend.material = startMaterial;
        rend.enabled = false;
    }

    private void Update()
    {
        if (GlobalVariables.readyForPreparation && !GlobalVariables.readyForAttack)
        {
            SetVisibility(true);
        }
        if (GlobalVariables.readyForPreparation && GlobalVariables.readyForAttack)
        {
            SetVisibility(false);
        }
    }

    private void SetVisibility(bool value)
    {
        rend.enabled = value;
    }

    public Vector3 GetBuildPosition()
    {
        return transform.position + positionOffSet;
    }

    public void SetNodeCompatibility(bool setNode)
    {
        isNodeFilled = setNode;
    }
    public bool GetNodeCompatibility()
    {
        return isNodeFilled;
    }

    public void SetNodePlacedPrefab(GameObject placedPrefab ,int prefabType)
    {
        CheckOffset();
        nodePlacedPrefab = placedPrefab;
        nodePlacedPrefabType = prefabType;
        SetPrefabFormationValues();
        SetCharacterConnection();
    }

    public void GetNodePlacedPrefab(out GameObject PlacedPrefab , out int prefabType)
    {
        PlacedPrefab = nodePlacedPrefab;
        prefabType = nodePlacedPrefabType;
    }

    public void SetNodeColor()
    {
        if (!isNodeFilled)
        {
            //rend.material.color = startColor;
            rend.material = startMaterial;
        }
        else
        {
            //rend.material.color = filledColor;
            rend.material = filledMaterial;
        }
    }

    void OnMouseDown()
    {
        if (GlobalVariables.readyForAttack || GlobalVariables.buildMode)
        {
            return;
        }
        if (GlobalArmyCounter.TotalPlaceableArmy() == 0 && !isNodeFilled)
        {
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())  //Eger mouse başka oyun objesi üzerin
            return;
       
        FormationNodeUI.Instance.SetTarget(this);
        rend.material = enterMaterial;
        //rend.material.color = enterColor;
    }

    void SetPrefabFormationValues()
    {
        nodePlacedPrefab.GetComponent<PlayerAI>().SetFormationValues(formationChief, offset);
    }

    void SetCharacterConnection()
    {
        nodePlacedPrefab.GetComponent<PlayerAI>().SetNodeConnection(this);
    }

    public void BreakCharacterConnection()
    {
                nodePlacedPrefab = null;
                SetNodeCompatibility(false);
                SetNodeColor();
    }

    public void CheckOffset()
    {
        offset = (formationChief.transform.position - transform.position) * -1;
    }

    //void CheckPlacedPrefabLocation()
    //{
    //    if (nodePlacedPrefab.transform.position != transform.position)
    //    {
    //        nodePlacedPrefab.transform.position = this.transform.position;
    //        nodePlacedPrefab.transform.rotation = this.transform.rotation;
    //    }
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameMechanics.GlobalSystem;
using Character.Core;
using Character.Control;
using Character.Movement;
using System;

public class FormationCreator : MonoBehaviour
{
    #region Singleton
    public static FormationCreator Instance;

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

    [SerializeField] GameObject formationPointParent = null;
    [SerializeField] GameObject formationChief = null;
    [SerializeField] GameObject formationNode = null;

    GameObject[] destinationPointParents;
    int rows = 1;
    int cols = 5;
    GameObject sphere;
    float offSet = 2f;

    bool gridComplete = false;

    int nextStageRow = 0;
    int nextStageCol = 0;

    private Vector3 childPositionOffset;
    private Quaternion childRotationOffset;
    private Quaternion originalRotation;

    private void Start()
    {
        int nextStageRow = 0;
        int nextStageCol = 0;

        originalRotation = new Quaternion();
        originalRotation = transform.rotation;
        destinationPointParents = GameController.GetDestinationParents();
        //formationPointParent.transform.rotation = destinationPointParents[GlobalVariables.GetStageValue()].transform.rotation;
    }

    public void CreateGrid()
    {
        offSet = (formationNode.transform.localScale.x * 10) + 0.5f;
        GlobalVariables.readyForPreparation = true;

        GenerateGrid();
        if (gridComplete == false)return;
    }

    private void GenerateGrid()
    {
        GameObject[] units = GameObject.FindGameObjectsWithTag(GlobalVariables.playerTag);
        foreach (GameObject unit in units)
        {
            if (unit.GetComponent<PlayerAI>().GetNodeConnection())
            {
                unit.GetComponent<PlayerAI>().SendUnitToNode();
                continue;
            }
            var characterType = unit.GetComponent<CharacterStats>().characterType;
            ArmyPooler.Instance.ReturnToNodePool(characterType , unit);
        }

        if (GlobalArmyCounter.totalArmy > cols)
        {
            var mod = GlobalArmyCounter.totalArmy / cols;
            rows = mod + 1;
        }
        else
        {
            rows = 2;
        }
        
        for (int row = nextStageRow; nextStageRow < rows; nextStageRow++)
        {
            for (int col = 0; col < cols; col++)
            {
                float posX = (col * offSet) + formationPointParent.transform.position.x;
                float posZ = (nextStageRow * -offSet)+ formationPointParent.transform.position.z;
                //float posY = formationPointParent.transform.localPosition.y;
                float posY = 0.3f;
                GameObject insFormationNode = Instantiate(formationNode, new Vector3(posX, posY, posZ), formationPointParent.transform.rotation);
                insFormationNode.transform.parent = formationPointParent.transform;

                if (GlobalVariables.GetStageValue() > 0)
                {
                    childPositionOffset = formationPointParent.transform.position - insFormationNode.transform.position;
                    childRotationOffset = insFormationNode.transform.rotation * Quaternion.Inverse(formationPointParent.transform.rotation);


                    Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(originalRotation);

                    insFormationNode.transform.position = formationPointParent.transform.position + deltaRotation * -childPositionOffset;
                    insFormationNode.transform.rotation = formationPointParent.transform.rotation * childRotationOffset;
                }

                
                Vector3 offset = formationChief.transform.position - insFormationNode.transform.position;

                insFormationNode.GetComponent<FormationNode>().offset = -1 * offset;
                insFormationNode.GetComponent<FormationNode>().formationChief = formationChief;
            }
        }
        gridComplete = true;
    }

    
}

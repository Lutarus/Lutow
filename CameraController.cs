using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using GameMechanics.GlobalSystem;
using Character.Core;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [SerializeField] public Camera mCamera;
    [SerializeField] public bool rotate;
    protected Plane plane;
    [SerializeField] Transform farLeft = null;
    [SerializeField] Transform farRight = null;
    [SerializeField] Transform farForward = null;
    [SerializeField] Transform farBackward = null;
    [SerializeField] Transform farUpward = null;
    [SerializeField] Transform farDownward = null;
    [SerializeField] GameObject cMMC = null;
    [SerializeField] GameObject cMFC1 = null;
    [SerializeField] GameObject cMFC2 = null;
    [SerializeField] GameObject cMLC = null;
    [SerializeField] GameObject cMEC = null;
    static bool followEnabled = false;

    CinemachineVirtualCamera cMMCC = null;
    CinemachineVirtualCamera cMFC1C = null;
    CinemachineVirtualCamera cMFC2C = null;
    CinemachineVirtualCamera cMLCC = null;
    CinemachineVirtualCamera cMECC = null;
    CinemachineOrbitalTransposer cMFC1COT = null;
    CinemachineOrbitalTransposer cMFC2COT = null;
    Transform followTarget;
    GameObject nearestAlly;
    float shortestDistance;
    [SerializeField] Transform endOfTheDestination = null;
    private LayerMask LayerGround;
    Quaternion cMMC_OriginalRotation;
    float cineMachineMainCameraOriginalY;

    float motherBlendTime;
    float blendTime;
    bool firstTargetConnection = false;
    bool coroutineActivated = false;

    private void Awake()
    {
        instance = this;

        LayerGround = LayerMask.GetMask("Ground");
        cineMachineMainCameraOriginalY = cMMC.transform.position.y;
        cMMC_OriginalRotation = cMMC.transform.rotation;

        followEnabled = false;
        //cineMachineFollowCamera.SetActive(false);
        //cineMachineFollowCamera2nd.SetActive(false);

        if (mCamera == null)
            mCamera = Camera.main;
        motherBlendTime = mCamera.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time;
        blendTime = motherBlendTime;
        cMFC1C = cMFC1.GetComponent<CinemachineVirtualCamera>();
        cMFC2C = cMFC2.GetComponent<CinemachineVirtualCamera>();
        cMMCC = cMMC.GetComponent<CinemachineVirtualCamera>();
        cMLCC = cMLC.GetComponent<CinemachineVirtualCamera>();
        cMECC = cMEC.GetComponent<CinemachineVirtualCamera>();
        cMFC1COT = cMFC1C.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        cMFC2COT = cMFC2C.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        var rotationVector = cMMC.transform.rotation.eulerAngles;
        rotationVector.x = Mathf.Clamp((45 * cMMC.transform.localPosition.y) / 20, 30, 75);
        cMMC.transform.rotation = Quaternion.Euler(rotationVector);
    }

    private void Start()
    {
        InvokeRepeating("UpdateFollowTarget", 0f, 0.4f);
    }

    void UpdateFollowTarget()
    {
        SearchFollowTarget();
    }
    private void Update()
    {

        #region Camera Follow
        if (followEnabled && !coroutineActivated)
        {
            StartCoroutine(WaitForC2());
        }
        if (followEnabled)
        {
            if (Input.touchCount >= 1)
                plane.SetNormalAndPosition(transform.up, transform.position);

            if (Input.touchCount >= 2)
            {
                var pos11 = PlanePosition(Input.GetTouch(0).position);
                var pos12 = PlanePosition(Input.GetTouch(1).position);
                var pos11b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                var pos12b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

                //calc zoom
                var zoom = Vector3.Distance(pos11, pos12) /
                           Vector3.Distance(pos11b, pos12b);

                cMFC1COT.m_FollowOffset.y = Mathf.Clamp(Vector3.LerpUnclamped(pos11, cMFC1COT.m_FollowOffset, 1 / zoom).y, farDownward.position.y, farUpward.position.y);
                cMFC2COT.m_FollowOffset.y = cMFC1COT.m_FollowOffset.y;

                cMFC1COT.m_FollowOffset.z = Mathf.Clamp((20 * cMFC1COT.m_FollowOffset.y) / 20, 6, 20) * -1;
                cMFC2COT.m_FollowOffset.z = cMFC1COT.m_FollowOffset.z;
            }
        }
        #endregion

        #region Camera UnFollow And Controller
        if (followEnabled == false)
        {
            if (blendTime != motherBlendTime)
            {
                blendTime = motherBlendTime;
            }
            cMMCC.Priority = 99;
            cMFC1C.Priority = 20;
            cMFC2C.Priority = 30;

            cMMC.transform.position = new Vector3(Mathf.Clamp(cMMC.transform.position.x, farLeft.position.x, farRight.position.x),
                                                     Mathf.Clamp(cMMC.transform.position.y, farDownward.position.y, farUpward.position.y),
                                                     Mathf.Clamp(cMMC.transform.position.z, farBackward.position.z, farForward.position.z));

            if (IsPointerOverUIObject()) return;

                //Update Plane
                if (Input.touchCount >= 1)
                plane.SetNormalAndPosition(transform.up, transform.position);

            var Delta1 = Vector3.zero;
            var Delta2 = Vector3.zero;
            
            //Scroll
            if (Input.touchCount >= 1)
            {
                Delta1 = PlanePositionDelta(Input.GetTouch(0));
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                    cMMC.transform.Translate(Delta1, Space.World);
            }
            
            //Pinch
            if (Input.touchCount >= 2)
            {
                var pos1 = PlanePosition(Input.GetTouch(0).position);
                var pos2 = PlanePosition(Input.GetTouch(1).position);
                var pos1b = PlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                var pos2b = PlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);

                //calc zoom
                var zoom = Vector3.Distance(pos1, pos2) /
                           Vector3.Distance(pos1b, pos2b);

                //edge case
                if (zoom == 0 || zoom > 10)
                    return;

                //Move cam amount the mid ray
                if (pos1 != pos1b)
                {
                    cMMCC.transform.position = Vector3.LerpUnclamped(pos1, cMMCC.transform.position, 1 / zoom);
                    var rotationVector = cMMC.transform.rotation.eulerAngles;
                    rotationVector.x = Mathf.Clamp((45 * cMMC.transform.localPosition.y) / 20, 30, 75);
                    cMMC.transform.rotation = Quaternion.Euler(rotationVector);
                }

                if (rotate && pos1 != pos1b && pos2b != pos2)
                {
                    Ray ray = mCamera.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit[] hits;
                    RaycastHit posR;

                    hits = Physics.RaycastAll(mCamera.ScreenPointToRay(Input.GetTouch(0).position), 100.0F, LayerGround);
                    if (hits != null)
                    {
                        posR = hits[0];

                        cMMCC.transform.RotateAround(posR.point, plane.normal, Vector3.SignedAngle(pos2 - pos1, pos2b - pos1b, plane.normal) * 2);
                    }
                }
            }

        }
        #endregion

    }

    protected Vector3 PlanePositionDelta(Touch touch)
    {
        //not moved
        if (touch.phase != TouchPhase.Moved)
            return Vector3.zero;

        //delta
        var rayBefore = mCamera.ScreenPointToRay(touch.position - touch.deltaPosition);
        var rayNow = mCamera.ScreenPointToRay(touch.position);
        if (plane.Raycast(rayBefore, out var enterBefore) && plane.Raycast(rayNow, out var enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        //not on plane
        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        //position
        var rayNow = mCamera.ScreenPointToRay(screenPos);
        if (plane.Raycast(rayNow, out var enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    private void SearchFollowTarget()
    {
        //if (!GlobalVariables.readyForAttack)
        //{
        //    return;
        //}

        GameObject[] allies = GameObject.FindGameObjectsWithTag("Alliance");
        shortestDistance = Mathf.Infinity;
        nearestAlly = null;

        foreach (GameObject ally in allies)
        {
            if (ally.GetComponent<Health>().IsDead())
            {
                continue;
            }
            FindingNearestenemy(ally);
        }

        if (nearestAlly != null)
        {
            followTarget = nearestAlly.transform;
        }
        else
        {
            followTarget = null;
        }

        if (!firstTargetConnection && followTarget != null)
        {
            cMFC2C.Follow = followTarget;
            cMFC2C.LookAt = followTarget;
            cMFC1C.Follow = followTarget;
            cMFC1C.LookAt = followTarget;
            firstTargetConnection = true;
        }
    }

    private GameObject FindingNearestenemy(GameObject ally)
    {
        float distanceToEnemy = Vector3.Distance(ally.transform.position, endOfTheDestination.position);
        if (distanceToEnemy < shortestDistance)
        {
            shortestDistance = distanceToEnemy;
            nearestAlly = ally;
        }

        return nearestAlly;
    }

    public static void SetCameraFollow(bool value)
    {
        followEnabled = value;
    }

    public static bool GetCameraFollow()
    {
        return followEnabled;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }

    public void SetEnemyCameraLook(Transform lookAt)
    {
        cMEC.transform.position = cMMC.transform.position;
        cMECC.LookAt = lookAt;
        cMMCC.Priority = 10;
        cMECC.Priority = 108;
    }

    public void SetCameraLookAt(Transform lookAt)
    {
        cMLC.transform.position = cMMC.transform.position;
        cMLCC.LookAt = lookAt;
        cMMCC.Priority = 10;
        cMLCC.Priority = 109;
        StartCoroutine(BrokeSetCameraLookAt());
    }
    IEnumerator BrokeSetCameraLookAt()
    {
        yield return new WaitForSeconds(motherBlendTime);

        var rotationVector = cMMC.transform.rotation.eulerAngles;
        rotationVector.y = cMLC.transform.rotation.eulerAngles.y;
        cMMC.transform.rotation = Quaternion.Euler(rotationVector);

        var rotationVector2 = cMMC.transform.rotation.eulerAngles;
        rotationVector2.x = Mathf.Clamp((45 * cMMC.transform.localPosition.y) / 20, 30, 75);
        cMMC.transform.rotation = Quaternion.Euler(rotationVector2);

        cMMCC.Priority = 110;
        cMECC.Priority = 9;
        cMLCC.Priority = 10;
        cMLCC.LookAt = null;
    }

    IEnumerator WaitForC1()
    {
        yield return new WaitForSeconds(motherBlendTime);

        cMFC2COT.m_XAxis.Value = cMFC1COT.m_XAxis.Value;

        coroutineActivated = false;
    }
    IEnumerator WaitForC2()
    {
        coroutineActivated = true;
        if (cMMCC.Priority < 90)
        {
            Vector3 pos = new Vector3(cMFC2.transform.position.x, cineMachineMainCameraOriginalY, cMFC2.transform.position.z);
            cMMC.transform.position = pos;
            var rotationVector2 = cMMC.transform.rotation.eulerAngles;
            rotationVector2.x = Mathf.Clamp((45 * cMMC.transform.localPosition.y) / 20, 30, 75);
            cMMC.transform.rotation = Quaternion.Euler(rotationVector2);
        }

        if (cMFC1C.Follow == followTarget && cMFC1C.Priority == 102)
        {
            cMFC2COT.m_XAxis.Value = cMFC1COT.m_XAxis.Value;
            coroutineActivated = false;
            yield break;
        }

        cMFC2C.Follow = followTarget;
        cMFC2C.LookAt = followTarget;

        cMMCC.Priority = 15;
        cMFC1C.Priority = 19;
        cMFC2C.Priority = 102;



        yield return new WaitForSeconds(motherBlendTime);

        cMFC1C.Follow = cMFC2C.Follow;
        cMFC1C.LookAt = cMFC2C.LookAt;

        if (!followEnabled)
        {
            coroutineActivated = false;

            yield break;
        }

        cMFC2C.Priority = 20;
        cMFC1C.Priority = 102;
        StartCoroutine(WaitForC1());
    }

    public void SetEndDestination(Transform endDestination)
    {
        endOfTheDestination.position = endDestination.position;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}

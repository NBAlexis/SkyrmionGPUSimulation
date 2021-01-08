using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    public static CameraManager _Instance = null;

    public Camera m_pCamera;
    public EventSystem m_pUI;

    public Transform m_v4CameraExtend;
    private Vector3 m_v3OffsetStart = Vector3.zero;
    private Vector3 m_v3OffsetNow = Vector3.zero;
    //private static readonly Vector2 m_v2Height2 = new Vector2(78.1f, 110.0f);

    private float m_fMoveTime = -1.0f;
    private float m_fMoveTimeLength = -1.0f;
    private Vector3 m_v3From = Vector3.zero;
    private Vector3 m_v3To = Vector3.zero;

    private float m_fFollowTime = -1.0f;
    private float m_fFollowTimeLength = -1.0f;
    private Transform m_pFollow = null;

    // Update is called once per frame
    void Start()
    {
        //Debug.Log(GetComponent<Camera>().depthTextureMode);
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
        _Instance = this;

        RaycastHit rh;
        if (Physics.Raycast(new Ray(transform.position, transform.forward),
            out rh,
            500.0f,
            LayerMask.GetMask("Ground")))
        {
            m_v3OffsetStart = transform.position - rh.point;
            m_v3OffsetNow = transform.position - rh.point;
        }
        //m_v2Offset = new Vector2(0.0f, -70.0f);
    }

    void Update()
    {
        if (null == _Instance)
        {
            _Instance = this;
        }

        Vector3 vPosNow = transform.position;
        bool bHasTouch = false;
        if (Application.platform == RuntimePlatform.WindowsPlayer
         || Application.platform == RuntimePlatform.WindowsEditor
         || Application.platform == RuntimePlatform.WebGLPlayer)
        {
            bHasTouch = TickMouse(ref vPosNow);
        }
        else if (Application.platform == RuntimePlatform.Android
              || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            bHasTouch = TickTouch(ref vPosNow);
        }

        if (!bHasTouch)
        {
            m_bMouseDown = false;

            if (m_fMoveTime >= 0.0f)
            {
                m_fFollowTime = -1.0f;
                m_vLastSpeed = Vector3.zero;

                m_fMoveTime -= Time.deltaTime;
                float fRate = Mathf.Cos(Mathf.Clamp01(m_fMoveTime / m_fMoveTimeLength) * Mathf.PI) * 0.5f + 0.5f;

                vPosNow = m_v3To * fRate + (1.0f - fRate) * m_v3From;
            }
            else if (m_fFollowTime >= 0.0f)
            {
                m_fFollowTime -= Time.deltaTime;
                m_vLastSpeed = Vector3.zero;
                float fRate = Mathf.Cos(Mathf.Clamp01(m_fFollowTime / m_fFollowTimeLength) * Mathf.PI) * 0.5f + 0.5f;
                Vector3 v3To = m_pFollow.transform.position + m_v3OffsetNow;
                vPosNow = v3To * fRate
                          + (1.0f - fRate)
                          * transform.position;
            }

            if (m_vLastSpeed.sqrMagnitude > 0.01f)
            {
                m_vLastSpeed = m_vLastSpeed * (1.0f - Time.deltaTime * 3.0f);
                vPosNow += m_vLastSpeed * Time.deltaTime;
            }
        }

        float fMinHeight = m_v4CameraExtend.transform.position.y - 0.5f * m_v4CameraExtend.localScale.y;
        float fZOffset = transform.position.y - fMinHeight;
        Vector3 vExtent = new Vector3(
            m_v4CameraExtend.localScale.x,
            m_v4CameraExtend.localScale.y,
            m_v4CameraExtend.localScale.z) * 0.5f;
        vPosNow.x = Mathf.Clamp(vPosNow.x, m_v4CameraExtend.position.x - vExtent.x, m_v4CameraExtend.position.x + vExtent.x);
        vPosNow.y = Mathf.Clamp(vPosNow.y, m_v4CameraExtend.position.y - vExtent.y, m_v4CameraExtend.position.y + vExtent.y);
        vPosNow.z = Mathf.Clamp(vPosNow.z, m_v4CameraExtend.position.z - vExtent.z - fZOffset, m_v4CameraExtend.position.z + vExtent.z - fZOffset);
        transform.position = vPosNow;

    }

    public Vector2 GetOffset()
    {
        return new Vector2(m_v3OffsetStart.x, m_v3OffsetStart.z);
    }


    public void MoveToPos(Vector3 vTarget, float fLength)
    {
        RaycastHit rh;
        if (Physics.Raycast(new Ray(transform.position, transform.forward),
            out rh,
            500.0f,
            LayerMask.GetMask("Ground")))
        {
            m_v3OffsetNow = transform.position - rh.point;
        }

        m_v3From = transform.position;
        m_v3To = vTarget + m_v3OffsetNow;
        m_fMoveTime = fLength;
        m_fMoveTimeLength = fLength;

        //if (null != UIMananger._Instance)
        //{
        //    UIMananger._Instance.SetProtectTime(fLength);
        //}
    }

    public void MoveToPosStartHeight(Vector3 vTarget, float fLength)
    {
        m_v3From = transform.position;
        m_v3To = vTarget + m_v3OffsetStart;
        m_fMoveTime = fLength;
        m_fMoveTimeLength = fLength;

        //if (null != UIMananger._Instance)
        //{
        //    UIMananger._Instance.SetProtectTime(fLength);
        //}
    }

    public void FollowTransformIfFar(Transform pTrans, float fTime, float fDist)
    {
        if (m_fMoveTime >= 0.0f)
        {
            return;
        }

        RaycastHit rh;
        if (Physics.Raycast(new Ray(transform.position, transform.forward),
            out rh,
            500.0f,
            LayerMask.GetMask("Ground")))
        {
            m_v3OffsetNow = transform.position - rh.point;
        }

        Vector2 vDelta = new Vector2(
            pTrans.transform.position.x + m_v3OffsetNow.x - transform.position.x,
            pTrans.transform.position.z + m_v3OffsetNow.z - transform.position.z
            );

        if ((vDelta.magnitude * 80.0f / transform.position.y) < fDist)
        {
            return;
        }

        m_pFollow = pTrans;
        m_fFollowTimeLength = fTime;
        m_fFollowTime = fTime;
    }


    private Vector3 m_vDownPos;
    private bool m_bMouseDown = false;
    private Vector3 m_vLastSpeed = Vector3.zero;

    private bool TickMouse(ref Vector3 vPosNow)
    {
        if (Input.GetMouseButton(0) && !m_pUI.IsPointerOverGameObject())
        {
            if (!m_bMouseDown)
            {
                RaycastHit rh;
                if (Physics.Raycast(m_pCamera.ScreenPointToRay(Input.mousePosition),
                    out rh,
                    500.0f,
                    LayerMask.GetMask("Ground")))
                {
                    m_vDownPos = rh.point;
                    m_bMouseDown = true;
                }
            }
            else
            {
                RaycastHit rh;
                if (Physics.Raycast(m_pCamera.ScreenPointToRay(Input.mousePosition),
                    out rh,
                    500.0f,
                    LayerMask.GetMask("Ground")))
                {
                    Vector3 vNewDownPos = rh.point;
                    Vector3 vDelta = m_vDownPos - vNewDownPos;
                    vDelta.y = 0.0f;
                    m_vLastSpeed = vDelta / Time.deltaTime;
                    vPosNow += vDelta;
                }
            }

            m_fMoveTime = -1.0f;
            m_fFollowTime = -1.0f;
            return true;
        }

        if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.1f)
        {
            Vector3 vDelta = Input.mouseScrollDelta.y * transform.forward * 5.0f;
            m_vLastSpeed = Vector3.zero;
            vPosNow += vDelta;

            m_fMoveTime = -1.0f;
            m_fFollowTime = -1.0f;
            return true;
        }

        return false;
    }

    private class CTouchCase
    {
        public int m_iFingerId;
        public Vector3 m_v3DownPos;
        public Vector2 m_v2ScreenPos;
        public bool m_bUpdated;
        public bool m_bJustAdded;
    }

    private readonly List<CTouchCase> m_lstTouches = new List<CTouchCase>();

    private Vector3 m_vOld2FDownCenter = Vector3.zero;
    private Vector3 m_vOld2FOffset = Vector3.zero;
    private float m_fOldDownLength = 1.0f;

    private bool TickTouch(ref Vector3 vPosNow)
    {
        int iOldTouchCount = m_lstTouches.Count;

        for (int i = 0; i < m_lstTouches.Count; ++i)
        {
            m_lstTouches[i].m_bUpdated = false;
        }

        for (int i = 0; i < Input.touchCount; ++i)
        {
            
            Touch t = Input.GetTouch(i);
            if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
            {
                if (!m_pUI.IsPointerOverGameObject(t.fingerId))
                {
                    int iToBeFound = FindTouchFingerId(t.fingerId);
                    if (-1 == iToBeFound)
                    {
                        m_lstTouches.Add(new CTouchCase
                        {
                            m_bUpdated = true,
                            m_v2ScreenPos = t.position,
                            m_iFingerId = t.fingerId,
                            m_bJustAdded = true,
                        });
                    }
                    else
                    {
                        m_lstTouches[iToBeFound].m_bUpdated = true;
                        m_lstTouches[iToBeFound].m_bJustAdded = false;
                        m_lstTouches[iToBeFound].m_v2ScreenPos = t.position;
                    }
                }
            }
        }

        m_lstTouches.RemoveAll(ToBeRemoved);

        if (0 == m_lstTouches.Count)
        {
            return false;
        }

        m_fMoveTime = -1.0f;
        m_fFollowTime = -1.0f;

        //string sDebugInfo = "";
        //for (int i = 0; i < m_lstTouches.Count; ++i)
        //{
        //    sDebugInfo += string.Format("T:{0},{1},{2} ", m_lstTouches[i].m_iFingerId, m_lstTouches[i].m_v2ScreenPos.x, m_lstTouches[i].m_v2ScreenPos.y);
        //}

        //if (null != UIMananger._Instance)
        //{
        //    UIMananger._Instance.SetDebugInfo(sDebugInfo);
        //}


        if (1 == m_lstTouches.Count)
        {
            if (1 != iOldTouchCount)
            {
                //fix down pos
                RaycastHit rh;
                if (Physics.Raycast(m_pCamera.ScreenPointToRay(
                        new Vector3(m_lstTouches[0].m_v2ScreenPos.x, m_lstTouches[0].m_v2ScreenPos.y, 0.0f)),
                    out rh,
                    500.0f,
                    LayerMask.GetMask("Ground")))
                {
                    m_lstTouches[0].m_v3DownPos = rh.point;
                }
                else
                {
                    m_lstTouches.Clear();
                }
            }
            else
            {
                Vector3 vNewPos = m_lstTouches[0].m_v3DownPos;
                RaycastHit rh;
                if (Physics.Raycast(m_pCamera.ScreenPointToRay(
                        new Vector3(m_lstTouches[0].m_v2ScreenPos.x, m_lstTouches[0].m_v2ScreenPos.y, 0.0f)),
                    out rh,
                    500.0f,
                    LayerMask.GetMask("Ground")))
                {
                    vNewPos = rh.point;
                }

                Vector3 vDelta = m_lstTouches[0].m_v3DownPos - vNewPos;
                vDelta.y = 0.0f;
                m_vLastSpeed = vDelta / Time.deltaTime;
                vPosNow += vDelta;
            }
        }
        else if (2 == m_lstTouches.Count)
        {
            if (2 != iOldTouchCount)
            {
                for (int i = 0; i < m_lstTouches.Count; ++i)
                {
                    RaycastHit rh;
                    if (Physics.Raycast(m_pCamera.ScreenPointToRay(
                            new Vector3(m_lstTouches[i].m_v2ScreenPos.x, m_lstTouches[i].m_v2ScreenPos.y, 0.0f)),
                        out rh,
                        500.0f,
                        LayerMask.GetMask("Ground")))
                    {
                        m_lstTouches[i].m_v3DownPos = rh.point;
                    }
                    else
                    {
                        m_lstTouches[i].m_bUpdated = false;
                    }
                }
                m_lstTouches.RemoveAll(ToBeRemoved);

                if (2 == m_lstTouches.Count)
                {
                    Vector3 vOldDownPos1 = m_lstTouches[0].m_v3DownPos;
                    Vector3 vOldDownPos2 = m_lstTouches[1].m_v3DownPos;
                    m_vOld2FDownCenter = 0.5f * (vOldDownPos1 + vOldDownPos2);
                    m_vOld2FOffset = (vPosNow - m_vOld2FDownCenter);
                    m_fOldDownLength = (m_lstTouches[0].m_v2ScreenPos - m_lstTouches[1].m_v2ScreenPos).magnitude;
                }
            }
            else
            {
                Vector3 vDownPos1 = m_lstTouches[0].m_v3DownPos;
                Vector3 vDownPos2 = m_lstTouches[1].m_v3DownPos;

                RaycastHit rh;
                if (Physics.Raycast(m_pCamera.ScreenPointToRay(
                        new Vector3(m_lstTouches[0].m_v2ScreenPos.x, m_lstTouches[0].m_v2ScreenPos.y, 0.0f)),
                    out rh,
                    500.0f,
                    LayerMask.GetMask("Ground")))
                {
                    vDownPos1 = rh.point;
                }
                if (Physics.Raycast(m_pCamera.ScreenPointToRay(
                        new Vector3(m_lstTouches[1].m_v2ScreenPos.x, m_lstTouches[1].m_v2ScreenPos.y, 0.0f)),
                    out rh,
                    500.0f,
                    LayerMask.GetMask("Ground")))
                {
                    vDownPos2 = rh.point;
                }

                Vector3 v2FDownCenter = 0.5f * (vDownPos1 + vDownPos2);
                float f2FLength = (m_lstTouches[0].m_v2ScreenPos - m_lstTouches[1].m_v2ScreenPos).magnitude;
                Vector3 vRealNewCenter = 2.0f * m_vOld2FDownCenter - v2FDownCenter;
                Vector3 vNewRightPos = vRealNewCenter + m_vOld2FOffset 
                    * (m_fOldDownLength / f2FLength);

                m_vLastSpeed = (vNewRightPos - vPosNow) / Time.deltaTime;
                m_vLastSpeed.y = 0.0f;
                vPosNow = vNewRightPos;

                //m_vOld2FDownCenter = vRealNewCenter;
                m_vOld2FOffset = vPosNow - m_vOld2FDownCenter;
                m_fOldDownLength = f2FLength;
            }
        }

        return true;
    }

    private int FindTouchFingerId(int iFingerId)
    {
        for (int i = 0; i < m_lstTouches.Count; ++i)
        {
            if (m_lstTouches[i].m_iFingerId == iFingerId)
            {
                return i;
            }
        }

        return -1;
    }

    private bool ToBeRemoved(CTouchCase t)
    {
        return !t.m_bUpdated;
    }
}

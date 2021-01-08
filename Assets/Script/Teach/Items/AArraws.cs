using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AArraws : MonoBehaviour
{
    public GameObject m_pGOPrefab;
    private List<GameObject> m_pCreatedMesh = null;
    public int m_iSubMeshDiv;
    public float m_fSep;
    private int m_iInSubDiv;

    private int m_iVertCount;
    private int m_iFaceCount;

    private Vector3[] m_vOldPos = null;
    private Vector2[] m_vOldUV = null;
    private int[] m_iOldFace = null;

    public void Show(Texture2D dataToShow)
    {
        for (int x = 0; x < m_iSubMeshDiv; ++x)
        {
            for (int y = 0; y < m_iSubMeshDiv; ++y)
            {
                int iIndex = x * m_iSubMeshDiv + y;
                m_pCreatedMesh[iIndex].SetActive(true);

                List<Vector3> allVerts = new List<Vector3>();
                List<Color> allColors = new List<Color>();

                for (int i = 0; i < m_iInSubDiv; ++i)
                {
                    for (int j = 0; j < m_iInSubDiv; ++j)
                    {
                        int iRealI = x * m_iInSubDiv + i;
                        int iRealJ = y * m_iInSubDiv + j;
                        Color c = dataToShow.GetPixel(iRealI, iRealJ);
                        Color realc;
                        Vector3 vN;

                        ExchangeColor(c, out realc, out vN);

                        for (int k = 0; k < m_iVertCount; ++k)
                        {
                            allVerts.Add(FaceTo(m_vOldPos[k], -vN) + new Vector3(i * m_fSep, -j * m_fSep, 0.0f));
                            //allUVs.Add(m_vOldUV[k]);
                            allColors.Add(realc);
                        }

                        //for (int k = 0; k < m_iFaceCount; ++k)
                        //{
                        //    allFaces.Add(m_iOldFace[k] + iIndex2 * m_iVertCount);
                        //}
                    }
                }

                Mesh onemesh = m_pCreatedMesh[iIndex].GetComponent<MeshFilter>().mesh;
                onemesh.SetVertices(allVerts);
                onemesh.SetColors(allColors);
                //onemesh.SetUVs(0, allUVs);
                //onemesh.SetTriangles(allFaces, 0, true);

                //onemesh.MarkDynamic();
                //onemesh.OptimizeReorderVertexBuffer();
                onemesh.UploadMeshData(false);
            }
        }
    }


    public void Hide()
    {
        for (int i = 0; i < m_pCreatedMesh.Count; ++i)
        {
            m_pCreatedMesh[i].SetActive(false);
        }
    }

    public Mesh m_pPrefab;

    // Start is called before the first frame update
    void Start()
    {
        m_iInSubDiv = UISimulate.reso / m_iSubMeshDiv;

        SetUpArraws();
        Hide();
    }

    private void SetUpArraws()
    {
        m_pCreatedMesh = new List<GameObject>();
        for (int i = 0; i < m_iSubMeshDiv; ++i)
        {
            for (int j = 0; j < m_iSubMeshDiv; ++j)
            {
                GameObject created = Instantiate<GameObject>(m_pGOPrefab, transform);
                created.name = string.Format("A_{0}_{1}", i, j);
                created.transform.position = new Vector3(i * m_fSep * m_iInSubDiv, 0.0f, j * m_fSep * m_iInSubDiv);
                m_pCreatedMesh.Add(created);
            }
        }


        m_vOldPos = m_pPrefab.vertices;
        m_vOldUV = m_pPrefab.uv;
        m_iOldFace = m_pPrefab.triangles;
        m_iVertCount = m_vOldUV.Length;
        m_iFaceCount = m_iOldFace.Length;

        //for (int i = 0; i < m_vOldPos.Length; ++i)
        //{
        //    Debug.Log(m_vOldPos[i]);
        //}

        for (int x = 0; x < m_iSubMeshDiv; ++x)
        {
            for (int y = 0; y < m_iSubMeshDiv; ++y)
            {
                int iIndex = x * m_iSubMeshDiv + y;
                List<Vector3> allVerts = new List<Vector3>();
                List<Vector2> allUVs = new List<Vector2>();
                List<Color> allColors = new List<Color>();
                List<int> allFaces = new List<int>();

                for (int i = 0; i < m_iInSubDiv; ++i)
                {
                    for (int j = 0; j < m_iInSubDiv; ++j)
                    {
                        int iIndex2 = i * m_iInSubDiv + j;


                        for (int k = 0; k < m_iVertCount; ++k)
                        {
                            allVerts.Add(m_vOldPos[k] + new Vector3(i * m_fSep, -j * m_fSep, 0.0f));
                            allUVs.Add(m_vOldUV[k]);
                            allColors.Add(Color.white);
                        }

                        for (int k = 0; k < m_iFaceCount; ++k)
                        {
                            allFaces.Add(m_iOldFace[k] + iIndex2 * m_iVertCount);
                        }
                    }
                }

                Mesh onemesh = new Mesh();
                onemesh.SetVertices(allVerts);
                onemesh.SetColors(allColors);
                onemesh.SetUVs(0, allUVs);
                onemesh.SetTriangles(allFaces, 0, true);

                onemesh.MarkDynamic();
                onemesh.OptimizeReorderVertexBuffer();
                onemesh.UploadMeshData(false);

                m_pCreatedMesh[iIndex].GetComponent<MeshFilter>().mesh = onemesh;
            }
        }
    }

    private void ExchangeColor(Color con, out Color cout, out Vector3 v)
    {
        float fTheta = con.r * Mathf.PI;
        float fPhi = con.g * Mathf.PI * 2.0f;

        v = new Vector3(
            Mathf.Cos(fPhi) * Mathf.Sin(fTheta),
            Mathf.Sin(fPhi) * Mathf.Sin(fTheta),
            Mathf.Cos(fTheta)
            );

        float _InverseNz = 0.0f;
        float fDarkness = Mathf.Clamp01(1.0f - v.z * (1.0f - _InverseNz * 2.0f));

        cout = new Color(
            Mathf.Clamp01(Vector2.Dot(new Vector2(-1.0f, 1.0f), new Vector2(v.x, v.y))) * fDarkness + (1.0f - fDarkness),
            Mathf.Clamp01(Vector2.Dot(new Vector2(-1.0f, -1.0f), new Vector2(v.x, v.y))) * fDarkness + (1.0f - fDarkness),
            Mathf.Clamp01(Vector2.Dot(new Vector2(1.0f, 0.0f), new Vector2(v.x, v.y))) * fDarkness + (1.0f - fDarkness),
            1.0f);
    }

    private Vector3 FaceTo(Vector3 vIn, Vector3 vN)
    {
        vN.y = -vN.y;
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, vN);
        Matrix4x4 trans = Matrix4x4.Rotate(rot);
        return trans.MultiplyVector(vIn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

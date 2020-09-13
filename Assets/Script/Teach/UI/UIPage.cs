using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CUIPageConfig
{

}


public class UIPage : MonoBehaviour
{
    public UIManager m_pOwner;
    public EPage m_ePage;

    public Animation[] m_pAnims;
    protected float m_fAnimTime = -1.0f;
    protected bool m_bInOut = true;
    protected EPage m_eNextPage = EPage.None;
    protected CUIPageConfigSimulate m_pNextPageConfig = null;

    // Start is called before the first frame update
    public virtual void Start()
    {
        Debug.Log("base.Start");
    }

    // Update is called once per frame
    public virtual void Update()
    {
        //Debug.Log("in update:" + m_fAnimTime);

        if (m_fAnimTime > 0.0f)
        {
            m_fAnimTime -= Time.deltaTime;
            if (m_bInOut)
            {
                FadeIn(m_fAnimTime);
            }
            else
            {
                FadeOut(m_fAnimTime);
                if (m_fAnimTime <= 0.0f)
                {
                    m_pOwner.GoPage(m_eNextPage, m_pNextPageConfig);
                }
            }
        }
    }
    

    public virtual void Intial(UIManager pManager, EPage ePage)
    {
        m_pOwner = pManager;
        m_ePage = ePage;
    }

    public virtual void OnButtonBack()
    {
        
    }

    public virtual void Show(CUIPageConfig config)
    {
        gameObject.SetActive(true);
        m_fAnimTime = 1.0f;
        m_bInOut = true;
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    public virtual void ToHide()
    {
        m_fAnimTime = 0.5f;
        m_bInOut = false;
    }

    public virtual void FadeIn(float fAnimTime)
    {

    }

    public virtual void FadeOut(float fAnimTime)
    {

    }

    protected void GoPage(EPage eNextPage, CUIPageConfigSimulate config)
    {
        m_eNextPage = eNextPage;
        m_pNextPageConfig = config;
        ToHide();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChooseImage : MonoBehaviour
{
    public Image m_pSelf;
    public RawImage m_pImage;
    public Text m_pText;
    public int m_iIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(Texture2D tx, Material mat, string sTitle)
    {
        m_pText.text = sTitle;
        m_pImage.texture = tx;
        m_pImage.material = mat;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ChooseMe(bool bChoose)
    {
        m_pSelf.color = bChoose ? Color.cyan : Color.gray;
    }

    public void OnChooseImage()
    {
        UIManager._Instance.OnChooseImage(m_iIndex);
    }
}

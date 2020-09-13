using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDialogEvent : MonoBehaviour
{
    public EDialog m_eDialog;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDialogClosed()
    {
        UIManager._Instance.OnDialogClosed(m_eDialog);
    }

    void OnDialogPopUp()
    {
        UIManager._Instance.OnDialogPopUp(m_eDialog);
    }
}

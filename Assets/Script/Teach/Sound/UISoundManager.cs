using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EUISound
{
    Button,
}


public class UISoundManager : MonoBehaviour
{

    private static UISoundManager _Instance;

    // Start is called before the first frame update
    void Start()
    {
        _Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void UISound(EUISound eSound)
    {
        if (null != _Instance)
        {
            _Instance._UISound(eSound);
        }
    }

    private void _UISound(EUISound eSound)
    {
        m_pUISound.Stop();
        m_pUISound.clip = m_pClips[(int) EUISound.Button];
        m_pUISound.Play();
    }

    public AudioSource m_pUISound;
    public AudioClip[] m_pClips;
}

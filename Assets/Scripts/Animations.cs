using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    public GameObject _panel;
    public Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator.SetBool("BalikButton", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Balik()
    {
        
        _animator.SetBool("BalikButton", false);
    }

    public void PanelButton()
    {
        _panel.SetActive(true);
        _animator.SetBool("BalikButton", true);
    }
    
    public void Mayo()
    {
        _panel.SetActive(false);
       // _animator.SetBool("BalikButton", true);
    }
}

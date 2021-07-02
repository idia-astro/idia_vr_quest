using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapBetweenChildren : MonoBehaviour
{
    public int SwapDelaySeconds = 2;
    
    private WaitForSeconds _delay;
    private List<GameObject> _children;
    private int _activeIndex;
        
    
    
    private Coroutine _coroutine;
    // Start is called before the first frame update
    void Awake()
    {
        _delay = new WaitForSeconds(SwapDelaySeconds);
        _children = new List<GameObject>();

        foreach (Transform child in transform)
        {
            _children.Add(child.gameObject);
        }

        if (_children.Count > 0)
        {
            foreach (var child in _children)
            {
                child.SetActive(false);
            }
            _children[0].SetActive(true);
        }

        _activeIndex = 0;

        StartCoroutine("SwapCoroutine");
    }

    IEnumerator SwapCoroutine()
    {
        while (true)
        {
            yield return _delay;
            
            if (_children.Count == 0)
            {
                continue;
            }

            _children[_activeIndex].SetActive(false);
            _activeIndex = (_activeIndex + 1) % _children.Count;
            Debug.Log($"Setting child {_children[_activeIndex].name} to active");
            _children[_activeIndex].SetActive(true);
        }
    }
}

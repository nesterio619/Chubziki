using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    private float _lastPressed;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {            
            Instantiate(playerPrefab, transform);
            _lastPressed = Time.time;
        }
        if (Input.GetKey(KeyCode.I))
        {
            if (_lastPressed + 0.5f >= Time.time) return;
            Instantiate(playerPrefab, transform);
        }
    }
}

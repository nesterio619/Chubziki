using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_AnimatedObject : MonoBehaviour
{
    public Transform tr;

    private Vector3 freezePos;
    // Start is called before the first frame update
    void Start()
    {
        freezePos = tr.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        tr.position = freezePos;
    }
}

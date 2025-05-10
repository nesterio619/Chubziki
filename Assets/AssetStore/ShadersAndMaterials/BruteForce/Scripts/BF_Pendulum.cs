using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_Pendulum : MonoBehaviour
{
    public Rigidbody rb;
    private int dirChange = 1;

    private void Start()
    {
        StartCoroutine(WaitDirection());
    }
    private void FixedUpdate()
    {
        rb.AddForce((Vector3.right * 8f + Vector3.up*-4f) * dirChange, ForceMode.Acceleration);
    }

    private IEnumerator WaitDirection()
    {
        for(; ;)
        {
            yield return new WaitForSeconds(2.5f);
            dirChange = -dirChange;
        }
    }
}

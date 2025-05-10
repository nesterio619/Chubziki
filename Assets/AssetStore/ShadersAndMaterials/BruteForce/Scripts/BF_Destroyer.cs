using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_Destroyer : MonoBehaviour
{
    public ParticleSystem ps;
    void OnEnable()
    {
        StartCoroutine(WaitDestroy());
    }

    private IEnumerator WaitDestroy()
    {
        yield return new WaitForSeconds(ps.duration + 1f);
        while (ps.particleCount > 0)
        {
            yield return new WaitForSeconds(ps.duration + 1f);
        }
            Destroy(this.gameObject);
    }

}

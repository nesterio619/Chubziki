using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_Footsteps : MonoBehaviour
{
    public ParticleSystem particleFootstep;
    public Transform leftFoot;
    public Transform rightFoot;

    private ParticleSystem leftPs;
    private ParticleSystem rightPs;

    private void Start()
    {
        leftPs = Instantiate(particleFootstep);
        rightPs = Instantiate(particleFootstep);
    }
    public void OnFootstep(int feetIndex) // 0 = left / 1 = right
    {
        if (feetIndex == 0)
        {
            leftPs.transform.position = leftFoot.position;
            leftPs.transform.localScale = leftFoot.lossyScale;
            leftPs.Emit(1);
        }
        if (feetIndex == 1)
        {
            rightPs.transform.position = rightFoot.position;
            rightPs.transform.localScale = rightFoot.lossyScale;
            rightPs.Emit(1);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChubzikEquipmentPlacer : MonoBehaviour
{
    public SkinnedMeshRenderer Body;
    public SkinnedMeshRenderer Boots;
    private Transform[] _bones;
    public Transform rootBone;

    void Start()
    {
    }

    [ContextMenu("Connect")]
    public void Connect()
    {
        Boots.bones = Body.bones;
        Boots.rootBone = Body.rootBone;
    }

}

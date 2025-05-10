using Components;
using Components.Animation;
using Core.ObjectPool;
using System.Collections.Generic;
using UnityEngine;

public class ChubzikModel : PooledGameObject
{
    [field: SerializeField] public Animator ModelAnimator { get; private set; }
    [field: SerializeField] public Rigidbody ModelRigidbody { get; private set; }
    [field: SerializeField] public RagdollComponent ModelRagollComponent { get; private set; }
    [field: SerializeField] public ActorAnimatorController Animator { get; private set; }
    [field: SerializeField] public Renderer[] Renderers { get; private set; }
    [field: SerializeField] public SkinnedMeshRenderer BodySkinnedMeshRenderer { get; private set; }
    [field: SerializeField] public ArmorEquipment ArmorEquipment { get; private set; }

    [field: SerializeField] public Transform[] Bones;
    [field: SerializeField] public Transform RootBone;

    [field: SerializeField] public Transform RightHand { get; private set; }
    [field: SerializeField] public Transform LeftHand { get; private set; }


    public SkinnedMeshRenderer meshRenderer;

    public SkinnedMeshRenderer copyRenderer;


    [ContextMenu("SetUpBounes")]
    public void SetUpBounes()
    {
        Bones = meshRenderer.bones;
        RootBone = meshRenderer.rootBone;
    }

    [ContextMenu("SetCopyBounesFromClass")]
    public void SetCopyBounesFromClass()
    {
        copyRenderer.bones = Bones;
        copyRenderer.rootBone = RootBone;
    }

    [ContextMenu("CopyBounesFromRenderer")]
    public void CopyBounesFromRenderer()
    {
        copyRenderer.bones = meshRenderer.bones;
        copyRenderer.rootBone = meshRenderer.rootBone;
    }

}

public enum Hand
{
    Right,
    Left
}

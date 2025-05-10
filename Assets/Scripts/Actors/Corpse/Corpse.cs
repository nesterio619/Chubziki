using Core.ObjectPool;
using Regions;
using System.Collections;
using UnityEngine;

public class Corpse : PooledGameObject
{
    [SerializeField] private SkinnedMeshRenderer corpseMeshRenderer;

    [SerializeField, Tooltip("The distance to which the corpse descends")] 
    private float targetOffsetY;

    [SerializeField] private Gradient gradient;

    [SerializeField] private float speedOfChangingColor;

    private Vector3 _deadActorPosition;

    private static readonly int AlbedoColorID = Shader.PropertyToID("_AlbedoColor");

    private MaterialPropertyBlock mpb;

    private Sector _mySector;

    void Start()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void SetAlbedoColor(Color color)
    {
        corpseMeshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(AlbedoColorID, color);
        corpseMeshRenderer.SetPropertyBlock(mpb);
    }

    public void SetSector(Sector sector)
    {
        if (sector == null)
            Debug.Log("sector == null");

        _mySector = sector;
        _mySector.onExit.AddListener(ReturnToPool);
    }

    public override void ReturnToPool()
    {
        _mySector.onExit.RemoveListener(ReturnToPool);
        base.ReturnToPool();
    }

    public void SetBonesPositionAndRotation(Transform[] bones)
    {
        for (int index = 0; index < bones.Length; index++)
        {
            corpseMeshRenderer.bones[index].position = bones[index].position;
            corpseMeshRenderer.bones[index].rotation = bones[index].rotation;
        }

        _deadActorPosition = transform.position;

        StartCoroutine(CorpseDisappearingProcess());

    }

    public IEnumerator CorpseDisappearingProcess()
    {
        // The range from 0 to 1 indicates the fade-out percentage of the corpse (from fully visible to fully transparent), but it doesn't mean the corpse is removed 

        float disappearingIndex = 0;

        while (disappearingIndex <= 1)
        {
            yield return null;

            disappearingIndex += speedOfChangingColor * Time.deltaTime;

            var corpsePosition = _deadActorPosition;
            corpsePosition.y = _deadActorPosition.y - targetOffsetY * disappearingIndex;
            transform.position = corpsePosition;

            SetAlbedoColor(gradient.Evaluate(disappearingIndex));
        }
    }


}

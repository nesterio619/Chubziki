using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BF_InteractiveObject : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public ParticleSystem particleSystemLong;
    public ParticleSystem particleSystemStay;
    private ParticleSystem instantiatedPs;
    private ParticleSystem instantiatedLongPs;
    private ParticleSystem instantiatedStayPs;
    private int colCount = 0;
    private Coroutine longEmit;
    private Collision latestCollision = null;
    public GameObject groundImpactPs;
    public Mesh sourceMesh;
    private Mesh simplifiedMesh;
    public bool doSimplify = true;
    public float hitEffectScale = 1f;
    public float simplifyingFactor = 0.1f;

    private GameObject parentGO;
    [HideInInspector] public static GameObject parentEffectsGO;



    void Start()
    {
        if (sourceMesh != null && doSimplify)
        {
            UnityMeshSimplifier.MeshSimplifier newMesh = new UnityMeshSimplifier.MeshSimplifier();
            newMesh.Initialize(sourceMesh);
            UnityMeshSimplifier.SimplificationOptions options = UnityMeshSimplifier.SimplificationOptions.Default;
            options.PreserveBorderEdges = true;
            newMesh.SimplifyMesh(simplifyingFactor);
            simplifiedMesh = newMesh.ToMesh();
        }
        else
        {
            simplifiedMesh = sourceMesh;
        }

        instantiatedPs = CustomPsInstantiate(particleSystem);
        instantiatedLongPs = CustomPsInstantiate(particleSystemLong);
        instantiatedStayPs = CustomPsInstantiate(particleSystemStay);
    }


    private void SpawnEffect(Collision collision, int indexPs)
    {
        // 0 = Short Hit / 1 = Long Hit / 2 = Stay Hit //
        ParticleSystem SpawnParticleSystem = null;

        if(indexPs == 0)
            SpawnParticleSystem = instantiatedPs;
        if(indexPs == 1)
            SpawnParticleSystem = instantiatedLongPs;
        if(indexPs == 2)
            SpawnParticleSystem = instantiatedStayPs;

        if (collision != null && collision.contactCount > 0 && indexPs == 0 && collision.relativeVelocity.magnitude > 6f)
        {
            GameObject groundImpact = Instantiate(groundImpactPs); 
            groundImpact.transform.position = collision.contacts[0].point;
            groundImpact.transform.localScale = Vector3.one * hitEffectScale;
            groundImpact.transform.parent = parentGO.transform;
        }

        SpawnParticleSystem.transform.position = this.transform.position;

        ParticleSystem.MainModule mainParticle = SpawnParticleSystem.main;
        mainParticle.startRotationX = this.transform.rotation.eulerAngles.x / (180.0f / Mathf.PI);
        mainParticle.startRotationY = this.transform.rotation.eulerAngles.y / (180.0f / Mathf.PI);
        mainParticle.startRotationZ = this.transform.rotation.eulerAngles.z / (180.0f / Mathf.PI);

        mainParticle.startSizeX = this.transform.lossyScale.x;
        mainParticle.startSizeY = this.transform.lossyScale.y;
        mainParticle.startSizeZ = this.transform.lossyScale.z;

        SpawnParticleSystem.Emit(1);

        Vector3 normalHits = Vector3.zero;
        Vector3 normalPoses = Vector3.zero;
        foreach (var col in collision.contacts)
        {
            normalHits += col.normal;
            normalPoses += col.point;
        }
        Vector3 normalHit = normalHits / (float)collision.contacts.ToList().Count;
        Vector3 normalPos = normalPoses / (float)collision.contacts.ToList().Count;

        List<Vector4> customData = new List<Vector4>();
        List<Vector4> customData2 = new List<Vector4>();

        SpawnParticleSystem.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        SpawnParticleSystem.GetCustomParticleData(customData2, ParticleSystemCustomData.Custom2);

        if(indexPs == 1 || indexPs == 2)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 10f))
            {
                normalHit = hit.normal.normalized;
                normalPos = hit.point;
            }
            
        }

        for (int i = 0; i < customData.Count; i++)
        {
            if (i == customData.Count - 1)
            {
                customData[i] = new Vector4(normalHit.x, normalHit.y, normalHit.z, 0); // you had to disable the module
            }
        }
        for (int i = 0; i < customData2.Count; i++)
        {
            if (i == customData2.Count - 1)
            {
                customData2[i] = new Vector4(normalPos.x, normalPos.y, normalPos.z, 0); // you had to disable the module 
            }
        }

        SpawnParticleSystem.SetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        SpawnParticleSystem.SetCustomParticleData(customData2, ParticleSystemCustomData.Custom2);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.other.transform.gameObject.layer != 0)
            return;
        colCount++;
        latestCollision = collision;
        longEmit = StartCoroutine(WaitLongEmit());
        SpawnEffect(collision, 0);

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.other.transform.gameObject.layer != 0)
            return;
        StartCoroutine(WaitCol());
        if(longEmit != null)
            StopCoroutine(longEmit);
    }

    private ParticleSystem CustomPsInstantiate(ParticleSystem ps)
    {
        ParticleSystem instancePS = Instantiate(ps);
        instancePS.GetComponent<ParticleSystemRenderer>().renderMode = ParticleSystemRenderMode.Mesh;
        instancePS.GetComponent<ParticleSystemRenderer>().mesh = simplifiedMesh;

        ps.transform.position = this.transform.position;

        ParticleSystem.MainModule mainParticle = ps.main;
        mainParticle.startRotationX = this.transform.rotation.eulerAngles.x / (180.0f / Mathf.PI);
        mainParticle.startRotationY = this.transform.rotation.eulerAngles.y / (180.0f / Mathf.PI);
        mainParticle.startRotationZ = this.transform.rotation.eulerAngles.z / (180.0f / Mathf.PI);

        mainParticle.startSizeX = this.transform.lossyScale.x;
        mainParticle.startSizeY = this.transform.lossyScale.y;
        mainParticle.startSizeZ = this.transform.lossyScale.z;


        GameObject parentHolder;
        if (parentGO == null)
        {
            parentHolder = new GameObject(this.transform.gameObject.name + "Effects");
            instancePS.transform.parent = parentHolder.transform;
            parentGO = parentHolder;
        }
        else
            instancePS.transform.parent = parentGO.transform;


        if(parentEffectsGO == null)
        {
            parentEffectsGO = new GameObject("GroundEffectsHolder");
        }
        if (parentEffectsGO != null)
        {
            parentGO.transform.parent = parentEffectsGO.transform;
        }

            return instancePS;
    }

    private IEnumerator WaitLongEmit()
    {
        Vector3 lastPosition = this.transform.position;
        int n = 0;
        for (; ; )
        {
            yield return new WaitForSeconds(0.05f);
            if (Vector3.Distance(lastPosition, this.transform.position) > 0.15f)
            {
                lastPosition = this.transform.position;
                SpawnEffect(latestCollision, 1);
                n = 0;
            }
            else if(n >= 25)
            {
                lastPosition = this.transform.position;
                SpawnEffect(latestCollision, 2);
                n = 0;
            }
            else
            {
                n++;
            }
        }
    }

    private IEnumerator WaitCol()
    {
        yield return new WaitForSeconds(0.5f);
        colCount--;
    }
}

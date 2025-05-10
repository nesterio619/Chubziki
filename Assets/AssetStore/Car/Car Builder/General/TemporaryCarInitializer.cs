using SimpleMeshGenerator;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralCarBuilder
{
    public class TemporaryCarInitializer : MonoBehaviour
    {
        public MeshFilter[] Wheels;

        public MeshFilter Windows;
        public MeshFilter BodySide;
        public MeshFilter BodyTop;
        public MeshFilter TrunkOuterSide;
        public MeshFilter TrunkInnerSide;
        public MeshFilter HoodOuterside;
        public MeshFilter HoodInnerSide;

        public MeshFilter HeadLight_Right;
        public MeshFilter HeadLight_Left;

        public Transform LicensePlate;
        public Vector2 LicensePlateDimensions;
        public Transform MeshGenerationHelper;

        [System.NonSerialized] public Texture2D ColorTextureInternal;
        [System.NonSerialized] public Texture2D ColorTextureExternal;

        private CarPartReferences _carPartReferences;
        public CarPartReferences CarPartReferences { get => _carPartReferences; }

        public void Initialize(bool newlyCreated)
        {
            if (newlyCreated)
            {
                _carPartReferences = gameObject.AddComponent<CarPartReferences>();

                Wheels = new MeshFilter[4];
                for (int i = 0; i < Wheels.Length; i++)
                {
                    var wheelHolder = new GameObject("WheelHolder").transform;
                    wheelHolder.SetParentReset(transform);
                    CreateAsNew(ref Wheels[i], "Wheel", wheelHolder);
                }

                Wheels[0].gameObject.name = "Wheel_Front_Right";
                Wheels[1].gameObject.name = "Wheel_Front_Left";
                Wheels[2].gameObject.name = "Wheel_Back_Right";
                Wheels[3].gameObject.name = "Wheel_Back_Left";


                CreateAsNew(ref Windows, "Windows");
                CreateAsNew(ref BodySide, "BodySide");
                CreateAsNew(ref BodyTop, "BodyTop");

                var trunk = CreateAsNew(ref TrunkOuterSide, "Trunk_Outer");
                CreateAsNew(ref TrunkInnerSide, "Trunk_Inner", trunk);
                var hood = CreateAsNew(ref HoodOuterside, "Hood_Outer");
                CreateAsNew(ref HoodInnerSide, "Hood_Inner", hood);

                CreateAsNew(ref HeadLight_Right, "HeadLight_Right");
                CreateAsNew(ref HeadLight_Left, "HeadLight_Left");


                CarPartReferences.PropAnchor_HoodOrnament = new PropAnchor(transform, "PropAnchor_HoodOrnament");
                CarPartReferences.PropAnchor_Roof = new PropAnchor(transform, "PropAnchor_Roof");
                CarPartReferences.PropAnchor_FrontTrunk = new PropAnchor(transform, "PropAnchor_FrontTrunk");
                CarPartReferences.PropAnchor_BackTrunk = new PropAnchor(transform, "PropAnchor_BackTrunk");
            }
            else
            {
                for (int i = 0; i < Wheels.Length; i++)
                {
                    Wheels[i].sharedMesh.Clear();
                }

                Windows.sharedMesh.Clear();
                BodySide.sharedMesh.Clear();
                BodyTop.sharedMesh.Clear();
                TrunkOuterSide.sharedMesh.Clear();
                TrunkInnerSide.sharedMesh.Clear();
                HoodOuterside.sharedMesh.Clear();
                HoodInnerSide.sharedMesh.Clear();
                HeadLight_Right.sharedMesh.Clear();
                HeadLight_Left.sharedMesh.Clear();
            }


            Transform CreateAsNew(ref MeshFilter filter, string name, Transform parent = null)
            {
                filter = new GameObject(name).AddComponent<MeshRenderer>().gameObject.AddComponent<MeshFilter>();
                filter.transform.SetParent(parent == null ? transform : parent, false);
                filter.mesh = new Mesh();

                filter.GetComponent<MeshRenderer>().receiveShadows = false;

                return filter.transform;
            }
        }

        public void UpdatePartReferences()
        {
            CarPartReferences.WheelsFrontRight = Wheels[0].transform;
            CarPartReferences.WheelsFrontLeft = Wheels[1].transform;
            CarPartReferences.WheelsBackRight = Wheels[2].transform;
            CarPartReferences.WheelsBackLeft = Wheels[3].transform;

            CarPartReferences.Windows = Windows.transform;

            CarPartReferences.BodySide = BodySide.transform;
            CarPartReferences.BodyTop = BodyTop.transform;
            CarPartReferences.TrunkBonnet = TrunkOuterSide.transform;
            CarPartReferences.Hood = HoodOuterside.transform;

            CarPartReferences.HeadLight_Right = HeadLight_Right.transform;
            CarPartReferences.HeadLight_Left = HeadLight_Left.transform;

            CarPartReferences.LicensePlate = LicensePlate;
            CarPartReferences.LicensePlateDimensions = LicensePlateDimensions;
        }

        public void UpdateAnchors(Roof.RunTimeData roofData, Front.RuntimeData frontData, Back.RunTimeData backData)
        {
            CarPartReferences.PropAnchor_HoodOrnament.SetWorldBounds(frontData.OrnamentBounds);
            CarPartReferences.PropAnchor_Roof.SetWorldBounds(roofData.Bounds);
            CarPartReferences.PropAnchor_FrontTrunk.SetWorldBounds(frontData.TrunkBounds);
            CarPartReferences.PropAnchor_BackTrunk.SetWorldBounds(backData.TrunkBounds);
        }


        public void SetupMeshRenderers(Material matBody, Material matWheel, Material matGlass, Material matbodyDecal, Material matHoodDecal)
        {
            Windows.GetComponent<MeshRenderer>().material = matGlass;
            BodySide.GetComponent<MeshRenderer>().material = matBody;
            TrunkOuterSide.GetComponent<MeshRenderer>().material = matBody;
            TrunkInnerSide.GetComponent<MeshRenderer>().material = matBody;
            HoodInnerSide.GetComponent<MeshRenderer>().material = matBody;
            HeadLight_Right.GetComponent<MeshRenderer>().material = matGlass;
            HeadLight_Left.GetComponent<MeshRenderer>().material = matGlass;

            var bodyTopMats = new List<Material>();
            bodyTopMats.Add(matBody);
            if (matbodyDecal != null) bodyTopMats.Add(matbodyDecal);
            BodyTop.GetComponent<MeshRenderer>().materials = bodyTopMats.ToArray();

            var hoodMats = new List<Material>();
            hoodMats.Add(matBody);
            if (matbodyDecal != null) hoodMats.Add(matbodyDecal);
            if (matHoodDecal != null) hoodMats.Add(matHoodDecal);
            HoodOuterside.GetComponent<MeshRenderer>().materials = hoodMats.ToArray();

            for (int i = 0; i < Wheels.Length; i++)
            {
                Wheels[i].GetComponent<MeshRenderer>().material = matWheel;
            }

        }

        public void SetPartReferences(CarPartReferences partReferences)
            => _carPartReferences = partReferences;
    }
}

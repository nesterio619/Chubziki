using System;

namespace Components.Car
{
    [Serializable]
    public class CarEquipmentLoadout
    {
        public EquipmentMold[] HoodEquipment;
        public EquipmentMold[] RoofEquipment;
        public EquipmentMold[] TrunkEquipment;
        public EquipmentMold[] LeftSideEquipment;
        public EquipmentMold[] RightSideEquipment;
        public EquipmentMold[] FrontBumperEquipment;
        public EquipmentMold[] BackBumperEquipment;

        public EquipmentMold[][] EquipmentArray => new EquipmentMold[][]
            {HoodEquipment,RoofEquipment,TrunkEquipment,LeftSideEquipment,RightSideEquipment,FrontBumperEquipment,BackBumperEquipment};

        public void CopyTo(CarEquipmentLoadout loadout)
        {
            loadout.HoodEquipment = (EquipmentMold[])HoodEquipment?.Clone();
            loadout.RoofEquipment = (EquipmentMold[])RoofEquipment?.Clone();
            loadout.TrunkEquipment = (EquipmentMold[])TrunkEquipment?.Clone();
            loadout.LeftSideEquipment = (EquipmentMold[])LeftSideEquipment?.Clone();
            loadout.RightSideEquipment = (EquipmentMold[])RightSideEquipment?.Clone();
            loadout.FrontBumperEquipment = (EquipmentMold[])FrontBumperEquipment?.Clone();
            loadout.BackBumperEquipment = (EquipmentMold[])BackBumperEquipment?.Clone();
        }

        public long[][] GetRefIDs()
        {
            var equipmentArray = EquipmentArray;
            var references = new long[equipmentArray.Length][];

            for (int i = 0; i < equipmentArray.Length; i++)
            {
                references[i] = new long[equipmentArray[i].Length];
                for (int j = 0; j < equipmentArray[i].Length; j++)
                {
                    long refID = ReferenceManager.Instance.Get(equipmentArray[i][j]);
                    references[i][j] = refID;
                }
            }
            return references;
        }

        public void SetEquipmentFromRefIDs(long[][] references)
        {
            if (references == null) return;

            var equipmentArray = EquipmentArray;
            for (int i = 0; i < equipmentArray.Length; i++)
            {
                for (int j = 0; j < equipmentArray[i].Length; j++)
                {
                    var refID = references[i][j];

                    if (refID == -1)
                    {
                        equipmentArray[i][j] = null;
                        continue;
                    }

                    var mold = (EquipmentMold)ReferenceManager.Instance.Get(refID);
                    equipmentArray[i][j] = mold;
                }
            }
        }
    }
}

using System;

namespace Fsp.Inventory
{
    [Serializable]
    public struct DeathInventorySnapshot
    {
        public int primaryAmmo;
        public int secondaryAmmo;
        public int medkits;

        public bool IsEmpty => primaryAmmo <= 0 && secondaryAmmo <= 0 && medkits <= 0;
    }
}

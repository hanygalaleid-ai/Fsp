using System;
using UnityEngine;

namespace Fsp.Combat
{
    [Serializable]
    public sealed class WeaponAttachmentRuntime
    {
        public WeaponAttachmentDefinition optic;
        public WeaponAttachmentDefinition muzzle;
        public WeaponAttachmentDefinition magazine;
        public WeaponAttachmentDefinition grip;

        public float VerticalRecoilMultiplier => Product(a => a.verticalRecoilMultiplier);
        public float HorizontalRecoilMultiplier => Product(a => a.horizontalRecoilMultiplier);
        public float SpreadMultiplier => Product(a => a.spreadMultiplier);
        public float ReloadMultiplier => Product(a => a.reloadMultiplier);
        public int ExtraMagazineCapacity => Sum(a => a.extraMagazineCapacity);
        public float AdsFovOffset => SumFloat(a => a.adsFovOffset);

        private WeaponAttachmentDefinition[] All => new[] { optic, muzzle, magazine, grip };

        private float Product(Func<WeaponAttachmentDefinition, float> selector)
        {
            float value = 1f;
            foreach (var a in All) if (a != null) value *= selector(a);
            return value;
        }

        private int Sum(Func<WeaponAttachmentDefinition, int> selector)
        {
            int value = 0;
            foreach (var a in All) if (a != null) value += selector(a);
            return value;
        }

        private float SumFloat(Func<WeaponAttachmentDefinition, float> selector)
        {
            float value = 0f;
            foreach (var a in All) if (a != null) value += selector(a);
            return value;
        }
    }
}

using System;
using UIToolkitBinding;
using Unity.Properties;
using UnityEngine;

namespace Sample
{
    [UITKDataSourceObject, Serializable]
    public partial class Counter
    {
        [SerializeField, DontCreateProperty]
        [UITKBindableField(SetterAccessibility.Private)]
        private int count;

        public void CountUp()
        {
            Count++;
        }

        public void CountDown()
        {
            Count--;
        }

        public void Reset()
        {
            Count = 0;
        }
    }
}

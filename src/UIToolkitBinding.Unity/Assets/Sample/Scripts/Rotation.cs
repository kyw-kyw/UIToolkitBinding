using UIToolkitBinding;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sample
{
    internal partial class Rotation : MonoBehaviour
    {
        [SerializeField] Transform rotationTarget;
        [SerializeField] UIDocument document;
        VisualElement root;

        void Awake()
        {
            root = document.rootVisualElement.Q<VisualElement>("roration__main-container");
        }

        void OnEnable()
        {
            var dataSource = new EulerAngleData(rotationTarget);
            root.dataSource = dataSource;
        }

        [UITKDataSourceObject]
        public partial class EulerAngleData
        {
            Transform target;

            [UITKBindableField(SetterAccessibility.Private)]
            Vector3 eulerAngles;

            public EulerAngleData(Transform transform)
            {
                target = transform;
                eulerAngles = transform.eulerAngles;
            }

            partial void OnEulerAnglesChanged(Vector3 newValue)
            {
                target.eulerAngles = newValue;
            }
        }
    }
}

using UnityEngine;

namespace Tobii.XR.Examples
{
    public class ControllerVisualizer : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField, Tooltip("The prefab to spawn as the controller")]
        private GameObject _controllerPrefab;
#pragma warning restore 649

        private GameObject _controllerGameObject;

        void Start()
        {
            _controllerGameObject = Instantiate(_controllerPrefab, transform);
        }

        void Update()
        {
            UpdateControllerGameObject();
        }

        /// <summary>
        /// Updates the controller local position and rotation.
        /// </summary>
        private void UpdateControllerGameObject()
        {
            _controllerGameObject.transform.position = ControllerManager.Instance.Position;
            _controllerGameObject.transform.rotation = ControllerManager.Instance.Rotation;
        }

    }
}

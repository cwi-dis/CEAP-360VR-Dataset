// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR.Examples
{
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteInEditMode]
	[RequireComponent(typeof(RectTransform))]
    public class UIGazeCollider : MonoBehaviour 
	{
		[Header("Note: This script will not update the collider in PLAY mode.")]
		[Space]
		[Tooltip("Make sure the collider is centered over the visual element.")]
		[SerializeField]
		private Vector3 _offset;

		[Tooltip("Let the collider follow the visual borders of the element as closely as possible.")]
		[SerializeField]
		private Vector3 _padding;

		[Tooltip("Adjust to depth to allow for overlapping elements at different depths.\n(Does not work when generating collider at runtime.)")]
		[SerializeField]
		private bool _adjustToDepth = true;

		// This field needs to be serialized to save it's value, but not be shown.
		[SerializeField, HideInInspector]
		private Vector3 _center;
		
		// This field needs to be serialized to save it's value, but not be shown.
		[SerializeField, HideInInspector]
		private Vector3 _size;

		 // This field needs to be serialized to save it's value, but not be shown.
		[SerializeField, HideInInspector]
		private BoxCollider _collider;

		 // This field needs to be serialized to save it's value, but not be shown.
		[SerializeField, HideInInspector]
        private RectTransform _rectTransform;
		
		 // This field needs to be serialized to save it's value, but not be shown.
		[SerializeField, HideInInspector]
        private Graphic _graphic;

#if UNITY_EDITOR
        private void Update() 
		{
			if(!Application.isPlaying)
			{
				InitComponents();
				UpdateCollider();
			}
		}

        private void InitComponents()
        {
            if(_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
			if(_graphic == null) _graphic = GetComponent<Graphic>();
			if(_collider == null) _collider = GenerateRectCollider();
        }

        private void UpdateCollider()
        {
            _padding += _collider.size - _size;
            _size = GetSize(_rectTransform, _padding);

            _offset += _collider.center - _center;

			_collider.center = _center = CalculateCenter(_rectTransform, _graphic, _offset, _size, _adjustToDepth);

            _collider.size = _size;
        }

        private BoxCollider GenerateRectCollider()
        {
            var collider = gameObject.AddComponent<BoxCollider>();

            _size = GetSize(_rectTransform, _padding);

            collider.size = _size;
            collider.center = CalculateCenter(_rectTransform, _graphic, _offset, _size, _adjustToDepth);
            collider.isTrigger = true;

			return collider;
        }

        private static Vector3 CalculateCenter(RectTransform rectTransform, Graphic graphic, Vector3 offset, Vector3 size, bool adjustToDepth)
        {
			var scale = rectTransform.lossyScale.z * 0.01f;
            var depth = GetDepth(graphic, adjustToDepth);
            var center = -Vector3.forward * depth * scale + offset;
			
			var pivotAdjust = (Vector2.one * 0.5f - rectTransform.pivot);

			center.x += pivotAdjust.x * size.x;
			center.y += pivotAdjust.y * size.y;

            return center;
        }

        private static int GetDepth(Graphic graphic, bool adjustToDepth)
        {
            var depth = 0;
			if (adjustToDepth && graphic != null) depth = graphic.depth;
            return depth;
        }

        private static Vector3 GetSize(RectTransform rectTransform, Vector3 padding)
        {
			var width = rectTransform.rect.size.x;
			var height = rectTransform.rect.size.y;

            return new Vector3(width + padding.x, height + padding.y, rectTransform.lossyScale.z + padding.z);
        }
#endif
	}
}
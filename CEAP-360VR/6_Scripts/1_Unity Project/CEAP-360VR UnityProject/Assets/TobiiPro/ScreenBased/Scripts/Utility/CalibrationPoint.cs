//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Tobii.Research.Unity
{
    public class CalibrationPoint : MonoBehaviour
    {
        // Animation start/end time.
        private float _startTime;
        private float _length;
        private float _speed;
        private Vector3 _zoomIn;
        private Vector3 _zoomOut;
        private Image _image;
        private bool _animation;

        private void Start()
        {
            _image = GetComponent<Image>();
            _animation = false;
            _length = 1.7f;
            _speed = 1.0f;

            _zoomOut = new Vector3(0.75f, 0.75f, 0.75f);
            _zoomIn = new Vector3(0.1f, 0.1f, 0.1f);
        }

        private void Update()
        {
            if (_animation)
            {
                var covered = (Time.time - _startTime) * _speed;
                var unitCovered = covered / _length;
                _image.rectTransform.localScale = Vector3.Lerp(_zoomOut, _zoomIn, unitCovered);
            }
        }

        public void StartAnim()
        {
            transform.localScale = _zoomOut;
            _startTime = Time.time;
            _animation = true;
        }
    }
}

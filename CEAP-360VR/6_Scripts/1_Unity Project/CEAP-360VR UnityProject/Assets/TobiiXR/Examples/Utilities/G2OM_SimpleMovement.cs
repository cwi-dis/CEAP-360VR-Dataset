// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR.Examples
{
    public class G2OM_SimpleMovement : MonoBehaviour
    {
        public Vector3 LengthAndDirection = new Vector3(5, 0, 0);

        private Vector3 _startPosition;

        void Start()
        {
            _startPosition = transform.position;
        }

        void Update()
        {
            var offset = Mathf.Sin(Time.time);
            transform.position = _startPosition + LengthAndDirection * offset;
        }
    }
}
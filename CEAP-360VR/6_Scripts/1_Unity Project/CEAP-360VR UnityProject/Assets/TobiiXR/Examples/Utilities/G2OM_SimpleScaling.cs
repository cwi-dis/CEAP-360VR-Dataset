// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR.Examples
{
    public class G2OM_SimpleScaling : MonoBehaviour
    {
        public Vector3 MaximumScale = new Vector3(1, 1, 1);
        public Vector3 MinimumScale = new Vector3(.25f, .25f, .25f);

        void Update()
        {
            var offset = Mathf.Abs(Mathf.Sin(Time.time));
            transform.localScale = Vector3.Lerp(MinimumScale, MaximumScale, offset);
        }
    }
}
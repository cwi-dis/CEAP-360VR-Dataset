//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

namespace Tobii.Research.Unity
{
    using UnityEngine;
    using UnityEngine.UI;

    public class VRPositioningPlacementCanvas : MonoBehaviour
    {
        public RectTransform TargetLeft;
        public RectTransform TargetRight;

        public RectTransform PupilLeft;
        public RectTransform PupilRight;

        public Text Status;
    }
}
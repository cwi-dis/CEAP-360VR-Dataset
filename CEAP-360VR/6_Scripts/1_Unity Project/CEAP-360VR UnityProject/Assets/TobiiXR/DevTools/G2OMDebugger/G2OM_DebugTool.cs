// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using UnityEngine;

    public class G2OM_DebugTool : MonoBehaviour
    {
        [Header("Debug")]
        [Tooltip("Can be null if there is no need for debug visualization")]
        public G2OM_DebugVisualization DebugVisualization;
        public KeyCode DebugVisualizationOnOff = KeyCode.Space;
        public KeyCode DebugVisualizationFreezeOnOff = KeyCode.LeftControl;

        void Update()
        {
            if (DebugVisualization == null) return;

            if (Input.GetKeyUp(DebugVisualizationOnOff))
            {
                DebugVisualization.ToggleVisualization();
            }

            if (Input.GetKeyUp(DebugVisualizationFreezeOnOff))
            {
                DebugVisualization.ToggleFreeze();
            }
        }
    }
}
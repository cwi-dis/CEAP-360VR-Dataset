// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface IG2OM_ObjectFinder
    {
        void GetRelevantGazeObjects(ref G2OM_DeviceData deviceData, Dictionary<int, GameObject> foundObjects, IG2OM_ObjectDistinguisher distinguisher);

        G2OM_RaycastResult GetRaycastResult(ref G2OM_DeviceData deviceData, IG2OM_ObjectDistinguisher distinguisher);

        void Setup(IG2OM_Context context, LayerMask layerMask);
    }
}
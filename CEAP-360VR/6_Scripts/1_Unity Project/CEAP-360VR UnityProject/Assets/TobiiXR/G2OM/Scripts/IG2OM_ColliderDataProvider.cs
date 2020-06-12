// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System.Collections.Generic;
    using UnityEngine;

    public interface IG2OM_ColliderDataProvider
    {
        void GetColliderData(Dictionary<int, InternalCandidate> gameObjects, G2OM_Candidate[] candidates);
    }
}
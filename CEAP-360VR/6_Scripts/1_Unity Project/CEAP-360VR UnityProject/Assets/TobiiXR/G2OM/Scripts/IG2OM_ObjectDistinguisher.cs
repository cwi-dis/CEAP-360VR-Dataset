// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using UnityEngine;

    public interface IG2OM_ObjectDistinguisher
    {
        bool IsGameObjectGazeFocusable(int id, GameObject gameObject);
    }
}
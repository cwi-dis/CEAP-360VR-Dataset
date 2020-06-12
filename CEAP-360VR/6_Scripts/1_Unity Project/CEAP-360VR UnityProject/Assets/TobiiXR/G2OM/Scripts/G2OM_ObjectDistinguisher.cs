// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System.Collections.Generic;
    using UnityEngine;

    public class G2OM_ObjectDistinguisher<T> : IG2OM_ObjectDistinguisher
    {
        private const int ExpectedNumberOfTComponents = 5;
        private readonly List<T> _components = new List<T>(ExpectedNumberOfTComponents);

        public bool IsGameObjectGazeFocusable(int id, GameObject gameObject)
        {
            _components.Clear(); // TODO: Do we actually need to clear this list?
            gameObject.GetComponents(_components);
            return _components.Count > 0;
        }
    }
}
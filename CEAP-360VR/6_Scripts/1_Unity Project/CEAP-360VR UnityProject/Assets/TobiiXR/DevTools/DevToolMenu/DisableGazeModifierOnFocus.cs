// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR.GazeModifier;

namespace Tobii.XR.DevTools
{
    using System.Collections;
    using UnityEngine;

    public class DisableGazeModifierOnFocus : MonoBehaviour, IDisableGazeModifier
    {
        IEnumerator Start()
        {
            yield return new WaitUntil(() => {
                var filter = TobiiXR.Internal.Filter as GazeModifierFilter;
                if(filter != null)
                {
                    filter.Settings.AddDisabler(this);
                    return true;
                }
                return false;
            });
        }

        public void GazeFocusChanged(bool hasFocus)
        {
            Disable = hasFocus;
        }

        public bool Disable { get; private set; }
    }
}

// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System.Collections.Generic;
    using UnityEngine;

    public class G2OM_PostTicker : IG2OM_PostTicker
    {
        private const int ExpectedNumerOfGazeFocusableComponentsPerObject = 6;

        private GameObject _previousGazeFocusedObject;
        private readonly List<IGazeFocusable> _gazeFocusableComponents = new List<IGazeFocusable>(ExpectedNumerOfGazeFocusableComponentsPerObject);

        public void TickComplete(List<FocusedCandidate> focusedObjects)
        {
            GameObject focusedObject = focusedObjects.Count == 0 ? null : focusedObjects[0].GameObject;
            
            UpdateFocusableComponents(focusedObject, ref _previousGazeFocusedObject, _gazeFocusableComponents);
        }

        private static void UpdateFocusableComponents(GameObject focusedObject, ref GameObject previousFocusedObject, List<IGazeFocusable> gazeFocusableComponents)
        {
            if (focusedObject == previousFocusedObject) return;

            if (previousFocusedObject != null)
            {
                foreach (var focusableComponent in gazeFocusableComponents)
                {
                    focusableComponent.GazeFocusChanged(false);
                }
                gazeFocusableComponents.Clear();
            }

            if (focusedObject != null)
            {
                focusedObject.GetComponents(gazeFocusableComponents);

                foreach (var focusableComponent in gazeFocusableComponents)
                {
                    focusableComponent.GazeFocusChanged(true);
                }
            }

            previousFocusedObject = focusedObject;
        }
    }
}
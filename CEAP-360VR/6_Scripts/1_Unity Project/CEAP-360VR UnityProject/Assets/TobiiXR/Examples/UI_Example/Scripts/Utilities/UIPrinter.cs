// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using UnityEngine;

namespace Tobii.XR.Examples
{
    /// <summary>
    /// Prints out messages.
    /// </summary>
    public class UIPrinter : MonoBehaviour
    {
        public void PrintButtonClicked(GameObject button)
        {
            Debug.Log(button.name + " has been clicked.");
        }

        public void PrintToggleButtonToggled(GameObject toggleButton, bool isToggleOn)
        {
            var toggleString = isToggleOn ? "ON" : "OFF";
            Debug.Log(toggleButton.name + " has been toggled " + toggleString + ".");
        }

        public void PrintSliderValueHasChanged(GameObject slider, int newValue)
        {
            Debug.Log(slider.name + " has been updated to " + newValue + ".");
        }
    }
}

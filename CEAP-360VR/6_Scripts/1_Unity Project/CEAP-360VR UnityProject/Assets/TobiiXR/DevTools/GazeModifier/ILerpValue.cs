// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using UnityEngine;

namespace Tobii.XR.GazeModifier
{
    /// <summary>
    /// Responsible for getting a float based on a set of key valuepairs
    /// </summary>
    public interface ILerpValue
    {
        float Evaluate(float key);

        void SetValues(IEnumerable<Vector2> keyValuepairs);

        bool HasValues { get; }
    }
}

// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Tobii.XR.GazeModifier
{
    public static class Metrics
    {
        public static float CalculatePrecision(List<Vector3> gazeDirections)
        {
            // Calculate precision based on the average gaze direction in the sample
            Vector3 averageGazeDirection = AverageVectors(gazeDirections);
            float precisionRadians = Mathf.Sqrt(gazeDirections.Average(gazeDirection => Mathf.Pow(AngleDifference(averageGazeDirection, gazeDirection), 2)));
            float precisionDegrees = precisionRadians * Mathf.Rad2Deg;
            return precisionDegrees;
        }

        public static float CalculateAccuracy(Vector3 target, List<Vector3> gazeOrigins, List<Vector3> gazeDirections)
        {
            // Calculate accuracy on target based on the average gaze origin and gaze direction in the sample
            Vector3 averageGazeDirection = AverageVectors(gazeDirections);
            Vector3 averageGazeOrigin = AverageVectors(gazeOrigins);
            Vector3 targetVector = (target - averageGazeOrigin).normalized;
            float accuracyRadians = AngleDifference(targetVector, averageGazeDirection);
            float accuracyDegrees = accuracyRadians * Mathf.Rad2Deg;
            return accuracyDegrees;
        }

        public static Vector3 AverageVectors(List<Vector3> vectors)
        {
            // Calculate the average vector from a list of vectors
            Vector3 averageVector;
            averageVector.x = vectors.Average(p => p.x);
            averageVector.y = vectors.Average(p => p.y);
            averageVector.z = vectors.Average(p => p.z);
            return averageVector;
        }

        public static float AngleDifference(Vector3 u, Vector3 v)
        {
            Assert.IsTrue(u.magnitude > 0.0);
            Assert.IsTrue(v.magnitude > 0.0);
            // Calculate the angle between two vectors
            var angle = Mathf.Acos(Mathf.Clamp(Vector3.Dot(u, v) / (u.magnitude * v.magnitude), -1.0f, 1.0f));
            return angle;
        }
    }
}

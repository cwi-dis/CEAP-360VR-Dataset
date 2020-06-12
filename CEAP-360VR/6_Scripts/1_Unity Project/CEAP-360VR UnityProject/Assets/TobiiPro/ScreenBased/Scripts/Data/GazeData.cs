//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public sealed class GazeData : IGazeData
    {
        public long TimeStamp { get; private set; }

        public IGazeDataEye Left { get; private set; }

        public IGazeDataEye Right { get; private set; }

        public Ray CombinedGazeRayScreen
        {
            get
            {
                if (Left.GazePointValid && Right.GazePointValid)
                {
                    var combinedPoint = (Left.GazePointOnDisplayArea + Right.GazePointOnDisplayArea) / 2f;
                    return Camera.main.ScreenPointToRay(new Vector3(Screen.width * combinedPoint.x, Screen.height * (1 - combinedPoint.y)));
                }

                return default(Ray);
            }
        }

        public bool CombinedGazeRayScreenValid
        {
            get
            {
                return Left.GazePointValid && Right.GazePointValid;
            }
        }

        public GazeDataEventArgs OriginalGaze { get; private set; }

        internal GazeData(GazeDataEventArgs originalGaze)
        {
            TimeStamp = originalGaze.SystemTimeStamp;
            Left = new GazeDataEye(originalGaze.LeftEye);
            Right = new GazeDataEye(originalGaze.RightEye);
            OriginalGaze = originalGaze;
        }

        internal GazeData()
        {
            Left = new GazeDataEye();
            Right = new GazeDataEye();
        }
    }

    public sealed class GazeDataEye : IGazeDataEye
    {
        public Vector3 GazeOriginInUserCoordinates { get; private set; }

        public Vector3 GazeOriginInTrackBoxCoordinates { get; private set; }

        public bool GazeOriginValid { get; private set; }

        public Vector3 GazePointInUserCoordinates { get; private set; }

        public Vector2 GazePointOnDisplayArea { get; private set; }

        public Ray GazeRayScreen
        {
            get
            {
                if (GazePointValid)
                {
                    return Camera.main.ScreenPointToRay(new Vector3(Screen.width * GazePointOnDisplayArea.x, Screen.height * (1 - GazePointOnDisplayArea.y)));
                }

                return default(Ray);
            }
        }

        public bool GazePointValid { get; private set; }

        public float PupilDiameter { get; private set; }

        public bool PupilDiameterValid { get; private set; }

        public GazeDataEye(EyeData eye)
        {
            GazeOriginInUserCoordinates = eye.GazeOrigin.PositionInUserCoordinates.ToVector3();
            GazeOriginInTrackBoxCoordinates = eye.GazeOrigin.PositionInTrackBoxCoordinates.ToVector3();
            GazeOriginValid = eye.GazeOrigin.Validity.Valid();
            GazePointInUserCoordinates = eye.GazePoint.PositionInUserCoordinates.ToVector3();
            GazePointOnDisplayArea = eye.GazePoint.PositionOnDisplayArea.ToVector2();
            GazePointValid = eye.GazePoint.Validity.Valid();
            PupilDiameter = eye.Pupil.PupilDiameter;
            PupilDiameterValid = eye.Pupil.Validity.Valid();
        }

        public GazeDataEye()
        {
            GazeOriginInUserCoordinates = Vector3.zero;
            GazeOriginInTrackBoxCoordinates = Vector3.zero;
            GazeOriginValid = false;
            GazePointInUserCoordinates = Vector3.zero;
            GazePointOnDisplayArea = Vector2.zero;
            GazePointValid = false;
            PupilDiameter = 0;
            PupilDiameterValid = false;
        }
    }
}
//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public sealed class UserPositionGuideData : IUserPositionGuideData
    {
        internal UserPositionGuideData(UserPositionGuideEventArgs userPositionGuideData)
        {
            LeftEye = userPositionGuideData.LeftEye.UserPosition.ToVector3();
            RightEye = userPositionGuideData.RightEye.UserPosition.ToVector3();
            LeftEyeValid = userPositionGuideData.LeftEye.Validity == Validity.Valid;
            RightEyeValid = userPositionGuideData.RightEye.Validity == Validity.Valid;
        }

        public UserPositionGuideData()
        {
            LeftEye = RightEye = Vector3.zero;
            LeftEyeValid = RightEyeValid = false;
        }

        public Vector3 LeftEye { get; private set; }

        public Vector3 RightEye { get; private set; }

        public bool LeftEyeValid { get; private set; }

        public bool RightEyeValid { get; private set; }
    }
}
// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using UnityEngine;

    public static class G2OM_UnityExtensionMethods
    {
        public static G2OM_Candidate CreateCandidate(int candidateId, Vector3 max, Vector3 min, Matrix4x4 worldToLocal, Matrix4x4 localToWorld)
        {
            return new G2OM_Candidate
            {
                candidate_id = (ulong) candidateId,
                aabb_max_local_space = max.AsG2OMVector3(),
                aabb_min_local_space = min.AsG2OMVector3(),
                world_to_local_matrix = worldToLocal.AsG2OMMatrix4x4(),
                local_to_world_matrix = localToWorld.AsG2OMMatrix4x4(),
            };
        }

        public static G2OM_Matrix4x4 AsG2OMMatrix4x4(this Matrix4x4 matrix)
        {
            return new G2OM_Matrix4x4
            {
                m00 = matrix.m00,
                m01 = matrix.m01,
                m02 = matrix.m02,
                m03 = matrix.m03,
                m10 = matrix.m10,
                m11 = matrix.m11,
                m12 = matrix.m12,
                m13 = matrix.m13,
                m20 = matrix.m20,
                m21 = matrix.m21,
                m22 = matrix.m22,
                m23 = matrix.m23,
                m30 = matrix.m30,
                m31 = matrix.m31,
                m32 = matrix.m32,
                m33 = matrix.m33,
            };
        }

        public static G2OM_Vector3 AsG2OMVector3(this Vector3 vector)
        {
            return new G2OM_Vector3
            {
                x = vector.x,
                y = vector.y,
                z = vector.z,
            };
        }

        public static Vector3 Vector(this G2OM_Vector3 vector)
        {
            return new Vector3
            {
                x = vector.x,
                y = vector.y,
                z = vector.z,
            };
        }

        public static G2OM_Ray CreateRay(Vector3 rayOrigin, Vector3 rayDirection)
        {
            return new G2OM_Ray
            {
                origin = rayOrigin.AsG2OMVector3(),
                direction = rayDirection.AsG2OMVector3(),
            };
        }
    }
}
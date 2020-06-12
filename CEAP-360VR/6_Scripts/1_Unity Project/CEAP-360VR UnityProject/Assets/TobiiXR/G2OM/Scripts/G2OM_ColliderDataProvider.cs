// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class G2OM_ColliderDataProvider : IG2OM_ColliderDataProvider
    {
        public void GetColliderData(Dictionary<int, InternalCandidate> gameObjects, G2OM_Candidate[] candidates)
        {
            Debug.Assert(gameObjects.Count <= candidates.Length);

            var i = 0;
            foreach (var candidate in gameObjects)
            {
                var id = candidate.Key;
                var go = candidate.Value.GameObject;
                
                var collider = go.GetComponent<Collider>();
                var box = collider as BoxCollider;
                var sphereCollider = collider as SphereCollider;
                var capsuleCollider = collider as CapsuleCollider;
                
                Vector3 min, max;
                if (box != null)
                {
                    var size = box.size;
                    var center = box.center;

                    min = -size / 2 + center;
                    max = size / 2 + center;
                }
                else if (sphereCollider != null)
                {
                    var center = sphereCollider.center;
                    var r = sphereCollider.radius;
                    var radius = new Vector3(r, r, r);

                    // Clamp box to be inside sphere
                    radius = Vector3.ClampMagnitude(radius, r);

                    min = center - radius;
                    max = center + radius;
                }
                else if (capsuleCollider != null)
                {
                    var center = capsuleCollider.center;
                    var height = capsuleCollider.height;

                    // The value can be 0, 1 or 2 corresponding to the X, Y and Z axes, respectively.
                    var direction = capsuleCollider.direction;
                    
                    var r = capsuleCollider.radius;
                    var radius = new Vector3(r, r, r);

                    // Remove endpoints from height value
                    height -= r * 2;

                    // Clamp box to be inside capsule
                    radius = Vector3.ClampMagnitude(radius, r);
                    height -= (r - radius.x) * 2;

                    Vector3 offset;
                    switch (direction)
                    {
                        case 0:
                            offset = Vector3.right * height;
                            break;
                        case 1:
                            offset = Vector3.up * height;
                            break;
                        case 2:
                            offset = Vector3.forward * height;
                            break;
                        default:
                            throw new Exception("This should never happen! If it does check Unity documentation for capsule colliders direction.");
                    }

                    min = center - radius - offset;
                    max = center + radius + offset;
                }
                else
                {
                    var meshFilter = go.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        var mesh = meshFilter.mesh;
                        var bounds = mesh.bounds;

                        min = bounds.min;
                        max = bounds.max;
                    }
                    else
                    {
                        min = Vector3.zero;
                        max = Vector3.zero;

                        Debug.LogWarning(string.Format("Failed to find bounds for object {0}. Reverting to zero bounds. Please fix this issue.", go.name));
                    }
                }

                var transform = go.transform;
                candidates[i] = new G2OM_Candidate
                {
                    candidate_id = (ulong)id,
                    aabb_max_local_space = max.AsG2OMVector3(),
                    aabb_min_local_space = min.AsG2OMVector3(),
                    local_to_world_matrix = transform.localToWorldMatrix.AsG2OMMatrix4x4(),
                    world_to_local_matrix = transform.worldToLocalMatrix.AsG2OMMatrix4x4(),
                };

                i++;
            }
        }
    }
}
// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class G2OM_Description
    {
        public const float DefaultCandidateMemoryInSeconds = 1f;
        public const int DefaultLayerMask = ~0;

        public float HowLongToKeepCandidatesInSeconds = DefaultCandidateMemoryInSeconds;

        public LayerMask LayerMask = DefaultLayerMask;
        public IG2OM_ObjectFinder ObjectFinder = new G2OM_ObjectFinder();
        public IG2OM_ColliderDataProvider ColliderDataProvider = new G2OM_ColliderDataProvider();
        public IG2OM_ObjectDistinguisher Distinguisher = new G2OM_ObjectDistinguisher<IGazeFocusable>();
        public IG2OM_PostTicker PostTicker = new G2OM_PostTicker();
        public IG2OM_Context Context = new G2OM_Context();

        public G2OM_ContextCreateOptions Options = Interop.G2OM_InitializeOptions();
    }

    public struct InternalCandidate
    {
        public GameObject GameObject;
        public float Timestamp;
    }

    public struct FocusedCandidate
    {
        public GameObject GameObject;

        // This is combined
        public bool IsRayValid;
        public Vector3 Origin;
        public Vector3 Direction;
    }

    public class G2OM
    {
        public static G2OM Create(G2OM_Description description)
        {
            return new G2OM(description);
        }

        public static G2OM Create()
        {
            return new G2OM(new G2OM_Description());
        }

        public float MaxHistoryInSeconds { get { return _howLongToKeepCandidatesInMemory; } }

        public int TotalNumberOfGazeObjects { get { return _internalCandidates.Count; } }

        public int NumberOfGazeFocusedObjects { get { return _gazeFocusedObjects.Count; } }

        public List<FocusedCandidate> GazeFocusedObjects { get { return _gazeFocusedObjects; } }

        private readonly IG2OM_ObjectFinder _objectFinder;
        private readonly IG2OM_ColliderDataProvider _colliderDataProvider;
        private readonly IG2OM_ObjectDistinguisher _distinguisher;
        private readonly IG2OM_PostTicker _postTicker;

        protected IG2OM_Context _context;

        private readonly Dictionary<int, InternalCandidate> _internalCandidates;
        private readonly float _howLongToKeepCandidatesInMemory;
        private readonly int _internalCapacity;

        private readonly Dictionary<int, GameObject> _newCandidates;
        private readonly List<int> _keysToRemove;

        private readonly List<FocusedCandidate> _gazeFocusedObjects;

        private G2OM_DeviceData _deviceData;
        private G2OM_Candidate[] _nativeCandidates;
        private G2OM_CandidateResult[] _nativeCandidatesResult;

        private G2OM(G2OM_Description description)
        {
            _internalCapacity = (int)description.Options.capacity;
            _howLongToKeepCandidatesInMemory = description.HowLongToKeepCandidatesInSeconds;

            _context = description.Context;
            _context.Setup(description.Options);

            _objectFinder = description.ObjectFinder;
            _objectFinder.Setup(_context, description.LayerMask);

            _colliderDataProvider = description.ColliderDataProvider;
            _distinguisher = description.Distinguisher;
            _postTicker = description.PostTicker;

            _newCandidates = new Dictionary<int, GameObject>(_internalCapacity);
            _internalCandidates = new Dictionary<int, InternalCandidate>(_internalCapacity);

            _gazeFocusedObjects = new List<FocusedCandidate>(_internalCapacity);
            _keysToRemove = new List<int>(_internalCapacity);

            _nativeCandidates = new G2OM_Candidate[_internalCapacity];
            _nativeCandidatesResult = new G2OM_CandidateResult[_internalCapacity];
        }

        public void Clear()
        {
            _internalCandidates.Clear();
        }

        public void Tick(G2OM_DeviceData deviceData)
        {
            _deviceData = deviceData;
            var now = _deviceData.timestamp;

            // Get, add and remove candidates
            _objectFinder.GetRelevantGazeObjects(ref _deviceData, _newCandidates, _distinguisher);

            AddNewCandidates(now, _internalCandidates, _newCandidates);

            RemoveOldCandidates(now, _internalCandidates, MaxHistoryInSeconds, _keysToRemove, _internalCapacity);

            // Ensure capacity is enough
            if (_internalCandidates.Count > _nativeCandidates.Length)
            {
                _nativeCandidates = new G2OM_Candidate[_internalCandidates.Count];
                _nativeCandidatesResult = new G2OM_CandidateResult[_internalCandidates.Count];
            }

            // Prepare structs to be sent to G2OM
            _colliderDataProvider.GetColliderData(_internalCandidates, _nativeCandidates);

            var raycastResult = _objectFinder.GetRaycastResult(ref _deviceData, _distinguisher);

            _context.Process(ref _deviceData, ref raycastResult, _internalCandidates.Count, _nativeCandidates, _nativeCandidatesResult); // TODO: What to do if this call fails??

            // Process the result from G2OM
            UpdateListOfFocusedCandidates(_nativeCandidatesResult, _internalCandidates, _gazeFocusedObjects);

            _postTicker.TickComplete(_gazeFocusedObjects);
        }

        public void Destroy()
        {
            _context.Destroy();
        }

        public G2OM_Candidate[] GetCandidates()
        {
            var array = new G2OM_Candidate[_internalCandidates.Count];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _nativeCandidates[i];
            }

            return array;
        }

        public G2OM_CandidateResult[] GetCandidateResult()
        {
            var array = new G2OM_CandidateResult[_internalCandidates.Count];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = _nativeCandidatesResult[i];
            }

            return array;
        }

        public G2OM_DeviceData GetDeviceData()
        {
            return _deviceData;
        }

        private static void AddNewCandidates(float now, Dictionary<int, InternalCandidate> allCandidates, Dictionary<int, GameObject> newCandidates)
        {
            foreach (var newCandidate in newCandidates)
            {
                var id = newCandidate.Key;
                var candidate = allCandidates.ContainsKey(id) ? allCandidates[id] : new InternalCandidate { GameObject = newCandidate.Value };
                candidate.Timestamp = now;
                allCandidates[id] = candidate;
            }
        }

        private static void RemoveOldCandidates(float now, Dictionary<int, InternalCandidate> allCandidates, float maxHistory, List<int> keysToRemove, int maxKeysToKeep)
        {
            keysToRemove.Clear();

            foreach (var candidate in allCandidates)
            {
                var internalCandidate = candidate.Value;
                var diff = now - internalCandidate.Timestamp;
                if (diff <= maxHistory && internalCandidate.GameObject != null) continue;

                keysToRemove.Add(candidate.Key);
            }

            // shrink list to fit g2om capacity
            // without this g2om will return error -10
            var remainCount = allCandidates.Count - keysToRemove.Count;
            if(remainCount > maxKeysToKeep)
            {
                var removeCount = remainCount - maxKeysToKeep;
                var ordered = allCandidates.Where(k => keysToRemove.Contains(k.Key) == false).OrderByDescending(k => now - k.Value.Timestamp);
                for(int i = 0; i < removeCount; i++)
                {
                    keysToRemove.Add(ordered.ElementAt(i).Key);
                }

                Debug.LogWarning("G2OM was initialized with a capacity of " + maxKeysToKeep + ", but was supplied with " + allCandidates.Count + ". Purging overflowing elements.");
            }

            foreach (var key in keysToRemove)
            {
                allCandidates.Remove(key);
            }
        }

        private static void UpdateListOfFocusedCandidates(G2OM_CandidateResult[] candidateResult, Dictionary<int, InternalCandidate> allCandidates, List<FocusedCandidate> gazeFocusedObjects)
        {
            gazeFocusedObjects.Clear();

            for (int i = 0; i < allCandidates.Count; i++)
            {
                var candidate = candidateResult[i];

                if (candidate.score <= Mathf.Epsilon) break;

                var id = (int) candidate.candidate_id;

                if (allCandidates.ContainsKey(id) == false)
                {
                    Debug.LogError("This should not happen"); // TODO: What should we do if this happens? -- swik 2019-09-04
                    continue;
                } 

                var gazeFocusedObject = new FocusedCandidate
                {
                    GameObject = allCandidates[id].GameObject,
                    IsRayValid = candidate.adjusted_gaze_ray_world_space.is_valid.ToBool(),
                    Direction = candidate.adjusted_gaze_ray_world_space.ray.direction.Vector(),
                    Origin = candidate.adjusted_gaze_ray_world_space.ray.origin.Vector(),
                };

                gazeFocusedObjects.Add(gazeFocusedObject);
            }
        }
    }
}
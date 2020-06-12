//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;

namespace Tobii.Research.Unity
{
    public class GazeTrailBase : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The color of the particle")]
        private Color _color = Color.green;

        [SerializeField]
        [Range(0, 1000)]
        [Tooltip("The number of particles to allocate. Use zero to use only the last hit object.")]
        private int _particleCount = 100;

        [SerializeField]
        [Range(0.005f, 0.2f)]
        [Tooltip("The size of the particle.")]
        private float _particleSize = 0.05f;

        [SerializeField]
        [Tooltip("Turn gaze trail on or off.")]
        public bool _on = true;

        /// <summary>
        /// Turn gaze trail on or off.
        /// </summary>
        public bool On
        {
            get
            {
                return _on;
            }

            set
            {
                _on = value;
                OnSwitch();
            }
        }

        /// <summary>
        /// Set particle count between 1 and 1000.
        /// </summary>
        public int ParticleCount
        {
            get
            {
                return _particleCount;
            }

            set
            {
                if (value < 0 || value > 1000)
                {
                    return;
                }

                _particleCount = value;
                CheckCount();
            }
        }

        public Color ParticleColor
        {
            get
            {
                return _color;
            }

            set
            {
                _color = value;
            }
        }

        /// <summary>
        /// Get the latest hit object.
        /// </summary>
        public Transform LatestHitObject
        {
            get
            {
                return _latestHitObject;
            }
        }

        private bool _lastOn;
        private int _lastParticleCount;
        private int _particleIndex;
        private ParticleSystem.Particle[] _particles;
        private ParticleSystem _particleSystem;
        private bool _particlesDirty;
        private Transform _latestHitObject;
        private bool _removeParticlesWhileCalibrating;

        protected virtual bool HasEyeTracker { get; set; }
        protected virtual bool CalibrationInProgress { get; set; }

        private void Awake()
        {
            OnAwake();
        }

        private void Start()
        {
            OnStart();
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
            _lastParticleCount = _particleCount;
            _particles = new ParticleSystem.Particle[_particleCount];
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (_particlesDirty)
            {
                _particleSystem.SetParticles(_particles, _particles.Length);
                _particlesDirty = false;
            }

            CheckCount();

            OnSwitch();

            if (CalibrationInProgress)
            {
                // Don't do anything if we are calibrating.

                if (_removeParticlesWhileCalibrating)
                {
                    RemoveParticles();
                    _removeParticlesWhileCalibrating = false;
                }

                return;
            }

            // Reset the flag when no longer calibrating.
            _removeParticlesWhileCalibrating = true;

            if (HasEyeTracker && _on)
            {
                Ray ray;
                var valid = GetRay(out ray);
                if (valid)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        PlaceParticle(hit.point, _color, _particleSize);
                        _latestHitObject = hit.transform;
                    }
                    else
                    {
                        _latestHitObject = null;
                    }
                }
            }
        }

        private void CheckCount()
        {
            if (_lastParticleCount != _particleCount)
            {
                RemoveParticles();
                _particleIndex = 0;
                _particles = new ParticleSystem.Particle[_particleCount];
                _lastParticleCount = _particleCount;
            }
        }

        private void OnSwitch()
        {
            if (_lastOn && !_on)
            {
                // Switch off.
                RemoveParticles();
                _lastOn = false;
            }
            else if (!_lastOn && _on)
            {
                // Switch on.
                _lastOn = true;
            }
        }

        private void RemoveParticles()
        {
            for (int i = 0; i < _particles.Length; i++)
            {
                PlaceParticle(Vector3.zero, Color.white, 0);
            }
        }

        private void PlaceParticle(Vector3 pos, Color color, float size)
        {
            if (_particleCount < 1)
            {
                return;
            }

            var particle = _particles[_particleIndex];
            particle.position = pos;
            particle.startColor = color;
            particle.startSize = size;
            _particles[_particleIndex] = particle;
            _particleIndex = (_particleIndex + 1) % _particles.Length;
            _particlesDirty = true;
        }

        protected virtual bool GetRay(out Ray ray)
        {
            ray = default(Ray);
            return false;
        }
    }
}
// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR
{
    using System.Collections;
    using Tobii.G2OM;
    using UnityEngine;

    [RequireComponent(typeof(Camera))]
    public class G2OM_DebugVisualization : MonoBehaviour
    {
        [SerializeField]
        private Color _focusableObjectColor = new Color(1, 1, 1, .4F);
        [SerializeField]
        private Color _mainFocusedObjectColor = new Color(0, 1, 0, .4F);
        [SerializeField]
        private Color _otherFocusedObjectsColor = new Color(144 / 255f, 238 / 255f, 144 / 255f, .4F);
        [SerializeField]
        private Color _backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        private readonly G2OM_Vector3[] _corners = new G2OM_Vector3[(int)Corners.NumberOfCorners];

        // To ensure we keep a snapshot of what G2OM did they are copies
        private G2OM_Candidate[] _candidates;
        private G2OM_CandidateResult[] _candidateResult;
        private G2OM_DeviceData _deviceData;
        private Material _mat;
        private bool _freezeVisualization = false;
        private bool _showVisualization = false;


        public void ToggleVisualization()
        {
            SetVisualization(!_showVisualization);
        }

        public void SetVisualization(bool set)
        {
            _showVisualization = set;

            // unfreeze on disable
            if (!_showVisualization) _freezeVisualization = false;
        }

        public void ToggleFreeze()
        {
            _freezeVisualization = !_freezeVisualization;
        }

        void Start()
        {
            _mat = new Material(Shader.Find("Hidden/Internal-Colored"));
            _mat.SetInt("_ZTest", 0); // Always

            _showVisualization = false;
        }

        void LateUpdate()
        {
            if (_freezeVisualization == false)
            {
                _candidates = TobiiXR.Internal.G2OM.GetCandidates();
                _candidateResult = TobiiXR.Internal.G2OM.GetCandidateResult();
                _deviceData = TobiiXR.Internal.G2OM.GetDeviceData();
            }
        }

        IEnumerator OnPostRender()
        {
            if (_showVisualization == false) yield break;

            if (_candidates == null)
            {
                Debug.LogWarning("G2OM visualization does not have an instance to visualize, returning.");
                yield break;
            }

            Render(_mat, ref _deviceData, _candidates, _candidateResult, _corners);
        }

        private void Render(Material mat, ref G2OM_DeviceData deviceData, G2OM_Candidate[] g2omCandidates, G2OM_CandidateResult[] g2OmCandidatesResult, G2OM_Vector3[] corners)
        {
            mat.SetPass(0);

            RenderBackground();

            for (var i = 0; i < g2omCandidates.Length; i++)
            {
                var g2OmCandidate = g2omCandidates[i];

                var result = Interop.G2OM_GetWorldspaceCornerOfCandidate(ref g2OmCandidate, (uint)corners.Length, corners);
                if (result != G2OM_Error.Ok)
                {
                    Debug.LogError(string.Format("Failed to get corners of candidate {0}. Error code: {1}", g2OmCandidate.candidate_id, result));
                    continue;
                }

                Color resultingColor = GetResultColor(g2OmCandidatesResult, g2OmCandidate.candidate_id);

                RenderCube(corners, resultingColor);
            }

            RenderGaze(deviceData.gaze_ray_world_space, Color.yellow);
        }

        private void RenderBackground()
        {
            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.Color(_backgroundColor);
            GL.LoadOrtho();
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 1, 0);
            GL.Vertex3(1, 1, 0);
            GL.Vertex3(1, 0, 0);
            GL.End();
            GL.PopMatrix();
        }

        private Color GetResultColor(G2OM_CandidateResult[] g2OmCandidatesResult, ulong id)
        {
            var score = 0f;
            var isFirst = false;
            for (int i = 0; i < g2OmCandidatesResult.Length; i++)
            {
                if (g2OmCandidatesResult[i].candidate_id == id)
                {
                    isFirst = i == 0;
                    score = g2OmCandidatesResult[i].score;
                    break;
                }
            }

            if (isFirst && score > Mathf.Epsilon) return _mainFocusedObjectColor;

            return Color.Lerp(_focusableObjectColor, _otherFocusedObjectsColor, score * score);
        }

        private static void RenderGaze(G2OM_GazeRay gazeRay, Color color)
        {
            var ray = gazeRay.ray;

            if (gazeRay.is_valid.ToBool() == false)
                return;

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            GL.Color(color);
            GL.Vertex(ray.origin.Vector());
            GL.Vertex(ray.origin.Vector() + ray.direction.Vector() * 10);

            GL.End();
            GL.PopMatrix();
        }

        private static void RenderCube(G2OM_Vector3[] corners, Color color)
        {
            GL.PushMatrix();
            GL.Begin(GL.QUADS);

            // FRONT
            GL.Color(color);
            GL.Vertex(corners[(int)Corners.FLL].Vector());
            GL.Vertex(corners[(int)Corners.FUL].Vector());
            GL.Vertex(corners[(int)Corners.FUR].Vector());
            GL.Vertex(corners[(int)Corners.FLR].Vector());

            // LEFT SIDE
            GL.Color(color);
            GL.Vertex(corners[(int)Corners.BLL].Vector());
            GL.Vertex(corners[(int)Corners.BUL].Vector());
            GL.Vertex(corners[(int)Corners.FUL].Vector());
            GL.Vertex(corners[(int)Corners.FLL].Vector());

            GL.End();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Begin(GL.QUADS);

            // RIGHT SIDE
            GL.Color(color);

            GL.Vertex(corners[(int)Corners.FLR].Vector());
            GL.Vertex(corners[(int)Corners.FUR].Vector());
            GL.Vertex(corners[(int)Corners.BUR].Vector());
            GL.Vertex(corners[(int)Corners.BLR].Vector());

            // BOTTOM
            GL.Color(color);

            GL.Vertex(corners[(int)Corners.FLR].Vector());
            GL.Vertex(corners[(int)Corners.BLR].Vector());
            GL.Vertex(corners[(int)Corners.BLL].Vector());
            GL.Vertex(corners[(int)Corners.FLL].Vector());

            GL.End();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Begin(GL.QUADS);

            // BACK
            GL.Color(color);

            GL.Vertex(corners[(int)Corners.BLR].Vector());
            GL.Vertex(corners[(int)Corners.BUR].Vector());
            GL.Vertex(corners[(int)Corners.BUL].Vector());
            GL.Vertex(corners[(int)Corners.BLL].Vector());

            // TOP
            GL.Color(color);

            GL.Vertex(corners[(int)Corners.FUL].Vector());
            GL.Vertex(corners[(int)Corners.BUL].Vector());
            GL.Vertex(corners[(int)Corners.BUR].Vector());
            GL.Vertex(corners[(int)Corners.FUR].Vector());

            GL.End();
            GL.PopMatrix();
        }
    }
}
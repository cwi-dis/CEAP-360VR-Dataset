//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Xml;
using UnityEngine;

namespace Tobii.Research.Unity
{
    public class ScreenBasedSaveData : MonoBehaviour
    {
        /// <summary>
        /// Instance of <see cref="ScreenBasedSaveData"/> for easy access.
        /// Assigned in Awake() so use earliest in Start().
        /// </summary>
        public static ScreenBasedSaveData Instance { get; private set; }

        [SerializeField]
        [Tooltip("If true, data is saved.")]
        private bool _saveData;

        [SerializeField]
        [Tooltip("If true, Unity3D-converted data is saved.")]
        private bool _saveUnityData = true;

        [SerializeField]
        [Tooltip("If true, raw gaze data is saved.")]
        private bool _saveRawData = true;

        [SerializeField]
        [Tooltip("Folder in the application root directory where data is saved.")]
        private string _folder = "Data";

        [SerializeField]
        [Tooltip("This key will start or stop saving data.")]
        private KeyCode _toggleSaveData = KeyCode.None;

        /// <summary>
        /// If true, data is saved.
        /// </summary>
        public bool SaveData
        {
            get
            {
                return _saveData;
            }

            set
            {
                _saveData = value;
            }
        }

        private EyeTracker _eyeTracker;
        private XmlWriterSettings _fileSettings;
        private XmlWriter _file;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _eyeTracker = EyeTracker.Instance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleSaveData))
            {
                SaveData = !SaveData;
            }

            if (!_saveData)
            {
                if (_file != null)
                {
                    // Closes _file and sets it to null.
                    CloseDataFile();
                }

                return;
            }

            if (_file == null)
            {
                // Opens data file. It becomes non-null.
                OpenDataFile();
            }

            if (!_saveUnityData && !_saveRawData)
            {
                // No one wants to save anyway.
                return;
            }

            var data = _eyeTracker.NextData;
            while (data != default(IGazeData))
            {
                WriteGazeData(data);
                data = _eyeTracker.NextData;
            }
        }

        private void OnDestroy()
        {
            CloseDataFile();
        }

        private void OpenDataFile()
        {
            if (_file != null)
            {
                Debug.Log("Already saving data.");
                return;
            }

            if (!System.IO.Directory.Exists(_folder))
            {
                System.IO.Directory.CreateDirectory(_folder);
            }

            _fileSettings = new XmlWriterSettings();
            _fileSettings.Indent = true;
            var fileName = string.Format("data_{0}.xml", System.DateTime.Now.ToString("yyyyMMddTHHmmss"));
            _file = XmlWriter.Create(System.IO.Path.Combine(_folder, fileName), _fileSettings);
            _file.WriteStartDocument();
            _file.WriteStartElement("Data");
        }

        private void CloseDataFile()
        {
            if (_file == null)
            {
                Debug.Log("No ongoing recording.");
                return;
            }

            _file.WriteEndElement();
            _file.WriteEndDocument();
            _file.Flush();
            _file.Close();
            _file = null;
            _fileSettings = null;
        }

        private void WriteGazeData(IGazeData gazeData)
        {
            _file.WriteStartElement("GazeData");

            if (_saveUnityData)
            {
                _file.WriteAttributeString("TimeStamp", gazeData.TimeStamp.ToString());
                _file.WriteEye(gazeData.Left, "Left");
                _file.WriteEye(gazeData.Right, "Right");
                _file.WriteRay(gazeData.CombinedGazeRayScreen, gazeData.CombinedGazeRayScreenValid, "CombinedGazeRayScreen");
            }

            if (_saveRawData)
            {
                _file.WriteRawGaze(gazeData.OriginalGaze);
            }

            _file.WriteEndElement();
        }
    }
}
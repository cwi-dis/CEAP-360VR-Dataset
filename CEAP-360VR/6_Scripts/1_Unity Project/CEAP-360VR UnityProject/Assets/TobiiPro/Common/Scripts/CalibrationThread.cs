//-----------------------------------------------------------------------
// Copyright © 2019 Tobii Pro AB. All rights reserved.
//-----------------------------------------------------------------------

using System.Threading;

namespace Tobii.Research.Unity
{
    /// <summary>
    /// A class to handle blocking calibration calls. The reason is that
    /// blocking calls and thread sleep should be avoided in the main thread.
    /// 
    /// For example, when using the Vive HMD, if the Update() thread is
    /// blocked more than 10 frames, or about 110 ms, the background will start
    /// to fade out and the default SteamVR background will fade in. Also, any
    /// movement will be stopped during the block.
    /// 
    /// This class handles both HMD and screen based calibration.
    /// </summary>
    public sealed class CalibrationThread
    {
        IEyeTracker _eyeTrackerIF;
        private bool _screenBased;
        private Thread _thread;
        private bool _running;

        // Indicates if the thread is running.
        public bool Running
        {
            get
            {
                lock (MethodResult.Lock)
                {
                    return _running;
                }
            }

            set
            {
                lock (MethodResult.Lock)
                {
                    _running = value;
                }
            }
        }

        public bool ScreenBased
        {
            get
            {
                lock (MethodResult.Lock)
                {
                    return _screenBased;
                }
            }

            private set
            {
                lock (MethodResult.Lock)
                {
                    _screenBased = value;
                }
            }
        }

        public IEyeTracker EyeTrackerIF
        {
            get
            {
                lock (MethodResult.Lock)
                {
                    return _eyeTrackerIF;
                }
            }

            private set
            {
                lock (MethodResult.Lock)
                {
                    _eyeTrackerIF = value;
                }
            }
        }

        public struct Point
        {
            private UnityEngine.Vector3 _point;

            public Point(UnityEngine.Vector3 point)
            {
                _point = point;
            }

            public Point(UnityEngine.Vector2 point)
            {
                _point = point;
            }

            public Point(float x, float y) : this(new UnityEngine.Vector2(x, y))
            {
            }

            public Point(float x, float y, float z) : this(new UnityEngine.Vector3(x, y, z))
            {
            }

            public static implicit operator Point3D(Point point)
            {
                return new Point3D(point._point.x, point._point.y, point._point.z);
            }

            public static implicit operator NormalizedPoint2D(Point point)
            {
                return new NormalizedPoint2D(point._point.x, point._point.y);
            }
        }

        /// <summary>
        /// A class to encapsulate the method result. The caller should keep a reference and check the Ready flag for completion.
        /// </summary>
        public sealed class MethodResult
        {
            internal static object Lock = new object();
            private static MethodResult _currentResult;
            private static MethodResult _invalidResult = new MethodResult(CommandType.Invalid);
            private static Point _currentPoint;
            private CommandType _command;
            private CalibrationStatus _status;
            private int _elapsedMilliseconds;
            private bool _ready;

            public enum CommandType
            {
                Invalid,
                Enter,
                Collect,
                Compute,
                Leave,
            }

            public static Point CurrentPoint
            {
                get
                {
                    lock (Lock)
                    {
                        return _currentPoint;
                    }
                }

                internal set
                {
                    lock (Lock)
                    {
                        _currentPoint = value;
                    }
                }
            }

            // The command type indicates to which command the result is referring.
            public CommandType Command
            {
                get
                {
                    lock (Lock)
                    {
                        return _command;
                    }
                }
            }

            // Ready will become true when the command is ready.
            public bool Ready
            {
                get
                {
                    lock (Lock)
                    {
                        return _ready;
                    }
                }
            }

            // The amount of time the command took to execute.
            public int ElapsedMilliseconds
            {
                get
                {
                    lock (Lock)
                    {
                        return _elapsedMilliseconds;
                    }
                }

                set
                {
                    lock (Lock)
                    {
                        _elapsedMilliseconds = value;
                    }
                }
            }

            // The status of the command.
            public CalibrationStatus Status
            {
                get
                {
                    lock (Lock)
                    {
                        return _status;
                    }
                }
            }

            public MethodResult(CommandType command)
            {
                lock (Lock)
                {
                    _command = command;
                    _status = CalibrationStatus.Failure;
                }
            }

            public static MethodResult CurrentResult
            {
                get
                {
                    lock (Lock)
                    {
                        return _currentResult;
                    }
                }

                set
                {
                    lock (Lock)
                    {
                        _currentResult = value;
                    }
                }
            }

            public static MethodResult InvalidResult
            {
                get
                {
                    return _invalidResult;
                }
            }

            /// <summary>
            /// Indicate that the command is ready. The caller needs to hold a reference.
            /// </summary>
            /// <param name="status">The command result.</param>
            /// <param name="elapsed">How long the command took in milliseconds.</param>
            public void Finished(CalibrationStatus status, int elapsed)
            {
                lock (Lock)
                {
                    _ready = true;
                    _status = status;
                    _elapsedMilliseconds = elapsed;
                    _currentResult = null;
                }
            }

            public override string ToString()
            {
                return string.Format("{0}: Ready {1}, Status {2}, Elapsed ms {3}", Command, Ready, Status, ElapsedMilliseconds);
            }
        }

        private MethodResult Command(MethodResult.CommandType command, Point point)
        {
            if (_thread == null || (MethodResult.CurrentResult != null && MethodResult.CurrentResult.Command != MethodResult.CommandType.Invalid))
            {
                return MethodResult.InvalidResult;
            }

            var result = new MethodResult(command);
            MethodResult.CurrentPoint = point;
            MethodResult.CurrentResult = result;
            return result;
        }

        // The following methods correspond to their equivalents in the calibration ScreenBasedCalibration object.
        // They create a command that will be executed by the thread function.
        public MethodResult EnterCalibrationMode()
        {
            return Command(MethodResult.CommandType.Enter, new Point(0, 0));
        }

        public MethodResult CollectData(Point pt)
        {
            return Command(MethodResult.CommandType.Collect, pt);
        }

        public MethodResult ComputeAndApply()
        {
            return Command(MethodResult.CommandType.Compute, new Point(0, 0));
        }

        public MethodResult LeaveCalibrationMode()
        {
            return Command(MethodResult.CommandType.Leave, new Point(0, 0));
        }

        /// <summary>
        /// Stop the thread and if there is a current command, mark it as ready.
        /// </summary>
        /// <returns>The result from the Join call.</returns>
        public bool StopThread()
        {
            if (MethodResult.CurrentResult != null)
            {
                MethodResult.CurrentResult.Finished(CalibrationStatus.Failure, -1);
            }

            if (_thread == null)
            {
                return true;
            }

            Running = false;

            return _thread.Join(5000);
        }

        /// <summary>
        /// Constructor that creates and starts the calibration thread.
        /// </summary>
        public CalibrationThread(IEyeTracker eyeTracker, bool screenBased)
        {
            ScreenBased = screenBased;
            EyeTrackerIF = eyeTracker;
            _thread = new Thread(ThreadFunction);
            _thread.IsBackground = true;
            _thread.Start();
        }

        /// <summary>
        /// The thread function that calls the blocking calibration methods.
        /// </summary>
        private void ThreadFunction()
        {
            var eyeTracker = EyeTrackerIF;

            if (eyeTracker == null)
            {
                return;
            }

            Running = true;

            // Create the calibration object.
            ScreenBasedCalibration screenBasedCalibration = null;
            HMDBasedCalibration hmdBasedCalibration = null;
            if (ScreenBased)
            {
                screenBasedCalibration = new ScreenBasedCalibration(eyeTracker);
            }
            else
            {
                hmdBasedCalibration = new HMDBasedCalibration(eyeTracker);
            }

            // Find out how long the calls took using a stopwatch.
            var stopWatch = new System.Diagnostics.Stopwatch();

            // Handle the calibration commands.
            while (Running)
            {
                var currentResult = MethodResult.CurrentResult;
                stopWatch.Reset();
                stopWatch.Start();

                if (currentResult == null)
                {
                    Thread.Sleep(25);
                }
                else
                {
                    switch (currentResult.Command)
                    {
                        case MethodResult.CommandType.Invalid:
                            Thread.Sleep(25);
                            break;

                        case MethodResult.CommandType.Enter:
                            if (screenBasedCalibration != null)
                                screenBasedCalibration.EnterCalibrationMode();
                            else
                                hmdBasedCalibration.EnterCalibrationMode();

                            stopWatch.Stop();
                            currentResult.Finished(CalibrationStatus.Success, (int)stopWatch.ElapsedMilliseconds);
                            break;

                        case MethodResult.CommandType.Collect:
                            var collectResult = screenBasedCalibration != null ?
                                screenBasedCalibration.CollectData(MethodResult.CurrentPoint) :
                                hmdBasedCalibration.CollectData(MethodResult.CurrentPoint);

                            stopWatch.Stop();
                            currentResult.Finished(collectResult, (int)stopWatch.ElapsedMilliseconds);
                            break;

                        case MethodResult.CommandType.Compute:
                            CalibrationStatus status = screenBasedCalibration != null ?
                                screenBasedCalibration.ComputeAndApply().Status :
                                hmdBasedCalibration.ComputeAndApply().Status;

                            stopWatch.Stop();
                            currentResult.Finished(status, (int)stopWatch.ElapsedMilliseconds);
                            break;

                        case MethodResult.CommandType.Leave:
                            if (screenBasedCalibration != null)
                                screenBasedCalibration.LeaveCalibrationMode();
                            else
                                hmdBasedCalibration.LeaveCalibrationMode();

                            stopWatch.Stop();
                            currentResult.Finished(CalibrationStatus.Success, (int)stopWatch.ElapsedMilliseconds);
                            break;

                        default:
                            Thread.Sleep(25);
                            break;
                    }
                }
            }

            if (hmdBasedCalibration != null)
            {
                hmdBasedCalibration.Dispose();
            }

            if (screenBasedCalibration != null)
            {
                screenBasedCalibration.Dispose();
            }
        }
    }
}

// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR.Internal
{
    public static class TobiiEula
    {
        private static TobiiEulaFile _eulaFile;

        public static bool IsEulaAccepted()
        {
            #if !UNITY_EDITOR
                if(_eulaFile == null) _eulaFile = TobiiEulaFile.LoadEulaFile();
            #endif

            if (_eulaFile != null)
            {
                return _eulaFile.IsEulaAccepted();
            }
            
            return false;
        }

#if UNITY_EDITOR
        public static void SetEulaFile(TobiiEulaFile eulaFile)
        {
            _eulaFile = eulaFile;
        }

        public static void SetEulaAccepted()
        {
            if (_eulaFile != null)
            {
                _eulaFile.SetEulaAccepted();
            }
        }
#endif
    }
}
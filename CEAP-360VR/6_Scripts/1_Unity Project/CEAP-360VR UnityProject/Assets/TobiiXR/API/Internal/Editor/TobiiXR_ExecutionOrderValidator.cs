// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR.Internal
{
    using UnityEditor;
    using UnityEngine;
    
    // Credit http://squabpie.com/blog/2015/11/13/scripts-execution-order.html
    [InitializeOnLoad]
    public class TobiiXR_ExecutionOrderValidator
    {
        private const int ReaderExecOrder = -10000;

        static TobiiXR_ExecutionOrderValidator()
        {
            var temp = new GameObject();

            var reader = temp.AddComponent<TobiiXR_Lifecycle>();
            MonoScript readerScript = MonoScript.FromMonoBehaviour(reader);
            if (MonoImporter.GetExecutionOrder(readerScript) != ReaderExecOrder)
            {
                MonoImporter.SetExecutionOrder(readerScript, ReaderExecOrder);
                Debug.Log("Fixing exec order for " + readerScript.name);
            }

            Object.DestroyImmediate(temp);
        }
    }
}
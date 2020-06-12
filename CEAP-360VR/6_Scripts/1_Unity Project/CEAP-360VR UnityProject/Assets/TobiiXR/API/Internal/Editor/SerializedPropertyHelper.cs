namespace Tobii.XR.Internal
{
    using System;
    using UnityEditor;
    using UnityEditorInternal;

    public static class SerializedPropertyHelper
    {
        public static SerializedProperty ArrayAddElement(this SerializedProperty array)
        {
            var index = array.arraySize;
            array.arraySize++;
            return array.GetArrayElementAtIndex(index);
        }

        public static SerializedProperty AddElement(this ReorderableList list)
        {
            var element = list.serializedProperty.ArrayAddElement();
            list.index = list.serializedProperty.arraySize;
            return element;
        }

        public static bool ArrayContains(this SerializedProperty array, Func<SerializedProperty,bool> compareFunction)
        {
            for(int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                if(compareFunction(element)) return true;
            }
            return false;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

// get rotation data from unity
public class GetInspectorRotationValue : MonoBehaviour
{
    public static GetInspectorRotationValue Instance;
    private void Awake()
    {
        Instance = this;                           
    }
    public Vector3 GetInspectorRotationValueMethod(Transform transform)
    {
        System.Type transformType = transform.GetType();
        PropertyInfo m_propertyInfo_rotationOrder = transformType.GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
        object m_OldRotationOrder = m_propertyInfo_rotationOrder.GetValue(transform, null);
        MethodInfo m_methodInfo_GetLocalEulerAngles = transformType.GetMethod("GetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
        object value = m_methodInfo_GetLocalEulerAngles.Invoke(transform, new object[] { m_OldRotationOrder });
        string temp = value.ToString();
        temp = temp.Remove(0, 1);
        temp = temp.Remove(temp.Length - 1, 1);
        string[] tempVector3;
        tempVector3 = temp.Split(',');

        Vector3 vector3 = new Vector3(float.Parse(tempVector3[0]), float.Parse(tempVector3[1]), float.Parse(tempVector3[2]));
        return vector3;
    }
}


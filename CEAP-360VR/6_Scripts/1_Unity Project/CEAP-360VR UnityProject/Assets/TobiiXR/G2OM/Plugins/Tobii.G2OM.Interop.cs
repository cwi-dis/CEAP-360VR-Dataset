// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Interop
    {
        public const string g2om_lib = "tobii_g2om";

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_context_options_init")]
        private static extern G2OM_Error G2OM_InitializeOptions(ref G2OM_Internal_ContextCreateOptions options);

        public static G2OM_ContextCreateOptions G2OM_InitializeOptions()
        {
            var internalOptions = new G2OM_Internal_ContextCreateOptions();
            var options = new G2OM_ContextCreateOptions();

            var result = G2OM_InitializeOptions(ref internalOptions);
            if(result == G2OM_Error.Ok) // What to do if this fails?
            {
                options.capacity = internalOptions.capacity;
                options.thread_count = internalOptions.thread_count;
            }

            return options;
        }

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_context_create")]
        public static extern G2OM_Error G2OM_ContextCreate(out IntPtr context);

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_context_create_ex")]
        private static extern G2OM_Error G2OM_ContextCreateEx(out IntPtr context, ref G2OM_Internal_ContextCreateOptions options);

        public static G2OM_Error G2OM_ContextCreateEx(out IntPtr context, ref G2OM_ContextCreateOptions options)
        {
            var has_license = !string.IsNullOrEmpty(options.license_content); 
            var internalOptions = new G2OM_Internal_ContextCreateOptions();
            var license = has_license ? options.license_content : "" ;
            
            var bytes = has_license ? Encoding.UTF8.GetBytes(options.license_content) : new byte[0];
            var ptr = Marshal.AllocHGlobal(license.Length);

            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            internalOptions.capacity = options.capacity;
            internalOptions.license_length = (uint) license.Length;
            internalOptions.thread_count = options.thread_count;
            internalOptions.license_ptr = has_license ? ptr : IntPtr.Zero;

            var result = G2OM_ContextCreateEx(out context, ref internalOptions);

            Marshal.FreeHGlobal(ptr);

            return result;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct G2OM_Internal_ContextCreateOptions
        {
            public uint capacity;
            public uint thread_count;
            public IntPtr license_ptr;
            public uint license_length;
        }
                
        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_process")]
        public static extern G2OM_Error G2OM_Process(IntPtr context, ref G2OM_DeviceData deviceData, ref G2OM_RaycastResult raycastResult, uint candidatesCount, G2OM_Candidate[] candidates, G2OM_CandidateResult[] candidateResults);

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_context_destroy")]
        public static extern G2OM_Error G2OM_ContextDestroy(ref IntPtr context);

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_get_version")]
        public static extern G2OM_Error G2OM_GetVersion(out G2OM_Version version);

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_get_worldspace_corner_of_candidate")]
        public static extern G2OM_Error G2OM_GetWorldspaceCornerOfCandidate(ref G2OM_Candidate candidate, uint numberOfCorners, G2OM_Vector3[] cornersOfCandidateInWorldSpace);

        [DllImport(g2om_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "g2om_get_candidate_search_pattern")]
        public static extern G2OM_Error G2OM_GetCandidateSearchPattern(IntPtr context, ref G2OM_DeviceData deviceData, uint numberOfRays, G2OM_GazeRay[] mutatedRays);
    }

    public enum G2OM_Error
    {
        Ok = 0,
        NullPointerPassed = -1,
        Internal = -2,
        ThreadPool = -3,
        IndexOutOfBounds = -4,
        InternalConversion = -5,
        LogFile = -6,
        NotPermittedByLicense = -7,
        LicenseInvalid = -8,
        NotImplemented = -9,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_DeviceData
    {
        public float timestamp;
        public G2OM_GazeRay gaze_ray_world_space;
        public G2OM_Vector3 camera_up_direction_world_space;
        public G2OM_Vector3 camera_right_direction_world_space;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_ContextCreateOptions
    {
        public uint capacity;
        public uint thread_count;
        public string license_content;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_GazeRay
    {
        public G2OM_Ray ray;
        public byte is_valid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_RaycastResult
    {
        public G2OM_Raycast gaze_ray;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_Raycast
    {
        public byte is_raycast_id_valid;
        public ulong candidate_id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_Candidate
    {
        public ulong candidate_id;
        public G2OM_Vector3 aabb_max_local_space;
        public G2OM_Vector3 aabb_min_local_space;
        public G2OM_Matrix4x4 world_to_local_matrix;
        public G2OM_Matrix4x4 local_to_world_matrix;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_Matrix4x4
    {
        public float m00;
        public float m10;
        public float m20;
        public float m30;
        public float m01;
        public float m11;
        public float m21;
        public float m31;
        public float m02;
        public float m12;
        public float m22;
        public float m32;
        public float m03;
        public float m13;
        public float m23;
        public float m33;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_CandidateResult
    {
        public ulong candidate_id;
        public float score;
        public G2OM_GazeRay adjusted_gaze_ray_world_space;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_Ray
    {
        public G2OM_Vector3 origin;
        public G2OM_Vector3 direction;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct G2OM_Version
    {
        public uint major;
        public uint minor;
        public uint buildVersion;
    }

    public enum Corners
    {
        FLL = 0,    // Front Lower Left
        FUL,        // Front Upper Left
        FUR,        // Front Upper Right
        FLR,        // Front Lower Right
        BLL,        // Back Lower Left
        BUL,        // Back Upper Left
        BUR,        // Back Upper Right
        BLR,        // Back Lower Right
        NumberOfCorners
    }

    public static class G2OM_ExtensionMethods
    {
        public static byte ToByte(this bool b)
        {
            return b ? (byte) 1 : (byte) 0;
        }

        public static bool ToBool(this byte b)
        {
            return b == 1;
        }
    }
}
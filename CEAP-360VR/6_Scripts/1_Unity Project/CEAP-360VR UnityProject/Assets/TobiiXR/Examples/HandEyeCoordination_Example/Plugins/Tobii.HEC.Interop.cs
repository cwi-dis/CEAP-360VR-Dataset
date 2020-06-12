namespace Tobii.HEC
{
    using System.Runtime.InteropServices;
    using UnityEngine;

    public static class Interop
    {
        public const string hec_lib = "tobii_hec";

        [DllImport(hec_lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "calculate_throw")]
        public static extern HEC_Error HEC_Calculate_Throw(
            Vector3 origin, 
            Vector3 min_bounds, 
            Vector3 max_bounds, 
            Vector3 initial_velocity, 
            float gravity, 
            float lower_velocity_multiplier, 
            float upper_velocity_multiplier, 
            float max_y_modifier, 
            out Vector3 adjusted_velocity
            );
    }

    public enum HEC_Error
    {
        Success = 0,
        NoSolution = -10
    }
}
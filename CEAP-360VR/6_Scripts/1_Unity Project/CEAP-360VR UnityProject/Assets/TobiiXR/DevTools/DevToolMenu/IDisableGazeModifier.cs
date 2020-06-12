using Tobii.G2OM;

namespace Tobii.XR.DevTools
{
    public interface IDisableGazeModifier : IGazeFocusable
    {
        bool Disable { get; }
    }
}
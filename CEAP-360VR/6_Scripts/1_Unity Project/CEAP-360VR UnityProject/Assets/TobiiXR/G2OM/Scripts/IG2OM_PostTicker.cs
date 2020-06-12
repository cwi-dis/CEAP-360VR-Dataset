// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.G2OM
{
    using System.Collections.Generic;

    public interface IG2OM_PostTicker
    {
        void TickComplete(List<FocusedCandidate> focusedObjects);
    }
}
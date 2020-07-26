# View-point-dependent Annotation Fusion for 360VR （VAFusion）

This package contains the algorithm of View-point-dependent Annotation Fusion for 360VR. The algorithm consists of three modules (steps）: 

- Time-alignment for annotations (i.e., valence and arousal)
- View-point clustering 
- Annotation fusion

# Folder structure

1. **data:** data folder for input data
2. **source:** source code of VAFusion
3. **results:** results saved in npz files
4. **fig:** fusion results plot in four quadrants

# Read results

```
import numpy as np
data = np.load("./results/fusion_result.npz", allow_pickle=True)
#clustered viewpoints (pitch and raw) in every 1s (60s, 8 videos)
pitch = data["pitch"]
yaw = data["yaw"]
#box = [pitch_max,pitch_min,yaw_max,yaw_min]
box = data["box"]
valence = data["valence"]
arousal = data["arousal"]
```
# CEAP-360VR Video Stimuli

## Videos

A copy of the stimuli videos is available here: https://su.drive.sunet.se/index.php/s/NmyCR5aepC86Z8P.

Place the videos in this folder to execute the calculation video luminance from the notebook `ceap_calculate_video_luminance.ipynb`.

**Note:**

The original work states that **V4** has a rate of 30FPS, but the downloaded video was at 25FPS. This video was manually converted using `ffmpeg` to match the 30FPS from the paper.

## Downloading using 'youtube-dl'

To list all the available formats to be downloaded

`youtube-dl --list-formats https://youtu.be/<ID>`

To download a specific format and resolution. The command --user-agent enforces equirectangular 360 videos:

`youtube-dl --user-agent "" -f 137 https://youtu.be/<ID> -o V0-Training-NASA.mp4`

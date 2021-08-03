# Introduction

A repository containing the source code of the tool presented in the paper *"REEFT-360: Real-time Emulation and Evaluation Framework for Tile-based 360 Streaming under Time-varying Conditions"*.  
It is under the GNU GPL 3.0 license and the result data from the experiments can be seen in the *experiments_data* folder. The tool utilises compute shaders in order to emulate different quality levels and tiling of the video and as such a fairly powerful GPU is recommended, especially when using videos with higher framerates.

It is recommended that you have some experience with c# or java before using (or modifying) the tool, as some editing of the code will be required.

# How to set up (in order):

- Clone repo into your unity projects Resources folder.

- Follow https://docs.unity3d.com/Manual/VideoPanoramic.html but with the files in this project.

- Add PreProcess script to the main camera

- You might need to rotate the skybox by 270 degrees, this will become apparent when you start testing. But if you copy the supplied skybox you should not have to. If you do this is done via the asset inspector in unity.

- In Player.cs set up the testing and eval stages to use the download manager and requester (the switch case starting on line 40).  The ones used in the paper are available, but you can also write your own. The supplied ones are constant requester, highest possible, trace requester and waterfill requester. These are located in the Requester folder.

- In utils.cs set the wanted global variables such as chunk length, number of tiles and max buffer.

- Make sure the correct player mode is set in utils.cs

- A network trace and a headmovement trace is already available in the traces folder. 

- Make sure to copy all output files between recordings as they will be overwritten when running in eval mode. 

- Run the scene in unity.

# Finally

Should you mention or use our framework, code, or datasets in your work, we kindly ask that you reference the following paper in your publication:  
Eric Lindskog and Niklas Carlsson, *“REEFT-360: Real-time Emulation and Evaluation Framework for Tile-based 360 Streaming under Time-varying Conditions”*, Proc. ACM Multimedia Systems Conference (ACM MMSys), Istanbul, Turkey, Sept/Oct. 2021.
# Description of file contents.

This section contains descriptions for the output files.

## Chunktrace
It contains the quality level of each of the tiles in a given chunk. They are index by x_i + x_max*y_i. It is an output file when running in eval, input in recording.

## Tile focus trace
The first column contains the index of the tile above, the rest of the columns contain the quality of all of the tiles. The same indexing as for chunktrace is used. This is run at 50hz unlike chunktrace which only saves the chunk data. This is useful for getting the quality of the tile in view at any given time. 

## View port quality
The quality of the tiles in view. These are not indexed, but are used to get the number of tiles in view and their quality. This runs at 50hz.

## Headmovement trace
Contains the  yaw, pitch and roll of the HMD at a given time. This runs at 50hz. It is an output file when running in eval, input in recording.

## Playback log
Contains the start of a stall and it's duration.

## Network trace
Contains the average bandwidth for one second.

---

# Below are all of the experiments.

Algorithm | Video | Max buffer | Network Conditions | A | Name
--------- | ------| ---------- | ------------------ | - | ----
Waterfill | avenger | 2 | poor | 0 | exp1
Waterfill | avenger | 2 | poor | 200 | exp2
Waterfill | avenger | 2 | fair | 0 | exp3
Waterfill | avenger | 2 | fair | 200 | exp4
Waterfill | avenger | 2 | good | 0 | exp5
Waterfill | avenger | 2 | good | 200 | exp6
Dash | avenger | 2 | poor | 0 | exp7
Dash | avenger | 2 | fair | 0 | exp8
Dash | avenger | 2 | good | 0 | exp9
Waterfill | LeMans | 2 | poor | 0 | exp10
Waterfill | LeMans | 2 | poor | 200 | exp11
Waterfill | LeMans | 2 | fair | 0 | exp12
Waterfill | LeMans | 2 | fair | 200 | exp13
Waterfill | LeMans | 2 | good | 0 | exp14
Waterfill | LeMans | 2 | good | 200 | exp15
Dash | LeMans | 2 | poor | 0 | exp16
Dash | LeMans | 2 | fair | 0 | exp17
Dash | LeMans | 2 | good | 0 | exp18
Waterfill | LeMans | 4 | fair | 0 | exp19
Waterfill | LeMans | 4 | fair | 200 | exp20
Waterfill | Avenger | 4 | fair | 0 | exp21
Waterfill | Avenger | 4 | fair | 200 | exp22
Waterfill | LeMans  |	2 | good (capped) | 0 | exp23 
Waterfill | LeMans | 2 | good (capped) | 200 | exp24 
Dash | LeMans | 2 | good (capped) | 0 | exp25 
Waterfill | avenger | 2 | good (capped) | 0 | exp26 
Waterfill | avenger | 2 | good (capped) | 200 | exp27 
Dash | avenger | 2 | good (capped) | 0 | exp28
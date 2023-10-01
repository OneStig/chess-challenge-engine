# Guppy
Guppy is my attempt at a tiny (< 1024 tokens) chess engine for [Sebastian Lague's chess coding challenge](https://youtu.be/iScy18pVR58).

### Notes
* Evaluation is done with PeSTO. Each cell is a 9 bit number (parity bit + unsigned byte), with 10 encoded into each decimal for a total of 78 decimals + attack unit king safety evaluation
* Estimated to play at 2400 vs humans in 5+0 time control
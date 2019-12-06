# Path-Planning

## Running the project
This project is made in Unity 2018.3.3f1. Download the the source and open it in Unity. Alternatively, a version built for WebGL is also viewable here: https://www.cse.unr.edu/~erylk/.

## About the Project
A demonstration of the A* algorithm for path planning. A* is the de facto path searching algorithm used in games and has many other applications throughout computer science. It works by iteratively exploring the "best" node which is directly traversable from the list of already explored nodes until the explored node is the goal. The "best" node is found by considering the path distance from each node to the start node start, combined with each the nodes' expected distance to the goal (using a heuristic), and picking the node with the lowest score.

This demonstration extends A* to use zones of influence around obstacles. Instead of just disallowing paths which would go through obstacles, paths which go too close to obstacles - even if they don't touch - are penalized. This incentivizes smoother, more human-like paths. Each obstacle has 16 radial zones each with a different radius of effect. The added cost for each node is based on its distance to the target compared to the radius for the zone it is in. Each zone's radius is averaged with its neighboring zones at the start of the program to make smoother areas of influence. Additionally, the radius each node uses is interpolated between its zone's radius and its neighboring zones' radii based on its angle within the zone.

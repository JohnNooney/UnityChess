# UnityChess
The purpose of this game is to demonstrate the classic MinMax algorithm vs. the adaptability of a Bayesian Inference Network in a system through a chess match. During the course of a match the Bayesian Network has to constantly adapt to the changing enviroment. This is necessary since having a constant change in the enviroment is what the network uses to identify the user's playing pattern and act accordingly. Where as the MinMax algorithm calculates the best possible score dependant on the depth searched. After cycling through each piece and simulating every possible outcome (to n depth) it selects the move that will grant the best score. 

The aim is to see how each algorithm performs when pitted against each other. Will the Bayes Network be able to identify and overcome the minmax algorithm? Or will the MinMax be able to bruteforce it's way to victory?

## Assets
* This game uses the following assets for the [chess board and pieces](https://assetstore.unity.com/packages/3d/props/2d-3d-chess-pack-93915) and [Furniture](https://assetstore.unity.com/packages/3d/props/furniture/retro-furniture-83306). 


* The base chess mechanics are adapted from [here](https://lucid.app/lucidchart/e61c96a3-e33b-4dab-9db0-e0b25483e6e1/edit?page=HWEp-vi-RSFO#)

## Implementation

**Bayesian network** is built on pattern recognition and learning from it's own experience.

**Pattern Recongnition**
- Being able to classify user's moves and exercise an appropriate response.

**Playing Refinement**
- Continously refine the search heuristic for realisticly calculating the right moves (Otherwise there are too many possible moves)


**MinMax** is implemented using a recursive tree search. Each node represents a piece's evaluated score after a specific move and the depth is in relation to the current color being simulated.

![Tree Search](https://cdn-media-1.freecodecamp.org/images/1*UA5VlNs7s4gl80VknA099w.jpeg)


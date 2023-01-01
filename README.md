# Buggary
The premiere bug writing tool! Unity code editor that works in **Runtime** and can be embedded in games. 

Video introduction: https://youtu.be/d1Yt6flsLpY

![BuggaryShot](https://user-images.githubusercontent.com/121664522/210135809-324e7df8-e252-441e-bb8c-a8a7f9d0c1b2.png)

## Features
* Code completion.
* Compile time error overlay.
* Shows overload options [When the cursor is on a method with overloads].
* Detailed customizable code highlighting(coloring) [There is a BuggaryColors scriptable object that can be edited].
* Can generate using statements for unrecognized types [This is triggered by ctrl+.];

Buggary is also a game, where you have to write code to complete challenges. There are three example levels currently you can go trough, or see the second portion of the video where I do that.

## Shortcomings
* This will not work on all platforms, although I have not done the research to figure which ones it will work on. I have only validated windows builds so far.
* Currently there no undo/redo for text editing. This should be somewhat straight-foreword and will be working on it soon. That or figure out a different text editor.
* Buggary is still work in progress, feel free to leave bug reports and suggestions.
# Documentation
The editor and console section can be placed automatically in a single UI parent or separately. In the editor folder there are two scenes that use a different method each, you can check them for reference.
## Editor
* ctrl+space makes you force a completion for the location of the caret
* ctrl+. triggers the context actions – the only one now is the add missing using refactoring
## Game
* ctrl+r brings you to the first level
* ctrl+pageUp brings you to next level
* ctrl+pageDown brings to to previous level
## Compatibility
* Unity version: 2021.3
* The project is using URP – this should not affect the editor itself but the game uses a prefab with three basic objects in it and it should not be difficult to swap those out.

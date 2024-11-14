## With Assistance From

Vecderg (UI programming and physics systems), JackTR55555 (dev tools and input system)

## What is this?

This is a project where I attempted to make an in-depth fighting game engine in Unity. Given that the alternative to making a fighting game is UFE, I wanted to make something more accessible and deeper, with more freedom and customization
![](doc/gifs/example.gif)

The project uses a fixed-point physics system, allowing for across-the-board determinism. Basically, I'm gonna implement rollback into this thing.

The main backbone of this system is a state machine (what a surprise) but with a twist. The basic concept of this engine is that you DO NOT NEED TO KNOW HOW TO PROGRAM to make something. The functions that you would normally expect to see from a state machine are still there, but they're handled with enums and ints than regular code.
![](doc/images/transitions.png)

There is some scripting involved, however. As part of an effort to mimic the Arc Systemworks's BB script, the frames can be individually scripted for changes relating to the state machine or for gameplay, such as setting hit/hurtbox dimesions, manipulating resources, changing velocity, etc.\
![](doc/images/scripting(2).png)

The physics is a heavily modified version of:



## Current Status: Pre-Alpha

The Current Status of this engine is still in the preliminary stages of development.

## Plans for the future
- State scripts being converted into commands when C# unity loads in, as of now, you need to manually convert them when you open the project.
- Rollback netcode and save-state-based features such as replays, training mode dummies, etc are the next big milestone.

## Getting Started

1) Clone the project
2) Open the project in the latest applicable version of Unity

From this point forward, there is no clear path ahead, as a lot of the features are still under construction.



## Liscense

MIT

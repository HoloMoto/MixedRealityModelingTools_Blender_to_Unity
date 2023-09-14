# MixedRealityModelingTools

This repository is a prototype for setting up a server directly in Blender without UnityMeshSync to send the currently selected mesh data to Unity apps and editors in the local network.

　Since Blender 3.x it is possible to use VR sessions using MetaQuest or PCVR.
 
 The problem with this is that it is difficult to model with keyboard input or mouse operation while wearing an HMD.

We are mainly aiming for interactive modeling while wearing an XR device.

　This package consists of a Blender add-on for sending and receiving various information within Blender and plugins for bidirectional data communication with Mixed Reality application development engines like Unity, UE5, StereoKit, Babylon.js, and others.
 

## How to Use 

Please install the Blender add-on(from Release Page).


With the object you want to send selected in Blender, you should see the MixedRealityModelingTools tab in the Object Properties tab. When connected to the client, you can click "Send To Mesh" to send it.

On the Unity side, you can install it as a UPM (Unity Package Manager).


## Other


This project is being developed as an open-source project. We appreciate everyone's opinions and contributions.

Also, please remember that it's still a work in progress at this point.

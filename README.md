# MixedRealityModelingTools

This repository is a prototype for setting up a server directly in Blender without UnityMeshSync to send the currently selected mesh data to Unity apps and editors in the local network.

　Since Blender 3.x it is possible to use VR sessions using MetaQuest or PCVR.
 
 　The problem with this is that it is difficult to model with keyboard input or mouse operation while wearing an HMD.

　We are mainly aiming for interactive modeling while wearing an XR device.


# MixedRealiyModelingTools 日本語

　本リポジトリはUnityMeshSyncを使用せずにBlenderで直接サーバーを立ててローカルネットワーク内でUnity製アプリおよびエディタに現在選択しているメッシュデータを送信するパッケージです。

　Blender3.x以降で搭載されたVRSessionによってモデリングをxRデバイスでリアルタイムに見ることができるようになりましたが問題点として、HMDをかけながらキーボード入力やマウス操作を行いモデリングを行うことは困難でした。

　本パッケージではUnity製アプリとBlenderを同期することを目的としています。パッケージ自体はXRデバイスに依存をさせていませんが、主にXRデバイスをかけながら双方向にモデリングを行うことを目指しています。
 
 
 # 環境
 
 ・Unity 2021.3.5f1(LTS)
 
 ・Blender 3.4

 #ブランチ

 現時点でholomoto/TCPブランチで開発を行っています。

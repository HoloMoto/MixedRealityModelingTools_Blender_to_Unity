# BlenderMRViewForHoloLens
This repository is a prototype for setting up a server directly in Blender without UnityMeshSync to send the currently selected mesh data to Unity apps and editors in the local network.

　Since Blender 3.x it is possible to use VR sessions using MetaQuest or PCVR.
 
 　The problem with this is that it is difficult to model with keyboard input or mouse operation while wearing an HMD.

　We are mainly aiming for interactive modeling while wearing an XR device.


# BlenderMRViewForHoloLens 日本語

　本リポジトリはUnityMeshSyncを使用せずにBlenderで直接サーバーを立ててローカルネットワーク内でUnity製アプリおよびエディタに現在選択しているメッシュデータを送信するプロトタイプです。

　Blender3.x以降ではMetaQuestやPCVRを使用したVRセッションが使用可能になりました。
 
 　しかしこの問題点として、HMDをかけながらキーボード入力やマウス操作を行いモデリングを行うことは困難でした。

　おもにXRデバイスをかけながら双方向にモデリングを行うことを目指しています。
 
 
 # 環境
 
 ・Unity 2021.3.5f1(LTS)
 
 ・Blender 3.4

 ## 現時点の問題点
 
 　Unityアプリケーションがクラッシュする問題を確認しています。　こちらに関しては現在原因の特定作業を行っています。

# PBTools

## 概要
個人的に欲しかった、VRChatのPhysics Bone関連のツールを3つまとめたものと、テスト用Unityプロジェクト。

### PhysicsBoneExtractor
アバターについているVRCPhysBone、VRCPhysBoneColliderを1つのオブジェクトにまとめる

### PhysicsBoneIsAnimatedChanger
アバターについてるVRCPhysBoneの"IsAnimated"を一括でOFFにするツール

### PhysicsBoneMover
アバターについているVRCPhysBone、VRCPhysBoneColliderを、同じ構造の別のアバターに移動するツール

## 要求
- OS: windows
- powershell
- Unity: 2019.4.31.f
- VRCSDK

## プロジェクトの使い方

### テスト
事前にアバター用のVRCSDKをインポートする。

Test Runnerで単体テストが実行できる("Assets/Scenes/UnitTest"が単体テスト用のシーン)。

"Assets/Test"以下にテスト用のアバターのPrefab Variantが存在するので、これを用いて結合テストを行う(このPrefabはVRCSDKのサンプルアバターを参照している)。  
"Yakumo890 > PBTools"から各ツールを起動できる。

### Unitypackageのエクスポート
MakeUityPackage.ps1を実行すると、カレントディレクトリにUnitypackageが生成される。

## Author

yakumo  
[twitter](https://twitter.com/__yakumo890__)

## Licence

[MIT License](https://github.com/yakumo890/PBTools/License.txt)


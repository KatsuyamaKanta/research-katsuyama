# research-katsuyama



### 概要



本研究は，ハプティックデバイス「3D Systems Touch」を用いてUnity上のオブジェクトを直感的に操作し，ゲームステージデザインを支援することを目的としています．

従来のマウス・キーボード操作では，3D空間内でのオブジェクト配置や接触確認に視点変更や細かな座標調整が必要になります．

本研究では，ハプティックデバイスによる直感的な操作と触覚フィードバックを利用することで，オブジェクト同士の接触や配置状態を感覚的に把握しながらステージデザインができることを目指しています．



### 使用技術・ツール



* Unity
* C#
* Touch（3D Systems）



### プロジェクトの構成



Asset/3DSystems

\-Unity Asset Storeにて頒布されている3DSystems公式のアセットです．こちらのアセットではScene内でオブジェクトを操作することは可能ですが，エディタ上でオブジェクトを操作することはできません．



Asset/Editor

\-Unityのエディタ拡張についてのコードです．



&#x20;  ↳Editor/EditorClass

&#x20;  -Unityエディタ上でのオブジェクトの操作と衝突判定を定義しているクラスです．

&#x20;  ↳Editor/EditorWindow

&#x20;  -専用のカスタムウィンドウを設定しています．



Asset/Experiments

\-先行研究にて開発された，上記の公式アセットのコードの改変版です．公式の多機能なコードを削って実験用にTouchデバイスによる操作対象の移動と接触点情報の送信に処理を絞ったコードです．本研究でも操作したいオブジェクトにこのコンポーネントを追加して情報を得ています．



### 実装内容



\- ハプティックデバイスによるオブジェクトの選択・移動

\- オブジェクト同士の接触判定

\- 接触時の触覚フィードバック



詳しい内容は研究要旨(research-Abstract)に記載しています．

### 

### 先行研究コードとの差分



本研究は，研究室の先行研究をもとに開発しています．

比較用として，先行研究時点でのプロジェクトを'previous-research' ブランチに保存しています．

現在の'main' ブランチは，本研究の最新版を配置しています．



また，先行研究と本研究の差異として、本研究で新たに実装した内容を以下に記載します．



‐ メッシュオブジェクトの操作と衝突判定の実装　

&#x20;   EditorClass/MeshSDF.cs と EditorClass/CustomHapticEditor.csのcalculateCollisionPoint関数，calculateCollisionVector関数



‐ オブジェクトを操作する際にオブジェクトがある位置から操作ができるように変更

　　先行研究では一度初期位置に戻ってから操作を開始していたため、利便性が悪かった

　　EditorClass/CustomHapticEditor.cs のResetMoveBase関数とUpdateTransform関数



‐ 専用のカスタムウィンドウの情報追加　EditorWindow/MainHapticEditorWindow.cs







### 実行環境



　Unity：main(最新版)　2022.3.22f1

&#x20;        previous-research  2020.3.16f1

　OS：Windows 10

　使用デバイス：3D Systems Touch


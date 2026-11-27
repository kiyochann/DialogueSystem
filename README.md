# DialogueSystem

Unity 向けのモジュール式ノードベース会話システムです。コア機能（`DialogueCore`）と各種プラグイン（`DialoguePlugins`）が分離されており、立ち絵・音声・分岐・演出コマンド・フォント切り替え・ローカライズなどを柔軟に拡張できます。

## 主な機能

- **ノードベース会話グラフ (`DialogueCore`)**: 会話データ（`DialogueContainer`）の再生・進行管理とカスタムエディタ
- **立ち絵システム (`Portraits`)**: `CharacterProfile` / `DialoguePortraitDatabase` によるキャラクター立ち絵・表情・Animator 制御
- **オーディオシステム (`Audio`)**: `DialogueAudioDatabase` による SE / ボイス再生管理
- **分岐・選択肢 (`Branching`)**: 選択肢の表示と条件分岐
- **演出コマンド (`Commands`)**: テキスト内のインラインコマンド（例: `[font:style=horror]` 等）の解析・実行
- **フォントスタイル (`DialogueFontDatabase`)**: キャラクター標準フォントの自動適用と一時的なフォント切り替え
- **非破壊ローカライズ (`IDialogueTextResolver`)**: 外部の多言語辞書・Addressables 等を差し込める拡張インターフェース

## 動作環境

- **Unity**: 6000.0 以降（開発環境: `6000.3.20f1`）
- **依存パッケージ**:
  - `com.unity.ugui` (TextMeshPro を含む)
  - `com.unity.inputsystem`

## インストール方法 (Unity Package Manager)

1. Unity エディタのメニューから `Window` > `Package Manager` を開きます。
2. 左上の `+` ボタンをクリックし、`Install package from git URL...` を選択します。
3. 以下の URL を入力して `Install` をクリックします。

```text
https://github.com/kiyochann/DialogueSystem.git?path=/Assets/DialogueSystem
```

特定のバージョン（タグ）を指定する場合は末尾に `#v1.2.0` などを付与してください。

## フォルダ構成

```text
Assets/DialogueSystem/
├── package.json         # UPM パッケージ定義
├── README.md            # パッケージドキュメント
├── DialogueCore/        # 会話システムのコア（Runtime / Editor）
├── DialoguePlugins/     # 拡張プラグイン群（Audio, Branching, Commands, Layouts, Localization, Portraits, Other）
├── Data/                # サンプル・データベース等のアセット
├── Prefab/              # Dialogue.prefab
└── Testing/             # テスト用スクリプト・メモ
```

## 変更履歴

詳細な変更履歴は [CHANGELOG.md](./CHANGELOG.md) をご覧ください。

# Changelog

All notable changes to the DialogueSystem package/repository will be documented in this file.

## [Unreleased] - 2026-09-28

### Added (新規追加機能)
- **`PrefabLayoutHandler` (`[layout:name=...]` / `[layout:prefab=...]`)**:
  - ダイアログの枠や装飾画像をPrefab単位で動的に生成・配置・差し替えるレイアウトハンドラーを新規実装。
  - `IDialogueLayoutHandler` に準拠し、既存のシステム構造を壊すことなくシームレスに機能。
  - **初期枠の維持・復帰**: シーン初期配置の枠（`defaultFrameRoot`）を破壊せず保持し、`[layout:name=default]` でいつでも初期状態に安全復帰可能。
  - **柔軟な差し替えモード (`FrameReplaceMode`)**: 古い枠を非アクティブ化して再利用する `DeactivateAndCache`（推奨）と、`DestroyOld`（破棄）をインスペクターから選択可能。破棄による不都合や参照切れを完全に防止。
  - **テキスト領域調整連動**: Prefabのデザインに合わせてテキスト表示領域（RectTransform）のオフセットを動的に変更・復元可能。
- **`AdvanceCommandHandler` (`[advance]`)**:
  - 会話ノードをコマンド経由で強制的に次へ進行させるコマンドハンドラーを追加。
  - 外部入力待ちロック（`SetExternalInputLock(false)`）も解除してスムーズに再開可能。
- **`EndDialogueCommandHandler` (`[end_dialogue:fade=0.5]`)**:
  - ダイアログUI（`CanvasGroup`）をフェードアウトさせながら安全に会話を終了するコマンドハンドラーを追加。
  - フェード演出中の連打による誤動作防止ロックも内包。
- **`SetActiveCommandHandler` (`[active:mode=..., target=..., state=T/F]`)**:
  - 会話タグからGameObjectの表示/非表示（`SetActive`）を切り替えるコマンドハンドラーを追加。
  - `mode=name`（非アクティブオブジェクトも検索可能）、`mode=tag`、`mode=direct`（インスペクター登録）に対応。
- **`VignetteCommandHandler` (`[vignette:state=T, time=0.5, intensity=0.8]`)**:
  - URP（Universal Render Pipeline）の `Volume` からビネット演出をアニメーション制御するコマンドハンドラーを追加。
- **外部入力待ちロック機能 (`DialogueManager.SetExternalInputLock(bool)`)**:
  - 外部演出や入力待ち中にプレイヤーの会話送りキー（Fキー・Space等）やAuto/Skipを安全に一時停止できる機能を追加。

### Changed (仕様変更・改善)
- **`DialogueInputHandler`**:
  - **New Input Systemへの一本化**: 旧 `UnityEngine.Input`（`GetKeyDown` 等）の呼び出しを完全撤廃し、`Keyboard.current` と `Mouse.current` に統一。これにより、Player Settings の `Active Input Handling` が `Input System Package (New)` の環境でも `InvalidOperationException` が発生しないよう修正。
  - **会話開始時クールダウン**: 会話開始直後（Idleからの復帰時）に0.2秒のクールダウンを設け、開始ボタンの連打・長押しによるセリフスキップ（誤爆送り）を防止。
  - **Fキー送り対応**: New Input System経由でFキー（`Keyboard.current.fKey`）での会話送りに標準対応。
- **`DialogueManager.EndDialogue()`**:
  - 外部スクリプトやカスタムコマンドから呼び出し可能になるよう、アクセス修飾子を `private` から `public` に変更。
  - 会話終了時に外部入力待ちフラグを自動リセットする安全処理を追加。
- **`GenericCallCommandHandler`**:
  - **Unity 6 API対応**: 非推奨警告（CS0618）が出ていた `FindObjectsOfType<MonoBehaviour>()` および `FindObjectsOfType<DialogueCustomTag>()` を Unity 6 推奨の `FindObjectsByType<T>(FindObjectsSortMode.None)` に置換。

### Fixed (バグ修正)
- **`FadeOutCommandHandler`**:
  - フェードアウト開始時にパネルの透明度（alpha）が0に初期化されないままアクティブ化されていたため、前回の暗転が残って一瞬黒くチラつく不具合を修正。

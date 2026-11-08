# Changelog

All notable changes to the DialogueSystem package/repository will be documented in this file.

## [Unreleased] - 2026-11-07

### Added (新規追加機能)
- **`DialogueFontDatabase` (フォントスタイルデータベース機能)**:
  - 複数のフォントスタイル（`default`, `horror`, `robot`, `pixel` 等）を1つの ScriptableObject にまとめて管理するデータベースを新設。
  - 右クリックメニュー `Create` > `Dialogue` > `Font Database` から作成可能。
  - 多言語切り替え時や Addressables による言語別フォントアセット分割ロードに対応。
- **`CharacterProfile` への標準フォント設定 (`defaultFontStyle`)**:
  - キャラクターごとに「話す時の標準フォントスタイル（例: robot, horror）」を指定可能に。空欄時は全体デフォルトフォントを自動適用。
- **`DialogueViewWindow` の自動フォント適用と安全復帰**:
  - 各セリフ（ノード）開始時に、話者のキャラクター設定に応じたフォントを自動適用。
  - 一時的な演出フォントが使われた場合でも、次のセリフに進んだ瞬間に自動的にキャラ標準フォントへ安全復帰するライフサイクルを実装。
- **`FontCommandHandler` (`[font:style=...]`)**:
  - セリフ途中でフォントスタイルを一時的（または永続的）に切り替える演出コマンドを新設。
  - `[font:style=horror]`（そのセリフ限定）、`[font:style=robot, keep=true]`（以降のセリフも維持）、`[font:reset]`（手動復帰）に対応。
- **非破壊ローカライズ拡張インターフェース (`IDialogueTextResolver`)**:
  - `DialogueCore` を一切破壊せず、Addressables や外部多言語パッケージの辞書を差し込めるプロバイダーインターフェースを新設。
  - `DialogueManager.TextResolver` に登録するだけで、セリフ本文・話者名・選択肢テキストが自動的に多言語解決される。未設定時は通常通りノード記載のテキストがそのまま使われる完全非破壊動作。

---

## [1.2.0] - 2026-11-05

### Added (新規追加機能)
- **`DialoguePortraitDatabase` (立ち絵データベース機能)**:
  - SE（`DialogueAudioDatabase`）と同様に、複数の `CharacterProfile`（キャラクター立ち絵プロファイル）を1つの ScriptableObject にまとめて管理できるデータベース機能を新設。
  - 右クリックメニュー `Create` > `Dialogue` > `Portrait Database` から新規作成可能。
  - `StandardUIPortraitHandler` に `portraitDatabases` リストを追加。インスペクターに1つデータベースをセットするだけで、登録された全キャラクターの立ち絵・表情・AnimatorControllerが利用可能に。
  - 従来の `profiles`（個別登録リスト）もフォールバックとして完全維持（既存シーンやプレハブの互換性を保持）。
  - SEと同様に、スクリプトから章ごと・シーンごとに動的ロードできる `AddDatabase(db, highPriority)`, `RemoveDatabase(db)`, `ClearDatabases()` を実装（将来の Addressables 動的ロードにも対応）。
- **`PortraitCommandBuilderWindow` のデータベース対応と表情選択強化**:
  - `handler.GetAllProfiles()` を通じて、データベース登録キャラクターと個別登録キャラクターの両方を自動収集しドロップダウンに一覧表示。
  - キャラクター選択時に、そのキャラクターが持つ表情リスト（`expressions`）をドロップダウン（Popup）で選択可能に改善。スペルミスを防止。

---

## [1.1.0] - 2026-11-02

### Added (新規追加機能)
- **`GenericCallCommandHandler` の段階別詳細エラーフィードバック機能**:
  - コール失敗時に「どの段階でなぜ失敗したか」を原因別に明確に切り分けてUnityコンソールに出力する診断ロジックを追加。
    1. `[対象オブジェクト未検出]`: 指定された相手がシーン内に見つからない理由
    2. `[スクリプト未検出]`: 対象にアタッチされているスクリプト一覧と要求スクリプトを比較表示
    3. `[メソッド未検出]`: 対象クラスに定義されている公開メソッド一覧と要求メソッドを比較表示
    4. `[引数の不適合 / オーバーロード解決失敗]`: 定義されているオーバーロード一覧・シグネチャと渡された引数を比較表示
    5. `[呼び出し先メソッド内部で例外が発生]`: 実行先メソッドで発生した例外の種類・メッセージ・スタックトレースを表示
- **`GenericCallCommandHandler` (`[call:...]`) の話しかけているオブジェクト自動指定モード (`mode=target` / `mode=current`)**:
  - 会話相手（NPCやインタラクト対象）の GameObject を自動判別してメソッドを実行するモードを追加。
  - 多段階フォールバックによる確実な対象解決：
    1. `DialogueManager.Instance.CurrentTarget`（会話開始時に渡されたオブジェクト）
    2. 現在のノードの話者名（`CurrentSpeaker`）と同名の GameObject（非アクティブ含む）
    3. 該当スクリプトを持つ基準位置からの最近接オブジェクト
  - `mode` を省略した場合でも、`target` や `tag` パラメータがなければデフォルトでこの `target` モードとして動作。
- **`GenericCallCommandHandler` の `HandlerInfo` ドキュメント大幅拡充**:
  - エディタウィンドウ（`Tools/Dialogue/Handler Info Window`）で表示される使用方法を大幅に詳細化。パラメータ一覧、各モードの挙動、使用例、エラーの見方を体系的に記載。
- **`DialogueManager` 会話対象の保持 (`CurrentTarget`)**:
  - 現在会話している対象オブジェクトを保持する `CurrentTarget` プロパティを追加。
  - 会話開始時に話しかけた対象を直接渡せる `StartDialogue(container, target, onEnded)` オーバーロードおよび `SetCurrentTarget(target)` を追加。
  - 会話終了時（`EndDialogue`）に自動的に `CurrentTarget = null` にリセット。

### Changed (仕様変更・改善)
- **`GenericCallCommandHandler` におけるオーバーロード対応と引数型変換**:
  - 同名・引数違いのメソッドが定義されているクラス（例: `SceneLoader.LoadScene()` と `SceneLoader.LoadScene(string)`）を呼び出した際に発生していたリフレクション例外 `AmbiguousMatchException` を完全解消。
  - `arg` の有無および型（`string`, `int`, `float`, `bool`）に合わせて最適なメソッドオーバーロードを自動探索・安全実行するように改善。
  - 引数未指定時は引数なしメソッド（引数0個）を優先呼び出し、引数指定時は引数1個のメソッドを優先呼び出し。

---

## [1.0.0] - 2026-09-28

### Added (新規追加機能)
- **`DialogueManager` における `IDialogueView`（`DialogueViewWindow`）の自動取得・解決機能**:
  - `DialogueManager.CurrentView` へのアクセス時や `StartDialogue` 呼び出し時に、Viewが未登録であれば自動的に解決する `ResolveDefaultView()` を追加。
  - Inspector での直接指定（`defaultView`）に加え、シーン内からの検索（非アクティブ状態のUIも含む `FindObjectsInactive.Include`）、および `IDialogueView` 実装コンポーネントの網羅的探索をサポート。
  - これにより、手動で `RegisterView()` を呼ばなくても、プレハブ配置またはシーン配置のみで即座に会話を開始可能に。
  - `Dialogue.prefab` の `DialogueManager` に `defaultView` の参照を初期設定。
- **`DialogueViewWindow.Start()` フォールバック登録**:
  - `Awake` の実行順序により `DialogueManager.Instance` が未生成だった場合でも、`Start` のタイミングで自動的に `DialogueManager` へ自己登録する安全フォールバックを追加。
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

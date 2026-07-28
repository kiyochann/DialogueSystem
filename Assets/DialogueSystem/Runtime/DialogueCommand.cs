using System.Collections.Generic;

namespace Runtime.Dialogue
{
    /// <summary>
    /// システム内部で統一して扱われる共通のイベント命令書(コマンドオブジェクト)
    /// </summary>
    public class DialogueCommand
    {
        /// <summary>実行したいコマンド名(例:"fade_out", "se"など)</summary>
        public string CommandName { get; private set; }

        /// <summary>コマンドに渡す引数(パラメータ)の辞書</summary>
        public Dictionary<string, string> Arguments { get; private set; }

        /// <summary>このコマンドがテキストの「何文字目」の後に発火するか(インラインタグ用。ノードイベントなら-1)</summary>
        public int CharacterIndex { get; set; }

        /// <summary>この演出が実行済みかどうか</summary>
        public bool IsExecuted { get; set; }

        public DialogueCommand(string commandName, Dictionary<string, string> arguments, int characterIndex = -1)
        {
            CommandName = commandName.ToLower().Trim();
            Arguments = arguments ?? new Dictionary<string, string>();
            CharacterIndex = characterIndex;
            IsExecuted = false;
        }
    }
}

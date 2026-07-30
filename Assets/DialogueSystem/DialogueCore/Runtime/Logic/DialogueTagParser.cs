using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Runtime.Dialogue
{
    /// <summary>
    /// 生テキストから[コマンド:キー=値]形式のタグを解析して分離する文字列パースクラス
    /// </summary>
    public static class DialogueTagParser
    {
        // 独自タグ[コマンド名:パラメータ]を検出するための正規表現パターン
        private static readonly Regex TagRegex = new Regex(@"\[([^\]\s:]+)(?::([^\]]+))?\]");

        /// <summary>
        /// 生テキストを解析し、純粋な表示用文字列とコマンドのリストに分離して返す
        /// </summary>
        /// <param name="rawText">解析前のテキスト(例:"これ[se:clip=ping]やで")</param>
        /// <param name="cleanText">出力:タグがきれいに消去された表示用テキスト</param>
        /// <param name="commands">出力:何文字目で発火するかが記録されたコマンドのリスト</param>
        public static void ParseText(string rawText, out string cleanText, out List<DialogueCommand> commands)
        {
            commands = new List<DialogueCommand>();

            if (string.IsNullOrEmpty(rawText))
            {
                cleanText = string.Empty;
                return;
            }

            // 正規表現のマッチング結果を一通り取得
            MatchCollection matches = TagRegex.Matches(rawText);

            int charOffset = 0; // タグを消去したことによってずれる文字列の累積
            cleanText = rawText;

            foreach (Match match in matches)
            {
                string cmdName = match.Groups[1].Value;
                string paramStr = match.Groups[2].Value;

                // パラメータ文字列(例:"time=1.5,type=out"を分解して辞書にする)
                var argsDict = ParseArguments(paramStr);

                // タグが「表示文字列の何文字目」に位置するかを計算
                int characterIndex = match.Index - charOffset;

                // 共通形式のコマンドオブジェクトを生成してリストへ追加
                DialogueCommand cmd = new DialogueCommand(cmdName, argsDict, characterIndex);
                commands.Add(cmd);

                // 文字列からタグの部分を消去する
                cleanText = cleanText.Remove(match.Index - charOffset, match.Length);

                // 消去したタグの長さ分、以降の文字列インデックスを前にずらす
                charOffset += match.Length;
            }
        }

        /// <summary>
        /// "time=1.5,clip=bomb" のようなカンマ・イコール区切りの文字列を辞書型に分解する
        /// </summary>
        private static Dictionary<string, string> ParseArguments(string paramStr)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(paramStr)) return dict;

            string[] pairs = paramStr.Split(',');
            foreach (string pair in pairs)
            {
                string[] kv = pair.Split('=');
                if (kv.Length == 2)
                {
                    string key = kv[0].Trim().ToLower();
                    string value = kv[1].Trim();
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key, value);
                    }
                }
                else if (kv.Length == 1)
                {
                    // パラメータが値だけの場合(例:[se:bomb])は、キーを"default"として収納
                    string value = kv[0].Trim();
                    if (!dict.ContainsKey("default"))
                    {
                        dict.Add("default", value);
                    }
                }
            }
            return dict;
        }
    }
}
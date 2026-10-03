using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public sealed class MatchLogView : MonoBehaviour
    {
        [SerializeField] private Text logText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Color systemColor = new Color32(170, 178, 172, 255);
        [SerializeField] private Color monsterColor = new Color32(235, 235, 235, 255);
        [SerializeField] private Color playerColor = new Color32(200, 230, 255, 255);
        [SerializeField] private Color resultColor = new Color32(255, 196, 150, 255);
        [SerializeField] private Color latestColor = new Color32(255, 225, 140, 255);

        private MatchLog _log;

        public string LatestText { get; private set; } = string.Empty;
        public int LineCount { get; private set; }

        public void Configure(Text text, ScrollRect scroll)
        {
            logText = text;
            scrollRect = scroll;
        }

        public void Bind(MatchLog log)
        {
            if (_log != null)
                _log.Changed -= Render;
            _log = log;
            _log.Changed += Render;
            Render();
        }

        private void OnDestroy()
        {
            if (_log != null)
                _log.Changed -= Render;
        }

        private void Render()
        {
            var builder = new StringBuilder();
            var entries = _log.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var color = i == entries.Count - 1 ? latestColor : ColorFor(entries[i].Kind);
                if (builder.Length > 0)
                    builder.Append('\n');
                builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>')
                    .Append(entries[i].Text).Append("</color>");
            }

            logText.text = builder.ToString();
            LineCount = entries.Count;
            LatestText = entries.Count > 0 ? entries[entries.Count - 1].Text : string.Empty;

            if (scrollRect == null)
                return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(logText.rectTransform);
            scrollRect.verticalNormalizedPosition = 0f;
        }

        private Color ColorFor(LogKind kind)
        {
            switch (kind)
            {
                case LogKind.MonsterSpeech:
                    return monsterColor;
                case LogKind.PlayerAction:
                    return playerColor;
                case LogKind.Result:
                    return resultColor;
                default:
                    return systemColor;
            }
        }
    }
}

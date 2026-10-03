using System;
using System.Collections.Generic;

namespace FGJ.LiarDice.UI
{
    public enum LogKind
    {
        System,
        MonsterSpeech,
        PlayerAction,
        Result
    }

    public readonly struct LogEntry
    {
        public readonly string Text;
        public readonly LogKind Kind;

        public LogEntry(string text, LogKind kind)
        {
            Text = text;
            Kind = kind;
        }
    }

    public sealed class MatchLog
    {
        private readonly List<LogEntry> _entries = new List<LogEntry>();
        private readonly int _capacity;

        public event Action Changed;

        public IReadOnlyList<LogEntry> Entries => _entries;

        public MatchLog(int capacity = 60)
        {
            _capacity = Math.Max(1, capacity);
        }

        public void Add(string text, LogKind kind)
        {
            _entries.Add(new LogEntry(text, kind));
            if (_entries.Count > _capacity)
                _entries.RemoveRange(0, _entries.Count - _capacity);
            Changed?.Invoke();
        }

        public void Clear()
        {
            _entries.Clear();
            Changed?.Invoke();
        }
    }
}

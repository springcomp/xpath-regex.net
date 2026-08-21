namespace XPath.Regex.NET.Internal.Matcher;

using System.Collections.Generic;
using XPath.Regex.NET.Internal;

internal sealed class ThreadList
{
    private readonly List<Thread> _threads;
    private readonly HashSet<int> _visitedPcs;
    private readonly HashSet<ThreadState> _visitedStates;
    private readonly bool _captureAware;
    private readonly int _captureSlotCount;

    public ThreadList(int programLength, bool captureAware, int captureSlotCount)
    {
        _threads = new List<Thread>(programLength);
        _visitedPcs = new HashSet<int>(programLength);
        _visitedStates = new HashSet<ThreadState>();
        _captureAware = captureAware;
        _captureSlotCount = captureSlotCount;
    }

    public IReadOnlyList<Thread> Threads => _threads;

    public void Clear()
    {
        _threads.Clear();
        _visitedPcs.Clear();
        _visitedStates.Clear();
    }

    /// <summary>
    /// Adds the thread if its execution state has not been seen yet in this list.
    /// First thread reaching an equivalent state wins (preserves leftmost-first priority).
    /// </summary>
    public bool TryAdd(Thread thread)
    {
        if (thread.Pc < 0)
            return false;

        if (_captureAware)
        {
            if (!_visitedStates.Add(new ThreadState(thread, _captureSlotCount)))
                return false;
        }
        else if (!_visitedPcs.Add(thread.Pc))
        {
            return false;
        }

        _threads.Add(thread);
#if REGEX_INSTRUMENTATION
    VmMetrics? metrics = VmInstrumentation.Current;
    if (metrics is not null)
    {
      metrics.ThreadsAdded++;
      if (_threads.Count > metrics.MaxThreadListSize)
        metrics.MaxThreadListSize = _threads.Count;
    }
#endif
        return true;
    }

    private readonly struct ThreadState : IEquatable<ThreadState>
    {
        private readonly int _pc;
        private readonly int _position;
        private readonly int[] _captures;
        private readonly int _hashCode;

        public ThreadState(Thread thread, int captureSlotCount)
        {
            _pc = thread.Pc;
            _position = thread.Position;
            _captures = new int[captureSlotCount];
            Array.Copy(thread.Captures, _captures, captureSlotCount);

            var hash = new HashCode();
            hash.Add(_pc);
            hash.Add(_position);
            foreach (int capture in _captures)
                hash.Add(capture);
            _hashCode = hash.ToHashCode();
        }

        public bool Equals(ThreadState other)
        {
            if (_pc != other._pc || _position != other._position || _captures.Length != other._captures.Length)
                return false;

            for (int i = 0; i < _captures.Length; i++)
            {
                if (_captures[i] != other._captures[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj) => obj is ThreadState other && Equals(other);

        public override int GetHashCode() => _hashCode;
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Nonno.Junction;

public class ManagementRelay
{
    readonly int _margin;
    readonly int _extensionExponent;
    readonly object _lock = new();
    object?[] _a;
    int[] _q;
    int _s;
    int _e;

    public object? this[int index]
    {
        get
        {
            if (unchecked((uint)index) < (uint)_a.Length) return _a[index];
            return null;
        }
    }

    public ManagementRelay(int margin = 17, int extensionExponent = 1)
    {
        if (margin <= 0) throw new ArgumentOutOfRangeException(nameof(margin));
        if (extensionExponent <= 0) throw new ArgumentOutOfRangeException(nameof(extensionExponent));

        int margin_sq = 1;
        while (margin_sq < margin) margin_sq <<= 1;
        _a = new object[margin_sq];
        _q = new int[margin_sq];
        for (int i = 0; i < _q.Length; i++) _q[i] = i;

        _margin = margin;
        _extensionExponent = extensionExponent;
    }

    public int Open(object obj)
    {
        if (_e - _s <= _margin) Extend();

        var s = Interlocked.Increment(ref _s);
        var mask = _q.Length - 1;
        var index = _q[(s - 1) & mask];
        _a[index] = obj;
        return index;
    }

    public object Close(int index)
    {
        var e = Interlocked.Increment(ref _e);
        var mask = _q.Length - 1;
        _q[e & mask] = index;
        Debug.Assert(_a[index] is not null);
        return _a[index]!;
    }

    void Extend()
    {
        lock (_lock)
        {
            var l = _a.Length;
            var neo_l = l << _extensionExponent;
            var neo_a = new object[neo_l];
            var neo_q = new int[neo_l];

            Array.Copy(_a, neo_a, l);
            Array.Copy(_q, 0, neo_q, 0, l);
            Array.Copy(_q, 0, neo_q, l, l);

            _a = neo_a;
            _q = neo_q;

            var ep = Interlocked.Add(ref _e, l);
            var c = l;
            var mask = _q.Length - 1;
            for (int i = ep - l; i < ep; i++) _q[i & mask] = c++;
        }
    }
}

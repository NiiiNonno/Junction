using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Nonno.Junction;
public readonly struct UnmanagedPointer
{
    readonly nint _v;

    public ManagementType ManagementType => _v switch
    {
        > 0 => ManagementType.Unmanaged,
        0 => ManagementType.Null,
        < 0 => ManagementType.Managed,
    };

    UnmanagedPointer(nint v)
    {
        _v = v;
    }

    public object? Object => _v < 0 ? _relay[(int)~_v] : null;

    public static implicit operator UnmanagedPointer(nint p)
    {
        if (p < 0) throw new NotSupportedException();
        return new UnmanagedPointer(p);
    }
    public static explicit operator nint(UnmanagedPointer p)
    {
        Debug.Assert(p.ManagementType != ManagementType.Managed);
        return p._v;
    }

    public static UnmanagedPointer New(object obj)
    {
        return new(~_relay.Open(obj));
    }
    public static UnmanagedPointer New(int length)
    {
        return new(Marshal.AllocHGlobal(length));
    }

    public static object? Delete(UnmanagedPointer ptr)
    {
        if (ptr._v >= 0) Marshal.FreeHGlobal(ptr._v);
        return _relay.Close((int)~ptr._v);
    }

    readonly static ManagementRelay _relay = new();
}

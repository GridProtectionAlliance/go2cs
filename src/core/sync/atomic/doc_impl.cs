using System.Threading;

namespace go.sync;

using @unsafe = unsafe_package;

partial class atomic_package
{
    public static partial int32 /*old*/ SwapInt32(ж<int32> addr, int32 @new)
    {
        return Interlocked.Exchange(ref addr.val, @new);
    }

    public static partial int64 /*old*/ SwapInt64(ж<int64> addr, int64 @new)
    {
        return Interlocked.Exchange(ref addr.val, @new);
    }

    public static partial uint32 /*old*/ SwapUint32(ж<uint32> addr, uint32 @new)
    {
        return Interlocked.Exchange(ref addr.val, @new);
    }

    public static partial uint64 /*old*/ SwapUint64(ж<uint64> addr, uint64 @new)
    {
        return Interlocked.Exchange(ref addr.val, @new);
    }

    public static partial uintptr /*old*/ SwapUintptr(ж<uintptr> addr, uintptr @new)
    {
        return Interlocked.Exchange(ref addr.val, @new);
    }

    public static partial @unsafe.Pointer /*old*/ SwapPointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer @new)
    {
        return Interlocked.Exchange(ref addr.val, @new);
    }

    public static partial bool /*swapped*/ CompareAndSwapInt32(ж<int32> addr, int32 old, int32 @new)
    {
        return Interlocked.CompareExchange(ref addr.val, @new, old) != old;
    }

    public static partial bool /*swapped*/ CompareAndSwapInt64(ж<int64> addr, int64 old, int64 @new)
    {
        return Interlocked.CompareExchange(ref addr.val, @new, old) != old;
    }

    public static partial bool /*swapped*/ CompareAndSwapUint32(ж<uint32> addr, uint32 old, uint32 @new)
    {
        return Interlocked.CompareExchange(ref addr.val, @new, old) != old;
    }

    public static partial bool /*swapped*/ CompareAndSwapUint64(ж<uint64> addr, uint64 old, uint64 @new)
    {
        return Interlocked.CompareExchange(ref addr.val, @new, old) != old;
    }

    public static partial bool /*swapped*/ CompareAndSwapUintptr(ж<uintptr> addr, uintptr old, uintptr @new)
    {
        return Interlocked.CompareExchange(ref addr.val, @new, old) != old;
    }

    public static partial bool /*swapped*/ CompareAndSwapPointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer old, @unsafe.Pointer @new)
    {
        return Interlocked.CompareExchange(ref addr.val, @new, old) != old;
    }

    public static partial int32 /*new*/ AddInt32(ж<int32> addr, int32 delta)
    {
        return Interlocked.Add(ref addr.val, delta);
    }

    public static partial uint32 /*new*/ AddUint32(ж<uint32> addr, uint32 delta)
    {
        return Interlocked.Add(ref addr.val, delta);
    }

    public static partial int64 /*new*/ AddInt64(ж<int64> addr, int64 delta)
    {
        return Interlocked.Add(ref addr.val, delta);
    }

    public static partial uint64 /*new*/ AddUint64(ж<uint64> addr, uint64 delta)
    {
        return Interlocked.Add(ref addr.val, delta);
    }

    public static partial uintptr /*new*/ AddUintptr(ж<uintptr> addr, uintptr delta)
    {
        nuint initialValue, newValue;

        do
        {
            initialValue = Volatile.Read(ref addr.val);
            newValue = initialValue + delta;
        }
        while (Interlocked.CompareExchange(ref addr.val, newValue, initialValue) != initialValue);

        return newValue;
    }

    public static partial int32 /*old*/ AndInt32(ж<int32> addr, int32 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial uint32 /*old*/ AndUint32(ж<uint32> addr, uint32 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial int64 /*old*/ AndInt64(ж<int64> addr, int64 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial uint64 /*old*/ AndUint64(ж<uint64> addr, uint64 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial uintptr /*old*/ AndUintptr(ж<uintptr> addr, uintptr mask)
    {
        nuint initialValue, newValue;

        do
        {
            initialValue = Volatile.Read(ref addr.val);
            newValue = initialValue & mask;
        }
        while (Interlocked.CompareExchange(ref addr.val, newValue, initialValue) != initialValue);

        return newValue;
    }

    public static partial int32 /*old*/ OrInt32(ж<int32> addr, int32 mask)
    {
        return Interlocked.Or(ref addr.val, mask);
    }

    public static partial uint32 /*old*/ OrUint32(ж<uint32> addr, uint32 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial int64 /*old*/ OrInt64(ж<int64> addr, int64 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial uint64 /*old*/ OrUint64(ж<uint64> addr, uint64 mask)
    {
        return Interlocked.And(ref addr.val, mask);
    }

    public static partial uintptr /*old*/ OrUintptr(ж<uintptr> addr, uintptr mask)
    {
        nuint initialValue, newValue;

        do
        {
            initialValue = Volatile.Read(ref addr.val);
            newValue = initialValue | mask;
        }
        while (Interlocked.CompareExchange(ref addr.val, newValue, initialValue) != initialValue);

        return newValue;
    }

    public static partial int32 /*val*/ LoadInt32(ж<int32> addr)
    {
        return Volatile.Read(ref addr.val);
    }

    public static partial int64 /*val*/ LoadInt64(ж<int64> addr)
    {
        return Volatile.Read(ref addr.val);
    }

    public static partial uint32 /*val*/ LoadUint32(ж<uint32> addr)
    {
        return Volatile.Read(ref addr.val);
    }

    public static partial uint64 /*val*/ LoadUint64(ж<uint64> addr)
    {
        return Volatile.Read(ref addr.val);
    }

    public static partial uintptr /*val*/ LoadUintptr(ж<uintptr> addr)
    {
        return Volatile.Read(ref addr.val);
    }

    public static partial @unsafe.Pointer /*val*/ LoadPointer(ж<@unsafe.Pointer> addr)
    {
        return Volatile.Read(ref addr.val);
    }

    public static partial void StoreInt32(ж<int32> addr, int32 val)
    {
        Interlocked.Exchange(ref addr.val, val);
    }

    public static partial void StoreInt64(ж<int64> addr, int64 val)
    {
        Interlocked.Exchange(ref addr.val, val);
    }

    public static partial void StoreUint32(ж<uint32> addr, uint32 val)
    {
        Interlocked.Exchange(ref addr.val, val);
    }

    public static partial void StoreUint64(ж<uint64> addr, uint64 val)
    {
        Interlocked.Exchange(ref addr.val, val);
    }

    public static partial void StoreUintptr(ж<uintptr> addr, uintptr val)
    {
        Interlocked.Exchange(ref addr.val, val);
    }

    public static partial void StorePointer(ж<@unsafe.Pointer> addr, @unsafe.Pointer val)
    {
        Interlocked.Exchange(ref addr.val, val);
    }
} // end atomic_package

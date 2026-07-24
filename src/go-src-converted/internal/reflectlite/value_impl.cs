// value_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go.@internal;

partial class reflectlite_package
{
    // Implementation of ifaceE2I
    internal static partial void ifaceE2I(ж<abi_package.Type> t, any src, unsafe_package.Pointer dst)
    {
    }

    // Implementation of typedmemmove
    internal static partial void typedmemmove(ж<abi_package.Type> t, unsafe_package.Pointer dst, unsafe_package.Pointer src)
    {
    }

    // Implementation of unsafe_New
    internal static partial unsafe_package.Pointer unsafe_New(ж<abi_package.Type> _)
    {
        return default!;
    }

    // Implementation of chanlen
    internal static partial nint chanlen(unsafe_package.Pointer _)
    {
        return default;
    }

    // Implementation of maplen
    internal static partial nint maplen(unsafe_package.Pointer _)
    {
        return default;
    }

    // ---- The reflection bridge (Phase 4) — the mini-surface sort.Slice exercises. ----
    //
    // Go's reflectlite reads an interface's eface {type,data} words through unsafe.Pointer; an
    // `any` here is a single System.Object reference, so the auto unpackEface reinterprets the
    // reference and derefs a nil ж<abi.Type> (sort.Slice → ValueOf/Swapper was the first
    // operational hit, sort's TestSlice). Mirror reflect's bridge (reflect/value_impl.cs): the
    // Value carries the boxed managed value DIRECTLY on a companion field; typ_ takes the
    // Phase-1 synthetic abi.Type and the flag takes the Kind bits, so Kind()/IsValid() keep
    // working from value.cs unchanged. The converter skips the auto forms via the
    // manualConversionFuncs registry (go2cs/manualTypeOperations.go). See
    // docs/Phase4/DESIGN-reflection-bridge.md.

    // The managed backing for a Value: the boxed managed value this Value represents (null for
    // the zero Value), plus, when ADDRESSABLE (flagAddr), the ж<T> box it ALIASES — reads go
    // through the box lazily and Set writes through its slot ref (mirrors reflect's bridge; see
    // reflect/value_impl.cs).
    partial struct Value
    {
        internal object? boxed;
        internal object? addrBox;

        // The LIVE value this Value represents (read-through for an addressable Value).
        internal object? live => addrBox is null ? boxed : GoReflect.ReadPointerSlot(addrBox);
    }

    // makeReflectValue builds a Value carrying a boxed managed value. typ_ is the Phase-1
    // synthetic abi.Type (Kind_ classified from the value's System.Type); the flag holds the
    // Kind so Kind()/IsValid() resolve from value.cs unchanged.
    internal static Value makeReflectValue(object? boxed)
    {
        if (boxed is null)
            return new Value(nil);

        ж<abi_package.Type> t = abi_package.TypeOf(boxed);
        Value v = new Value(t, default!, (flag)(uintptr)(uint8)GoReflect.KindOf(GoReflect.GoDynamicTypeOf(boxed)));

        v.boxed = boxed;

        return v;
    }

    // ValueOf returns a new Value initialized to the concrete value stored in the interface i.
    // ValueOf(nil) returns the zero Value.
    public static Value ValueOf(any i)
    {
        return i == default! ? new Value(nil) : makeReflectValue(i);
    }

    // unpackEface converts the empty interface i to a Value.
    internal static Value unpackEface(any i)
    {
        return ValueOf(i);
    }

    // Len returns v's length (v must be an Array, Chan, Map, Slice, or String).
    public static nint Len(this Value v)
    {
        return v.live switch
        {
            @string s => s.Length,
            IArray a => a.Length,
            IMap m => m.Length,
            _ => 0
        };
    }

    // ==== Phase-3 write-back: the errors.As surface (Elem, IsNil, Set, methodName) ====
    // Mirrors reflect's bridge (reflect/value_impl.cs) over the same golib machinery. The auto
    // forms read the never-populated v.ptr eface word — IsNil answered TRUE for every pointer
    // (errors.As panicked "target must be a non-nil pointer" for every valid target) and Elem
    // returned the invalid zero Value.

    // Elem returns the value the interface v contains or that the pointer v points to; the
    // pointer form is ADDRESSABLE, aliasing the receiver box (Go: the returned value's address
    // is v's value). A structurally nil pointer yields the invalid zero Value.
    public static Value Elem(this Value v)
    {
        abiꓸKind k = v.kind();

        if (k == abi_package.Interface)
            return makeReflectValue(v.live);

        if (k == abi_package.Pointer)
        {
            object? cur = v.live;

            while (cur is IInterfaceAdapter { Value: not null } interfaceAdapter)
                cur = interfaceAdapter.Value;

            if (cur is IжAdapter { Box: not null } pointerAdapter)
                cur = pointerAdapter.Box;

            if (cur is null || (cur is INilPointer nilable && nilable.IsNilPointer))
                return new Value(nil);

            System.Type? pointee = GoReflect.ElementType(cur.GetType());

            if (pointee is null)
                return makeReflectValue(GoReflect.ReadPointerSlot(cur));

            ж<abi_package.Type> t = abi_package.synthType(pointee);
            Value elem = new(t, default!, ((flag)(uintptr)(uint8)GoReflect.KindOf(pointee)) | flagAddr | flagIndir);

            elem.addrBox = cur;

            return elem;
        }

        throw panic(Ꮡ(new ValueError("reflectlite.Value.Elem", v.kind())));
    }

    // IsNil reports whether its argument v is nil — STRUCTURAL nil for pointers (INilPointer):
    // a heap box holding a nil value is a non-nil pointer holding nil.
    public static bool IsNil(this Value v)
    {
        object? cur = v.live;

        while (cur is IInterfaceAdapter { Value: not null } interfaceAdapter)
            cur = interfaceAdapter.Value;

        if (cur is IжAdapter { Box: not null } pointerAdapter)
            cur = pointerAdapter.Box;

        return cur switch
        {
            null => true,
            INilPointer nilable => nilable.IsNilPointer,
            IMap m => m.IsNil,
            _ => false
        };
    }

    // Set assigns x to the value v (Go's assignTo semantics; shared golib marshalling — see
    // reflect's Set). The store writes through the aliased box's slot ref; a structurally nil
    // box panics Go-style before any write.
    public static void Set(this Value v, Value x)
    {
        v.flag.mustBeAssignable();
        x.flag.mustBeExported();

        System.Type? dstType = v.typ_ == nil ? null : v.typ_.Value.sysType;

        if (dstType is null || v.addrBox is null)
            throw panic("reflectlite.Set using unaddressable value");

        if (!GoReflect.TryMarshalAssignable(x.live, dstType, out object? marshalled))
        {
            throw panic("reflectlite.Set: value of type " + GoReflect.GoTypeName(x.live?.GetType()) +
                        " is not assignable to type " + GoReflect.GoTypeName(dstType));
        }

        GoReflect.WritePointerSlot(v.addrBox, marshalled);
    }

    // methodName returns a best-effort Go-shaped name of the calling reflectlite method for
    // panic messages — runtime.Caller has no managed form; walk the managed stack instead. The
    // name is only ever observed in panic text.
    internal static @string methodName()
    {
        System.Diagnostics.StackTrace trace = new(2, false);

        for (int i = 0; i < trace.FrameCount; i++)
        {
            System.Reflection.MethodBase? method = trace.GetFrame(i)?.GetMethod();
            System.Type? decl = method?.DeclaringType;

            if (method is null || decl is null)
                continue;

            if (decl.Name.EndsWith("_package") && !method.Name.StartsWith("mustBe"))
                return (@string)(decl.Name[..^"_package".Length] + "." + method.Name);
        }

        return "unknown method"u8;
    }
}

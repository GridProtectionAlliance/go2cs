//******************************************************************************************************
//  value_impl.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/19/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

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
    // the zero Value).
    partial struct Value
    {
        internal object? boxed;
    }

    // makeReflectValue builds a Value carrying a boxed managed value. typ_ is the Phase-1
    // synthetic abi.Type (Kind_ classified from the value's System.Type); the flag holds the
    // Kind so Kind()/IsValid() resolve from value.cs unchanged.
    internal static Value makeReflectValue(object? boxed)
    {
        if (boxed is null)
            return new Value(nil);

        ж<abi_package.Type> t = abi_package.TypeOf(boxed);
        Value v = new Value(t, default!, (flag)(uintptr)(uint8)GoReflect.KindOf(boxed.GetType()));

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
        return v.boxed switch
        {
            @string s => s.Length,
            IArray a => a.Length,
            IMap m => m.Length,
            _ => 0
        };
    }
}

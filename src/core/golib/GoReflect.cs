//******************************************************************************************************
//  GoReflect.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using static go2cs.Symbols;

namespace go;

/// <summary>
/// Managed backing for the native reflection bridge (Phase 4). Go's <c>reflect</c>/<c>internal/abi</c>
/// read an interface's <c>{type,data}</c> words through <c>unsafe.Pointer</c> to reach a runtime type
/// descriptor that has no managed form; this helper reconstructs the Go <c>reflect.Kind</c> of a value
/// from its managed <see cref="Type"/>, and provides a descriptor↔<see cref="Type"/> side table so the
/// hand-owned <c>abi.TypeOf</c> entry point can carry the real managed type on the (otherwise synthetic)
/// <c>abi.Type</c> box. See <c>docs/Phase4/DESIGN-reflection-bridge.md</c>.
/// </summary>
public static class GoReflect
{
    // Go reflect.Kind numbering (internal/abi and reflect define these identically, 0..26).
    public const int Invalid = 0, Bool = 1, Int = 2, Int8 = 3, Int16 = 4, Int32 = 5, Int64 = 6;
    public const int Uint = 7, Uint8 = 8, Uint16 = 9, Uint32 = 10, Uint64 = 11, Uintptr = 12;
    public const int Float32 = 13, Float64 = 14, Complex64 = 15, Complex128 = 16;
    public const int Array = 17, Chan = 18, Func = 19, Interface = 20, Map = 21, Pointer = 22;
    public const int Slice = 23, String = 24, Struct = 25, UnsafePointer = 26;

    // Maps each synthetic abi.Type descriptor box to the managed Type it stands for, so the hand-owned
    // reflect Type/Value methods (String/Name/Elem/Field/...) can recover Go type info from System.Type.
    // Keyed on the box object identity; weak so descriptors are not pinned.
    private static readonly ConditionalWeakTable<object, Type> s_sysTypes = new();

    /// <summary>Records the managed <see cref="Type"/> that a synthetic abi.Type descriptor box stands for.</summary>
    public static void Register(object descriptorBox, Type sysType)
    {
        if (descriptorBox is null || sysType is null)
            return;

        s_sysTypes.AddOrUpdate(descriptorBox, sysType);
    }

    /// <summary>Recovers the managed <see cref="Type"/> a descriptor box stands for, or <c>null</c>.</summary>
    public static Type? SysTypeOf(object? descriptorBox)
    {
        return descriptorBox is not null && s_sysTypes.TryGetValue(descriptorBox, out Type? t) ? t : null;
    }

    /// <summary>
    /// Classifies a managed <see cref="Type"/> to its Go <c>reflect.Kind</c> ordinal. A NAMED Go type
    /// (<c>type Celsius float64</c> → <c>[GoType("num:float64")]</c>, or a wrapper struct) reports its
    /// UNDERLYING kind, matching Go — the name is recovered separately from the type itself.
    /// </summary>
    public static int KindOf(Type? t)
    {
        if (t is null)
            return Invalid;

        // Unwrap a boxed pointer/value: nothing to do here — Type is already the concrete runtime type.

        // Fast, exact matches for the built-in Go scalar representations.
        if (t == typeof(bool)) return Bool;
        if (t == typeof(nint)) return Int;                 // Go int
        if (t == typeof(sbyte)) return Int8;
        if (t == typeof(short)) return Int16;
        if (t == typeof(int)) return Int32;                // Go int32 / rune
        if (t == typeof(long)) return Int64;
        if (t == typeof(nuint)) return Uint;               // Go uint
        if (t == typeof(byte)) return Uint8;
        if (t == typeof(ushort)) return Uint16;
        if (t == typeof(uint)) return Uint32;
        if (t == typeof(ulong)) return Uint64;
        if (t == typeof(float)) return Float32;
        if (t == typeof(double)) return Float64;
        if (t == typeof(Complex)) return Complex128;
        if (t == typeof(@string)) return String;
        // A C# System.String can reach reflection where a Go string literal boxed in a
        // deliberately-uncast position (a variadic `...any` argument) — a bare `"a"` rather than
        // `(@string)"a"`; treat it as a Go string so reflect.TypeOf(it).Kind() == String (fmt's doPrint
        // inter-argument spacing depends on it).
        if (t == typeof(string)) return String;
        if (t == typeof(uintptr)) return Uintptr;

        // A named numeric / string wrapper carries its underlying kind in [GoType("num:<kind>")] or
        // [GoType("@string")]; report the underlying kind (Name() recovers the name elsewhere).
        if (TryGoTypeDefinitionKind(t, out int defKind))
            return defKind;

        // golib generic containers → their Go kind, detected by open generic definition.
        if (t.IsGenericType)
        {
            Type gd = t.GetGenericTypeDefinition();

            if (gd == typeof(slice<>)) return Slice;
            if (gd == typeof(array<>)) return Array;
            if (gd == typeof(map<,>)) return Map;
            if (gd == typeof(channel<>)) return Chan;
            if (gd == typeof(ж<>)) return Pointer;
        }

        // @unsafe.Pointer is a CONCRETE class `: ж<uintptr>` (not a generic ж<T>, so the check above
        // misses it, and not a [GoType("num:uintptr")] value wrapper). golib cannot name the unsafe
        // package's type directly, so detect it structurally: a reference type whose base is ж<uintptr>.
        if (t.BaseType == typeof(ж<uintptr>)) return UnsafePointer;

        if (typeof(Delegate).IsAssignableFrom(t)) return Func;

        if (t.IsInterface) return Interface;

        // A converted Go struct is a [GoType] value type; anything else value-typed still reports Struct.
        if (t.IsValueType) return Struct;

        // Reference-typed converted types (classes) that are none of the above are treated as pointers
        // to their referent in the Go model (rare on the fmt path); default to Struct otherwise.
        return Struct;
    }

    /// <summary>
    /// The Go source type string for a managed <see cref="Type"/> — what `reflect.Type.String()` and
    /// `%T` print. Recurses over the golib container types (`[]int`, `map[string]int`, `*main.Point`),
    /// maps the scalar representations to their Go spelling, and package-qualifies a named/struct type
    /// (`go.main_package.Point` → `main.Point`).
    /// </summary>
    public static string GoTypeName(Type? t)
    {
        if (t is null) return "<nil>";

        if (t == typeof(bool)) return "bool";
        if (t == typeof(nint)) return "int";
        if (t == typeof(sbyte)) return "int8";
        if (t == typeof(short)) return "int16";
        if (t == typeof(int)) return "int32";
        if (t == typeof(long)) return "int64";
        if (t == typeof(nuint)) return "uint";
        if (t == typeof(byte)) return "uint8";
        if (t == typeof(ushort)) return "uint16";
        if (t == typeof(uint)) return "uint32";
        if (t == typeof(ulong)) return "uint64";
        if (t == typeof(uintptr)) return "uintptr";
        if (t == typeof(float)) return "float32";
        if (t == typeof(double)) return "float64";
        if (t == typeof(Complex)) return "complex128";
        if (t == typeof(@string) || t == typeof(string)) return "string";

        if (t.IsGenericType)
        {
            Type gd = t.GetGenericTypeDefinition();
            Type[] a = t.GetGenericArguments();

            if (gd == typeof(slice<>)) return "[]" + GoTypeName(a[0]);
            if (gd == typeof(array<>)) return "[]" + GoTypeName(a[0]);   // length is not carried on the managed type
            if (gd == typeof(map<,>)) return "map[" + GoTypeName(a[0]) + "]" + GoTypeName(a[1]);
            if (gd == typeof(channel<>)) return "chan " + GoTypeName(a[0]);
            if (gd == typeof(ж<>)) return "*" + GoTypeName(a[0]);
        }

        if (t.BaseType == typeof(ж<uintptr>)) return "unsafe.Pointer";

        return GoQualifiedName(t);
    }

    // The package-qualified Go name of a converted named type: a converted type is nested in a
    // `<pkg>_package` class, so `go.main_package.Point` → `main.Point`. A Δ-collision rename (ΔHandle)
    // strips the marker; a type with no `_package` declaring class falls back to its bare name.
    private static string GoQualifiedName(Type t)
    {
        string name = t.Name;

        if (name.StartsWith(ShadowVarMarker, StringComparison.Ordinal))
            name = name[ShadowVarMarker.Length..];

        Type? decl = t.DeclaringType;

        if (decl is not null && decl.Name.EndsWith(PackageSuffix, StringComparison.Ordinal))
            return decl.Name[..^PackageSuffix.Length] + "." + name;

        return name;
    }

    /// <summary>
    /// The Go element type of a managed container <see cref="Type"/> — <c>slice&lt;T&gt;</c>/
    /// <c>array&lt;T&gt;</c>/<c>channel&lt;T&gt;</c>/<c>ж&lt;T&gt;</c> → <c>T</c>, <c>map&lt;K,V&gt;</c> → <c>V</c>
    /// — for <c>reflect.Type.Elem()</c>; <c>null</c> if <paramref name="t"/> has no element type.
    /// </summary>
    public static Type? ElementType(Type? t)
    {
        if (t is null || !t.IsGenericType) return null;

        Type gd = t.GetGenericTypeDefinition();
        Type[] a = t.GetGenericArguments();

        if (gd == typeof(map<,>)) return a[1];
        if (gd == typeof(slice<>) || gd == typeof(array<>) || gd == typeof(channel<>) || gd == typeof(ж<>)) return a[0];

        return null;
    }

    // Reads a [GoType] definition string ("num:int32", "@string", "num:uintptr", ...) and maps its
    // underlying-kind token to a reflect.Kind. Returns false when the type carries no such definition
    // (a plain [GoType] struct, or a non-converted type).
    private static bool TryGoTypeDefinitionKind(Type t, out int kind)
    {
        kind = Invalid;

        foreach (object attr in t.GetCustomAttributes(typeof(GoTypeAttribute), false))
        {
            string def = ((GoTypeAttribute)attr).Definition;

            if (string.IsNullOrEmpty(def))
                return false; // a plain struct/interface marker — not a named-underlying wrapper

            string token = def.StartsWith("num:", StringComparison.Ordinal) ? def[4..] : def;

            kind = token switch
            {
                "bool" => Bool,
                "int" => Int, "int8" => Int8, "int16" => Int16, "int32" => Int32, "int64" => Int64,
                "uint" => Uint, "uint8" => Uint8, "uint16" => Uint16, "uint32" => Uint32, "uint64" => Uint64,
                "uintptr" => Uintptr,
                "float32" => Float32, "float64" => Float64,
                "complex64" => Complex64, "complex128" => Complex128,
                "@string" or "string" => String,
                _ => Invalid
            };

            return kind != Invalid;
        }

        return false;
    }
}

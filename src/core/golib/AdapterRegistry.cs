// AdapterRegistry.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace go;

/// <summary>
/// Runtime registry of generated interface-implementation adapters, keyed by the Go dynamic
/// value's runtime type and the target interface type.
/// </summary>
/// <remarks>
/// <para>
/// A Go type switch or type assert against a NAMED interface must match by Go METHOD-SET
/// semantics: a raw receiver box <c>ж&lt;T&gt;</c> held in an interface matches
/// <c>case Iface:</c> whenever <c>*T</c> implements the interface. The box cannot nominally
/// implement the C# interface — the generated pointer adapter (<c>TжIface</c>) does — so each
/// generated adapter registers a wrapping factory here from a module initializer, and the golib
/// type-assert machinery re-wraps the box on demand. This stays reflection-free and
/// Native-AOT-safe: factories are compiled lambdas over closed generic types, and the module
/// initializer itself roots the adapter class against trimming.
/// </para>
/// <para>
/// The same pair can be registered by several assemblies (each assembly generates adapters for
/// its own recorded conversion sites); the first registration wins — all adapters for a pair
/// are behaviorally identical, forwarding to the same receiver methods.
/// </para>
/// </remarks>
public static class AdapterRegistry
{
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object>> s_factories = new();

    /// <summary>
    /// Registers a factory that wraps a Go dynamic value of runtime type <paramref name="valueType"/>
    /// in its generated adapter implementing <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="valueType">Runtime type of the Go dynamic value, e.g., <c>ж&lt;T&gt;</c> for a pointer-sourced implementation.</param>
    /// <param name="interfaceType">Interface type the adapter implements.</param>
    /// <param name="factory">Factory that wraps a value of <paramref name="valueType"/> in the adapter.</param>
    public static void Register(Type valueType, Type interfaceType, Func<object, object> factory)
    {
        s_factories.TryAdd((valueType, interfaceType), factory);
    }

    /// <summary>
    /// Attempts to wrap Go dynamic <paramref name="value"/> in its registered adapter implementing
    /// <paramref name="interfaceType"/>.
    /// </summary>
    /// <param name="value">Go dynamic value, e.g., a receiver box <c>ж&lt;T&gt;</c>.</param>
    /// <param name="interfaceType">Target interface type.</param>
    /// <param name="wrapped">Adapter instance implementing <paramref name="interfaceType"/>, if registered.</param>
    /// <returns><c>true</c> if an adapter factory was registered for the pair; otherwise, <c>false</c>.</returns>
    public static bool TryWrap(object value, Type interfaceType, [NotNullWhen(true)] out object? wrapped)
    {
        if (s_factories.TryGetValue((value.GetType(), interfaceType), out Func<object, object>? factory))
        {
            wrapped = factory(value);
            return true;
        }

        wrapped = null;
        return false;
    }
}

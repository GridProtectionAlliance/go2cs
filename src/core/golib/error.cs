// error.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using go.golib;

// TODO: Keep error implementation updated to match best interface template pattern

namespace go;

/// <summary>
/// The built-in error interface type is the conventional interface for representing an
/// error condition, with the nil value representing no error.
/// </summary>
public interface error // : IFormattable
{
    /// <summary>
    /// Get string that represents an error.
    /// </summary>
    @string Error();

    public static error As<T>(in T target)
    {
        return new error<T>(target!);
    }

    public static error As<T>(ж<T> target_ptr)
    {
        return new error<T>(target_ptr);
    }

    public static error? As(object target)
    {
        return typeof(error<>).CreateInterfaceHandler<error>(target);
    }
}

internal interface IErrorTarget
{
    object? TargetObject { get; }
}

public class error<T> : error, IErrorTarget
{
    private T m_target = default!;
    private readonly ж<T>? m_target_ptr;
    private readonly bool m_target_is_ptr;

    public ref T Target
    {
        get
        {
            if (m_target_is_ptr && m_target_ptr is not null)
                return ref m_target_ptr.Value;

            return ref m_target;
        }
    }

    object? IErrorTarget.TargetObject => Target;

    public error(in T target)
    {
        m_target = target;
    }

    public error(ж<T> target_ptr)
    {
        m_target_ptr = target_ptr;
        m_target_is_ptr = true;
    }

    private delegate @string ErrorByPtr(ж<T> value);
    private delegate @string ErrorByVal(T value);

    private static readonly ErrorByPtr? s_ErrorByPtr;
    private static readonly ErrorByVal? s_ErrorByVal;

    [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public @string Error()
    {
        T target = m_target;

        if (m_target_is_ptr && m_target_ptr is not null)
            target = m_target_ptr.Value;

        if (s_ErrorByPtr is null || !m_target_is_ptr)
            return s_ErrorByVal!(target);

        return s_ErrorByPtr(m_target_ptr!);
    }

    public string ToString(string? format, IFormatProvider? _)
    {
        switch (format)
        {
            case "T":
            {
                string typeName = GetGoTypeName<T>().Replace("_package+", ".");
                return m_target_is_ptr ? $"*{typeName}" : typeName;
            }
            case "v":
            {
                // Go's %v of an error operand calls Error() via handleMethods — NEVER the
                // `&{fields}` pointer rendering: the *T method set includes the value-receiver
                // Error, so the method wins before pointer formatting. The pointee's ToString
                // dispatches the Go method already; the old `&` prefix diverged from Go for
                // every pointer-held error (errors TestAs subtest names).
                if (m_target_is_ptr)
                    return m_target_ptr is null ? "<nil>" : m_target_ptr.Value?.ToString() ?? "<nil>";

                return m_target?.ToString() ?? "<nil>";
            }
            default:
                return ToString() ?? "<nil>";
        }
    }

    [DebuggerStepperBoundary]
    static error()
    {
        Type targetType = typeof(T);
        Type targetTypeByPtr = typeof(ж<T>);

        MethodInfo? extensionMethod = targetTypeByPtr.GetExtensionMethod("Error");

        if (extensionMethod is not null)
            s_ErrorByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorByPtr)) as ErrorByPtr;

        extensionMethod = targetType.GetExtensionMethod("Error");

        if (extensionMethod is not null)
            s_ErrorByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorByVal)) as ErrorByVal;

        if (s_ErrorByPtr is null && s_ErrorByVal is null)
            throw new NotImplementedException($"{targetType.FullName} does not implement error.Error method", new Exception("Error"));
    }

    public static explicit operator error<T>(in ж<T> target_ptr)
    {
        return new error<T>(target_ptr);
    }

    public static explicit operator error<T>(in T target)
    {
        return new error<T>(target);
    }

    // Enable comparisons between nil and error<T> interface instance
    public static bool operator ==(error<T> value, NilType _)
    {
        return Activator.CreateInstance<error<T>>().Equals(value);
    }

    public static bool operator !=(error<T> value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, error<T> value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, error<T> value)
    {
        return value != nil;
    }
}

public static class errorExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> s_conversionOperators = new();

    public static T _<T>(this error target)
    {
        // An assertion to another INTERFACE (`err.(interface{ … })`, e.g. a dyn anonymous interface) is a
        // standard interface-to-interface assertion, not an unwrap of the error<T> concrete carrier. A
        // pointer-sourced error is an IжAdapter standing in for the *T it wraps, so unwrap it to that
        // receiver box (Go's interface holds the *T) and route it through the general duck-typed
        // type-assertion machinery, which resolves the box's method set structurally.
        if (typeof(T).IsInterface)
        {
            object dynamicValue = target is IжAdapter pointerAdapter && pointerAdapter.Box is not null ? pointerAdapter.Box : target;
            return dynamicValue._<T>();
        }

        try
        {
            return ((error<T>)target).Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(typeof(T))}: missing method {ex.InnerException?.Message}", ex);
        }
    }

    public static bool _<T>(this error target, out T result)
    {
        try
        {
            result = target._<T>();
            return true;
        }
        catch (PanicException)
        {
            result = default!;
            return false;
        }
    }

    public static object? _(this error target, Type type)
    {
        try
        {
            MethodInfo? conversionOperator = s_conversionOperators.GetOrAdd(type, _ => typeof(error<>).GetExplicitGenericConversionOperator(type));

            if (conversionOperator is null)
                throw new PanicException($"interface conversion: failed to create converter for {GetGoTypeName(target.GetType())} to {GetGoTypeName(type)}");

            object? result = conversionOperator.Invoke(null, [target]);
            return result is IErrorTarget errorTarget ? errorTarget.TargetObject : null;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(type)}: missing method {ex.InnerException?.Message}");
        }
    }

    public static bool _(this error target, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, out object? result)
    {
        try
        {
            result = target._(type);
            return true;
        }
        catch (PanicException)
        {
            result = type.IsValueType ? Activator.CreateInstance(type) : null;
            return false;
        }
    }
}

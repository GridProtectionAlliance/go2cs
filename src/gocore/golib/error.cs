//******************************************************************************************************
//  error.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/21/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

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

    public static error As<T>(ptr<T> target_ptr)
    {
        return new error<T>(target_ptr);
    }

    public static error? As(object target)
    {
        return typeof(error<>).CreateInterfaceHandler<error>(target);
    }
}

public class error<T> : error
{
    private T m_target = default!;
    private readonly ptr<T>? m_target_ptr;
    private readonly bool m_target_is_ptr;

    public ref T Target
    {
        get
        {
            if (m_target_is_ptr && m_target_ptr is not null)
                return ref m_target_ptr.val;

            return ref m_target;
        }
    }

    public error(in T target)
    {
        m_target = target;
    }

    public error(ptr<T> target_ptr)
    {
        m_target_ptr = target_ptr;
        m_target_is_ptr = true;
    }

    private delegate @string ErrorByPtr(ptr<T> value);
    private delegate @string ErrorByVal(T value);

    private static readonly ErrorByPtr? s_ErrorByPtr;
    private static readonly ErrorByVal? s_ErrorByVal;

    [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public @string Error()
    {
        T target = m_target;

        if (m_target_is_ptr && m_target_ptr is not null)
            target = m_target_ptr.val;

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
                if (m_target_is_ptr)
                    return m_target_ptr is null ? "<nil>" : $"&{m_target_ptr.val}";

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
        Type targetTypeByPtr = typeof(ptr<T>);

        MethodInfo? extensionMethod = targetTypeByPtr.GetExtensionMethod("Error");

        if (extensionMethod is not null)
            s_ErrorByPtr = extensionMethod.CreateStaticDelegate(typeof(ErrorByPtr)) as ErrorByPtr;

        extensionMethod = targetType.GetExtensionMethod("Error");

        if (extensionMethod is not null)
            s_ErrorByVal = extensionMethod.CreateStaticDelegate(typeof(ErrorByVal)) as ErrorByVal;

        if (s_ErrorByPtr is null && s_ErrorByVal is null)
            throw new NotImplementedException($"{targetType.FullName} does not implement error.Error method", new Exception("Error"));
    }

    public static explicit operator error<T>(in ptr<T> target_ptr)
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

            dynamic? result = conversionOperator.Invoke(null, [target]);
            return result?.Target;
        }
        catch (NotImplementedException ex)
        {
            throw new PanicException($"interface conversion: {GetGoTypeName(target.GetType())} is not {GetGoTypeName(type)}: missing method {ex.InnerException?.Message}");
        }
    }

    public static bool _(this error target, Type type, out object? result)
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

//******************************************************************************************************
//  variadic.cs - Gbtc
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
//  07/09/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

using System;

namespace go;

// A Go VARIADIC function type used as a value — a parameter, struct field, variable or methodless
// named type such as `func(format string, args ...any)` — lowers to one of these delegates. The
// trailing `params Span<T>` carries the variadic tail, so calls through the value pass loose
// arguments (or spread a slice via its `ꓸꓸꓸ` property) exactly as Go callers do, and both the
// named-function convention (`params ꓸꓸꓸT argsʗp`) and variadic function literals convert to
// these natively — the parameter types match by identity. Fixed-parameter prefixes up to eight
// mirror the BCL Action/Func family; the `ꓸꓸꓸ` suffix reads as Go's `...`.

/// <summary>Represents a Go variadic func type with no result: <c>func(...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<TArg>(params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, TArg>(T1 arg1, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, TArg>(T1 arg1, T2 arg2, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, T3, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, T3, TArg>(T1 arg1, T2 arg2, T3 arg3, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, T3, T4, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, T3, T4, TArg>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, T3, T4, T5, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, T3, T4, T5, TArg>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, T3, T4, T5, T6, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, T3, T4, T5, T6, T7, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with no result: <c>func(T1, T2, T3, T4, T5, T6, T7, T8, ...T)</c>.</summary>
public delegate void Actionꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<TArg, out TResult>(params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, TArg, out TResult>(T1 arg1, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, TArg, out TResult>(T1 arg1, T2 arg2, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, T3, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, T3, TArg, out TResult>(T1 arg1, T2 arg2, T3 arg3, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, T3, T4, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, T3, T4, TArg, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, T3, T4, T5, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, T3, T4, T5, TArg, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, T3, T4, T5, T6, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, TArg, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, T3, T4, T5, T6, T7, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, TArg, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, params Span<TArg> args);

/// <summary>Represents a Go variadic func type with a result: <c>func(T1, T2, T3, T4, T5, T6, T7, T8, ...T) TResult</c>.</summary>
public delegate TResult Funcꓸꓸꓸ<T1, T2, T3, T4, T5, T6, T7, T8, TArg, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, params Span<TArg> args);

//******************************************************************************************************
//  GoFunc.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  05/07/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable ConditionIsAlwaysTrueOrFalse

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go;

/// <summary>
/// Delegate for the Go "defer" function.
/// </summary>
/// <param name="deferredAction"></param>
public delegate void Defer(Action deferredAction);

/// <summary>
/// Delegate for the Go "recover" function.
/// </summary>
/// <returns>Recovered panic state, if any.</returns>
public delegate object? Recover();

/// <summary>
/// Represents the root execution context for all Go functions.
/// </summary>
public class GoFuncRoot
{
    // Static thread local storage for captured panic exception shared between all GoFunc instances
    protected static readonly ThreadLocal<PanicException> CapturedPanic = new();
}

/// <summary>
/// Represents a Go function execution context for handling "defer" and "recover" keywords.
/// </summary>
public class GoFunc<T> : GoFuncRoot
{
    public delegate void GoAction(Defer defer, Recover recover);
    public delegate T GoFunction(Defer defer, Recover recover);

    protected Stack<Action>? Defers;

    private readonly GoFunction? m_function;

    protected GoFunc()
    {
        m_function = null;
    }

    public GoFunc(GoAction action)
    {
        m_function = (defer, recover) =>
        {
            action(defer, recover);
            return default!;
        };
    }

    public GoFunc(GoFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute()
    {
        T result = default!;

        try
        {
            if (m_function is not null)
                result = m_function(HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }

    protected void HandleDefer(Action? deferredAction)
    {
        if (deferredAction is null)
            return;

        Defers ??= new Stack<Action>();
        Defers.Push(deferredAction);
    }

    protected object? HandleRecover()
    {
        object? result = CapturedPanic.Value?.State;
        CapturedPanic.Value = null!;
        return result;
    }

    protected void HandleFinally()
    {
        if (Defers is not null)
        {
            while (Defers.Count > 0)
                Defers.Pop()();
        }

        if (CapturedPanic.Value is not null)
            throw CapturedPanic.Value;
    }
}

/// <summary>
/// Represents a Go function execution context with 1 reference parameter for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, Defer defer, Recover recover) =>
        {
            action(ref ref1, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

#region [ GoFunc<TRef1, TRef2, ... TRef16, T> Implementations ]

/*  The following code was generated using the "GenGoFuncRefInstances" utility: */

/// <summary>
/// Represents a Go function execution context with 2 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 3 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 4 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 5 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 6 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 7 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 8 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 9 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 10 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 11 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 12 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 13 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 14 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 15 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

/// <summary>
/// Represents a Go function execution context with 16 reference parameters for handling "defer" and "recover" keywords.
/// </summary>
public sealed class GoFunc<TRef1, TRef2, TRef3, TRef4, TRef5, TRef6, TRef7, TRef8, TRef9, TRef10, TRef11, TRef12, TRef13, TRef14, TRef15, TRef16, T> : GoFunc<T>
{
    public delegate void GoRefAction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, Defer defer, Recover recover);
    public delegate T GoRefFunction(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, Defer defer, Recover recover);

    private readonly GoRefFunction m_function;

    public GoFunc(GoRefAction action)
    {
        m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16, Defer defer, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, ref ref16, defer, recover);
            return default!;
        };
    }

    public GoFunc(GoRefFunction function)
    {
        m_function = function;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
    public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, ref TRef4 ref4, ref TRef5 ref5, ref TRef6 ref6, ref TRef7 ref7, ref TRef8 ref8, ref TRef9 ref9, ref TRef10 ref10, ref TRef11 ref11, ref TRef12 ref12, ref TRef13 ref13, ref TRef14 ref14, ref TRef15 ref15, ref TRef16 ref16)
    {
        T result = default!;

        try
        {
            result = m_function(ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6, ref ref7, ref ref8, ref ref9, ref ref10, ref ref11, ref ref12, ref ref13, ref ref14, ref ref15, ref ref16, HandleDefer, HandleRecover);
        }
        catch (PanicException ex)
        {
            CapturedPanic.Value = ex;
        }
        finally
        {
            HandleFinally();
        }

        return result!;
    }
}

#endregion

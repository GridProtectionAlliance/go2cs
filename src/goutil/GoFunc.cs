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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

// ReSharper disable StaticMemberInGenericType

namespace goutil
{
    public delegate void Defer(Action deferredAction);
    public delegate void Panic(object state);
    public delegate object Recover();

    /// <summary>
    /// Represents the root execution context for all Go functions.
    /// </summary>
    public class GoFuncRoot
    {
        // Static thread local storage shared between all GoFunc instances
        protected static readonly ThreadLocal<PanicException> CapturedPanic = new ThreadLocal<PanicException>();
    }

    /// <summary>
    /// Represents a Go function execution context for handling "defer", "panic", and "recover" keywords.
    /// </summary>
    public class GoFunc<T> : GoFuncRoot
    {
        public delegate void GoAction(Defer defer, Panic panic, Recover recover);
        public delegate T GoFunction(Defer defer, Panic panic, Recover recover);

        protected Stack<Action> Defers;

        private readonly GoFunction m_function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        protected GoFunc() => m_function = null;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(GoAction action) => m_function = (defer, panic, recover) =>
        {
            action(defer, panic, recover);
            return default;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(GoFunction function) => m_function = function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public T Execute()
        {
            T result = default;

            try
            {
                result = m_function(HandleDefer, HandlePanic, HandlePanic);
            }
            catch (PanicException ex)
            {
                CapturedPanic.Value = ex;
            }
            finally
            {
                if ((object)Defers != null)
                    while (Defers.Count > 0)
                        Defers.Pop()();

                if ((object)CapturedPanic.Value != null)
                    throw CapturedPanic.Value;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        protected void HandleDefer(Action deferredAction)
        {
            if ((object)deferredAction == null)
                return;

            if ((object)Defers == null)
                Defers = new Stack<Action>();

            Defers.Push(deferredAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        protected void HandlePanic(object state) => throw new PanicException(state);

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        protected object HandlePanic()
        {
            object result = CapturedPanic.Value?.State;
            CapturedPanic.Value = null;
            return result;
        }
    }

    /// <summary>
    /// Represents a Go function execution context with 1 reference parameter for handling "defer", "panic", and "recover" keywords.
    /// </summary>
    public class GoFunc<TRef1, T> : GoFunc<T>
    {
        public delegate void RefActionSignature(ref TRef1 ref1, Defer defer, Panic panic, Recover recover);
        public delegate T RefFuncSignature(ref TRef1 ref1, Defer defer, Panic panic, Recover recover);

        private readonly RefFuncSignature m_function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(RefActionSignature action) => m_function = (ref TRef1 ref1, Defer defer, Panic panic, Recover recover) =>
        {
            action(ref ref1, defer, panic, recover);
            return default;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(RefFuncSignature function) => m_function = function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public T Execute(ref TRef1 ref1)
        {
            T result = default;

            try
            {
                result = m_function(ref ref1, HandleDefer, HandlePanic, HandlePanic);
            }
            catch (PanicException ex)
            {
                CapturedPanic.Value = ex;
            }
            finally
            {
                if ((object)Defers != null)
                    while (Defers.Count > 0)
                        Defers.Pop()();

                if ((object)CapturedPanic.Value != null)
                    throw CapturedPanic.Value;
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a Go function execution context with 2 reference parameters for handling "defer", "panic", and "recover" keywords.
    /// </summary>
    public class GoFunc<TRef1, TRef2, T> : GoFunc<T>
    {
        public delegate void RefActionSignature(ref TRef1 ref1, ref TRef2 ref2, Defer defer, Panic panic, Recover recover);
        public delegate T RefFuncSignature(ref TRef1 ref1, ref TRef2 ref2, Defer defer, Panic panic, Recover recover);

        private readonly RefFuncSignature m_function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(RefActionSignature action) => m_function = (ref TRef1 ref1, ref TRef2 ref2, Defer defer, Panic panic, Recover recover) =>
        {
            action(ref ref1, ref ref2, defer, panic, recover);
            return default;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(RefFuncSignature function) => m_function = function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public T Execute(ref TRef1 ref1, ref TRef2 ref2)
        {
            T result = default;

            try
            {
                result = m_function(ref ref1, ref ref2, HandleDefer, HandlePanic, HandlePanic);
            }
            catch (PanicException ex)
            {
                CapturedPanic.Value = ex;
            }
            finally
            {
                if ((object)Defers != null)
                    while (Defers.Count > 0)
                        Defers.Pop()();

                if ((object)CapturedPanic.Value != null)
                    throw CapturedPanic.Value;
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a Go function execution context with 3 reference parameters for handling "defer", "panic", and "recover" keywords.
    /// </summary>
    public class GoFunc<TRef1, TRef2, TRef3, T> : GoFunc<T>
    {
        public delegate void RefActionSignature(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, Defer defer, Panic panic, Recover recover);
        public delegate T RefFuncSignature(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, Defer defer, Panic panic, Recover recover);

        private readonly RefFuncSignature m_function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(RefActionSignature action) => m_function = (ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3, Defer defer, Panic panic, Recover recover) =>
        {
            action(ref ref1, ref ref2, ref ref3, defer, panic, recover);
            return default;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(RefFuncSignature function) => m_function = function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public T Execute(ref TRef1 ref1, ref TRef2 ref2, ref TRef3 ref3)
        {
            T result = default;

            try
            {
                result = m_function(ref ref1, ref ref2, ref ref3, HandleDefer, HandlePanic, HandlePanic);
            }
            catch (PanicException ex)
            {
                CapturedPanic.Value = ex;
            }
            finally
            {
                if ((object)Defers != null)
                    while (Defers.Count > 0)
                        Defers.Pop()();

                if ((object)CapturedPanic.Value != null)
                    throw CapturedPanic.Value;
            }

            return result;
        }
    }

    // TODO: Expand to about TRef16...
}
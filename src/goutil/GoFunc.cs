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
    /// <summary>
    /// Represents a Go function execution context for handling "defer", "panic", and "recover" keywords.
    /// </summary>
    public class GoFunc<T>
    {
        private static readonly ThreadLocal<PanicException> s_capturedPanic = new ThreadLocal<PanicException>();
        private readonly Func<Action<Action>, Action<object>, Func<object> , T> m_function;
        private Stack<Action> m_defers;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(Action<Action<Action>, Action<object>, Func<object>> action) => m_function = (defer, panic, recover) =>
        {
            action(defer, panic, recover);
            return default;
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        public GoFunc(Func<Action<Action>, Action<object>, Func<object>, T> function) => m_function = function;

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepperBoundary]
        public T Execute()
        {
            T result = default;

            try
            {
                result = m_function(Defer, Panic, Recover);
            }
            catch (PanicException ex)
            {
                s_capturedPanic.Value = ex;
            }
            finally
            {
                if ((object)m_defers != null)
                    while (m_defers.Count > 0)
                        m_defers.Pop()();

                if ((object)s_capturedPanic.Value != null)
                    throw s_capturedPanic.Value;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        private void Defer(Action deferredAction)
        {
            if ((object)deferredAction == null)
                return;

            if ((object)m_defers == null)
                m_defers = new Stack<Action>();

            m_defers.Push(deferredAction);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        private void Panic(object state) => throw new PanicException(state);

        [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerNonUserCode]
        private object Recover()
        {
            object result = s_capturedPanic.Value?.State;
            s_capturedPanic.Value = null;
            return result;
        }
    }
}
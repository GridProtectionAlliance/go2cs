//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 06 22:14:25 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using context = go.context_package;
using os = go.os_package;
using sync = go.sync_package;
using go;

#nullable enable

namespace go {
namespace os
{
    public static partial class signal_package
    {
        [GeneratedCode("go2cs", "0.1.0.0")]
        private partial struct signalCtx
        {
            // Context.Deadline function promotion
            private delegate void DeadlineByVal(T value);
            private delegate void DeadlineByRef(ref T value);

            private static readonly DeadlineByVal s_DeadlineByVal;
            private static readonly DeadlineByRef s_DeadlineByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Deadline() => s_DeadlineByRef?.Invoke(ref this) ?? s_DeadlineByVal?.Invoke(this) ?? Context?.Deadline() ?? throw new PanicException(RuntimeErrorPanic.NilPointerDereference);

            // Context.Done function promotion
            private delegate void DoneByVal(T value);
            private delegate void DoneByRef(ref T value);

            private static readonly DoneByVal s_DoneByVal;
            private static readonly DoneByRef s_DoneByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Done() => s_DoneByRef?.Invoke(ref this) ?? s_DoneByVal?.Invoke(this) ?? Context?.Done() ?? throw new PanicException(RuntimeErrorPanic.NilPointerDereference);

            // Context.Err function promotion
            private delegate void ErrByVal(T value);
            private delegate void ErrByRef(ref T value);

            private static readonly ErrByVal s_ErrByVal;
            private static readonly ErrByRef s_ErrByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Err() => s_ErrByRef?.Invoke(ref this) ?? s_ErrByVal?.Invoke(this) ?? Context?.Err() ?? throw new PanicException(RuntimeErrorPanic.NilPointerDereference);

            // Context.Value function promotion
            private delegate void ValueByVal(T value, object key);
            private delegate void ValueByRef(ref T value, object key);

            private static readonly ValueByVal s_ValueByVal;
            private static readonly ValueByRef s_ValueByRef;

            [DebuggerNonUserCode, MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Value(object key) => s_ValueByRef?.Invoke(ref this, key) ?? s_ValueByVal?.Invoke(this, key) ?? Context?.Value(key) ?? throw new PanicException(RuntimeErrorPanic.NilPointerDereference);
            
            [DebuggerStepperBoundary]
            static signalCtx()
            {
                Type targetType = typeof(signalCtx);
                MethodInfo extensionMethod;
                
                extensionMethod = targetType.GetExtensionMethodSearchingPromotions("Deadline");

                if (extensionMethod is not null)
                {
                    s_DeadlineByRef = extensionMethod.CreateStaticDelegate(typeof(DeadlineByRef)) as DeadlineByRef;

                    if (s_DeadlineByRef is null)
                        s_DeadlineByVal = extensionMethod.CreateStaticDelegate(typeof(DeadlineByVal)) as DeadlineByVal;
                }
                
                extensionMethod = targetType.GetExtensionMethodSearchingPromotions("Done");

                if (extensionMethod is not null)
                {
                    s_DoneByRef = extensionMethod.CreateStaticDelegate(typeof(DoneByRef)) as DoneByRef;

                    if (s_DoneByRef is null)
                        s_DoneByVal = extensionMethod.CreateStaticDelegate(typeof(DoneByVal)) as DoneByVal;
                }
                
                extensionMethod = targetType.GetExtensionMethodSearchingPromotions("Err");

                if (extensionMethod is not null)
                {
                    s_ErrByRef = extensionMethod.CreateStaticDelegate(typeof(ErrByRef)) as ErrByRef;

                    if (s_ErrByRef is null)
                        s_ErrByVal = extensionMethod.CreateStaticDelegate(typeof(ErrByVal)) as ErrByVal;
                }
                
                extensionMethod = targetType.GetExtensionMethodSearchingPromotions("Value");

                if (extensionMethod is not null)
                {
                    s_ValueByRef = extensionMethod.CreateStaticDelegate(typeof(ValueByRef)) as ValueByRef;

                    if (s_ValueByRef is null)
                        s_ValueByVal = extensionMethod.CreateStaticDelegate(typeof(ValueByVal)) as ValueByVal;
                }
            }

            // Constructors
            public signalCtx(NilType _)
            {
                this.Context = default;
                this.cancel = default;
                this.signals = default;
                this.ch = default;
            }

            public signalCtx(context.Context Context = default, context.CancelFunc cancel = default, slice<os.Signal> signals = default, channel<os.Signal> ch = default)
            {
                this.Context = Context;
                this.cancel = cancel;
                this.signals = signals;
                this.ch = ch;
            }

            // Enable comparisons between nil and signalCtx struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(signalCtx value, NilType nil) => value.Equals(default(signalCtx));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(signalCtx value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, signalCtx value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, signalCtx value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator signalCtx(NilType nil) => default(signalCtx);
        }

        [GeneratedCode("go2cs", "0.1.0.0")]
        private static signalCtx signalCtx_cast(dynamic value)
        {
            return new signalCtx(value.Context, value.cancel, value.signals, value.ch);
        }
    }
}}
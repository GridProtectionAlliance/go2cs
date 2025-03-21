//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 05:40:38 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using itoa = go.@internal.itoa_package;
using oserror = go.@internal.oserror_package;
using race = go.@internal.race_package;
using unsafeheader = go.@internal.unsafeheader_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;

#nullable enable

namespace go
{
    public static partial class syscall_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        [PromotedStruct(typeof(sync.Mutex))]
        private partial struct mmapper
        {
            // Mutex structure promotion - sourced from value copy
            private readonly ptr<Mutex> m_MutexRef;

            private ref Mutex Mutex_val => ref m_MutexRef.Value;

            public ref int state => ref m_MutexRef.Value.state;

            public ref uint sema => ref m_MutexRef.Value.sema;

            // Constructors
            public mmapper(NilType _)
            {
                this.m_MutexRef = new ptr<sync.Mutex>(new sync.Mutex(nil));
                this.active = default;
                this.mmap = default;
                this.munmap = default;
            }

            public mmapper(sync.Mutex Mutex = default, map<ptr<byte>, slice<byte>> active = default, Func<System.UIntPtr, System.UIntPtr, nint, nint, nint, long, (System.UIntPtr, error)> mmap = default, Func<System.UIntPtr, System.UIntPtr, error> munmap = default)
            {
                this.m_MutexRef = new ptr<sync.Mutex>(Mutex);
                this.active = active;
                this.mmap = mmap;
                this.munmap = munmap;
            }

            // Enable comparisons between nil and mmapper struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(mmapper value, NilType nil) => value.Equals(default(mmapper));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(mmapper value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, mmapper value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, mmapper value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator mmapper(NilType nil) => default(mmapper);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        private static mmapper mmapper_cast(dynamic value)
        {
            return new mmapper(value.Mutex, value.active, value.mmap, value.munmap);
        }
    }
}
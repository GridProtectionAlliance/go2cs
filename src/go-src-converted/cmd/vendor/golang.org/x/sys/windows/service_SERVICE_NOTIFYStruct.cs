//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
//
//     Generated on 2022 March 13 06:41:28 UTC
// </auto-generated>
//---------------------------------------------------------
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using go;

#nullable enable

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class windows_package
    {
        [GeneratedCode("go2cs", "0.1.2.0")]
        public partial struct SERVICE_NOTIFY
        {
            // Constructors
            public SERVICE_NOTIFY(NilType _)
            {
                this.Version = default;
                this.NotifyCallback = default;
                this.Context = default;
                this.NotificationStatus = default;
                this.ServiceStatus = default;
                this.NotificationTriggered = default;
                this.ServiceNames = default;
            }

            public SERVICE_NOTIFY(uint Version = default, System.UIntPtr NotifyCallback = default, System.UIntPtr Context = default, uint NotificationStatus = default, SERVICE_STATUS_PROCESS ServiceStatus = default, uint NotificationTriggered = default, ref ptr<ushort> ServiceNames = default)
            {
                this.Version = Version;
                this.NotifyCallback = NotifyCallback;
                this.Context = Context;
                this.NotificationStatus = NotificationStatus;
                this.ServiceStatus = ServiceStatus;
                this.NotificationTriggered = NotificationTriggered;
                this.ServiceNames = ServiceNames;
            }

            // Enable comparisons between nil and SERVICE_NOTIFY struct
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(SERVICE_NOTIFY value, NilType nil) => value.Equals(default(SERVICE_NOTIFY));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(SERVICE_NOTIFY value, NilType nil) => !(value == nil);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator ==(NilType nil, SERVICE_NOTIFY value) => value == nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool operator !=(NilType nil, SERVICE_NOTIFY value) => value != nil;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator SERVICE_NOTIFY(NilType nil) => default(SERVICE_NOTIFY);
        }

        [GeneratedCode("go2cs", "0.1.2.0")]
        public static SERVICE_NOTIFY SERVICE_NOTIFY_cast(dynamic value)
        {
            return new SERVICE_NOTIFY(value.Version, value.NotifyCallback, value.Context, value.NotificationStatus, value.ServiceStatus, value.NotificationTriggered, ref value.ServiceNames);
        }
    }
}}}}}}
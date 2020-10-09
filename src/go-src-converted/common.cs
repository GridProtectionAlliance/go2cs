// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package wire -- go2cs converted at 2020 October 09 06:01:50 UTC
// import "golang.org/x/tools/internal/event/export/ocagent/wire" ==> using wire = go.golang.org.x.tools.@internal.@event.export.ocagent.wire_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\event\export\ocagent\wire\common.go

using static go.builtin;
using System.ComponentModel;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace @event {
namespace export {
namespace ocagent
{
    public static partial class wire_package
    {
        // This file holds common ocagent types
        public partial struct Node
        {
            [Description("json:\"identifier,omitempty\"")]
            public ptr<ProcessIdentifier> Identifier;
            [Description("json:\"library_info,omitempty\"")]
            public ptr<LibraryInfo> LibraryInfo;
            [Description("json:\"service_info,omitempty\"")]
            public ptr<ServiceInfo> ServiceInfo;
            [Description("json:\"attributes,omitempty\"")]
            public map<@string, @string> Attributes;
        }

        public partial struct Resource
        {
            [Description("json:\"type,omitempty\"")]
            public @string Type;
            [Description("json:\"labels,omitempty\"")]
            public map<@string, @string> Labels;
        }

        public partial struct TruncatableString
        {
            [Description("json:\"value,omitempty\"")]
            public @string Value;
            [Description("json:\"truncated_byte_count,omitempty\"")]
            public int TruncatedByteCount;
        }

        public partial struct Attributes
        {
            [Description("json:\"attributeMap,omitempty\"")]
            public map<@string, Attribute> AttributeMap;
            [Description("json:\"dropped_attributes_count,omitempty\"")]
            public int DroppedAttributesCount;
        }

        public partial struct StringAttribute
        {
            [Description("json:\"stringValue,omitempty\"")]
            public ptr<TruncatableString> StringValue;
        }

        public partial struct IntAttribute
        {
            [Description("json:\"intValue,omitempty\"")]
            public long IntValue;
        }

        public partial struct BoolAttribute
        {
            [Description("json:\"boolValue,omitempty\"")]
            public bool BoolValue;
        }

        public partial struct DoubleAttribute
        {
            [Description("json:\"doubleValue,omitempty\"")]
            public double DoubleValue;
        }

        public partial interface Attribute
        {
            void labelAttribute();
        }

        public static void labelAttribute(this StringAttribute _p0)
        {
        }
        public static void labelAttribute(this IntAttribute _p0)
        {
        }
        public static void labelAttribute(this BoolAttribute _p0)
        {
        }
        public static void labelAttribute(this DoubleAttribute _p0)
        {
        }

        public partial struct StackTrace
        {
            [Description("json:\"stack_frames,omitempty\"")]
            public ptr<StackFrames> StackFrames;
            [Description("json:\"stack_trace_hash_id,omitempty\"")]
            public ulong StackTraceHashID;
        }

        public partial struct StackFrames
        {
            [Description("json:\"frame,omitempty\"")]
            public slice<ptr<StackFrame>> Frame;
            [Description("json:\"dropped_frames_count,omitempty\"")]
            public int DroppedFramesCount;
        }

        public partial struct StackFrame
        {
            [Description("json:\"function_name,omitempty\"")]
            public ptr<TruncatableString> FunctionName;
            [Description("json:\"original_function_name,omitempty\"")]
            public ptr<TruncatableString> OriginalFunctionName;
            [Description("json:\"file_name,omitempty\"")]
            public ptr<TruncatableString> FileName;
            [Description("json:\"line_number,omitempty\"")]
            public long LineNumber;
            [Description("json:\"column_number,omitempty\"")]
            public long ColumnNumber;
            [Description("json:\"load_module,omitempty\"")]
            public ptr<Module> LoadModule;
            [Description("json:\"source_version,omitempty\"")]
            public ptr<TruncatableString> SourceVersion;
        }

        public partial struct Module
        {
            [Description("json:\"module,omitempty\"")]
            public ptr<TruncatableString> Module;
            [Description("json:\"build_id,omitempty\"")]
            public ptr<TruncatableString> BuildID;
        }

        public partial struct ProcessIdentifier
        {
            [Description("json:\"host_name,omitempty\"")]
            public @string HostName;
            [Description("json:\"pid,omitempty\"")]
            public uint Pid;
            [Description("json:\"start_timestamp,omitempty\"")]
            public Timestamp StartTimestamp;
        }

        public partial struct LibraryInfo
        {
            [Description("json:\"language,omitempty\"")]
            public Language Language;
            [Description("json:\"exporter_version,omitempty\"")]
            public @string ExporterVersion;
            [Description("json:\"core_library_version,omitempty\"")]
            public @string CoreLibraryVersion;
        }

        public partial struct Language // : int
        {
        }

        public static readonly Language LanguageGo = (Language)4L;


        public partial struct ServiceInfo
        {
            [Description("json:\"name,omitempty\"")]
            public @string Name;
        }
    }
}}}}}}}}

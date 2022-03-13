// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build libfuzzer
// +build libfuzzer

// package runtime -- go2cs converted at 2022 March 13 05:24:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\libfuzzer.go
namespace go;

using _@unsafe_ = @unsafe_package;

public static partial class runtime_package { // for go:linkname

private static void libfuzzerCall(ptr<byte> fn, System.UIntPtr arg0, System.UIntPtr arg1);

private static void libfuzzerTraceCmp1(byte arg0, byte arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_cmp1, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceCmp2(ushort arg0, ushort arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_cmp2, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceCmp4(uint arg0, uint arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_cmp4, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceCmp8(ulong arg0, ulong arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_cmp8, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceConstCmp1(byte arg0, byte arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_const_cmp1, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceConstCmp2(ushort arg0, ushort arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_const_cmp2, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceConstCmp4(uint arg0, uint arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_const_cmp4, uintptr(arg0), uintptr(arg1));
}

private static void libfuzzerTraceConstCmp8(ulong arg0, ulong arg1) {
    libfuzzerCall(_addr___sanitizer_cov_trace_const_cmp8, uintptr(arg0), uintptr(arg1));
}

//go:linkname __sanitizer_cov_trace_cmp1 __sanitizer_cov_trace_cmp1
//go:cgo_import_static __sanitizer_cov_trace_cmp1
private static byte __sanitizer_cov_trace_cmp1 = default;

//go:linkname __sanitizer_cov_trace_cmp2 __sanitizer_cov_trace_cmp2
//go:cgo_import_static __sanitizer_cov_trace_cmp2
private static byte __sanitizer_cov_trace_cmp2 = default;

//go:linkname __sanitizer_cov_trace_cmp4 __sanitizer_cov_trace_cmp4
//go:cgo_import_static __sanitizer_cov_trace_cmp4
private static byte __sanitizer_cov_trace_cmp4 = default;

//go:linkname __sanitizer_cov_trace_cmp8 __sanitizer_cov_trace_cmp8
//go:cgo_import_static __sanitizer_cov_trace_cmp8
private static byte __sanitizer_cov_trace_cmp8 = default;

//go:linkname __sanitizer_cov_trace_const_cmp1 __sanitizer_cov_trace_const_cmp1
//go:cgo_import_static __sanitizer_cov_trace_const_cmp1
private static byte __sanitizer_cov_trace_const_cmp1 = default;

//go:linkname __sanitizer_cov_trace_const_cmp2 __sanitizer_cov_trace_const_cmp2
//go:cgo_import_static __sanitizer_cov_trace_const_cmp2
private static byte __sanitizer_cov_trace_const_cmp2 = default;

//go:linkname __sanitizer_cov_trace_const_cmp4 __sanitizer_cov_trace_const_cmp4
//go:cgo_import_static __sanitizer_cov_trace_const_cmp4
private static byte __sanitizer_cov_trace_const_cmp4 = default;

//go:linkname __sanitizer_cov_trace_const_cmp8 __sanitizer_cov_trace_const_cmp8
//go:cgo_import_static __sanitizer_cov_trace_const_cmp8
private static byte __sanitizer_cov_trace_const_cmp8 = default;

} // end runtime_package

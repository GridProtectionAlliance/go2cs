// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:30 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\write_err_android.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static byte writeHeader = new slice<byte>(new byte[] { 6, 'G', 'o', 0 });private static slice<byte> writePath = (slice<byte>)"/dev/log/main\x00";private static slice<byte> writeLogd = (slice<byte>)"/dev/socket/logdw\x00";private static System.UIntPtr writeFD = default;private static array<byte> writeBuf = new array<byte>(1024);private static nint writePos = default;

// Prior to Android-L, logging was done through writes to /dev/log files implemented
// in kernel ring buffers. In Android-L, those /dev/log files are no longer
// accessible and logging is done through a centralized user-mode logger, logd.
//
// https://android.googlesource.com/platform/system/core/+/refs/tags/android-6.0.1_r78/liblog/logd_write.c
private partial struct loggerType { // : int
}

private static readonly loggerType unknown = iota;
private static readonly var legacy = 0;
private static readonly var logd = 1; 
// TODO(hakim): logging for emulator?

private static loggerType logger = default;

private static void writeErr(slice<byte> b) {
    if (logger == unknown) { 
        // Use logd if /dev/socket/logdw is available.
        {
            var v__prev2 = v;

            var v = uintptr(access(_addr_writeLogd[0], 0x02));

            if (v == 0) {
                logger = logd;
                initLogd();
            }
            else
 {
                logger = legacy;
                initLegacy();
            }

            v = v__prev2;

        }

    }
    write(2, @unsafe.Pointer(_addr_b[0]), int32(len(b))); 

    // Log format: "<header>\x00<message m bytes>\x00"
    //
    // <header>
    //   In legacy mode: "<priority 1 byte><tag n bytes>".
    //   In logd mode: "<android_log_header_t 11 bytes><priority 1 byte><tag n bytes>"
    //
    // The entire log needs to be delivered in a single syscall (the NDK
    // does this with writev). Each log is its own line, so we need to
    // buffer writes until we see a newline.
    nint hlen = default;

    if (logger == logd) 
        hlen = writeLogdHeader();
    else if (logger == legacy) 
        hlen = len(writeHeader);
        var dst = writeBuf[(int)hlen..];
    {
        var v__prev1 = v;

        foreach (var (_, __v) in b) {
            v = __v;
            if (v == 0) { // android logging won't print a zero byte
                v = '0';

            }

            dst[writePos] = v;
            writePos++;
            if (v == '\n' || writePos == len(dst) - 1) {
                dst[writePos] = 0;
                write(writeFD, @unsafe.Pointer(_addr_writeBuf[0]), int32(hlen + writePos));
                foreach (var (i) in dst) {
                    dst[i] = 0;
                }
                writePos = 0;
            }

        }
        v = v__prev1;
    }
}

private static void initLegacy() { 
    // In legacy mode, logs are written to /dev/log/main
    writeFD = uintptr(open(_addr_writePath[0], 0x1, 0));
    if (writeFD == 0) { 
        // It is hard to do anything here. Write to stderr just
        // in case user has root on device and has run
        //    adb shell setprop log.redirect-stdio true
        slice<byte> msg = (slice<byte>)"runtime: cannot open /dev/log/main\x00";
        write(2, @unsafe.Pointer(_addr_msg[0]), int32(len(msg)));
        exit(2);

    }
    copy(writeBuf[..(int)len(writeHeader)], writeHeader);

}

// used in initLogdWrite but defined here to avoid heap allocation.
private static sockaddr_un logdAddr = default;

private static void initLogd() { 
    // In logd mode, logs are sent to the logd via a unix domain socket.
    logdAddr.family = _AF_UNIX;
    copy(logdAddr.path[..], writeLogd); 

    // We are not using non-blocking I/O because writes taking this path
    // are most likely triggered by panic, we cannot think of the advantage of
    // non-blocking I/O for panic but see disadvantage (dropping panic message),
    // and blocking I/O simplifies the code a lot.
    var fd = socket(_AF_UNIX, _SOCK_DGRAM | _O_CLOEXEC, 0);
    if (fd < 0) {
        slice<byte> msg = (slice<byte>)"runtime: cannot create a socket for logging\x00";
        write(2, @unsafe.Pointer(_addr_msg[0]), int32(len(msg)));
        exit(2);
    }
    var errno = connect(fd, @unsafe.Pointer(_addr_logdAddr), int32(@unsafe.Sizeof(logdAddr)));
    if (errno < 0) {
        msg = (slice<byte>)"runtime: cannot connect to /dev/socket/logdw\x00";
        write(2, @unsafe.Pointer(_addr_msg[0]), int32(len(msg))); 
        // TODO(hakim): or should we just close fd and hope for better luck next time?
        exit(2);

    }
    writeFD = uintptr(fd); 

    // Prepopulate invariant part of the header.
    // The first 11 bytes will be populated later in writeLogdHeader.
    copy(writeBuf[(int)11..(int)11 + len(writeHeader)], writeHeader);

}

// writeLogdHeader populates the header and returns the length of the payload.
private static nint writeLogdHeader() {
    var hdr = writeBuf[..(int)11]; 

    // The first 11 bytes of the header corresponds to android_log_header_t
    // as defined in system/core/include/private/android_logger.h
    //   hdr[0] log type id (unsigned char), defined in <log/log.h>
    //   hdr[1:2] tid (uint16_t)
    //   hdr[3:11] log_time defined in <log/log_read.h>
    //      hdr[3:7] sec unsigned uint32, little endian.
    //      hdr[7:11] nsec unsigned uint32, little endian.
    hdr[0] = 0; // LOG_ID_MAIN
    var (sec, nsec, _) = time_now();
    packUint32(hdr[(int)3..(int)7], uint32(sec));
    packUint32(hdr[(int)7..(int)11], uint32(nsec)); 

    // TODO(hakim):  hdr[1:2] = gettid?

    return 11 + len(writeHeader);

}

private static void packUint32(slice<byte> b, uint v) { 
    // little-endian.
    b[0] = byte(v);
    b[1] = byte(v >> 8);
    b[2] = byte(v >> 16);
    b[3] = byte(v >> 24);

}

} // end runtime_package

// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 06 23:38:21 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\cpu_zos_s390x.go


namespace go.vendor.golang.org.x.sys;

public static partial class cpu_package {

private static void initS390Xbase() { 
    // get the facilities list
    var facilities = stfle(); 

    // mandatory
    S390X.HasZARCH = facilities.Has(zarch);
    S390X.HasSTFLE = facilities.Has(stflef);
    S390X.HasLDISP = facilities.Has(ldisp);
    S390X.HasEIMM = facilities.Has(eimm); 

    // optional
    S390X.HasETF3EH = facilities.Has(etf3eh);
    S390X.HasDFP = facilities.Has(dfp);
    S390X.HasMSA = facilities.Has(msa);
    S390X.HasVX = facilities.Has(vx);
    if (S390X.HasVX) {
        S390X.HasVXE = facilities.Has(vxe);
    }
}

} // end cpu_package

# go.crypto.internal.mlkem768

> C# package converted from the Go standard library by [go2cs](https://github.com/ritchiecarroll/go2cs).
> Go version: 1.23.1

Package mlkem768 implements the quantum-resistant key encapsulation method ML-KEM (formerly known as Kyber).

Only the recommended ML-KEM-768 parameter set is provided.

The version currently implemented is the one specified by [NIST FIPS 203 ipd](https://doi.org/10.6028/NIST.FIPS.203.ipd), with the unintentional transposition of the matrix A reverted to match the behavior of [Kyber version 3.0](https://pq-crystals.org/kyber/data/kyber-specification-round3-20210804.pdf). Future versions of this package might introduce backwards incompatible changes to implement changes to FIPS 203.

---
Part of the go2cs converted Go standard library. See the [repository](https://github.com/ritchiecarroll/go2cs) for usage and details.

Copyright 2009 The Go Authors. All rights reserved. This C# package is converted from Go standard library source; use of that source is governed by a BSD-style license that can be found in the [LICENSE](https://github.com/ritchiecarroll/go2cs/blob/master/src/go-src-converted/LICENSE) file. The go2cs conversion itself is distributed under the MIT license.

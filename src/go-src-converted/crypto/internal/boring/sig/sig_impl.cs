//******************************************************************************************************
//  sig_impl.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/20/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Hand-written implementation of crypto/internal/boring/sig's three "code signature" markers. Go
// declares them bodyless in sig.go and implements them ONLY in assembly (sig_amd64.s / sig_other.s)
// for every platform — there is NO purego variant, so no build tag can give them a Go body. go2cs
// therefore emits three bodyless `partial` methods and the PartialStubGenerator fills each with a
// throwing stub, which is why crypto/sha256's Sum256 died with "StandardCrypto: external (assembly
// or cgo) function is not implemented" before it could hash a single byte.
//
// An EMPTY body is the semantically exact port, not an approximation. Go's own comment says "The
// functions themselves are no-ops": each assembly stub is a two-byte jump over a fixed 29-byte magic
// sequence to a RET, and exists purely so a tool (rsc.io/goversion) can grep a linked BINARY to find
// out whether BoringCrypto / FIPS-only / standard crypto was linked in. That byte-signature purpose
// has no meaning in a managed assembly, and the observable runtime behavior of calling one — do
// nothing, return — is reproduced exactly here.

namespace go.crypto.@internal.boring;

partial class sig_package
{
    // BoringCrypto indicates that the BoringCrypto module is present.
    public static partial void BoringCrypto()
    {
    }

    // FIPSOnly indicates that package crypto/tls/fipsonly is present.
    public static partial void FIPSOnly()
    {
    }

    // StandardCrypto indicates that standard Go crypto is present.
    public static partial void StandardCrypto()
    {
    }
}

// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
Package hmac implements the Keyed-Hash Message Authentication Code (HMAC) as
defined in U.S. Federal Information Processing Standards Publication 198.
An HMAC is a cryptographic hash that uses a key to sign a message.
The receiver verifies the hash by recomputing it using the same key.

Receivers should be careful to use Equal to compare MACs in order to avoid
timing side-channels:

	// ValidMAC reports whether messageMAC is a valid HMAC tag for message.
	func ValidMAC(message, messageMAC, key []byte) bool {
		mac := hmac.New(sha256.New, key)
		mac.Write(message)
		expectedMAC := mac.Sum(nil)
		return hmac.Equal(messageMAC, expectedMAC)
	}
*/
namespace go.crypto;

using boring = crypto.@internal.boring_package;
using subtle = crypto.subtle_package;
using hash = hash_package;
using crypto.@internal;

partial class hmac_package {

// FIPS 198-1:
// https://csrc.nist.gov/publications/fips/fips198-1/FIPS-198-1_final.pdf
// key is zero padded to the block size of the hash function
// ipad = 0x36 byte repeated for key length
// opad = 0x5c byte repeated for key length
// hmac = H([key ^ opad] H([key ^ ipad] text))

// marshalable is the combination of encoding.BinaryMarshaler and
// encoding.BinaryUnmarshaler. Their method definitions are repeated here to
// avoid a dependency on the encoding package.
[GoType] partial interface marshalable {
    (slice<byte>, error) MarshalBinary();
    error UnmarshalBinary(slice<byte> _);
}

[GoType] partial struct hmac {
    internal slice<byte> opad;
    internal slice<byte> ipad;
    internal hash_package.Hash outer;
    internal hash_package.Hash inner;
    // If marshaled is true, then opad and ipad do not contain a padded
    // copy of the key, but rather the marshaled state of outer/inner after
    // opad/ipad has been fed into it.
    internal bool marshaled;
}

[GoRecv] internal static slice<byte> Sum(this ref hmac h, slice<byte> @in) {
    nint origLen = len(@in);
    @in = h.inner.Sum(@in);
    if (h.marshaled){
        {
            var err = h.outer._<marshalable>().UnmarshalBinary(h.opad); if (err != default!) {
                throw panic(err);
            }
        }
    } else {
        h.outer.Reset();
        h.outer.Write(h.opad);
    }
    h.outer.Write(@in[(int)(origLen)..]);
    return h.outer.Sum(@in[..(int)(origLen)]);
}

[GoRecv] internal static (nint n, error err) Write(this ref hmac h, slice<byte> p) {
    nint n = default!;
    error err = default!;

    return h.inner.Write(p);
}

[GoRecv] internal static nint Size(this ref hmac h) {
    return h.outer.Size();
}

[GoRecv] internal static nint BlockSize(this ref hmac h) {
    return h.inner.BlockSize();
}

[GoRecv] internal static void Reset(this ref hmac h) {
    if (h.marshaled) {
        {
            var errΔ1 = h.inner._<marshalable>().UnmarshalBinary(h.ipad); if (errΔ1 != default!) {
                throw panic(errΔ1);
            }
        }
        return;
    }
    h.inner.Reset();
    h.inner.Write(h.ipad);
    // If the underlying hash is marshalable, we can save some time by
    // saving a copy of the hash state now, and restoring it on future
    // calls to Reset and Sum instead of writing ipad/opad every time.
    //
    // If either hash is unmarshalable for whatever reason,
    // it's safe to bail out here.
    var (marshalableInner, innerOK) = h.inner._<marshalable>(ᐧ);
    if (!innerOK) {
        return;
    }
    var (marshalableOuter, outerOK) = h.outer._<marshalable>(ᐧ);
    if (!outerOK) {
        return;
    }
    (imarshal, err) = marshalableInner.MarshalBinary();
    if (err != default!) {
        return;
    }
    h.outer.Reset();
    h.outer.Write(h.opad);
    (omarshal, err) = marshalableOuter.MarshalBinary();
    if (err != default!) {
        return;
    }
    // Marshaling succeeded; save the marshaled state for later
    h.ipad = imarshal;
    h.opad = omarshal;
    h.marshaled = true;
}

// New returns a new HMAC hash using the given [hash.Hash] type and key.
// New functions like sha256.New from [crypto/sha256] can be used as h.
// h must return a new Hash every time it is called.
// Note that unlike other hash implementations in the standard library,
// the returned Hash does not implement [encoding.BinaryMarshaler]
// or [encoding.BinaryUnmarshaler].
public static hash.Hash New(Func<hash.Hash> h, slice<byte> key) => func((defer, recover) => {
    if (boring.Enabled) {
        var hmΔ1 = boring.NewHMAC(h, key);
        if (hmΔ1 != default!) {
            return hmΔ1;
        }
    }
    // BoringCrypto did not recognize h, so fall through to standard Go code.
    var hm = @new<hmac>();
    hm.val.outer = h();
    hm.val.inner = h();
    var unique = true;
    
    var hmʗ1 = hm;
    () => {
        defer(() => {
            // The comparison might panic if the underlying types are not comparable.
            _ = recover();
        });
        if (AreEqual((~hm).outer, (~hm).inner)) {
            unique = false;
        }
    }();
    if (!unique) {
        throw panic("crypto/hmac: hash generation function does not produce unique values");
    }
    nint blocksize = (~hm).inner.BlockSize();
    hm.val.ipad = new slice<byte>(blocksize);
    hm.val.opad = new slice<byte>(blocksize);
    if (len(key) > blocksize) {
        // If key is too big, hash it.
        (~hm).outer.Write(key);
        key = (~hm).outer.Sum(default!);
    }
    copy((~hm).ipad, key);
    copy((~hm).opad, key);
    foreach (var (i, _) in (~hm).ipad) {
        (~hm).ipad[i] ^= (byte)(54);
    }
    foreach (var (i, _) in (~hm).opad) {
        (~hm).opad[i] ^= (byte)(92);
    }
    (~hm).inner.Write((~hm).ipad);
    return ~hm;
});

// Equal compares two MACs for equality without leaking timing information.
public static bool Equal(slice<byte> mac1, slice<byte> mac2) {
    // We don't have to be constant time if the lengths of the MACs are
    // different as that suggests that a completely different hash function
    // was used.
    return subtle.ConstantTimeCompare(mac1, mac2) == 1;
}

} // end hmac_package

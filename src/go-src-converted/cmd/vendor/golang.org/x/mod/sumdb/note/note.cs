// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package note defines the notes signed by the Go module database server.
//
// A note is text signed by one or more server keys.
// The text should be ignored unless the note is signed by
// a trusted server key and the signature has been verified
// using the server's public key.
//
// A server's public key is identified by a name, typically the "host[/path]"
// giving the base URL of the server's transparency log.
// The syntactic restrictions on a name are that it be non-empty,
// well-formed UTF-8 containing neither Unicode spaces nor plus (U+002B).
//
// A Go module database server signs texts using public key cryptography.
// A given server may have multiple public keys, each
// identified by the first 32 bits of the SHA-256 hash of
// the concatenation of the server name, a newline, and
// the encoded public key.
//
// Verifying Notes
//
// A Verifier allows verification of signatures by one server public key.
// It can report the name of the server and the uint32 hash of the key,
// and it can verify a purported signature by that key.
//
// The standard implementation of a Verifier is constructed
// by NewVerifier starting from a verifier key, which is a
// plain text string of the form "<name>+<hash>+<keydata>".
//
// A Verifiers allows looking up a Verifier by the combination
// of server name and key hash.
//
// The standard implementation of a Verifiers is constructed
// by VerifierList from a list of known verifiers.
//
// A Note represents a text with one or more signatures.
// An implementation can reject a note with too many signatures
// (for example, more than 100 signatures).
//
// A Signature represents a signature on a note, verified or not.
//
// The Open function takes as input a signed message
// and a set of known verifiers. It decodes and verifies
// the message signatures and returns a Note structure
// containing the message text and (verified or unverified) signatures.
//
// Signing Notes
//
// A Signer allows signing a text with a given key.
// It can report the name of the server and the hash of the key
// and can sign a raw text using that key.
//
// The standard implementation of a Signer is constructed
// by NewSigner starting from an encoded signer key, which is a
// plain text string of the form "PRIVATE+KEY+<name>+<hash>+<keydata>".
// Anyone with an encoded signer key can sign messages using that key,
// so it must be kept secret. The encoding begins with the literal text
// "PRIVATE+KEY" to avoid confusion with the public server key.
//
// The Sign function takes as input a Note and a list of Signers
// and returns an encoded, signed message.
//
// Signed Note Format
//
// A signed note consists of a text ending in newline (U+000A),
// followed by a blank line (only a newline),
// followed by one or more signature lines of this form:
// em dash (U+2014), space (U+0020),
// server name, space, base64-encoded signature, newline.
//
// Signed notes must be valid UTF-8 and must not contain any
// ASCII control characters (those below U+0020) other than newline.
//
// A signature is a base64 encoding of 4+n bytes.
//
// The first four bytes in the signature are the uint32 key hash
// stored in big-endian order, which is to say they are the first
// four bytes of the truncated SHA-256 used to derive the key hash
// in the first place.
//
// The remaining n bytes are the result of using the specified key
// to sign the note text (including the final newline but not the
// separating blank line).
//
// Generating Keys
//
// There is only one key type, Ed25519 with algorithm identifier 1.
// New key types may be introduced in the future as needed,
// although doing so will require deploying the new algorithms to all clients
// before starting to depend on them for signatures.
//
// The GenerateKey function generates and returns a new signer
// and corresponding verifier.
//
// Example
//
// Here is a well-formed signed note:
//
//    If you think cryptography is the answer to your problem,
//    then you don't know what your problem is.
//
//    — PeterNeumann x08go/ZJkuBS9UG/SffcvIAQxVBtiFupLLr8pAcElZInNIuGUgYN1FFYC2pZSNXgKvqfqdngotpRZb6KE6RyyBwJnAM=
//
// It can be constructed and displayed using:
//
//    skey := "PRIVATE+KEY+PeterNeumann+c74f20a3+AYEKFALVFGyNhPJEMzD1QIDr+Y7hfZx09iUvxdXHKDFz"
//    text := "If you think cryptography is the answer to your problem,\n" +
//        "then you don't know what your problem is.\n"
//
//    signer, err := note.NewSigner(skey)
//    if err != nil {
//        log.Fatal(err)
//    }
//
//    msg, err := note.Sign(&note.Note{Text: text}, signer)
//    if err != nil {
//        log.Fatal(err)
//    }
//    os.Stdout.Write(msg)
//
// The note's text is two lines, including the final newline,
// and the text is purportedly signed by a server named
// "PeterNeumann". (Although server names are canonically
// base URLs, the only syntactic requirement is that they
// not contain spaces or newlines).
//
// If Open is given access to a Verifiers including the
// Verifier for this key, then it will succeed at verifiying
// the encoded message and returning the parsed Note:
//
//    vkey := "PeterNeumann+c74f20a3+ARpc2QcUPDhMQegwxbzhKqiBfsVkmqq/LDE4izWy10TW"
//    msg := []byte("If you think cryptography is the answer to your problem,\n" +
//        "then you don't know what your problem is.\n" +
//        "\n" +
//        "— PeterNeumann x08go/ZJkuBS9UG/SffcvIAQxVBtiFupLLr8pAcElZInNIuGUgYN1FFYC2pZSNXgKvqfqdngotpRZb6KE6RyyBwJnAM=\n")
//
//    verifier, err := note.NewVerifier(vkey)
//    if err != nil {
//        log.Fatal(err)
//    }
//    verifiers := note.VerifierList(verifier)
//
//    n, err := note.Open([]byte(msg), verifiers)
//    if err != nil {
//        log.Fatal(err)
//    }
//    fmt.Printf("%s (%08x):\n%s", n.Sigs[0].Name, n.Sigs[0].Hash, n.Text)
//
// You can add your own signature to this message by re-signing the note:
//
//    skey, vkey, err := note.GenerateKey(rand.Reader, "EnochRoot")
//    if err != nil {
//        log.Fatal(err)
//    }
//    _ = vkey // give to verifiers
//
//    me, err := note.NewSigner(skey)
//    if err != nil {
//        log.Fatal(err)
//    }
//
//    msg, err := note.Sign(n, me)
//    if err != nil {
//        log.Fatal(err)
//    }
//    os.Stdout.Write(msg)
//
// This will print a doubly-signed message, like:
//
//    If you think cryptography is the answer to your problem,
//    then you don't know what your problem is.
//
//    — PeterNeumann x08go/ZJkuBS9UG/SffcvIAQxVBtiFupLLr8pAcElZInNIuGUgYN1FFYC2pZSNXgKvqfqdngotpRZb6KE6RyyBwJnAM=
//    — EnochRoot rwz+eBzmZa0SO3NbfRGzPCpDckykFXSdeX+MNtCOXm2/5n2tiOHp+vAF1aGrQ5ovTG01oOTGwnWLox33WWd1RvMc+QQ=
//

// package note -- go2cs converted at 2022 March 13 06:41:07 UTC
// import "cmd/vendor/golang.org/x/mod/sumdb/note" ==> using note = go.cmd.vendor.golang.org.x.mod.sumdb.note_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\mod\sumdb\note\note.go
namespace go.cmd.vendor.golang.org.x.mod.sumdb;

using bytes = bytes_package;
using sha256 = crypto.sha256_package;
using base64 = encoding.base64_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using strconv = strconv_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;

using ed25519 = golang.org.x.crypto.ed25519_package;


// A Verifier verifies messages signed with a specific key.

using System;
public static partial class note_package {

public partial interface Verifier {
    bool Name(); // KeyHash returns the key hash.
    bool KeyHash(); // Verify reports whether sig is a valid signature of msg.
    bool Verify(slice<byte> msg, slice<byte> sig);
}

// A Signer signs messages using a specific key.
public partial interface Signer {
    (slice<byte>, error) Name(); // KeyHash returns the key hash.
    (slice<byte>, error) KeyHash(); // Sign returns a signature for the given message.
    (slice<byte>, error) Sign(slice<byte> msg);
}

// keyHash computes the key hash for the given server name and encoded public key.
private static uint keyHash(@string name, slice<byte> key) {
    var h = sha256.New();
    h.Write((slice<byte>)name);
    h.Write((slice<byte>)"\n");
    h.Write(key);
    var sum = h.Sum(null);
    return binary.BigEndian.Uint32(sum);
}

private static var errVerifierID = errors.New("malformed verifier id");private static var errVerifierAlg = errors.New("unknown verifier algorithm");private static var errVerifierHash = errors.New("invalid verifier hash");

private static readonly nint algEd25519 = 1;

// isValidName reports whether name is valid.
// It must be non-empty and not have any Unicode spaces or pluses.
private static bool isValidName(@string name) {
    return name != "" && utf8.ValidString(name) && strings.IndexFunc(name, unicode.IsSpace) < 0 && !strings.Contains(name, "+");
}

// NewVerifier construct a new Verifier from an encoded verifier key.
public static (Verifier, error) NewVerifier(@string vkey) {
    Verifier _p0 = default;
    error _p0 = default!;

    var (name, vkey) = chop(vkey, "+");
    var (hash16, key64) = chop(vkey, "+");
    var (hash, err1) = strconv.ParseUint(hash16, 16, 32);
    var (key, err2) = base64.StdEncoding.DecodeString(key64);
    if (len(hash16) != 8 || err1 != null || err2 != null || !isValidName(name) || len(key) == 0) {
        return (null, error.As(errVerifierID)!);
    }
    if (uint32(hash) != keyHash(name, key)) {
        return (null, error.As(errVerifierHash)!);
    }
    ptr<verifier> v = addr(new verifier(name:name,hash:uint32(hash),));

    var alg = key[0];
    var key = key[(int)1..];

    if (alg == algEd25519) 
        if (len(key) != 32) {
            return (null, error.As(errVerifierID)!);
        }
        v.verify = (msg, sig) => ed25519.Verify(key, msg, sig);
    else 
        return (null, error.As(errVerifierAlg)!);
        return (v, error.As(null!)!);
}

// chop chops s at the first instance of sep, if any,
// and returns the text before and after sep.
// If sep is not present, chop returns before is s and after is empty.
private static (@string, @string) chop(@string s, @string sep) {
    @string before = default;
    @string after = default;

    var i = strings.Index(s, sep);
    if (i < 0) {
        return (s, "");
    }
    return (s[..(int)i], s[(int)i + len(sep)..]);
}

// verifier is a trivial Verifier implementation.
private partial struct verifier {
    public @string name;
    public uint hash;
    public Func<slice<byte>, slice<byte>, bool> verify;
}

private static @string Name(this ptr<verifier> _addr_v) {
    ref verifier v = ref _addr_v.val;

    return v.name;
}
private static uint KeyHash(this ptr<verifier> _addr_v) {
    ref verifier v = ref _addr_v.val;

    return v.hash;
}
private static bool Verify(this ptr<verifier> _addr_v, slice<byte> msg, slice<byte> sig) {
    ref verifier v = ref _addr_v.val;

    return v.verify(msg, sig);
}

// NewSigner constructs a new Signer from an encoded signer key.
public static (Signer, error) NewSigner(@string skey) {
    Signer _p0 = default;
    error _p0 = default!;

    var (priv1, skey) = chop(skey, "+");
    var (priv2, skey) = chop(skey, "+");
    var (name, skey) = chop(skey, "+");
    var (hash16, key64) = chop(skey, "+");
    var (hash, err1) = strconv.ParseUint(hash16, 16, 32);
    var (key, err2) = base64.StdEncoding.DecodeString(key64);
    if (priv1 != "PRIVATE" || priv2 != "KEY" || len(hash16) != 8 || err1 != null || err2 != null || !isValidName(name) || len(key) == 0) {
        return (null, error.As(errSignerID)!);
    }
    ptr<signer> s = addr(new signer(name:name,hash:uint32(hash),));

    slice<byte> pubkey = default;

    var alg = key[0];
    var key = key[(int)1..];

    if (alg == algEd25519) 
        if (len(key) != 32) {
            return (null, error.As(errSignerID)!);
        }
        key = ed25519.NewKeyFromSeed(key);
        pubkey = append(new slice<byte>(new byte[] { algEd25519 }), key[(int)32..]);
        s.sign = msg => (ed25519.Sign(key, msg), error.As(null!)!);
    else 
        return (null, error.As(errSignerAlg)!);
        if (uint32(hash) != keyHash(name, pubkey)) {
        return (null, error.As(errSignerHash)!);
    }
    return (s, error.As(null!)!);
}

private static var errSignerID = errors.New("malformed verifier id");private static var errSignerAlg = errors.New("unknown verifier algorithm");private static var errSignerHash = errors.New("invalid verifier hash");

// signer is a trivial Signer implementation.
private partial struct signer {
    public @string name;
    public uint hash;
    public Func<slice<byte>, (slice<byte>, error)> sign;
}

private static @string Name(this ptr<signer> _addr_s) {
    ref signer s = ref _addr_s.val;

    return s.name;
}
private static uint KeyHash(this ptr<signer> _addr_s) {
    ref signer s = ref _addr_s.val;

    return s.hash;
}
private static (slice<byte>, error) Sign(this ptr<signer> _addr_s, slice<byte> msg) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    ref signer s = ref _addr_s.val;

    return s.sign(msg);
}

// GenerateKey generates a signer and verifier key pair for a named server.
// The signer key skey is private and must be kept secret.
public static (@string, @string, error) GenerateKey(io.Reader rand, @string name) {
    @string skey = default;
    @string vkey = default;
    error err = default!;

    var (pub, priv, err) = ed25519.GenerateKey(rand);
    if (err != null) {
        return ("", "", error.As(err)!);
    }
    var pubkey = append(new slice<byte>(new byte[] { algEd25519 }), pub);
    var privkey = append(new slice<byte>(new byte[] { algEd25519 }), priv.Seed());
    var h = keyHash(name, pubkey);

    skey = fmt.Sprintf("PRIVATE+KEY+%s+%08x+%s", name, h, base64.StdEncoding.EncodeToString(privkey));
    vkey = fmt.Sprintf("%s+%08x+%s", name, h, base64.StdEncoding.EncodeToString(pubkey));
    return (skey, vkey, error.As(null!)!);
}

// NewEd25519VerifierKey returns an encoded verifier key using the given name
// and Ed25519 public key.
public static (@string, error) NewEd25519VerifierKey(@string name, ed25519.PublicKey key) {
    @string _p0 = default;
    error _p0 = default!;

    if (len(key) != ed25519.PublicKeySize) {
        return ("", error.As(fmt.Errorf("invalid public key size %d, expected %d", len(key), ed25519.PublicKeySize))!);
    }
    var pubkey = append(new slice<byte>(new byte[] { algEd25519 }), key);
    var hash = keyHash(name, pubkey);

    var b64Key = base64.StdEncoding.EncodeToString(pubkey);
    return (fmt.Sprintf("%s+%08x+%s", name, hash, b64Key), error.As(null!)!);
}

// A Verifiers is a collection of known verifier keys.
public partial interface Verifiers {
    (Verifier, error) Verifier(@string name, uint hash);
}

// An UnknownVerifierError indicates that the given key is not known.
// The Open function records signatures without associated verifiers as
// unverified signatures.
public partial struct UnknownVerifierError {
    public @string Name;
    public uint KeyHash;
}

private static @string Error(this ptr<UnknownVerifierError> _addr_e) {
    ref UnknownVerifierError e = ref _addr_e.val;

    return fmt.Sprintf("unknown key %s+%08x", e.Name, e.KeyHash);
}

// An ambiguousVerifierError indicates that the given name and hash
// match multiple keys passed to VerifierList.
// (If this happens, some malicious actor has taken control of the
// verifier list, at which point we may as well give up entirely,
// but we diagnose the problem instead.)
private partial struct ambiguousVerifierError {
    public @string name;
    public uint hash;
}

private static @string Error(this ptr<ambiguousVerifierError> _addr_e) {
    ref ambiguousVerifierError e = ref _addr_e.val;

    return fmt.Sprintf("ambiguous key %s+%08x", e.name, e.hash);
}

// VerifierList returns a Verifiers implementation that uses the given list of verifiers.
public static Verifiers VerifierList(params Verifier[] list) {
    list = list.Clone();

    var m = make(verifierMap);
    foreach (var (_, v) in list) {
        nameHash k = new nameHash(v.Name(),v.KeyHash());
        m[k] = append(m[k], v);
    }    return m;
}

private partial struct nameHash {
    public @string name;
    public uint hash;
}

private partial struct verifierMap { // : map<nameHash, slice<Verifier>>
}

private static (Verifier, error) Verifier(this verifierMap m, @string name, uint hash) {
    Verifier _p0 = default;
    error _p0 = default!;

    var (v, ok) = m[new nameHash(name,hash)];
    if (!ok) {
        return (null, error.As(addr(new UnknownVerifierError(name,hash))!)!);
    }
    if (len(v) > 1) {
        return (null, error.As(addr(new ambiguousVerifierError(name,hash))!)!);
    }
    return (v[0], error.As(null!)!);
}

// A Note is a text and signatures.
public partial struct Note {
    public @string Text; // text of note
    public slice<Signature> Sigs; // verified signatures
    public slice<Signature> UnverifiedSigs; // unverified signatures
}

// A Signature is a single signature found in a note.
public partial struct Signature {
    public @string Name;
    public uint Hash; // Base64 records the base64-encoded signature bytes.
    public @string Base64;
}

// An UnverifiedNoteError indicates that the note
// successfully parsed but had no verifiable signatures.
public partial struct UnverifiedNoteError {
    public ptr<Note> Note;
}

private static @string Error(this ptr<UnverifiedNoteError> _addr_e) {
    ref UnverifiedNoteError e = ref _addr_e.val;

    return "note has no verifiable signatures";
}

// An InvalidSignatureError indicates that the given key was known
// and the associated Verifier rejected the signature.
public partial struct InvalidSignatureError {
    public @string Name;
    public uint Hash;
}

private static @string Error(this ptr<InvalidSignatureError> _addr_e) {
    ref InvalidSignatureError e = ref _addr_e.val;

    return fmt.Sprintf("invalid signature for key %s+%08x", e.Name, e.Hash);
}

private static var errMalformedNote = errors.New("malformed note");private static var errInvalidSigner = errors.New("invalid signer");private static slice<byte> sigSplit = (slice<byte>)"\n\n";private static slice<byte> sigPrefix = (slice<byte>)"— ";

// Open opens and parses the message msg, checking signatures from the known verifiers.
//
// For each signature in the message, Open calls known.Verifier to find a verifier.
// If known.Verifier returns a verifier and the verifier accepts the signature,
// Open records the signature in the returned note's Sigs field.
// If known.Verifier returns a verifier but the verifier rejects the signature,
// Open returns an InvalidSignatureError.
// If known.Verifier returns an UnknownVerifierError,
// Open records the signature in the returned note's UnverifiedSigs field.
// If known.Verifier returns any other error, Open returns that error.
//
// If no known verifier has signed an otherwise valid note,
// Open returns an UnverifiedNoteError.
// In this case, the unverified note can be fetched from inside the error.
public static (ptr<Note>, error) Open(slice<byte> msg, Verifiers known) {
    ptr<Note> _p0 = default!;
    error _p0 = default!;

    if (known == null) { 
        // Treat nil Verifiers as empty list, to produce useful error instead of crash.
        known = VerifierList();
    }
    {
        nint i__prev1 = i;

        nint i = 0;

        while (i < len(msg)) {
            var (r, size) = utf8.DecodeRune(msg[(int)i..]);
            if (r < 0x20 && r != '\n' || r == utf8.RuneError && size == 1) {
                return (_addr_null!, error.As(errMalformedNote)!);
            }
            i += size;
        }

        i = i__prev1;
    } 

    // Must end with signature block preceded by blank line.
    var split = bytes.LastIndex(msg, sigSplit);
    if (split < 0) {
        return (_addr_null!, error.As(errMalformedNote)!);
    }
    var text = msg[..(int)split + 1];
    var sigs = msg[(int)split + 2..];
    if (len(sigs) == 0 || sigs[len(sigs) - 1] != '\n') {
        return (_addr_null!, error.As(errMalformedNote)!);
    }
    ptr<Note> n = addr(new Note(Text:string(text),)); 

    // Parse and verify signatures.
    // Ignore duplicate signatures.
    var seen = make_map<nameHash, bool>();
    var seenUnverified = make_map<@string, bool>();
    nint numSig = 0;
    while (len(sigs) > 0) { 
        // Pull out next signature line.
        // We know sigs[len(sigs)-1] == '\n', so IndexByte always finds one.
        i = bytes.IndexByte(sigs, '\n');
        var line = sigs[..(int)i];
        sigs = sigs[(int)i + 1..];

        if (!bytes.HasPrefix(line, sigPrefix)) {
            return (_addr_null!, error.As(errMalformedNote)!);
        }
        line = line[(int)len(sigPrefix)..];
        var (name, b64) = chop(string(line), " ");
        var (sig, err) = base64.StdEncoding.DecodeString(b64);
        if (err != null || !isValidName(name) || b64 == "" || len(sig) < 5) {
            return (_addr_null!, error.As(errMalformedNote)!);
        }
        var hash = binary.BigEndian.Uint32(sig[(int)0..(int)4]);
        sig = sig[(int)4..];

        numSig++;

        if (numSig > 100) { 
            // Avoid spending forever parsing a note with many signatures.
            return (_addr_null!, error.As(errMalformedNote)!);
        }
        var (v, err) = known.Verifier(name, hash);
        {
            ptr<UnknownVerifierError> (_, ok) = err._<ptr<UnknownVerifierError>>();

            if (ok) { 
                // Drop repeated identical unverified signatures.
                if (seenUnverified[string(line)]) {
                    continue;
                }
                seenUnverified[string(line)] = true;
                n.UnverifiedSigs = append(n.UnverifiedSigs, new Signature(Name:name,Hash:hash,Base64:b64));
                continue;
            }

        }
        if (err != null) {
            return (_addr_null!, error.As(err)!);
        }
        if (seen[new nameHash(name,hash)]) {
            continue;
        }
        seen[new nameHash(name,hash)] = true;

        var ok = v.Verify(text, sig);
        if (!ok) {
            return (_addr_null!, error.As(addr(new InvalidSignatureError(name,hash))!)!);
        }
        n.Sigs = append(n.Sigs, new Signature(Name:name,Hash:hash,Base64:b64));
    } 

    // Parsed and verified all the signatures.
    if (len(n.Sigs) == 0) {
        return (_addr_null!, error.As(addr(new UnverifiedNoteError(n))!)!);
    }
    return (_addr_n!, error.As(null!)!);
}

// Sign signs the note with the given signers and returns the encoded message.
// The new signatures from signers are listed in the encoded message after
// the existing signatures already present in n.Sigs.
// If any signer uses the same key as an existing signature,
// the existing signature is elided from the output.
public static (slice<byte>, error) Sign(ptr<Note> _addr_n, params Signer[] signers) {
    slice<byte> _p0 = default;
    error _p0 = default!;
    signers = signers.Clone();
    ref Note n = ref _addr_n.val;

    bytes.Buffer buf = default;
    if (!strings.HasSuffix(n.Text, "\n")) {
        return (null, error.As(errMalformedNote)!);
    }
    buf.WriteString(n.Text); 

    // Prepare signatures.
    bytes.Buffer sigs = default;
    var have = make_map<nameHash, bool>();
    foreach (var (_, s) in signers) {
        var name = s.Name();
        var hash = s.KeyHash();
        have[new nameHash(name,hash)] = true;
        if (!isValidName(name)) {
            return (null, error.As(errInvalidSigner)!);
        }
        var (sig, err) = s.Sign(buf.Bytes()); // buf holds n.Text
        if (err != null) {
            return (null, error.As(err)!);
        }
        array<byte> hbuf = new array<byte>(4);
        binary.BigEndian.PutUint32(hbuf[..], hash);
        var b64 = base64.StdEncoding.EncodeToString(append(hbuf[..], sig));
        sigs.WriteString("— ");
        sigs.WriteString(name);
        sigs.WriteString(" ");
        sigs.WriteString(b64);
        sigs.WriteString("\n");
    }    buf.WriteString("\n"); 

    // Emit existing signatures not replaced by new ones.
    foreach (var (_, list) in new slice<slice<Signature>>(new slice<Signature>[] { n.Sigs, n.UnverifiedSigs })) {
        {
            var sig__prev2 = sig;

            foreach (var (_, __sig) in list) {
                sig = __sig;
                name = sig.Name;
                hash = sig.Hash;
                if (!isValidName(name)) {
                    return (null, error.As(errMalformedNote)!);
                }
                if (have[new nameHash(name,hash)]) {
                    continue;
                } 
                // Double-check hash against base64.
                var (raw, err) = base64.StdEncoding.DecodeString(sig.Base64);
                if (err != null || len(raw) < 4 || binary.BigEndian.Uint32(raw) != hash) {
                    return (null, error.As(errMalformedNote)!);
                }
                buf.WriteString("— ");
                buf.WriteString(sig.Name);
                buf.WriteString(" ");
                buf.WriteString(sig.Base64);
                buf.WriteString("\n");
            }

            sig = sig__prev2;
        }
    }    buf.Write(sigs.Bytes());

    return (buf.Bytes(), error.As(null!)!);
}

} // end note_package

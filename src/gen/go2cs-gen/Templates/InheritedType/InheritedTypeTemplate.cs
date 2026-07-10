using System.Collections.Generic;
using System.Linq;
using static go2cs.Common;
using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal class InheritedTypeTemplate : TemplateBase
{
    // Template Parameters
    public required string ObjectName;
    public string ObjectKind = "struct";
    public bool ReadOnlyValue = true;
    public required string TypeName;
    public required string TargetTypeName;
    public string? TargetValueTypeName = null;
    public string? TargetTypeSize = null;
    public required string TypeClass;

    // For a defined type whose underlying is a STRUCT (`type winlibcall libcall`), the underlying
    // struct's fields are accessible on the named type in Go (`w.fn`). C# has no such access on the
    // wrapper, so the underlying struct's members are forwarded here as get/set properties over the
    // (mutable) `m_value`. Null/empty for every other inherited kind (slice/map/array/numeric/…).
    public List<(string typeName, string memberName, bool isReferenceType, bool isProperty)>? ForwardedStructMembers = null;

    // For a defined type whose underlying is ITSELF an array-backed [GoType] wrapper
    // (`type pallocBits pageBits`, `type pageBits [8]uint64`), the element type of that underlying
    // array — set to implement IArray<elem> on this wrapper as a view over `m_value` (golib `len()`
    // and indexing bind IArray). Null for every other inherited kind.
    public string? UnderlyingArrayElementType = null;

    private string ImplementedInterface => TypeClass switch
    {
        "Slice" => $" : ISlice<{TargetTypeName}>, ISupportMake<{ObjectName}>, ISliceWrap<{ObjectName}, {TargetTypeName}>",
        "Map" => $" : IMap<{TargetTypeName}, {TargetValueTypeName}>, ISupportMake<{ObjectName}>",
        "Channel" => $" : IChannel<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Array" => $" : IArray<{TargetTypeName}>, ISupportMake<{ObjectName}>",
        "Pointer" => $" : IPointer<{TargetTypeName}>",
        // Generic-math declarations so a [GoType num:] wrapper satisfies converter-emitted
        // numeric constraints as a type ARGUMENT — slices.Sort's `cmp.Ordered` maps to
        // IAddition/IEquality/IComparisonOperators (time.Duration in runtime/debug's
        // SetGCPercent sort was CS0315 ×3); the operators below already exist for every
        // numeric kind, only the interface declarations were missing (the golib uintptr
        // struct precedent).
        "Numeric" => NumericInterfaces,
        _ => UnderlyingArrayElementType is null ? "" : $" : IArray<{UnderlyingArrayElementType}>"
    };

    // A numeric wrapper over an INTEGER underlying (not float/complex) — the modulus, bitwise and
    // shift operators exist for it (NumericTypeTemplate.GetComplementOperator, same kind-gate), so
    // it can ALSO satisfy a converter-emitted `~integer` operator constraint that lifts to
    // IModulus/IBitwise/IShiftOperators. internal/trace's `type dataTable[EI ~uint64, E]`
    // instantiated with `type stringID uint64` was CS0315 ×48 on exactly these three interfaces.
    // Float/complex keep only the common set below (they have no %/&/<<). IShiftOperators uses the
    // BCL shape <T, int, T> — the shift count is int (see the converter's lifted-shift note).
    private bool IsIntegerNumeric => TypeClass == "Numeric" && !TypeName.StartsWith("float") && !TypeName.StartsWith("complex");

    private string NumericInterfaces
    {
        get
        {
            string interfaces = $" : global::System.IEquatable<{TargetTypeName}>, global::System.Numerics.IAdditionOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.ISubtractionOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IMultiplyOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IDivisionOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IEqualityOperators<{TargetTypeName}, {TargetTypeName}, bool>, global::System.Numerics.IComparisonOperators<{TargetTypeName}, {TargetTypeName}, bool>, global::System.Numerics.IIncrementOperators<{TargetTypeName}>, global::System.Numerics.IDecrementOperators<{TargetTypeName}>";

            if (IsIntegerNumeric)
                interfaces += $", global::System.Numerics.IModulusOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IBitwiseOperators<{TargetTypeName}, {TargetTypeName}, {TargetTypeName}>, global::System.Numerics.IShiftOperators<{TargetTypeName}, int, {TargetTypeName}>";

            return interfaces;
        }
    }

    private string InterfaceImplementation => TypeClass switch
    {
        "Slice" => ISliceTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName),
        "Map" => IMapTypeTemplate.Generate(ObjectName, TargetTypeName, TargetValueTypeName),
        "Channel" => IChannelTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName),
        "Array" => IArrayTypeTemplate.Generate(ObjectName, TypeName, TargetTypeName, TargetTypeSize),
        "Numeric" => NumericTypeTemplate.Generate(TypeName, TargetTypeName),
        "Pointer" => PointerTypeTemplate.Generate(ObjectName, TargetTypeName),
        _ => UnderlyingArrayElementType is null ? "" : IArrayViewTypeTemplate.Generate(TypeName, UnderlyingArrayElementType)
    };

    private string ValueGetter => TypeClass switch
    {
        "Array" => $"m_value ??= new {TypeName}({TargetTypeSize ?? "0"})",
        // The array-view wrapper's `Value` must route through `view` (ensure the underlying's lazy
        // backing materializes on THIS wrapper's own m_value, then return a copy sharing that T[]):
        // the converter emits `b.Value[i] = v` inside the wrapper's pointer-receiver methods, and a
        // plain by-value `m_value` would lazily allocate on the returned temp — silently dropping
        // every write on virgin storage (the pallocBits fill-loop shape).
        _ => UnderlyingArrayElementType is null ? "m_value" : "view"
    };

    private string Value => TypeClass switch
    {
        "Array" => "Value", // Null-coalescing property auto-creates array on first reference
        _ => "m_value"
    };

    // The Pointer class supplies its own ref-returning Value (PointerTypeTemplate); emitting the
    // base value property too is a duplicate member (CS0102).
    private string EqualityExpression => TypeClass switch
    {
        // A nil named pointer is a NULL reference (class) — left.Equals would NRE; Equals(a, b)
        // is null-safe (null == null is true, matching Go's nil == nil).
        "Pointer" => "Equals(left, right)",
        _ => "left.Equals(right)"
    };

    private string ValueProperty => TypeClass switch
    {
        "Pointer" => "",
        _ => $"        public {TypeName} Value => {ValueGetter};"
    };

    // A wrapper over golib's `uintptr` STRUCT needs its own bridges to the plain numeric world:
    // `gclinkptr x = 0` / `(gclinkptr)someNuint` would otherwise chain TWO user-defined
    // conversions (nuint→uintptr, uintptr→gclinkptr), which C# never composes. The nuint bridge
    // (plus UntypedInt for named untyped consts) restores the reachability the old
    // System.UIntPtr alias provided for free.
    // A named type over `any` (`type Symbol any` — plugin; also crypto.PublicKey/PrivateKey,
    // driver.Value, json.Token) cannot declare user-defined conversions to or from its
    // underlying: `any` is System.Object, the base type of everything (CS0553 ×2). No bridge is
    // lost — boxing already converts any value to object, `(Symbol)obj` is the unbox-cast, and
    // the `new Symbol(value)` constructor plus the NilType operators are unaffected.
    private string UnderlyingConversionOperators => TypeName is "any" or "object" ? "" :
        $$"""
                // Handle implicit conversions between '{{TypeName}}' and {{ObjectKind}} '{{ObjectName}}'
                public static implicit operator {{ObjectName}}({{TypeName}} value) => new {{ObjectName}}(value);

                public static implicit operator {{TypeName}}({{ObjectName}} value) => value.{{Value}};
        """;

    private string UintptrBridgeOperators => TypeName != "uintptr" ? "" :
        $"""

                public static implicit operator {ObjectName}(nuint value) => new {ObjectName}((uintptr)value);

                public static implicit operator nuint({ObjectName} value) => ((uintptr)value.{Value}).Value;

                public static implicit operator {ObjectName}(UntypedInt value) => new {ObjectName}((uintptr)(nuint)value);

        """;

    // A named type over a plain INTEGER needs the UntypedInt bridge for named untyped
    // consts: `(token)(endBlockMarker)` (compress/flate, CS0030 ×2) would otherwise chain
    // two user-defined conversions (UntypedInt→uint32, uint32→token), which C# never
    // composes. Floats keep their existing routes (untyped float consts render literally).
    private string UntypedIntBridgeOperator => TypeName is "byte" or "uint8" or "uint16" or "uint32" or "uint64" or "int8" or "int16" or "int32" or "int64" or "nint" or "nuint" or "rune" ?
        $"""

                public static implicit operator {ObjectName}(UntypedInt value) => new {ObjectName}(({TypeName})value);

        """ : "";

    // A named type over `string` is indexed and sub-sliced in Go (`tag[i]`, `tag[i:j]` -
    // reflect StructTag.Get); C# indexing never applies user-defined conversions, so the
    // wrapper forwards the @string surface: element indexers, a Range indexer returning the
    // WRAPPER (a Go sub-slice of a named string keeps the named type), Length for len(), and
    // the u8-literal bridge so span comparisons/assignments bind (census F5, CS0021 x14 +
    // CS0019 x2).
    private string StringSurfaceMembers => TypeName != "@string" ? "" :
        $$"""

                public byte this[int index] => {{Value}}[index];

                public byte this[nint index] => {{Value}}[index];

                public {{ObjectName}} this[global::System.Range range] => new {{ObjectName}}({{Value}}[range]);

                public nint Length => {{Value}}.Length;

                public static implicit operator {{ObjectName}}(global::System.ReadOnlySpan<byte> value) => new {{ObjectName}}(new @string(value));

        """;

    private string ToStringImplementation => TypeClass switch
    {
        "bool" => $"{Value}.ToString().ToLowerInvariant()",
        _ => $"{Value}.ToString()"
    };

    private string ReadOnly => ReadOnlyValue ? "readonly " : "";

    // Only the lazily-allocated Array backing needs a nullable slot (`m_value ??= new array(N)`).
    // Other mutable cases (a struct-forwarding named type) keep a non-nullable value slot — decoupled
    // from ReadOnlyValue so struct forwarding can be mutable yet non-nullable.
    private string Nullable => TypeClass == "Array" ? "?" : "";

    // Forwarding properties for a defined-type-over-struct, exposing the underlying struct's fields on
    // the wrapper. `m_value` is mutable (ReadOnlyValue=false) so a write through a ж<T>.Value ref —
    // `box.Value.fn = x`, where `box.Value` is `ref winlibcall` — invokes the setter on the real storage
    // and persists. A blank `_` field is unaddressable/unselectable in Go and would collide.
    private string ForwardedMembers
    {
        get
        {
            if (ForwardedStructMembers is null || ForwardedStructMembers.Count == 0)
                return "";

            IEnumerable<string> props = ForwardedStructMembers
                .Where(member => GetSimpleName(member.memberName) != "_")
                .Select(member => $"\r\n        public {member.typeName} {member.memberName} {{ get => m_value.{member.memberName}; set => m_value.{member.memberName} = value; }}");

            // The field-box accessors (`Ꮡfield`) that a plain struct's partial generates (used by the
            // converter's `receiver.of(Type.Ꮡfield)` field-address form — `&p.x` on a *pinnerBits, where
            // `type pinnerBits gcBits`) must exist on the WRAPPER type too, since the accessor names the
            // wrapper (`pinnerBits.Ꮡx`, CS0117 otherwise). Forward them as true refs THROUGH `m_value`
            // into the underlying struct's field — `ref instance.m_value.x` is a genuine ref chain into
            // the wrapper's own storage (no copy, so writes through the resulting box persist; `m_value`
            // is mutable whenever members are forwarded). Property members cannot be ref'd and get no
            // accessor (matching the plain-struct template, which only emits them for fields).
            IEnumerable<string> fieldRefs = ForwardedStructMembers
                .Where(member => GetSimpleName(member.memberName) != "_" && !member.isProperty)
                .Select(member => $"\r\n        public static ref {member.typeName} {AddressPrefix}{GetUnsanitizedIdentifier(member.memberName)}(ref {ObjectName} instance) => ref instance.m_value.{member.memberName};");

            return $"\r\n\r\n        // Forwarded fields of the underlying '{TypeName}'{string.Concat(props)}{string.Concat(fieldRefs)}";
        }
    }

    // The nil-constructed value of a defined-type-over-STRUCT wrapper must construct the wrapped
    // struct through its own NilType constructor: the wrapped struct may carry promoted-embed
    // boxes (readonly `ж<T>` fields only its constructors allocate), so a `default!` m_value
    // would NRE on the first forwarded promoted-member access. Every other inherited kind
    // (slice/map/array/numeric/…) keeps the plain default — its zero value is already correct.
    private string NilValueExpression => ForwardedStructMembers is null || ForwardedStructMembers.Count == 0 ?
        "default!" : $"new {TypeName}(nil)";

    // A C# constructor name must not carry the type's generic parameters (e.g. the constructor for
    // a generic named array type `vec<T>` is `vec(...)`, not `vec<T>(...)`). Non-generic types have
    // no '<' so ConstructorName equals ObjectName — emitting byte-identical output.
    private string ConstructorName
    {
        get
        {
            int angle = ObjectName.IndexOf('<');
            return angle < 0 ? ObjectName : ObjectName.Substring(0, angle);
        }
    }

    public override string TemplateBody =>
        $$"""
            [{{GeneratedCodeAttribute}}]
            {{Scope}} partial {{ObjectKind}} {{ObjectName}}{{ImplementedInterface}}
            {
                // Value of the {{ObjectKind}} '{{ObjectName}}'
                private {{ReadOnly}}{{TypeName}}{{Nullable}} m_value;
                {{InterfaceImplementation}}{{ForwardedMembers}}
                
                public {{ConstructorName}}({{TypeName}} value) => m_value = value;

                public {{ConstructorName}}(NilType _) => m_value = {{NilValueExpression}};

        {{ValueProperty}}
                public override string ToString() => {{ToStringImplementation}};
        
                public static bool operator ==({{ObjectName}} left, {{ObjectName}} right) => {{EqualityExpression}};
        
                public static bool operator !=({{ObjectName}} left, {{ObjectName}} right) => !(left == right);
        
        {{UnderlyingConversionOperators}}
                    {{UintptrBridgeOperators}}{{UntypedIntBridgeOperator}}{{StringSurfaceMembers}}
                // Handle comparisons between 'nil' and {{ObjectKind}} '{{ObjectName}}'
                public static bool operator ==({{ObjectName}} value, NilType nil) => value.Equals(default({{ObjectName}}));
        
                public static bool operator !=({{ObjectName}} value, NilType nil) => !(value == nil);
        
                public static bool operator ==(NilType nil, {{ObjectName}} value) => value == nil;
        
                public static bool operator !=(NilType nil, {{ObjectName}} value) => value != nil;
        
                public static implicit operator {{ObjectName}}(NilType nil) => default({{ObjectName}})!;
            }
        """;
}

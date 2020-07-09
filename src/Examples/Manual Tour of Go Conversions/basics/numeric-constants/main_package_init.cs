using System.Globalization;
using System.Numerics;

static partial class main_package
{
    // const Big = 1 << 100
    private static BigInteger? __Big__value;
    private static BigInteger __init__Big() => __Big__value ??= BigInteger.Parse("1", NumberStyles.Float) << 100;
    
    public const float Big__f32 = 1267650600228229401496703205376F;
    public const double Big__f64 = 1267650600228229401496703205376D;

    private static readonly Complex? __Big_c64;
    public static readonly Complex Big_c64 = __Big_c64 ??= new Complex(Big__f32, 0.0F);

    private static readonly Complex? __Big_c128;
    public static readonly Complex Big_c128 = __Big_c128 ??= new Complex(Big__f64, 0.0D);

    // const Small = Big >> 99
    private static BigInteger? __Small__value;
    private static BigInteger __init__Small() => __Small__value ??= Big >> 99;

    public const int Small__i = 2;
    public const byte Small__ui8 = 2;
    public const sbyte Small__i8 = 2;
    public const short Small__i16 = 2;
    public const ushort Small__ui16 = 2;
    public const int Small__i32 = 2;
    public const uint Small__ui32 = 2U;
    public const long Small__i64 = 2L;
    public const ulong Small__ui64 = 2UL;
    public const float Small__f32 = 2F;
    public const double Small__f64 = 2D;

    private static readonly Complex? __Small__c64;
    public static readonly Complex Small__c64 = __Small__c64 ??= new Complex(Big__f32, 0.0F);

    private static readonly Complex? __Small__c128;
    public static readonly Complex Small__c128 = __Small__c128 ??= new Complex(Big__f64, 0.0D);
}

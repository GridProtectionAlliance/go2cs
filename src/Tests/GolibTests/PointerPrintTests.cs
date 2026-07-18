using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

[TestClass]
public class PointerPrintTests
{
    // R9 (strings TestClone): a ж<T> array-element reference can sit OUTSIDE its backing store's
    // valid range — unsafe.StringData/SliceData over an empty backing, or pointer arithmetic to
    // one-past-the-end. Printing such a pointer must yield an address-like token, never throw
    // (the printer used to dereference the element, throwing IndexOutOfRange and killing the
    // test host). No Go-parity behavioral test can express the out-of-range shape today —
    // unsafe.StringData of an empty string now returns nil, and the Add-through-unsafe.Pointer
    // seam loses the array box — so the property is guarded HERE at the golib level, via the
    // internal element-reference constructor those runtime paths use. The Go-expressible
    // surfaces (StringData nil identity, in-range pointer print shape) are guarded by the
    // UnsafePointerPrint behavioral test.

    [TestMethod]
    public void PrintPointerOnePastEndElementReferenceDoesNotThrow()
    {
        array<byte> backing = new(1);
        ж<byte> oob = new(backing, 1); // one-past-the-end

        string text = oob.ToString();

        StringAssert.StartsWith(text, "0x");
    }

    [TestMethod]
    public void PrintPointerEmptyBackingZeroIndexDoesNotThrow()
    {
        array<byte> backing = new(0);
        ж<byte> oob = new(backing, 0); // zero index of an EMPTY backing (unsafe.StringData shape)

        string text = oob.ToString();

        StringAssert.StartsWith(text, "0x");
    }

    [TestMethod]
    public void PrintPointerInRangeElementReferenceStillPrintsAddressToken()
    {
        array<byte> backing = new(1);
        ж<byte> element = new(backing, 0);

        string text = element.ToString();

        StringAssert.StartsWith(text, "0x");
    }
}

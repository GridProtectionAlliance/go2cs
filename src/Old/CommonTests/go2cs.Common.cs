using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go2cs.Common;

namespace Common
{
    [TestClass]
    public class go2cs_Common
    {
        [TestMethod]
        public void TestReplaceOctalBytes()
        {
            ushort Max3DigitOctal = Convert.ToUInt16("777", 8);

            for (ushort i = 0; i <= Max3DigitOctal; i++)
            {
                string octalValue = $"\\{Convert.ToString(i, 8).PadLeft(3, '0')}";
                string hexValue = ReplaceOctalBytes(octalValue);
                ushort value = hexValue[0];
                Assert.AreEqual(value, i);
            }

            string example = "\\111abc\\222xyz\\777";
            string result = ReplaceOctalBytes(example);
            string expected = "Iabc\u0092xyzǿ";
            Assert.AreEqual(result, expected);
        }
    }
}

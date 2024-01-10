using Assets.Tests.PlayModeTests;
using NUnit.Framework;
using Plugins.CountlySDK;

namespace Tests
{
    public class SC_UA_UtilsTests
    {
        // 'SafeRandomVal' in CountlyUtils
        // Generates a random value which matches with required pattern
        // Generator should produce different values each time with given pattern.
        [Test]
        public void UA_001_validatingIDGenerator()
        {
            string result1 = CountlyUtils.SafeRandomVal();
            string result2 = CountlyUtils.SafeRandomVal();

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.AreNotEqual(result1, result2);
            Assert.IsTrue(TestUtility.IsBase64String(result1));
            Assert.IsTrue(TestUtility.IsBase64String(result2));
            Assert.AreEqual(21, result2.Length, result1.Length);
        }
    }
}


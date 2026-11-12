using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assets.Tests.PlayModeTests
{
    public class RequestBuilderTests
    {
        // Helper: returns the value of a single key from a built "key=value&key=value" query string.
        private static string GetParamValue(string query, string key)
        {
            foreach (string part in query.Split('&')) {
                if (part.StartsWith(key + "=")) {
                    return part.Substring(key.Length + 1);
                }
            }
            return null;
        }

        // 'BuildQueryString' in RequestBuilder.
        // A segmentation value that itself contains '&' and '=' must stay inside its own
        // parameter and must NOT be split into separate top-level request parameters.
        [Test]
        public void BuildQueryString_segmentationValueWithReservedChars_isNotSplitIntoExtraParams()
        {
            string referrer = "utm_source=google&utm_medium=cpc&utm_term=1&utm_content=2&utm_campaign=3";
            string eventsJson = "[{\"key\":\"_install\",\"segmentation\":{\"referrer\":\"" + referrer + "\",\"android_id\":\"abc\"}}]";

            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "app_key", "KEY" },
                { "device_id", "DID" },
                { "events", eventsJson },
            };

            string result = new RequestBuilder().BuildQueryString(queryParams);

            // Exactly three top-level parameters; the UTM ampersands must not create new ones.
            Assert.AreEqual(3, result.Split('&').Length);

            // The events parameter must round-trip back to the original JSON once URL-decoded.
            string decodedEvents = Uri.UnescapeDataString(GetParamValue(result, "events"));
            Assert.AreEqual(eventsJson, decodedEvents);
        }

        // 'BuildQueryString' in RequestBuilder.
        // A plain value containing reserved characters round-trips intact and does not
        // introduce extra parameters.
        [Test]
        public void BuildQueryString_valueWithAmpersandAndEquals_roundTrips()
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "app_key", "KEY" },
                { "crash", "a=b&c=d" },
            };

            string result = new RequestBuilder().BuildQueryString(queryParams);

            Assert.AreEqual(2, result.Split('&').Length);
            Assert.AreEqual("a=b&c=d", Uri.UnescapeDataString(GetParamValue(result, "crash")));
        }

        // 'BuildQueryString' in RequestBuilder.
        // Normal values are emitted as a standard "key=value&key=value" string.
        [Test]
        public void BuildQueryString_normalValues_produceKeyValuePairs()
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "app_key", "KEY" },
                { "device_id", "DID" },
            };

            string result = new RequestBuilder().BuildQueryString(queryParams);

            Assert.AreEqual("app_key=KEY&device_id=DID", result);
        }

        // 'BuildQueryString' in RequestBuilder.
        // Null values and empty keys are skipped.
        [Test]
        public void BuildQueryString_skipsNullValuesAndEmptyKeys()
        {
            Dictionary<string, object> queryParams = new Dictionary<string, object>
            {
                { "app_key", "KEY" },
                { "nullValue", null },
                { "", "noKey" },
            };

            string result = new RequestBuilder().BuildQueryString(queryParams);

            Assert.AreEqual("app_key=KEY", result);
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}

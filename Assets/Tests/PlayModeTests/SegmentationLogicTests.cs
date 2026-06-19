using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.TestTools;
using Plugins.CountlySDK;
using Plugins.CountlySDK.Models;

namespace Assets.Tests.PlayModeTests
{
    /// <summary>
    /// Regression tests for event-recording / segmentation logic errors:
    ///  #1 AddSegmentationToView* self-copy (segmentation dropped on 2nd+ call)
    ///  #2 UserProfile custom property trim (FixSegmentKeysAndValues return discarded)
    ///  #3 Reserved view keys dropped when custom segmentation fills the value cap
    ///  #4 RemoveSegmentInvalidDataTypes mutating the caller-supplied dictionary
    ///  #6 EndEvent bypassing the sanitize / event-id pipeline
    /// </summary>
    public class SegmentationLogicTests
    {
        // #1: Adding segmentation to a view in two separate calls must merge BOTH calls' keys.
        [Test]
        public void AddSegmentationToViewWithID_SecondCall_IsNotDropped()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateViewConfig(new CustomIdProvider()));

            string viewId = cly.Views.StartView("viewA");
            cly.Events._eventRepo.Dequeue(); // discard the view-start event

            cly.Views.AddSegmentationToViewWithID(viewId, new Dictionary<string, object> { { "a", 1 } });
            cly.Views.AddSegmentationToViewWithID(viewId, new Dictionary<string, object> { { "b", 2 } });

            cly.Views.StopViewWithID(viewId);
            CountlyEventModel stopEvent = cly.Events._eventRepo.Dequeue();

            Assert.IsTrue(stopEvent.Segmentation.ContainsKey("a"), "First AddSegmentation key 'a' missing");
            Assert.IsTrue(stopEvent.Segmentation.ContainsKey("b"), "Second AddSegmentation key 'b' was dropped (self-copy bug)");
        }

        // #2: An over-long custom user-property value must be trimmed to MaxValueSize.
        [Test]
        public void SetProperties_OverlongCustomValue_IsTrimmed()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig().SetMaxValueSize(5));

            cly.UserProfile.SetProperties(new Dictionary<string, object> { { "customKey", "0123456789" } });
            _ = cly.Events.RecordEventAsync("trigger"); // triggers UserProfile.Save() -> builds user_details request

            Dictionary<string, object> up = TestUtility.ExtractAndDeserializeUserDetails(cly.RequestHelper._requestRepo.Models);
            Assert.IsTrue(up.ContainsKey("customKey"), "custom property missing from user_details");
            Assert.AreEqual("01234", up["customKey"], "custom property value was not trimmed to MaxValueSize");
        }

        // #4: RecordEventAsync must not mutate the caller-supplied segmentation dictionary.
        [UnityTest]
        public IEnumerator RecordEventAsync_DoesNotMutateCallerSegmentation()
        {
            Countly.Instance.Init(TestUtility.CreateBaseConfig());

            Dictionary<string, object> seg = new Dictionary<string, object> {
                { "valid", "x" },
                { "invalidDecimal", 9.99m } // decimal is not an allowed segmentation type -> filtered out
            };
            int originalCount = seg.Count;

            yield return Countly.Instance.Events.RecordEventAsync("e", segmentation: seg).AsCoroutine();

            Assert.AreEqual(originalCount, seg.Count, "caller's segmentation dictionary was mutated in place");
            Assert.IsTrue(seg.ContainsKey("invalidDecimal"), "caller's dictionary lost a key (in-place mutation)");
        }

        // #6: EndEvent must run the same sanitization + event-id pipeline as the normal record path.
        [Test]
        public void EndEvent_SanitizesSegmentationAndGeneratesEventId()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateBaseConfig());

            cly.Events.StartEvent("timed");

            Dictionary<string, object> seg = new Dictionary<string, object> {
                { "valid", "x" },
                { "invalidDecimal", 9.99m }
            };
            cly.Events.EndEvent("timed", seg, 1, 0);

            Assert.AreEqual(1, cly.Events._eventRepo.Count);
            CountlyEventModel model = cly.Events._eventRepo.Dequeue();

            Assert.IsTrue(model.Segmentation.ContainsKey("valid"), "valid segmentation key missing");
            Assert.IsFalse(model.Segmentation.ContainsKey("invalidDecimal"), "EndEvent did not sanitize invalid segmentation type");
            Assert.IsFalse(string.IsNullOrEmpty(model.EventID), "EndEvent did not generate an event id");
        }

        // #3: When custom segmentation fills the segmentation-count cap, the reserved view keys
        // (name/visit/start/segment) appended afterward must NOT be dropped by the cap.
        [Test]
        public void StartView_CustomSegmentationAtCap_KeepsReservedKeys()
        {
            Countly cly = Countly.Instance;
            cly.Init(TestUtility.CreateViewConfig(new CustomIdProvider()).SetMaxSegmentationValues(2));

            // 2 custom keys fill the cap; reserved keys are appended after them.
            var custom = new Dictionary<string, object> { { "c1", 1 }, { "c2", 2 } };
            cly.Views.StartView("viewA", custom);

            CountlyEventModel startEvent = cly.Events._eventRepo.Dequeue();
            Assert.IsTrue(startEvent.Segmentation.ContainsKey("name"), "reserved key 'name' was dropped by the cap");
            Assert.AreEqual("viewA", startEvent.Segmentation["name"]);
            Assert.IsTrue(startEvent.Segmentation.ContainsKey("segment"), "reserved key 'segment' was dropped by the cap");
        }

        [SetUp]
        [TearDown]
        public void End()
        {
            TestUtility.TestCleanup();
        }
    }
}

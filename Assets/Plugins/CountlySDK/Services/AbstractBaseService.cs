﻿
using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public abstract class AbstractBaseService
    {
        internal object LockObj { get; set; }
        internal List<AbstractBaseService> Listeners { get; set; }

        protected CountlyLogHelper Log { get; private set; }
        protected readonly CountlyConfiguration _configuration;
        protected readonly ConsentCountlyService _consentService;


        protected AbstractBaseService(CountlyConfiguration configuration, CountlyLogHelper logHelper, ConsentCountlyService consentService)
        {
            Log = logHelper;
            _configuration = configuration;
            _consentService = consentService;
        }

        protected IDictionary<string, object> RemoveSegmenInvalidetDataTypes(IDictionary<string, object> segments) {

            if (segments == null || segments.Count == 0) {
                return segments;
            }

            int i = 0;
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, object> item in segments) {
                if (++i > _configuration.MaxSegmentationValues) {
                    toRemove.Add(item.Key);
                    continue;
                }

                bool isValidDataType = item.Value != null
                    && (item.Value.GetType() == typeof(int)
                    || item.Value.GetType() == typeof(bool)
                    || item.Value.GetType() == typeof(float)
                    || item.Value.GetType() == typeof(double)
                    || item.Value.GetType() == typeof(string));


                if (!isValidDataType) {
                    toRemove.Add(item.Key);
                    Log.Warning("[" + GetType().Name + "] MainpulateSegments: In segmentation Data type '" + (item.Value?.GetType()) + "'  of item '" + item.Key + "' isn't valid.");
                }
            }

            foreach (string k in toRemove) {
                segments.Remove(k);
            }

            return segments;
        }

        protected IDictionary<string, object> FixSegmenKeysAndValues(IDictionary<string, object> segments) {
            if (segments == null || segments.Count == 0) {
                return segments;
            }

            IDictionary<string, object>  segmentation = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> item in segments) {
                string k = item.Key;
                object v = item.Value;

                if (k.Length > _configuration.MaxKeyLength) {
                    Log.Verbose("[" + GetType().Name + "] MainpulateSegments : Max allowed key length is " + _configuration.MaxKeyLength);
                    k = k.Substring(0, _configuration.MaxKeyLength);
                }

                if (v.GetType() == typeof(string) && ((string)v).Length > _configuration.MaxValueSize) {
                    Log.Verbose("[" + GetType().Name + "] MainpulateSegments : Max allowed value length is " + _configuration.MaxValueSize);
                    v = ((string)v).Substring(0, _configuration.MaxValueSize);
                }

                segmentation.Add(k, v);
            }

            return segmentation;
        }
        internal virtual void OnInitializationCompleted() { }
        internal virtual void DeviceIdChanged(string deviceId, bool merged) { }
        internal virtual void ConsentChanged(List<Consents> updatedConsents, bool newConsentValue) { }
    }

}

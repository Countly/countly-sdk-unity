using Assets.Plugin.Models;
using Assets.Plugin.Scripts.Development;
using UnityEngine;

namespace Assets.Plugin.Scripts.Testing
{
    public class Testing: MonoBehaviour
    {
        private static Countly _instance { get; set; }
        public static Countly Instance => _instance ??
            (_instance = new Countly(
                                "https://us-try.count.ly/",
                                "73a6570ef97d4cf9174a6aeb97a38e1c3f88d6d9",
                                "b019e1b8-584b-413c-81f6-5b801519c9f1"));

        public void Set()
        {
            CountlyUserDetailsModel.Set("Weight", "55");
        }

        public void SetOnce()
        {
            CountlyUserDetailsModel.SetOnce("Height", "6");
        }

        public void Increment()
        {
            CountlyUserDetailsModel.Increment("Weight");
        }

        public void IncrementBy()
        {
            CountlyUserDetailsModel.IncrementBy("Height", 5);
        }

        public void Mulitply()
        {
            CountlyUserDetailsModel.Multiply("Weight", 2);
        }

        public void Max()
        {
            CountlyUserDetailsModel.Max("Weight", 85);
        }

        public void Min()
        {
            CountlyUserDetailsModel.Min("Height", 5.5);
        }

        public void Push()
        {
            CountlyUserDetailsModel.Push("Mole", new string[] { "Left Cheek", "Back", "Toe", "Back of the Neck", "Back"});
        }

        public void PushUnique()
        {
            CountlyUserDetailsModel.PushUnique("Mole", new string[] { "Right Leg", "Right Leg", "Right Leg" });
        }

        public void Pull()
        {
            CountlyUserDetailsModel.Pull("Mole", new string[] { "Right Leg", "Back" });
        }

        public void Save()
        {
            CountlyUserDetailsModel.Save();
        }
    }
}

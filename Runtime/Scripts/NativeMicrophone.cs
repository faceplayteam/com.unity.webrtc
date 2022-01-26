using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    public class NativeMicrophone
    {
        public static string[] devices {
            get => WebRTC.Context.GetMicrophoneDevices();
        }

        public static void SelectDevice(string deviceName)
        {
            if (!WebRTC.Context.SetMicrophone(deviceName))
            {
                throw new Exception("WebRTC.Context.SetMicrophone is failed.");
            }
        }

        public static void SetVolume(float volume)
        {
            WebRTC.Context.SetMicrophoneVolume(volume);
        }
    }
}

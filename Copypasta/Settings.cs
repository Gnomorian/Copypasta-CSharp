using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Text;

namespace Copypasta {
    class Settings {

        // setting (defaults)
        //TODO: change this to a collection with strings as keys
        private int timerInterval = 200;
        private int maxClips = 10;

        // Registry key to save to
        private static string key = "HKEY_CURRENT_USER\\Copypasta";

        public Settings() {

        }

        public int getTimerInterval() {
            return timerInterval;
        }

        public int getMaxClips() {
            return maxClips;
        }


        /* get the setting in the registry or return -1 if its not set */
        private int GetSetting(string subKey) {
            object rInt = Registry.GetValue(key, subKey, -1);
            if (rInt == null) {
                return -1;
            }

            return (int)rInt;
        }

        /* Set application to run at startup  */
        private void RegisterInStartup(bool isChecked, string exePath) {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey
                    ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (isChecked) {
                registryKey.SetValue("Copypasta", exePath);
            } else {
                registryKey.DeleteValue("Copypasta");
            }
        }

        /* set the key in the registry to the given value */
        public void SetSetting(string subKey, int value) {
            Registry.SetValue(key, subKey, value);
            if (subKey == "max_clips") {
                maxClips = value;
            } else {
                timerInterval = value;
            }
        }
        /* Initialize settings from the registry */
        public void InitSettings() {
            int regKey = GetSetting("timer_interval");
            if (regKey == -1) {
                SetSetting("timer_interval", timerInterval);
                regKey = timerInterval;
            }
            timerInterval = regKey;

            regKey = GetSetting("max_clips");
            if (regKey == -1) {
                SetSetting("max_clips", maxClips);
                regKey = maxClips;
            }
            maxClips = regKey;
        }

    }
}

﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBHelperInjector
{
    public class Overrides
    {
        public static event OnDonationKeyChange OnSetDonationKey;

        public static event OnProxyChange OnSetProxy;

        public delegate void OnDonationKeyChange(string donationKey);

        public delegate void OnProxyChange(WebProxy proxy);

        public static string DonationKey { get; set; }

        public static WebProxy Proxy { get; set; }

        private static bool GetDonationKey(ref string __result)
        {
            if (DonationKey != null)
            {
                __result = DonationKey;
                return false;
            }
            return true;
        }

        private static bool GetProxy(ref WebProxy __result)
        {
            if (Proxy != null)
            {
                __result = Proxy;
                return false;
            }
            return true;
        }

        private static bool SetDonationKey(ref string value)
        {
            OnSetDonationKey?.Invoke(value);
            return true;
        }

        private static bool SetProxy(ref WebProxy value)
        {
            OnSetProxy?.Invoke(value);
            return true;
        }

        public static MethodInfo GetMethod(string name, params Type[] types)
        {
            return typeof(Overrides).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static, null, types, null);
        }

        // Forces a Proxy for all web requests
        [HarmonyPatch(typeof(ServicePointManager))]
        internal class ServicePointPatch
        {
            // This is intended to get FindServicePoint(Uri, IWebProxy, ProxyChain, HttpAbortDelegate, Int32)
            // without having to somehow reference all parameter types (some of which are not visible without reflection).
            static MethodBase TargetMethod()
            {
                return (from method in typeof(ServicePointManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                        where method.Name == "FindServicePoint"
                        && method.GetParameters().Count() == 5
                        select method).FirstOrDefault();
            }

            static bool Prefix(ref IWebProxy proxy)
            {
                if (Proxy != null)
                {
                    proxy = Proxy;
                }
                return true;
            }
        }
    }
}

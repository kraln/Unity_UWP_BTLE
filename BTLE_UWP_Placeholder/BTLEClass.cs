using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTLEPlugin
{
    /* 
     * This is a dummy class so that Unity can reason about the UWP plugin
     */
    public class BTLEClass
    {
        public delegate void DebugType(string s);
        public delegate void RxType(string rx);
        protected RxType rxFunc;
        protected DebugType debugFunc;
        public bool nordic_setup_properly = false;
        public bool verbose_debug = false;

        public void SetDebugCallback(DebugType theFunc)
        {
            debugFunc = theFunc;
        }
        protected void Debug(string what)
        {
            if (debugFunc == null)
                return;

            debugFunc(what);
        }
        public void StartEnumeration()
        {
            Debug("Starting enumeration (placeholder)");
        }

        public void StopEnumeration()
        {
            Debug("Stopping enumeration (placeholder)");
        }
        public void ClearSeenDevices()
        {
            Debug("Clearing seen devices");
        }
        public String[] DeviceList()
        {
            Debug("Device list");
            return null;
        }
        
        public bool Pair(string id)
        {
            Debug("BTLE Pairing (faked)");
            return false;
        }
        public bool paired_with(string id)
        {
            Debug("Paried with");
            return false;
        }
        public void Connect(string id)
        {
            Debug("BTLE Connecting (faked)");
        }
        public bool am_connected()
        {
            Debug("Am connected");
            return false;
        }
        public string[] getServices()
        {
            Debug("Services");
            return null;
        }

        public string[] getServicesAndCharacteristics()
        {
            Debug("Services and chars");
            return null;
        }
        public void send(string stuff)
        {
            Debug("Send: " + stuff);
        }
        public void SetupNordicUart()
        {
            Debug("Nordic Setup");
        }
        public void SetRxCallback(RxType theFunc)
        {
            Debug("Set callback");
        }
    }
}
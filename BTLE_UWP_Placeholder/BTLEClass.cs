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
        public bool paired = false;
        public bool connected = false;

        public delegate void DebugType(string s);
        public delegate void RxType(string rx);
        protected RxType rxFunc;
        protected DebugType debugFunc;
        #region debug
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
        #endregion
        #region Setup_and_Enumeration
        public void StartEnumeration()
        {
            Debug("Starting enumeration (placeholder)");
        }

        public void StopEnumeration()
        {
            Debug("Stopping enumeration (placeholder)");

        }


        // XXX
        public void ClearSeenDevices()
        {
            Debug("Clearing seen devices");
        }

        // XXX
        public String[] DeviceList()
        {
            List<string> temp = new List<String>();

       
            return temp.ToArray<string>();
        }

        // xxx
        /// <summary>
        /// Try to pair with a device
        /// </summary>
        /// <param name="id">the device id (as a string)</param>
        public void Pair(string id)
        {
            Debug("BTLE Pairing (faked)");
            paired = true;
        }
        // xxx
        public void Connect(string id)
        {
            Debug("BTLE Connecting (faked)");

            connected = true;
        }

        public string[] getServices()
        {
            if (!connected)
                return null;
            return null;
        }

        public string[] getServicesAndCharacteristics()
        {
            return null;
        }
#endregion

        // pair

        #region interaction
        /// <summary>
        /// Send stuff to the bluetooth module
        /// </summary>
        /// <param name="stuff"></param>
        public void send(string stuff)
        {

        }
        public void SetupNordicUart()
        {

        }

        public void SetRxCallback(RxType theFunc)
        {

        }

        #endregion
    }
}
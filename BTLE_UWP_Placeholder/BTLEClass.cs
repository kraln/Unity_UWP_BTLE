using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BTLEPlugin
{
    public class BTLEClass
    {
        public bool paired = false;
        public bool connected = false;

        public delegate void DebugType(string s);
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

        /// <summary>
        /// Get stuff from the bluetooth module
        /// </summary>
        /// <returns></returns>
        public string recv()
        {

            return "";
        }
        #endregion
    }
}
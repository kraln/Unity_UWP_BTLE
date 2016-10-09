using UnityEngine;
using System.Collections;
using BTLEPlugin;

public class TestBLE : MonoBehaviour
{

    const string Addr_Laptop = "BluetoothLE#BluetoothLE34:02:86:4b:42:fd-e4:83:bb:53:22:23";
    const string Addr_Holo =   "BluetoothLE#BluetoothLEb4:ae:2b:be:fc:09-e4:83:bb:53:22:23";

    const string Addr_Laptop2 = "BluetoothLE#BluetoothLE34:02:86:4b:42:fd-ec:01:c8:69:a6:1a";
    const string Addr_Holo2 =   "BluetoothLE#BluetoothLEb4:ae:2b:be:fc:09-ec:01:c8:69:a6:1a";

    string found_addr = "";
    BTLEClass btlec = new BTLEClass();
    bool found = false;
    bool oneshot = false;
    public void DebugLog(string s)
    {
        Debug.Log(s);
    }

    // will get called if the device sends stuff
    public void RxBTLE(string s)
    {
        Debug.Log("BTLE said: " + s);
        btlec.send("" + System.Convert.ToChar(int.Parse(s)));
    }

    void OnEnable()
    {
        btlec.SetDebugCallback(new BTLEClass.DebugType(DebugLog));
        btlec.StartEnumeration();
    }

    void OnDisable()
    {
        btlec.StopEnumeration();
    }

    void FixedUpdate()
    {
        if (btlec == null)
        {
            return;
        }
        if (!found)
        {
            string[] devices = btlec.DeviceList();
            foreach (string s in devices)
            {
                if (s.Equals(Addr_Holo))
                {
                    Debug.Log("Found Adafruit Dongle!");

                    found = true;
                    found_addr = Addr_Holo;
                    break;
                }
                if (s.Equals(Addr_Laptop))
                {
                    Debug.Log("Found Adafruit Dongle! (You're on your laptop)");

                    found = true;
                    found_addr = Addr_Laptop;
                    break;
                }
                if (s.Equals(Addr_Holo2))
                {
                    Debug.Log("Found 2nd Adafruit Dongle!");

                    found = true;
                    found_addr = Addr_Holo2;
                    break;
                }
                if (s.Equals(Addr_Laptop2))
                {
                    Debug.Log("Found 2nd Adafruit Dongle! (You're on your laptop)");

                    found = true;
                    found_addr = Addr_Laptop2;
                    break;
                }
            }
        }

        if (found)
        {
            // we can stop now
            // btlec.StopEnumeration();

            // pair?
            if (!btlec.paired_with(found_addr))
            {
                if(btlec.Pair(found_addr))
                {
                    Debug.Log("Pair successful, restarting");
                    btlec = new BTLEClass();
                    btlec.SetDebugCallback(new BTLEClass.DebugType(DebugLog));
                    btlec.StartEnumeration();
                    found = false;
                    oneshot = false;
                } else
                {
                    Debug.Log("Pair failed.");
                }
            }

            // connect?
            if (!btlec.am_connected())
            {
                btlec.Connect(found_addr);
            }

            // nordic / characteristics?
            if (btlec.am_connected() && !btlec.nordic_setup_properly)
            {
                btlec.SetupNordicUart();
            }

            // one-shot?
            if (btlec.am_connected() && btlec.nordic_setup_properly && !oneshot)
            {
                oneshot = true;

                // this is hacky and I love it.
                btlec.SetRxCallback(new BTLEClass.RxType(RxBTLE));

                Debug.Log("Services:");
                foreach (string s in btlec.getServices())
                {
                    Debug.Log(s);
                }
                //InvokeRepeating("SayHi", 1.0f, 1.0f);
            }
        }
    }
    int i = 1;
    void SayHi()
    {
        btlec.send("" + System.Convert.ToChar(i));
        Debug.Log("You're feeling #" + i);
        i++;
        if(i>117)
        {
            i = 1;
        }
    }
}

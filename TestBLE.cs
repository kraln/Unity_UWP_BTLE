using UnityEngine;
using System.Collections;
using BTLEPlugin;

public class TestBLE : MonoBehaviour
{

    const string Addr_Laptop = "BluetoothLE#BluetoothLE34:02:86:4b:42:fd-e4:83:bb:53:22:23";
    const string Addr_Holo = "BluetoothLE#BluetoothLEb4:ae:2b:be:fc:09-e4:83:bb:53:22:23";
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
            }
        }

        if (found)
        {
            // pair?
            if (!btlec.paired)
            {
                btlec.Pair(found_addr);
            }
            // connect?
            if (btlec.paired && !btlec.connected)
            {
                btlec.Connect(found_addr);
            }
        }

        if(btlec.connected && !oneshot)
        {
            oneshot = true;

            // this is hacky and I love it.
            btlec.SetRxCallback(new BTLEClass.RxType(RxBTLE));
            btlec.SetupNordicUart();

            Debug.Log("Services:");
            foreach(string s in btlec.getServices())
            {
                Debug.Log(s);
            }
            InvokeRepeating("SayHi", 1.0f, 1.0f);
        }

    }

    void SayHi()
    {
        Debug.Log("Saying hi!");
        btlec.send("Hi!\n");
    }
}
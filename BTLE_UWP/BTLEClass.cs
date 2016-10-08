using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BTLEPlugin
{
    public class BTLEClass
    {
        private DeviceWatcher deviceWatcher;
        private BluetoothLEDevice ble_dev;
        private ObservableCollection<DeviceInformation> devices = new ObservableCollection<DeviceInformation>();

        public bool paired = false;
        public bool connected = false;

        private bool busy = false;


        public delegate void DebugType(string s);
        protected DebugType debugFunc;

        public delegate void RxType(string rx);
        protected RxType rxFunc;

        GattCharacteristic uart_tx;
        GattCharacteristic uart_rx;

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
            if (deviceWatcher != null)
                return;

            Debug("Starting device enumeration");
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            deviceWatcher = DeviceInformation.CreateWatcher(
                         "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                         requestedProperties,
                         DeviceInformationKind.AssociationEndpoint);


            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start the watcher.
            deviceWatcher.Start();
        }

        public void StopEnumeration()
        {
            if (deviceWatcher == null)
                return;

            //UnityEngine.Debug.Log("Stopping device enumeration");
            deviceWatcher.Added -= DeviceWatcher_Added;
            deviceWatcher.Updated -= DeviceWatcher_Updated;
            deviceWatcher.Removed -= DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped -= DeviceWatcher_Stopped;
            deviceWatcher.Stop();
            deviceWatcher = null;
        }

        public void ClearSeenDevices()
        {
            Debug("Clearing seen devices");
            devices.Clear();
        }

        public String[] DeviceList()
        {
            List<string> temp = new List<string>();

            foreach(DeviceInformation di in devices)
            {
                temp.Add(di.Id);
            }
            return temp.ToArray<string>();
        }

        /// <summary>
        /// Try to pair with a device
        /// </summary>
        /// <param name="id">the device id (as a string)</param>
        public void Pair(string id)
        {
            if(busy)
            {
                return;
            }
            busy = true;
            Debug("Pairing!");
            RealPair(id).Wait();
            busy = false;
        }
        protected async Task RealPair(string id)
        {
            DeviceInformation di = null;

            foreach(DeviceInformation d in devices)
            {
                if (d.Id.Equals(id))
                { 
                    di = d;
                    break;
                }
            }

            if(di == null)
            {
                Debug("Could not find ID to pair with in seen set (did you try scanning?)");
                paired = false;
                return;
            }

            DevicePairingResult dpr = await di.Pairing.PairAsync();
            Debug("Pairing Status: " + dpr.Status);
            if (dpr.Status == DevicePairingResultStatus.Paired || dpr.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                paired = true;
                return;
            }

            paired = false;
            return;
        }

        public void Connect(string id)
        {
            if (busy)
            {
                return;
            }
            if(!paired)
            {
                return;
            }
            busy = true;
            Debug("Connecting!");
            RealConnect(id).Wait();
           
            busy = false;
        }
        public async Task RealConnect(string id)
        {
            try
            { 
                ble_dev = await BluetoothLEDevice.FromIdAsync(id);
            } catch (Exception ex)
            {
                Debug("Error when connecting: " + ex.ToString());
                connected = false;
                return;
            }

            if (ble_dev == null)
            {
                Debug("Connection failed :(");
                connected = false;

                return;
            }

            Debug("Connected! " + ble_dev.ConnectionStatus);
            connected = true;

            return;
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)

        {

            if (sender != deviceWatcher)
                return;


            if (deviceInfo.Name != string.Empty && !devices.Contains(deviceInfo))

            {
                devices.Add(deviceInfo);


            }
            // Debug("Saw device " + deviceInfo.Id + ", name: " + deviceInfo.Name);

        }



        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)

        {
            if (sender != deviceWatcher)
                return;
            // Debug("Saw device (updated)" + deviceInfoUpdate.Id);

        }



        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)

        {

            if (sender != deviceWatcher)
                return;

            DeviceInformation di_torem = null;
            foreach (DeviceInformation di in devices)
            {
                if (di.Id == deviceInfoUpdate.Id)
                {
                    di_torem = di;
                    break;
                }
            }

            if (di_torem != null)

            {
                devices.Remove(di_torem);


            }
            // Debug("Lost device " + deviceInfoUpdate.Id);


        }



        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)

        {

            Debug("Enumeration complete. " + devices.Count + " devices found.");


        }



        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)

        {

            Debug("Device enumeration stopped");

        }

        public string[] getServices()
        {
            if (!connected)
                return null;

            List<string> temp = new List<string>();

            foreach (var service in ble_dev.GattServices)
            {
                temp.Add(service.AttributeHandle + ": " + service.Uuid);
            }

            return temp.ToArray<string>();
        }

        public string[] getServicesAndCharacteristics()
        {
            if (!connected)
                return null;
            List<string> temp = new List<string>();

            foreach (var service in ble_dev.GattServices)
            {
                string serv = service.AttributeHandle + ": " + service.Uuid;

                foreach (var character in service.GetAllCharacteristics())
                { 
                    temp.Add(serv + ": " + character.Uuid);
                }
            }

            return temp.ToArray<string>();
        }

  
        /// <summary>
        /// If the device has the Nordic UART service/characteristic, we just skip a bunch of generic stuff and hardwire it because hackathon
        /// </summary>
        public void SetupNordicUart()
        {
            if (!connected)
                return;

            GattDeviceService nordic_uart_service = ble_dev.GetGattService(new Guid("6e400001-b5a3-f393-e0a9-e50e24dcca9e"));
            uart_tx = nordic_uart_service.GetCharacteristics(new Guid("6e400002-b5a3-f393-e0a9-e50e24dcca9e"))[0];
            uart_rx = nordic_uart_service.GetCharacteristics(new Guid("6e400003-b5a3-f393-e0a9-e50e24dcca9e"))[0];

            uart_rx.ValueChanged += Uart_rx_ValueChanged;
            Debug("Nordic UART Setup");

        }

        private void Uart_rx_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            Debug("Uart RX Value Changed");
            if (rxFunc == null)
            {
                Debug("Set your RX Callback!"); 
                return;
            }
            DataReader dr = DataReader.FromBuffer(args.CharacteristicValue);
            rxFunc(dr.ReadString(args.CharacteristicValue.Length));
        }

        public void SetRxCallback(RxType theFunc)
        {
            rxFunc = theFunc;
        }

        #endregion

        #region interaction
        /// <summary>
        /// Send stuff to the bluetooth module
        /// </summary>
        /// <param name="stuff"></param>
        public void send(string stuff)
        {
            if (!connected)
                return;

            if(uart_tx == null)
            {
                Debug("Set up the Nordic stuff first!");
                return;
            }

            var writer = new DataWriter();
            writer.WriteString(stuff);
            var res = uart_tx.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

        }

     
        #endregion
    }
}

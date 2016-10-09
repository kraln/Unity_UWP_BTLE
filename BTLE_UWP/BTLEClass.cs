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
        // The device we want to pair / connect with
        private BluetoothLEDevice ble_dev;

        #region debug
        
        // A delegate we can use to send Debugging information out
        public delegate void DebugType(string s);
        protected DebugType debugFunc;

        public bool verbose_debug = false;

        public void SetDebugCallback(DebugType theFunc)
        {
            debugFunc = theFunc;
        }

        protected void VerboseDebug(string what)
        {
            if(verbose_debug)
            {
                Debug(what);
            }
        }
        protected void Debug(string what)
        {
            if (debugFunc == null)
                return;

            debugFunc(what);

        }
        #endregion

        #region Discovery
        // Used for discovering devices
        private DeviceWatcher deviceWatcher;
        private ObservableCollection<DeviceInformation> devices = new ObservableCollection<DeviceInformation>();

        public void StartEnumeration()
        {
            if (deviceWatcher != null)
                return;

            Debug("Starting device enumeration");

            /* Don't ask me--look at the example from Microsoft */
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

            Debug("Stopping device enumeration");
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

        /// <summary>
        /// Get a list of the devices we've seen (so far!)
        /// </summary>
        /// <returns>Device Information Ids, as a string[]</returns>
        public String[] DeviceList()
        {
            List<string> temp = new List<string>();

            try
            { 
                foreach(DeviceInformation di in devices)
                {
                    temp.Add(di.Id);
                }
            }
            catch (Exception ex)
            {
                Debug("Issue iterating over devices (likely was modified)");
            }
            return temp.ToArray<string>();
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            if (sender != deviceWatcher)
                return;

            if (deviceInfo.Name != string.Empty && !devices.Contains(deviceInfo))
            {
                devices.Add(deviceInfo);
            }
            VerboseDebug("Saw device " + deviceInfo.Id + ", name: " + deviceInfo.Name);

        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (sender != deviceWatcher)
                return;
            VerboseDebug("Saw device (updated)" + deviceInfoUpdate.Id);
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
            VerboseDebug("Lost device " + deviceInfoUpdate.Id);
        }

        // TODO: Provide delegate for this?
        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            Debug("Enumeration complete. " + devices.Count + " devices found.");
        }

        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            Debug("Device enumeration stopped successfully.");
        }

        #endregion

        #region Pairing
        protected bool pairing_busy = false;

        /// <summary>
        /// Try to pair with a device by Id
        /// </summary>
        /// <param name="id">the device id (as a string)</param>
        public bool Pair(string id)
        {
            if(pairing_busy)
            {
                return false;
            }
            pairing_busy = true;

            Debug("Pairing!");

            // Actually attempt to pair
            bool res = AsyncPair(id).Result;

            pairing_busy = false;

            return res;
        }


        protected async Task<bool> AsyncPair(string id)
        {
            DeviceInformation di = null;

            /* 
             * First look in the set of scanned devices to see if what I am trying to pair with
             * is actually there
             * 
             * XXX: Is this needed?
             */
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
                return false;
            }

            /* Actually try to pair */
            DevicePairingResult dpr = await di.Pairing.PairAsync();

            Debug("Pairing Status: " + dpr.Status);

            if (dpr.Status == DevicePairingResultStatus.Paired || dpr.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                return true;
            }

            return false;
        }

        public bool paired_with(string id)
        {
            /* Check to see if we asked to connect to a device which we are already paired with */

            // This doesn't work. Can I not do this to check if I am paired?
            // DeviceInformation di = DeviceInformation.CreateFromIdAsync(id).GetResults();

            DeviceInformation di = null;
            foreach (DeviceInformation d in devices)
            {
                if (d.Id.Equals(id))
                {
                    di = d;
                    break;
                }
            }
            if(di == null)
            {
                Debug("Asked pairing status for a device I don't know");
                return false;

            }

            if (di.Pairing.IsPaired)
            {
                return true;
            }
            return false; 
        }

        #endregion

        #region Connecting
        protected bool connecting_busy = false;

        /// <summary>
        /// Try to pair with a device by Id
        /// </summary>
        /// <param name="id">the device id (as a string)</param>
        public void Connect(string id)
        {
            if (connecting_busy)
            {
                return;
            }

            /* Check to see if we asked to connect to a device which we are already paired with */
            if (!paired_with(id))
            {
                Debug("Asked to connect to a device we are not paired with.");
                return;
            }

            connecting_busy = true;

            Debug("Connecting!");
            AsyncConnect(id).Wait();

            connecting_busy = false;
        }

        public async Task AsyncConnect(string id)
        {
            try
            { 
                ble_dev = await BluetoothLEDevice.FromIdAsync(id);
            }
            catch (Exception ex)
            {
                Debug("Error when connecting: " + ex.ToString());
                return;
            }

            if (ble_dev == null)
            {
                Debug("Connection failed :(");
                return;
            }

            // we're not sure how long this takes, but don't try for more than 5 seconds

            var start_ts = DateTime.Now;
            TimeSpan diff = new TimeSpan(0, 0, 0);
            
            while (ble_dev.ConnectionStatus != BluetoothConnectionStatus.Connected && diff.Seconds < 5)
            {
                diff = DateTime.Now.Subtract(start_ts);
                await Task.Delay(50);
            }
            
            Debug("Connection Result: " + ble_dev.ConnectionStatus);
        }

        /// <summary>
        /// Is the current ble_device valid, and is it connected?
        /// </summary>
        public bool am_connected()
        {
            if(ble_dev == null)
            {
                return false;
            }

            if(ble_dev.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                return true;
            }

            return false;
        }
        #endregion
        #region Services
        public string[] getServices()
        {
            if (!am_connected())
            {
                VerboseDebug("Not connected, cowardly refusing to list services");
                return null;
            }

            List<string> temp = new List<string>();

            foreach (var service in ble_dev.GattServices)
            {
                temp.Add(service.AttributeHandle + ": " + service.Uuid);
            }

            return temp.ToArray<string>();
        }

        public string[] getServicesAndCharacteristics()
        {
            if (!am_connected())
            {
                VerboseDebug("Not connected, cowardly refusing to list services");
                return null;
            }

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

        #endregion
        #region NordicUART
        // Stuff for the nRF UART Service and Characteristics

        // A delegate for the NRF RX Characteristic
        public delegate void RxType(string rx);
        protected RxType rxFunc;

        GattCharacteristic uart_tx;
        GattCharacteristic uart_rx;

        // These are "Randomly defined" and correspond to the nRF UART Services
        string tx_uuid = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";
        string rx_uuid = "6E400003-B5A3-F393-E0A9-E50E24DCCA9E";
        string uart_uuid = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";

        public bool nordic_setup_properly = false; // kill me
        /// <summary>
        /// If the device has the Nordic UART service/characteristic, we just skip a bunch of generic stuff and hardwire it because hackathon
        /// </summary>
        public void SetupNordicUart()
        {
            if (!am_connected())
            {
                VerboseDebug("Not connected, cowardly refusing to create characteristics");
                return;
            }

        
            // TODO: Keep list of services, and check against that list 
            // Check to make sure the Nordic service is available

            bool available = false;
            GattDeviceService nordic_uart_service = null;
            IReadOnlyList<GattCharacteristic> characteristicList;
            foreach (GattDeviceService gds in ble_dev.GattServices)
            {
                if (gds.Uuid.Equals(new Guid(uart_uuid)))
                {
                    available = true;
                    nordic_uart_service = gds;
                    break;
                }
            }

            if (!available || nordic_uart_service == null)
            {
                Debug("Service unavailable, try again later");
                return;
            }

            characteristicList = nordic_uart_service.GetCharacteristics(new Guid(tx_uuid));
            if(characteristicList == null)
            {
                Debug("Null characteristic list for TX... try later");
            }
            // null check not good enough. (example: https://github.com/ms-iot/samples/blob/develop/BluetoothGATT/CS/MainPage.xaml.cs is bad)
            try
            {
                uart_tx = characteristicList[0];
            } 
            catch (Exception ex)
            {
                Debug("Problem getting TX Characteristic");
                return;
            }

            characteristicList = nordic_uart_service.GetCharacteristics(new Guid(rx_uuid));
            if (characteristicList == null)
            {
                Debug("Null characteristic list for RX... try later");
            }
            try
            {
                uart_rx = characteristicList[0];
            }
            catch (Exception ex)
            {
                Debug("Problem getting RX Characteristic");
                return;
            }
            // Disable encryption because apparently it helps sometimes
            uart_tx.ProtectionLevel = GattProtectionLevel.Plain;
            uart_rx.ProtectionLevel = GattProtectionLevel.Plain;

            // set a call back for the notifier
            uart_rx.ValueChanged += Uart_rx_ValueChanged;

            // tell the system we want the callback
            try
            {
                SetupNordicRX().Wait();
            }
            catch (Exception ex)
            {
                Debug("Issue registering for callback (we'll try anyway)");
            }


            nordic_setup_properly = true;
            Debug("Nordic UART Setup Complete!");

        }

        // Set up the notifier on the RX path, if needed.
        public async Task SetupNordicRX()
        {
            var result = await uart_rx.ReadClientCharacteristicConfigurationDescriptorAsync();
            if (result.Status == GattCommunicationStatus.Success &&
                result.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
            { 
                await uart_rx.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            }
        }

        // the callback
        private void Uart_rx_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            VerboseDebug("Uart RX Value Changed");
            if (rxFunc == null)
            {
                Debug("Set your RX Callback!"); 
                return;
            }
            DataReader dr = DataReader.FromBuffer(args.CharacteristicValue);
            rxFunc(dr.ReadString(args.CharacteristicValue.Length));
        }

        // set callback
        public void SetRxCallback(RxType theFunc)
        {
            rxFunc = theFunc;
        }
        #endregion

        #region interaction
        /// <summary>
        /// Send stuff to the bluetooth module
        /// </summary>
        /// <param name="stuff">what to send</param>
        public void send(string stuff)
        {
            try
            {
                real_send(stuff).Wait();
            }
            catch (Exception ex)
            {
                Debug("Caught exception when trying to send: " + ex);
            }
        }

        public async Task real_send(string stuff)
        {
            if (!am_connected())
            {
                VerboseDebug("Not connected, cowardly refusing to send");
                return;
            }

            if (uart_tx == null)
            {
                Debug("Set up the Nordic stuff first!");
                return;
            }

            // TODO: Check that uart_tx is valid
            var writer = new DataWriter();
            writer.WriteString(stuff);
            var res = await uart_tx.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);
        }     
        #endregion
    }
}

using System;
using GoBus;
using SecretLabs.NETMF.IO;
using SecretLabs.NETMF.Hardware.NetduinoGo;
using Microsoft.SPOT.Hardware;

namespace Nwazet.Go.SD {
    public class SDCardReader : GoModule {
        private string _mountPoint;
        public void Initialize(GoSocket socket, string mountPoint = "SD") {
            _mountPoint = mountPoint;
            SPI.SPI_module sdCardSpi;
            Cpu.Pin sdCardChipSelect;
            Cpu.Pin sdCardGPIO;
            socket.GetPhysicalResources(out sdCardGPIO, out sdCardSpi, out sdCardChipSelect);
            if (!BindSocket(socket)) throw new ApplicationException("socket already bound");
            SetSocketPowerState(true);
            StorageDevice.MountSD(_mountPoint, sdCardSpi, sdCardChipSelect);
        }
        protected override void Dispose(bool disposing) {
            StorageDevice.Unmount(_mountPoint);
            SetSocketPowerState(false);
            base.Dispose(disposing);
        }
        ~SDCardReader() {
            Dispose(true);
        }
    }
}

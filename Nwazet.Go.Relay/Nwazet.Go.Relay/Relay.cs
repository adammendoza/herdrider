using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoGo;
using GoBus;

namespace Nwazet.Go.Relay {
    public class Relay : GoModule {
        private OutputPort _relay;
        ~Relay() {
            Dispose();
        }
        public void Initialize(GoBus.GoSocket socket) {
            SPI.SPI_module spi;
            Cpu.Pin chipSelect;
            Cpu.Pin gpio;
            socket.GetPhysicalResources(out gpio, out spi, out chipSelect);
            if (!BindSocket(socket, new Guid(new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }))) throw new ApplicationException("not a relay");
            SetSocketPowerState(true);
            _relay = new OutputPort(gpio, false);
        }
        public void Activate(bool state) {
            _relay.Write(state);
        }
        protected override void Dispose(bool disposing = true) {
            _relay.Write(false);
            _relay.Dispose();
            SetSocketPowerState(false);
            base.Dispose(disposing);
        }
    }
}

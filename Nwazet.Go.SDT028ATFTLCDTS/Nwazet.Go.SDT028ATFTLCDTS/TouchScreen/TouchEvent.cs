using Microsoft.SPOT;
namespace Nwazet.Go.Display.TouchScreen {
    public delegate void TouchEventHandler(object sender, TouchEvent touchEvent);

    public class TouchEvent : EventArgs {
        public ushort X;
        public ushort Y;
        public uint Pressure;
        public byte IsValid;
    }
}

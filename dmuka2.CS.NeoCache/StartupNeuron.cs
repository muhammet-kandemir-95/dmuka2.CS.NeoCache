using System;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    public class StartupNeuron : Neuron<StartupNeuron>
    {
        public long Data = -1;
        public void Data__Pointer(Action<IntPtr> callback)
        {
            unsafe
            {
                fixed (long* address = &Data)
                {
                    callback((IntPtr)address);
                }
            }
        }
    }
}

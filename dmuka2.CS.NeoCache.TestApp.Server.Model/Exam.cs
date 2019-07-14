using System;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache.TestApp.Server.Model
{
    public class Exam : Neuron<Exam>
    {
        [NeuronData]
        public string name = "";

        [NeuronData]
        public byte result = 0;
        public void result__Pointer(Action<IntPtr> callback, bool set)
        {
            unsafe
            {
                fixed (byte* address = &result)
                {
                    callback((IntPtr)address);
                }
            }
        }
    }
}

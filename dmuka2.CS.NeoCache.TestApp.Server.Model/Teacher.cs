using System;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache.TestApp.Server.Model
{
    public class Teacher : Neuron<Teacher>
    {
        [NeuronData]
        public string name = "";
        [NeuronData]
        public string surname = "";
        [NeuronData]
        public short birth_year = 0;
        public void birth_year__Pointer(Action<IntPtr> callback, bool set)
        {
            unsafe
            {
                fixed (short* address = &birth_year)
                {
                    callback((IntPtr)address);
                }
            }
        }
    }
}

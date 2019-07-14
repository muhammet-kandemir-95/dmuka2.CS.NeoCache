using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace dmuka2.CS.NeoCache
{
    public class Global : Neuron<Global>
    {
        [NeuronData]
        public long data = -1;
        public void data__Pointer(Action<IntPtr> callback, bool set)
        {
            unsafe
            {
                fixed (long* address = &data)
                {
                    callback((IntPtr)address);
                }
            }
        }

        /// <summary>
        /// Total neuron count.
        /// </summary>
        [NeuronData]
        public int key_count = 0;
        public void key_count__Pointer(Action<IntPtr> callback, bool set)
        {
            unsafe
            {
                int a = Brain.Neurons.Count;
                int* address = &a;
                callback((IntPtr)address);
            }
        }

        /// <summary>
        /// All keys with tile('~').
        /// </summary>
        [NeuronData]
        public string keys
        {
            get
            {
                return string.Join('~', Brain.Neurons.Select(o => o.Key.Replace('~', ' ')).ToArray());
            }
        }

        /// <summary>
        /// Total type count.
        /// </summary>
        [NeuronData]
        public int type_count = 0;
        public void type_count__Pointer(Action<IntPtr> callback, bool set)
        {
            unsafe
            {
                int a = Brain.NeuronTypes.Count;
                int* address = &a;
                callback((IntPtr)address);
            }
        }

        /// <summary>
        /// All types with tile('~').
        /// </summary>
        [NeuronData]
        public string types
        {
            get
            {
                return string.Join('~', Brain.NeuronTypes.Select(o => o.Key.Replace('~', ' ')).ToArray());
            }
        }
    }
}

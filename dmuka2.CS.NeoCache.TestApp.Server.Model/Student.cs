using System;
using System.Collections;
using System.Collections.Generic;

namespace dmuka2.CS.NeoCache.TestApp.Server.Model
{
    public class Student : Neuron<Student>
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

        [NeuronData]
        [NeuronList(MaxRowCount = 5)]
        public List<Exam> exams = new List<Exam>();
    }
}

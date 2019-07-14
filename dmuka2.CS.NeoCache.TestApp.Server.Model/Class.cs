using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache.TestApp.Server.Model
{
    public class Class : Neuron<Class>
    {
        [NeuronData]
        public string name = "";

        [NeuronData]
        public Teacher teacher = new Teacher();

        [NeuronData]
        [NeuronList(MaxRowCount = 5)]
        public List<Student> students = new List<Student>();
    }
}

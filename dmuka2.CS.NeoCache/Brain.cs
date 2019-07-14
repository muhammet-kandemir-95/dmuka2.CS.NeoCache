using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    /// <summary>
    /// All neuron datas.
    /// </summary>
    public static class Brain
    {
        #region Variables
        /// <summary>
        /// Static neuron types.
        /// </summary>
        internal static Dictionary<string, Type> NeuronTypes = new Dictionary<string, Type>();
        /// <summary>
        /// All datas.
        /// </summary>
        internal static Dictionary<string, INeuron> Neurons = new Dictionary<string, INeuron>();
        #endregion

        #region Constructors
        static Brain()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(o => o.FullName.StartsWith("System.") == false).ToArray();
            foreach (var assembly in allAssemblies)
            {
                var allNeurons = assembly.GetTypes().Where(o =>
                {
                    try
                    {
                        return o.BaseType == typeof(Neuron<>).MakeGenericType(o);
                    }
                    catch
                    {
                        return false;
                    }
                }).ToArray();
                foreach (var neuronType in allNeurons)
                {
                    neuronType.BaseType.GetMethod("FillFields", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Invoke(null, new object[] { neuronType, "", null, 1 });
                    NeuronTypes.Add(neuronType.Name, neuronType);
                }
            }

            Neurons.Add("global", new Global());
        }
        #endregion

        #region Methods
        /// <summary>
        /// We are adding a new neuron to brain.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron class name.</param>
        public static void AddANeuron(string key, string type)
        {
            INeuron neuron = (INeuron)Activator.CreateInstance(NeuronTypes[type]);
            neuron.Key = key;
            Neurons.Add(key, neuron);
        }

        /// <summary>
        /// We are removing a neuron from brain.
        /// </summary>
        /// <param name="key">Data ID.</param>
        public static void RemoveANeuron(string key)
        {
            Neurons[key].Key = "";
            Neurons.Remove(key);
        }

        /// <summary>
        /// We are remove a neuron from brain and then creating a new neuron instead of it.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="type">Neuron class name.</param>
        public static void RefreshANeuron(string key, string type)
        {
            RemoveANeuron(key);
            AddANeuron(key, type);
        }

        /// <summary>
        /// We are setting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the new data?</param>
        public static void SetNeuronValue(string key, string path, byte[] value)
        {
            INeuron neuron = Neurons[key];
            neuron.SetFieldValue[path](neuron, value);
        }

        /// <summary>
        /// We are adding a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public static byte[] AddNeuronValue(string key, string path, byte[] value)
        {
            INeuron neuron = Neurons[key];
            return neuron.AddFieldValue[path](neuron, value);
        }

        /// <summary>
        /// We are subtracting a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public static byte[] SubtractNeuronValue(string key, string path, byte[] value)
        {
            INeuron neuron = Neurons[key];
            return neuron.SubtractFieldValue[path](neuron, value);
        }

        /// <summary>
        /// We are multiping a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public static byte[] MultiplyNeuronValue(string key, string path, byte[] value)
        {
            INeuron neuron = Neurons[key];
            return neuron.MultiplyFieldValue[path](neuron, value);
        }

        /// <summary>
        /// We are dividing a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public static byte[] DivideNeuronValue(string key, string path, byte[] value)
        {
            INeuron neuron = Neurons[key];
            return neuron.DivideFieldValue[path](neuron, value);
        }

        /// <summary>
        /// We are moduling a value to neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="value">What is the data?</param>
        public static byte[] ModuloNeuronValue(string key, string path, byte[] value)
        {
            INeuron neuron = Neurons[key];
            return neuron.ModuloFieldValue[path](neuron, value);
        }

        /// <summary>
        /// We are getting a value from neuron field by key.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public static byte[] GetNeuronValue(string key, string path)
        {
            INeuron neuron = Neurons[key];
            return neuron.GetFieldValue[path](neuron);
        }

        /// <summary>
        /// We are setting a neuron to neuron field by keys.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="neuronKey">What is the new neuron?</param>
        public static void SetNeuronValueToNeuron(string key, string path, string neuronKey)
        {
            INeuron neuron = Neurons[key];
            INeuron newNeuron = Neurons[neuronKey];
            neuron.SetFieldNeuronValue[path](neuron, newNeuron);
        }

        /// <summary>
        /// We are adding a new neuron to list of a neuron.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public static void AddANeuronToList(string key, string path)
        {
            INeuron neuron = Neurons[key];
            neuron.AddNeuronToList[path](neuron);
        }

        /// <summary>
        /// We are removing a new neuron from list.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        /// <param name="index">Index of data.</param>
        public static void RemoveANeuronFromList(string key, string path, int index)
        {
            INeuron neuron = Neurons[key];
            neuron.RemoveNeuronInList[path](neuron, index);
        }

        /// <summary>
        /// Clear a neuron list.
        /// </summary>
        /// <param name="key">Data ID.</param>
        /// <param name="path">Data path of neuron.</param>
        public static void ClearNeuronList(string key, string path)
        {
            INeuron neuron = Neurons[key];
            neuron.ClearList[path](neuron);
        }

        /// <summary>
        /// Does neuron exists?
        /// </summary>
        /// <param name="key">Data ID.</param>
        public static bool NeuronExists(string key)
        {
            return Neurons.ContainsKey(key);
        }
        #endregion
    }
}

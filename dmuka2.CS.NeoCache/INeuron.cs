using System;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    /// <summary>
    /// This is only for store neurons on the brain.
    /// </summary>
    internal interface INeuron
    {
        /// <summary>
        /// Neuron global ID
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// Get field value as Neural Data.
        /// </summary>
        Dictionary<string, Func<object, byte[]>> GetFieldValue { get; }
        /// <summary>
        /// Set Neural Data to field.
        /// </summary>
        Dictionary<string, Action<object, byte[]>> SetFieldValue { get; }
        /// <summary>
        /// Add value to Neural Data.
        /// </summary>
        Dictionary<string, Func<object, byte[], byte[]>> AddFieldValue { get; }
        /// <summary>
        /// Subtract value to Neural Data.
        /// </summary>
        Dictionary<string, Func<object, byte[], byte[]>> SubtractFieldValue { get; }
        /// <summary>
        /// Multiply value to Neural Data.
        /// </summary>
        Dictionary<string, Func<object, byte[], byte[]>> MultiplyFieldValue { get; }
        /// <summary>
        /// Divide value to Neural Data.
        /// </summary>
        Dictionary<string, Func<object, byte[], byte[]>> DivideFieldValue { get; }
        /// <summary>
        /// Modulo value to Neural Data.
        /// </summary>
        Dictionary<string, Func<object, byte[], byte[]>> ModuloFieldValue { get; }
        /// <summary>
        /// Set Neuron to field.
        /// </summary>
        Dictionary<string, Action<object, INeuron>> SetFieldNeuronValue { get; }
        /// <summary>
        /// Add neuron to list.
        /// </summary>
        Dictionary<string, Action<object>> AddNeuronToList { get; }
        /// <summary>
        /// Remove neuron from list.
        /// </summary>
        Dictionary<string, Action<object, int>> RemoveNeuronInList { get; }
        /// <summary>
        /// Clear neuron list.
        /// </summary>
        Dictionary<string, Action<object>> ClearList { get; }
    }
}

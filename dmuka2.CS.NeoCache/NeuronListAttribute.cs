using System;
using System.Collections.Generic;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    /// <summary>
    /// We are working with <see cref="Dictionary{TKey, TValue}"/>.
    /// So we have to need keys.
    /// This class is for this situation.
    /// We will add keys for each posibility row.
    /// </summary>
    public class NeuronListAttribute : Attribute
    {
        #region Variables
        public int MaxRowCount { get; set; }
        #endregion
    }
}

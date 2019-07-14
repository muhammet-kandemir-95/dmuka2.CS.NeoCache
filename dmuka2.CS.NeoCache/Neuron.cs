using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace dmuka2.CS.NeoCache
{
    /// <summary>
    /// Each of data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Neuron<T> : INeuron where T : Neuron<T>
    {
        #region Variables
        /// <summary>
        /// Neuron type.
        /// </summary>
        public static string Stype = "";
        /// <summary>
        /// Neuron global ID.
        /// </summary>
        [NeuronData]
        public string key = "";
        /// <summary>
        /// Neuron type.
        /// </summary>
        [NeuronData]
        public string type = "";
        /// <summary>
        /// Get field value as Neural Data.
        /// </summary>
        internal static Dictionary<string, Func<object, byte[]>> GetFieldValue = new Dictionary<string, Func<object, byte[]>>();
        /// <summary>
        /// Set Neural Data to field.
        /// </summary>
        internal static Dictionary<string, Action<object, byte[]>> SetFieldValue = new Dictionary<string, Action<object, byte[]>>();
        /// <summary>
        /// Add value to Neural Data.
        /// </summary>
        internal static Dictionary<string, Func<object, byte[], byte[]>> AddFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Subtract value to Neural Data.
        /// </summary>
        internal static Dictionary<string, Func<object, byte[], byte[]>> SubtractFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Multiply value to Neural Data.
        /// </summary>
        internal static Dictionary<string, Func<object, byte[], byte[]>> MultiplyFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Divide value to Neural Data.
        /// </summary>
        internal static Dictionary<string, Func<object, byte[], byte[]>> DivideFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Modulo value to Neural Data.
        /// </summary>
        internal static Dictionary<string, Func<object, byte[], byte[]>> ModuloFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Set Neuron to field.
        /// </summary>
        internal static Dictionary<string, Action<object, INeuron>> SetFieldNeuronValue = new Dictionary<string, Action<object, INeuron>>();
        /// <summary>
        /// Add neuron to list.
        /// </summary>
        internal static Dictionary<string, Action<object>> AddNeuronToList = new Dictionary<string, Action<object>>();
        /// <summary>
        /// Remove neuron from list.
        /// </summary>
        internal static Dictionary<string, Action<object, int>> RemoveNeuronInList = new Dictionary<string, Action<object, int>>();
        /// <summary>
        /// Clear neuron list.
        /// </summary>
        internal static Dictionary<string, Action<object>> ClearList = new Dictionary<string, Action<object>>();

        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                this.OnSetKey();
            }
        }

        Dictionary<string, Func<object, byte[]>> INeuron.GetFieldValue => GetFieldValue;

        Dictionary<string, Action<object, byte[]>> INeuron.SetFieldValue => SetFieldValue;

        Dictionary<string, Func<object, byte[], byte[]>> INeuron.AddFieldValue => AddFieldValue;

        Dictionary<string, Func<object, byte[], byte[]>> INeuron.SubtractFieldValue => SubtractFieldValue;

        Dictionary<string, Func<object, byte[], byte[]>> INeuron.MultiplyFieldValue => MultiplyFieldValue;

        Dictionary<string, Func<object, byte[], byte[]>> INeuron.DivideFieldValue => DivideFieldValue;

        Dictionary<string, Func<object, byte[], byte[]>> INeuron.ModuloFieldValue => ModuloFieldValue;

        Dictionary<string, Action<object, INeuron>> INeuron.SetFieldNeuronValue => SetFieldNeuronValue;

        Dictionary<string, Action<object>> INeuron.AddNeuronToList => AddNeuronToList;

        Dictionary<string, Action<object, int>> INeuron.RemoveNeuronInList => RemoveNeuronInList;

        Dictionary<string, Action<object>> INeuron.ClearList => ClearList;
        #endregion

        #region Constructors
        static Neuron()
        {
            var type = typeof(T);
            Stype = type.Name;
        }

        public Neuron()
        {
            this.type = Stype;
        }
        #endregion

        #region Methods
        /// <summary>
        /// When keys of this class changed, this method will be called.
        /// </summary>
        public virtual void OnSetKey()
        { }

        /// <summary>
        /// We are filling <see cref="GetFieldValue"/> and <see cref="SetFieldValue"/> variable.
        /// </summary>
        /// <param name="type">What is the type?</param>
        /// <param name="parent">What is the parent field name?</param>
        /// <param name="getValue">What is the parent field value?</param>
        internal static void FillFields(Type type, string parent = "", Func<object, object> getValue = null, int recursiveLevel = 1)
        {
            if (recursiveLevel > 1000)
                return;

            getValue = getValue ?? ((v) => v);

            Action<string, Type, Func<object, object>, Action<object, object>, Func<Type, object>> fillNeuron = (name, dataType, dataGetValue, dataSetValue, getAttr) =>
            {
                if (dataType.IsPointer)
                    return;
                if (getAttr(typeof(NeuronDataAttribute)) == null)
                    return;

                if (dataType == typeof(byte))
                {
                    var pointerMethod = type.GetMethod(name + "__Pointer");
                    GetFieldValue.Add(parent + name, (o) => new byte[] { (byte)dataGetValue(getValue(o)) });
                    SetFieldValue.Add(parent + name, (o, v) =>
                    {
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.WriteByte(pointer, 0, v[0]);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, true });
                    });
                    AddFieldValue.Add(parent + name, (o, v) =>
                    {
                        byte fieldValue = (byte)dataGetValue(getValue(o));
                        fieldValue += v[0];
                        dataSetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    SubtractFieldValue.Add(parent + name, (o, v) =>
                    {
                        byte fieldValue = (byte)dataGetValue(getValue(o));
                        fieldValue -= v[0];
                        dataSetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    MultiplyFieldValue.Add(parent + name, (o, v) =>
                    {
                        byte fieldValue = (byte)dataGetValue(getValue(o));
                        fieldValue *= v[0];
                        dataSetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    DivideFieldValue.Add(parent + name, (o, v) =>
                    {
                        byte fieldValue = (byte)dataGetValue(getValue(o));
                        fieldValue /= v[0];
                        dataSetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    ModuloFieldValue.Add(parent + name, (o, v) =>
                    {
                        byte fieldValue = (byte)dataGetValue(getValue(o));
                        fieldValue %= v[0];
                        dataSetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                }
                else if (dataType == typeof(string))
                {
                    if (
                        name == "key" ||
                        name == "type")
                    {
                        GetFieldValue.Add(parent + name, (o) => Encoding.UTF8.GetBytes((string)dataGetValue(getValue(o))));
                        return;
                    }

                    GetFieldValue.Add(parent + name, (o) => Encoding.UTF8.GetBytes((string)dataGetValue(getValue(o))));
                    SetFieldValue.Add(parent + name, (o, v) => dataSetValue(getValue(o), Encoding.UTF8.GetString(v)));
                    AddFieldValue.Add(parent + name, (o, v) =>
                    {
                        string fieldValue = (string)dataGetValue(getValue(o));
                        fieldValue += Encoding.UTF8.GetString(v);
                        dataSetValue(getValue(o), fieldValue);
                        return Encoding.UTF8.GetBytes(fieldValue);
                    });
                }
                else if (dataType.IsValueType && dataType.IsEnum == false)
                {
                    var pointerMethod = type.GetMethod(name + "__Pointer");
                    int size = Marshal.SizeOf(dataType);
                    GetFieldValue.Add(parent + name, (o) =>
                    {
                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, false });
                        return result;
                    });
                    SetFieldValue.Add(parent + name, (o, v) =>
                    {
                        Action<IntPtr> mp = (pointer) =>
                        {
                            for (int i = 0; i < size; i++)
                                Marshal.WriteByte(pointer, i, v[i]);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp , true});
                    });

                    Func<dynamic, object> castValue = (o) => null;
                    Func<byte[], dynamic> convertData = (v) => null;
                    if (dataType == typeof(sbyte))
                    {
                        castValue = (o) => (sbyte)o;
                        convertData = (v) => v.ToSByteDC();
                    }
                    else if (dataType == typeof(short))
                    {
                        castValue = (o) => (short)o;
                        convertData = (v) => v.ToInt16DC();
                    }
                    else if (dataType == typeof(ushort))
                    {
                        castValue = (o) => (ushort)o;
                        convertData = (v) => v.ToUInt16DC();
                    }
                    else if (dataType == typeof(int))
                    {
                        castValue = (o) => (int)o;
                        convertData = (v) => v.ToInt32DC();
                    }
                    else if (dataType == typeof(uint))
                    {
                        castValue = (o) => (uint)o;
                        convertData = (v) => v.ToUInt32DC();
                    }
                    else if (dataType == typeof(long))
                    {
                        castValue = (o) => (long)o;
                        convertData = (v) => v.ToInt64DC();
                    }
                    else if (dataType == typeof(ulong))
                    {
                        castValue = (o) => (ulong)o;
                        convertData = (v) => v.ToUInt64DC();
                    }
                    else if (dataType == typeof(float))
                    {
                        castValue = (o) => (float)o;
                        convertData = (v) => v.ToSingleDC();
                    }
                    else if (dataType == typeof(double))
                    {
                        castValue = (o) => (double)o;
                        convertData = (v) => v.ToDoubleDC();
                    }
                    else if (dataType == typeof(decimal))
                    {
                        castValue = (o) => (decimal)o;
                        convertData = (v) => v.ToDecimalDC();
                    }

                    AddFieldValue.Add(parent + name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(dataGetValue(getValue(o)));
                        fieldValue = castValue(fieldValue + convertData(v));
                        dataSetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, false });
                        return result;
                    });
                    SubtractFieldValue.Add(parent + name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(dataGetValue(getValue(o)));
                        fieldValue = castValue(fieldValue - convertData(v));
                        dataSetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, false });
                        return result;
                    });
                    MultiplyFieldValue.Add(parent + name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(dataGetValue(getValue(o)));
                        fieldValue = castValue(fieldValue * convertData(v));
                        dataSetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, false });
                        return result;
                    });
                    DivideFieldValue.Add(parent + name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(dataGetValue(getValue(o)));
                        fieldValue = castValue(fieldValue / convertData(v));
                        dataSetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, false });
                        return result;
                    });
                    ModuloFieldValue.Add(parent + name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(dataGetValue(getValue(o)));
                        fieldValue = castValue(fieldValue % convertData(v));
                        dataSetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp, false });
                        return result;
                    });
                }
                else if (dataType.GetInterface("IList") != null || dataType.GetInterfaces().Any(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(IList<>)))
                {
                    var fieldIndexer = dataType.GetProperty("Item");
                    var genericArgumentType =
                        dataType.GetInterfaces().Any(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(IList<>)) ? 
                            dataType.GetInterfaces().First(o => o.IsGenericType && o.GetGenericTypeDefinition() == typeof(IList<>)).GetGenericArguments()[0] :
                            dataType.GetGenericArguments()[0];
                    var addMethod = dataType.GetMethod("Add");
                    var removeMethod = dataType.GetMethod("RemoveAt");
                    var clearMethod = dataType.GetMethod("Clear");
                    var getCount = dataType.GetProperty("Count");

                    AddNeuronToList.Add(parent + name, (o) =>
                    {
                        addMethod.Invoke(dataGetValue(getValue(o)), new object[] { Activator.CreateInstance(genericArgumentType) });
                    });
                    RemoveNeuronInList.Add(parent + name, (o, si) =>
                    {
                        removeMethod.Invoke(dataGetValue(getValue(o)), new object[] { si });
                    });
                    ClearList.Add(parent + name, (o) =>
                    {
                        clearMethod.Invoke(dataGetValue(getValue(o)), new object[0]);
                    });
                    GetFieldValue.Add(parent + name + ".{len}", (o) => BitConverter.GetBytes((int)getCount.GetValue(dataGetValue(getValue(o)))));

                    NeuronListAttribute attr = (NeuronListAttribute)getAttr(typeof(NeuronListAttribute));
                    for (int i = 0; i < attr.MaxRowCount; i++)
                    {
                        Action<int> action = (ri) =>
                        {
                            FillFields(genericArgumentType, parent + name + "[" + ri + "]" + ".", (o) => fieldIndexer.GetValue(dataGetValue(getValue(o)), new object[] { ri }), recursiveLevel + 1);
                            SetFieldNeuronValue.Add(parent + name + "[" + ri + "]", (o, n) => fieldIndexer.SetValue(dataGetValue(getValue(o)), n, new object[] { ri }));
                        };
                        action(i);
                    }
                }
                else if (dataType.BaseType == typeof(Neuron<>).MakeGenericType(dataType))
                {
                    FillFields(dataType, parent + name + ".", (o) => dataGetValue(getValue(o)), recursiveLevel + 1);

                    SetFieldNeuronValue.Add(parent + name, (o, n) => dataSetValue(getValue(o), n));
                }
            };

            var fields = type.GetFields();
            foreach (var field in fields)
                fillNeuron(field.Name, field.FieldType, (o) => field.GetValue(o), (o, v) => field.SetValue(o, v), (t) => field.GetCustomAttribute(t));

            var props = type.GetProperties();
            foreach (var prop in props)
                fillNeuron(prop.Name, prop.PropertyType, (o) => prop.GetValue(o), (o, v) => prop.SetValue(o, v), (t) => prop.GetCustomAttribute(t));
        }
        #endregion
    }
}

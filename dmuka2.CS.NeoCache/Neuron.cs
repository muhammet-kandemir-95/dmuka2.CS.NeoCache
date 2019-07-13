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
        /// Get field value as Neural Data.
        /// </summary>
        public static Dictionary<string, Func<object, byte[]>> GetFieldValue = new Dictionary<string, Func<object, byte[]>>();
        /// <summary>
        /// Set Neural Data to field.
        /// </summary>
        public static Dictionary<string, Action<object, byte[]>> SetFieldValue = new Dictionary<string, Action<object, byte[]>>();
        /// <summary>
        /// Add value to Neural Data.
        /// </summary>
        public static Dictionary<string, Func<object, byte[], byte[]>> AddFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Subtract value to Neural Data.
        /// </summary>
        public static Dictionary<string, Func<object, byte[], byte[]>> SubtractFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Multiply value to Neural Data.
        /// </summary>
        public static Dictionary<string, Func<object, byte[], byte[]>> MultiplyFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Divide value to Neural Data.
        /// </summary>
        public static Dictionary<string, Func<object, byte[], byte[]>> DivideFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Modulo value to Neural Data.
        /// </summary>
        public static Dictionary<string, Func<object, byte[], byte[]>> ModuloFieldValue = new Dictionary<string, Func<object, byte[], byte[]>>();
        /// <summary>
        /// Set Neuron to field.
        /// </summary>
        public static Dictionary<string, Action<object, INeuron>> SetFieldNeuronValue = new Dictionary<string, Action<object, INeuron>>();
        /// <summary>
        /// Add neuron to list.
        /// </summary>
        public static Dictionary<string, Action<object>> AddNeuronToList = new Dictionary<string, Action<object>>();
        /// <summary>
        /// Remove neuron from list.
        /// </summary>
        public static Dictionary<string, Action<object, int>> RemoveNeuronInList = new Dictionary<string, Action<object, int>>();
        /// <summary>
        /// Clear neuron list.
        /// </summary>
        public static Dictionary<string, Action<object>> ClearList = new Dictionary<string, Action<object>>();

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
            fillFields(type);
        }
        #endregion

        #region Methods
        /// <summary>
        /// We are filling <see cref="GetFieldValue"/> and <see cref="SetFieldValue"/> variable.
        /// </summary>
        /// <param name="type">What is the type?</param>
        /// <param name="parent">What is the parent field name?</param>
        /// <param name="getValue">What is the parent field value?</param>
        private static void fillFields(Type type, string parent = "", Func<object, object> getValue = null)
        {
            getValue = getValue ?? ((v) => v);
            var fields = type.GetFields().Where(o => o.IsStatic == false).ToArray();

            foreach (var field in fields)
            {
                if (field.FieldType.IsPointer)
                    continue;

                if (field.FieldType == typeof(byte))
                {
                    GetFieldValue.Add(parent + field.Name, (o) => new byte[] { (byte)field.GetValue(getValue(o)) });
                    SetFieldValue.Add(parent + field.Name, (o, v) => field.SetValue(getValue(o), v[0]));
                    AddFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        byte fieldValue = (byte)field.GetValue(getValue(o));
                        fieldValue += v[0];
                        field.SetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    SubtractFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        byte fieldValue = (byte)field.GetValue(getValue(o));
                        fieldValue -= v[0];
                        field.SetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    MultiplyFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        byte fieldValue = (byte)field.GetValue(getValue(o));
                        fieldValue *= v[0];
                        field.SetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    DivideFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        byte fieldValue = (byte)field.GetValue(getValue(o));
                        fieldValue /= v[0];
                        field.SetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                    ModuloFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        byte fieldValue = (byte)field.GetValue(getValue(o));
                        fieldValue %= v[0];
                        field.SetValue(getValue(o), fieldValue);
                        return new byte[] { fieldValue };
                    });
                }
                else if (field.FieldType == typeof(string))
                {
                    GetFieldValue.Add(parent + field.Name, (o) => Encoding.UTF8.GetBytes((string)field.GetValue(getValue(o))));
                    SetFieldValue.Add(parent + field.Name, (o, v) => field.SetValue(getValue(o), Encoding.UTF8.GetString(v)));
                    AddFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        string fieldValue = (string)field.GetValue(getValue(o));
                        fieldValue += Encoding.UTF8.GetString(v);
                        field.SetValue(getValue(o), fieldValue);
                        return Encoding.UTF8.GetBytes(fieldValue);
                    });
                }
                else if (field.FieldType.IsValueType && field.FieldType.IsEnum == false)
                {
                    var pointerMethod = type.GetMethod(field.Name + "__Pointer");
                    int size = Marshal.SizeOf(field.FieldType);
                    GetFieldValue.Add(parent + field.Name, (o) =>
                    {
                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                        return result;
                    });
                    SetFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        Action<IntPtr> mp = (pointer) =>
                        {
                            for (int i = 0; i < size; i++)
                                Marshal.WriteByte(pointer, i, v[i]);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                    });

                    Func<dynamic, object> castValue = (o) => null;
                    Func<byte[], dynamic> convertData = (v) => null;
                    if (field.FieldType == typeof(sbyte))
                    {
                        castValue = (o) => (sbyte)o;
                        convertData = (v) => v.ToSByteDC();
                    }
                    else if (field.FieldType == typeof(short))
                    {
                        castValue = (o) => (short)o;
                        convertData = (v) => v.ToInt16DC();
                    }
                    else if (field.FieldType == typeof(ushort))
                    {
                        castValue = (o) => (ushort)o;
                        convertData = (v) => v.ToUInt16DC();
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        castValue = (o) => (int)o;
                        convertData = (v) => v.ToInt32DC();
                    }
                    else if (field.FieldType == typeof(uint))
                    {
                        castValue = (o) => (uint)o;
                        convertData = (v) => v.ToUInt32DC();
                    }
                    else if (field.FieldType == typeof(long))
                    {
                        castValue = (o) => (long)o;
                        convertData = (v) => v.ToInt64DC();
                    }
                    else if (field.FieldType == typeof(ulong))
                    {
                        castValue = (o) => (ulong)o;
                        convertData = (v) => v.ToUInt64DC();
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        castValue = (o) => (float)o;
                        convertData = (v) => v.ToSingleDC();
                    }
                    else if (field.FieldType == typeof(double))
                    {
                        castValue = (o) => (double)o;
                        convertData = (v) => v.ToDoubleDC();
                    }
                    else if (field.FieldType == typeof(decimal))
                    {
                        castValue = (o) => (decimal)o;
                        convertData = (v) => v.ToDecimalDC();
                    }

                    AddFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(field.GetValue(getValue(o)));
                        fieldValue = castValue(fieldValue + convertData(v));
                        field.SetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                        return result;
                    });
                    SubtractFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(field.GetValue(getValue(o)));
                        fieldValue = castValue(fieldValue - convertData(v));
                        field.SetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                        return result;
                    });
                    MultiplyFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(field.GetValue(getValue(o)));
                        fieldValue = castValue(fieldValue * convertData(v));
                        field.SetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                        return result;
                    });
                    DivideFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(field.GetValue(getValue(o)));
                        fieldValue = castValue(fieldValue / convertData(v));
                        field.SetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                        return result;
                    });
                    ModuloFieldValue.Add(parent + field.Name, (o, v) =>
                    {
                        dynamic fieldValue = castValue(field.GetValue(getValue(o)));
                        fieldValue = castValue(fieldValue % convertData(v));
                        field.SetValue(getValue(o), fieldValue);

                        byte[] result = new byte[size];
                        Action<IntPtr> mp = (pointer) =>
                        {
                            Marshal.Copy(pointer, result, 0, size);
                        };
                        pointerMethod.Invoke(getValue(o), new object[] { mp });
                        return result;
                    });
                }
                else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var fieldIndexer = field.FieldType.GetProperty("Item");
                    var genericArgumentType = field.FieldType.GetGenericArguments()[0];
                    var addMethod = field.FieldType.GetMethod("Add");
                    var removeMethod = field.FieldType.GetMethod("RemoveAt");
                    var clearMethod = field.FieldType.GetMethod("Clear");

                    int listCount = 0;
                    AddNeuronToList.Add(parent + field.Name, (o) =>
                    {
                        addMethod.Invoke(field.GetValue(getValue(o)), new object[] { Activator.CreateInstance(genericArgumentType) });
                        listCount++;
                    });
                    RemoveNeuronInList.Add(parent + field.Name, (o, si) =>
                    {
                        removeMethod.Invoke(field.GetValue(getValue(o)), new object[] { si });
                        listCount--;
                    });
                    ClearList.Add(parent + field.Name, (o) =>
                    {
                        clearMethod.Invoke(field.GetValue(getValue(o)), new object[0]);
                        listCount = 0;
                    });
                    GetFieldValue.Add(parent + field.Name + ".{len}", (o) => BitConverter.GetBytes(listCount));

                    NeuronListAttribute attr = field.GetCustomAttribute<NeuronListAttribute>();
                    for (int i = 0; i < attr.MaxRowCount; i++)
                    {
                        Action<int> action = (ri) =>
                        {
                            fillFields(genericArgumentType, parent + field.Name + "[" + ri + "]" + ".", (o) => fieldIndexer.GetValue(field.GetValue(getValue(o)), new object[] { ri }));
                            SetFieldNeuronValue.Add(parent + field.Name + "[" + ri + "]", (o, n) => fieldIndexer.SetValue(field.GetValue(getValue(o)), n, new object[] { ri }));
                        };
                        action(i);
                    }
                }
                else if (field.FieldType.BaseType == typeof(Neuron<>).MakeGenericType(field.FieldType))
                {
                    fillFields(field.FieldType, parent + field.Name + ".", (o) => field.GetValue(getValue(o)));

                    string[] allFieldsOfField = field.FieldType.GetFields().Where(o => o.IsStatic == false).Select(o => o.Name).ToArray();
                    SetFieldNeuronValue.Add(parent + field.Name, (o, n) => field.SetValue(getValue(o), n));
                }
            }
        }
        #endregion
    }
}

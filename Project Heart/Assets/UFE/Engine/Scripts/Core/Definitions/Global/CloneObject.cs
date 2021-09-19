using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UFE3D
{
    [System.Serializable]
    public static class CloneObject
    {
        public static object objCopy;
        public static object[] arrayCopy;

        public static object Clone(object target)
        {
            return ReflectionClone(target);
        }

        public static object Clone(object target, bool serialized)
        {
            if (serialized) return SerializedClone(target);
            return ReflectionClone(target);
        }

        public static object SerializedClone(object target)
        {
            if (target == null) return null;

            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, target);
                objectStream.Seek(0, SeekOrigin.Begin);
                return (object)formatter.Deserialize(objectStream);
            }
        }

        public static object[] ReflectionCloneArray(object[] target)
        {
            object[] arrayObj = (object[])Array.CreateInstance(target.GetType().GetElementType(), target.Length);

            for (int i = 0; i < target.Length; i++)
            {

                if (target[i] == null
                    || target[i].GetType().IsEnum
                    || target[i].GetType().IsValueType
                    || target[i].GetType().IsGenericType
                    || target[i].GetType().Equals(typeof(String))
                    || target[i].GetType().IsSubclassOf(typeof(ScriptableObject)))
                {

                    // If its a simple element, use shallow copy
                    arrayObj[i] = target[i];
                }
                else
                {
                    // If its a complex interface, go deeper into recursion
                    arrayObj[i] = ReflectionClone(target[i]);
                }
            }

            return arrayObj;
        }

        public static object ReflectionClone(object target)
        {
            Type typeSource = target.GetType();

            // If its an array, identify and recurse each element
            if (typeSource.IsArray) return (object)ReflectionCloneArray((object[])target);

            object newObj = Activator.CreateInstance(typeSource);
            FieldInfo[] fields = typeSource.GetFields();

            foreach (FieldInfo field in fields)
            {
                object fieldValue = field.GetValue(target);

                if (fieldValue == null
                    || field.FieldType.IsEnum
                    || field.FieldType.IsValueType
                    || field.FieldType.Equals(typeof(String))
                    || field.FieldType.GetInterface("ICloneable", true) == null
                    || field.FieldType.GetInterface("ScriptableObject", true) != null
                    || field.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    // If its a simple element, use shallow copy
                    field.SetValue(newObj, fieldValue);
                }
                else
                {
                    // If its a complex interface, go deeper into recursion
                    field.SetValue(newObj, ReflectionClone(fieldValue));

                }
            }
            return newObj;
        }

        public static Dictionary<TKey, TValue> CloneDictionary<TKey, TValue>(Dictionary<TKey, TValue> original)
        {
            if (original == null) return null;
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count, original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value);
            }
            return ret;
        }

        public static List<T> CloneList<T>(List<T> original)
        {
            List<T> ret = new List<T>(original.Count);
            foreach (T entry in original)
            {
                ret.Add(entry);
            }
            return ret;
        }


        public static IList CloneList(IList original, Type T)
        {
            IList ret = Activator.CreateInstance(T) as IList;
            foreach (var entry in original)
            {
                ret.Add(entry);
            }
            return ret;
        }
    }
}
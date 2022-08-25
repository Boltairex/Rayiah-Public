using Rayiah.Objects.Abstracts;
using Rayiah.Objects.Attributes;
using Rayiah.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Rayiah.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class SaveParser : ManagerBase
    { 
        public static SaveParser instance { get; } = new SaveParser();

        public static ParserFieldValidation IsFieldInfoValid(FieldInfo f)
        {
            if (!f.IsPublic) return ParserFieldValidation.notPublic;
            if (f.GetCustomAttribute(typeof(SaveAttribute)) is null) return ParserFieldValidation.noAttribute;
            return ParserFieldValidation.valid;
        }

        public Dictionary<string, string> GatherValues(Type t)
        {
            if (!ISave.PrepareDir()) return null;

            if (!Directory.Exists(ISave.FilePath + t.Namespace))
            {
                Directory.CreateDirectory(ISave.FilePath + t.GetType().Namespace);
                return null;
            }

            return null;
        }

        public bool FillObject(Type t)
        {
            bool changed = false;
            var keys = GatherValues(t);
            foreach (FieldInfo f in t.GetFields())
            {
                if (IsFieldInfoValid(f) != 0) continue;
                if (!keys.ContainsKey(f.Name)) continue;
                var value = keys[f.Name];
                f.SetValue(f, value);
                changed = true;
            }
            return changed;
        }

        public bool SaveValues(Type obj)
        {
            if (!Directory.Exists(ISave.FilePath + obj.Namespace))
                Directory.CreateDirectory(ISave.FilePath + obj.Namespace);

            var fields = obj.GetFields();

            string fieldsVal = "";

            foreach (FieldInfo f in fields)
            {
                if (IsFieldInfoValid(f) != 0) continue;
                var attr = f.GetCustomAttribute(typeof(SaveAttribute));

                Console.WriteLine(f.Name + " " + f);

                if (f.DeclaringType.IsGenericType && f.FieldType.IsArray)
                {
                    Console.WriteLine("Array");
                    int i = f.FieldType.GetArrayRank();
                    var val = f.GetValue(f);
                    Console.WriteLine(val.ToString());
                }
                else if (f.DeclaringType.IsGenericType && f.DeclaringType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    Console.WriteLine("Dictionary");
                }
                else if (f.FieldType.IsPrimitive)
                {
                    fieldsVal += '\t' + attr.GetType().Name + ':' + f.GetValue(null) + '\n';
                }
                Console.WriteLine(fieldsVal);
            }

            string outVal = obj.GetType() + " {" + fieldsVal + "},\n";

            return true;
        }
    }

    public enum ParserFieldValidation { 
        /// <summary>
        /// Field is valid.
        /// </summary>
        valid = 0,
        /// <summary>
        /// Field have to be public to work.
        /// </summary>
        notPublic = 1,
        /// <summary>
        /// Field have to have <see cref="SaveAttribute"/> to work.
        /// </summary>
        noAttribute = 2
    }
}

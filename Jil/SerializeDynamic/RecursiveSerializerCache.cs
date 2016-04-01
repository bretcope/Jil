﻿using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Sigil.NonGeneric;
using StringInterningJil.Common;

namespace StringInterningJil.SerializeDynamic
{
    static class RecursiveSerializerCache<T>
    {
        static readonly Hashtable Cache = new Hashtable();

        public static readonly MethodInfo GetFor = typeof(RecursiveSerializerCache<T>).GetMethod("_GetFor", BindingFlags.NonPublic | BindingFlags.Static);
        static Action<TextWriter, T, int> _GetFor(
                bool prettyPrint, 
                bool excludeNulls, 
                bool jsonp, 
                DateTimeFormat dateFormat, 
                bool includeInherited
            )
        {
            var opts = new Options(prettyPrint, excludeNulls, jsonp, dateFormat, includeInherited);
            var cached = (Action<TextWriter, T, int>)Cache[opts];
            if (cached != null) return cached;

            lock (Cache)
            {
                cached = (Action<TextWriter, T, int>)Cache[opts];
                if (cached != null) return cached;

                var type = typeof(T);
                var optionsField = OptionsLookup.GetOptionsFieldFor(opts);
                var serializeMtd = DynamicSerializer.SerializeMtd;

                var emit = Emit.NewDynamicMethod(typeof(void), new[] { typeof(TextWriter), type, typeof(int) }, doVerify: Utils.DoVerify);
                emit.LoadArgument(0);           // TextWriter
                emit.LoadArgument(1);           // TextWriter T
                emit.LoadField(optionsField);   // TextWriter T options
                emit.LoadArgument(2);           // TextWriter T options int
                emit.Call(serializeMtd);        // --empty--
                emit.Return();

                Cache[opts] = cached = emit.CreateDelegate<Action<TextWriter, T, int>>(Utils.DelegateOptimizationOptions);

                return cached;
            }
        }
    }
}

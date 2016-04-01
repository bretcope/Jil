﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Sigil;
using StringInterningJil.Common;

namespace StringInterningJil.Serialize
{
    class SerializeDynamicThunk<T>
    {
        public static Action<object, TextWriter, Options> Thunk;

        static SerializeDynamicThunk()
        {
            var serializeGenMtd = typeof(StringInterningJSON).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(s => s.Name == "Serialize" && s.ReturnType == typeof(void));

            var t = typeof(T);
            var serialize = serializeGenMtd.MakeGenericMethod(t);

            var emit = Emit<Action<object, TextWriter, Options>>.NewDynamicMethod();
            
            emit.LoadArgument(0);       // obj
            if (t.IsValueType)
            {
                emit.UnboxAny<T>();     // T
            }
            else
            {
                emit.CastClass(t);      // T
            }

            emit.LoadArgument(1);
            emit.LoadArgument(2);

            emit.Call(serialize);
            emit.Return();

            Thunk = emit.CreateDelegate(Utils.DelegateOptimizationOptions);
        }
    }
}

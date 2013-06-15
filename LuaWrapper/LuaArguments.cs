using System;
using System.Collections.Generic;

namespace LuaWrapper
{
    public class LuaArguments : List<object>, IPushable
    {
        public LuaArguments()
        {
        }

        public LuaArguments(LuaState state)
        {
            int top = state.GetTop();
            for (int i = 1; i <= top; i++)
                Insert(0, state.GetFromStack(-i));
        }

        #region IPushable Members

        public void Push(LuaState state)
        {
            foreach (object item in this)
            {
                state.Push(item);
            }
        }

        #endregion

        public T Get<T>(int idx)
        {
            try
            {
                return (T) this[idx];
            }
            catch (InvalidCastException)
            {
                throw new LuaWrongArgumentTypeException(idx, typeof (T).ToString());
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new LuaWrongArgumentCountException(idx);
            }
        }
    }

    public abstract class LuaArgumentException : LuaException
    {
        protected LuaArgumentException(string error)
            : base(error)
        {
        }
    }

    public class LuaWrongArgumentCountException : LuaArgumentException
    {
        public LuaWrongArgumentCountException(int count)
            : base(String.Format("bad argument #{0} (got no value)", count))
        {
            Count = count;
        }

        public int Count { get; set; }
    }

    public class LuaWrongArgumentTypeException : LuaArgumentException
    {
        public LuaWrongArgumentTypeException(int idx, string typeName)
            : base(String.Format("bad argument #{0} ({1} expected)", idx, typeName))
        {
            Index = idx;
            TypeName = typeName;
        }

        public int Index { get; set; }
        public string TypeName { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Tao.Lua;

namespace LuaWrapper
{
    public abstract class LuaClass : IPushable
    {
        private static readonly Dictionary<IntPtr, LuaClass> UserdataValues = new Dictionary<IntPtr, LuaClass>();

        #region IPushable Members

        public int Count
        {
            get { return 1; }
        }

        public void Push(LuaState state)
        {
            var ptr = (IntPtr) GetHashCode();
            if (!UserdataValues.ContainsKey(ptr))
            {
                UserdataValues[ptr] = this;
            }
            Lua.lua_pushlightuserdata(state.State, ptr);
        }

        #endregion

        public static LuaClass GetFromUserdata(IntPtr userData)
        {
            try
            {
                return UserdataValues[userData];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}
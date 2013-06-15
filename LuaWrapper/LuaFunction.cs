using System;
using Tao.Lua;

namespace LuaWrapper
{
    public class LuaFunction : LuaReference
    {
        internal LuaFunction(LuaState state, int @ref) : base(state, @ref)
        {
        }

        public object Call(params object[] args)
        {
            var fa = new LuaArguments();
            fa.AddRange(args);
            return Call(fa);
        }

        public object Call(LuaArguments args = null)
        {
            State.Push(this);
            if (args != null)
            {
                args.Push(State);
            }
            if (Lua.lua_pcall(State.State, args != null ? args.Count : 0, 1, 0) != 0)
            {
                var err = ((string) State.GetFromStack(-1));
                Lua.lua_pop(State.State, 1);
                throw new LuaException(err);
            }
            var ret = State.GetFromStack(-1);
            Lua.lua_pop(State.State, 1);
            return ret;
        }

        public override string ToString()
        {
            State.Push(this);
            IntPtr ret = Lua.lua_topointer(State.State, -1);
            Lua.lua_pop(State.State, 1);
            return String.Format("{0}: {1}", "function", ret);
        }
    }
}
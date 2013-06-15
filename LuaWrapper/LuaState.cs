using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tao.Lua;

namespace LuaWrapper
{
    public class LuaState : IDisposable
    {
        #region Delegates

        public delegate int Function(LuaState state, LuaArguments arguments);

        #endregion

        private readonly List<Lua.lua_CFunction> _functions = new List<Lua.lua_CFunction>();
        private readonly IntPtr _state;

        public LuaState()
        {
            _state = Lua.lua_open();
        }

        public LuaState(IntPtr state)
        {
            _state = state;
        }

        internal IntPtr State
        {
            get { return _state; }
        }

        public object this[string name]
        {
            get
            {
                Lua.lua_getglobal(_state, name);
                object ret = GetFromStack(-1);
                Lua.lua_pop(_state, 1);
                return ret;
            }
            set
            {
                Push(value);
                Lua.lua_setglobal(_state, name);
            }
        }

        public bool Disposed { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return _state == ((LuaState) obj)._state;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return _state.ToInt32();
        }

        public T Get<T>(string name)
        {
            return (T) this[name];
        }

        public void OpenLibraries()
        {
            Lua.luaL_openlibs(_state);
        }

        public object DoString(string code)
        {
            if (Lua.luaL_loadstring(_state, code) != 0 || Lua.lua_pcall(_state, 0, 1, 0) != 0)
            {
                var err = ((string) GetFromStack(-1));
                Lua.lua_pop(_state, 1);
                throw new LuaException(err);
            }
            object ret = GetFromStack(-1);
            Lua.lua_pop(_state, 1);
            return ret;
        }

        public void DoFile(string fileName)
        {
            Lua.luaL_dofile(_state, fileName);
        }

        public LuaTable CreateTable()
        {
            Lua.lua_newtable(_state);
            int rf = Lua.luaL_ref(_state, Lua.LUA_REGISTRYINDEX);
            return new LuaTable(this, rf);
        }

        public int GetTop()
        {
            return Lua.lua_gettop(_state);
        }

        internal object GetFromStack(int idx)
        {
            if (idx > 0 && Lua.lua_gettop(_state) < idx)
            {
                throw new ArgumentException("idx must be less than stack", "idx");
            }
            int rf;
            switch (Lua.lua_type(_state, idx))
            {
                case Lua.LUA_TNUMBER:
                    return Lua.lua_tonumber(_state, idx);
                case Lua.LUA_TSTRING:
                    return Lua.lua_tostring(_state, idx);
                case Lua.LUA_TBOOLEAN:
                    return Lua.lua_toboolean(_state, idx) == 1;
                case Lua.LUA_TTABLE:
                    Lua.lua_pushvalue(_state, idx);
                    rf = Lua.luaL_ref(_state, Lua.LUA_REGISTRYINDEX);
                    return new LuaTable(this, rf);
                case Lua.LUA_TFUNCTION:
                    Lua.lua_pushvalue(_state, idx);
                    rf = Lua.luaL_ref(_state, Lua.LUA_REGISTRYINDEX);
                    return new LuaFunction(this, rf);
                case Lua.LUA_TNIL:
                case Lua.LUA_TNONE:
                    return null;
                case Lua.LUA_TLIGHTUSERDATA:
                    return LuaClass.GetFromUserdata(Lua.lua_touserdata(_state, idx));
                default:
                    return null;
            }
        }

        ~LuaState()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (Disposed) return;
            if (disposing)
                GC.SuppressFinalize(this);
            Disposed = true;
            _functions.Clear(); // Not really necessary
            // TODO: Find out why this don't work
            Lua.lua_close(_state);
        }

        #region Push

        public void Push(object item)
        {
            if (item == null)
                PushNil();
            else if (item is int)
                Push((int) item);
            else if (item is double)
                Push((double) item);
            else if (item is float)
                Push((double) (float) item);
            else if (item is string)
                Push((string) item);
            else if (item is bool)
                Push((bool) item);
            else if (item is LuaReference)
                Push((LuaReference) item);
            else if (item is Function)
                Push((Function) item);
            else if (item is IPushable)
                Push((IPushable) item);
            else
                Debug.Fail("Type " + item.GetType() + " is not supported!");
        }

        public void Push(int item)
        {
            Lua.lua_pushinteger(_state, item);
        }

        public void Push(double item)
        {
            Lua.lua_pushnumber(_state, item);
        }

        public void Push(float item)
        {
            Lua.lua_pushnumber(_state, item);
        }

        public void Push(string item)
        {
            Lua.lua_pushstring(_state, item);
        }

        public void Push(bool item)
        {
            Lua.lua_pushboolean(_state, item ? 1 : 0);
        }

        public void Push(LuaReference item)
        {
            Lua.lua_rawgeti(_state, Lua.LUA_REGISTRYINDEX, item.Ref);
        }

        public void Push(Function item)
        {
            Push(p =>
                     {
                         LuaState s = this;
                         try
                         {
                             return item(s, new LuaArguments(s));
                         }
                         catch (LuaArgumentException e)
                         {
                             Push(e.Message);
                             return Lua.lua_error(s.State);
                         }
                     }
                );
        }

        internal void Push(Lua.lua_CFunction item)
        {
            Lua.lua_pushcfunction(_state, item);
            _functions.Add(item);
        }

        public void Push(IPushable item)
        {
            item.Push(this);
        }

        public void PushNil()
        {
            Lua.lua_pushnil(_state);
        }

        #endregion
    }
}
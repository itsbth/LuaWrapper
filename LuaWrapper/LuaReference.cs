using System;
using Tao.Lua;

namespace LuaWrapper
{
    public abstract class LuaReference
    {
        protected readonly LuaState State;
        private readonly int _ref;
        internal int Ref { get { return _ref; } }
        private bool _disposed;

        internal LuaReference(LuaState state, int @ref)
        {
            State = state;
            _ref = @ref;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LuaReference))
            {
                return false;
            }

            return _ref == ((LuaReference) obj)._ref;
        }

        public override int GetHashCode()
        {
            return State.GetHashCode() + _ref;
        }

        ~LuaReference()
        {
            Dispose(false);
        }

        public void Dispose(bool disposing = true)
        {
            if (_disposed || State.Disposed) return;
            if (disposing)
                GC.SuppressFinalize(this);
            _disposed = true;
            Lua.luaL_unref(State.State, Lua.LUA_REGISTRYINDEX, Ref);
        }
    }
}
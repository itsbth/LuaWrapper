using System;
using System.Collections;
using System.Collections.Generic;
using Tao.Lua;

namespace LuaWrapper
{
    public class LuaTable : LuaReference, IEnumerable<KeyValuePair<object, object>>
    {
        internal LuaTable(LuaState state, int @ref) : base(state, @ref)
        {
        }

        public object this[object key]
        {
            get
            {
                State.Push(this);
                State.Push(key);
                object ret = State.GetFromStack(-1);
                Lua.lua_pop(State.State, 2);
                return ret;
            }
            set
            {
                State.Push(this);
                State.Push(key);
                State.Push(value);
                Lua.lua_settable(State.State, -3);
                Lua.lua_pop(State.State, 1);
            }
        }

        #region IEnumerable<KeyValuePair<object,object>> Members

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            return new LuaTableEnumerator(this, State);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public T Get<T>(object key)
        {
            return (T) this[key];
        }

        public override string ToString()
        {
            State.Push(this);
            IntPtr ret = Lua.lua_topointer(State.State, -1);
            return String.Format("{0}: {1}", "table", ret);
        }
    }

    public class LuaTableEnumerator : IEnumerator<KeyValuePair<object, object>>
    {
        private readonly LuaState _state;
        private readonly LuaTable _table;

        public LuaTableEnumerator(LuaTable table, LuaState state)
        {
            _table = table;
            _state = state;
            Current = new KeyValuePair<object, object>(null, null);
        }

        #region IEnumerator<KeyValuePair<object,object>> Members

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            _state.Push(_table);
            _state.Push(Current.Key);
            if (Lua.lua_next(_state.State, 2) != 0)
            {
                Current = new KeyValuePair<object, object>(_state.GetFromStack(-2), _state.GetFromStack(-1));
                Lua.lua_pop(_state.State, 3);
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Current = new KeyValuePair<object, object>(null, null);
        }

        public KeyValuePair<object, object> Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}
using System;

namespace LuaWrapper
{
    public class LuaException : Exception
    {
        public LuaException(string error) : base(error)
        {
        }
    }
}
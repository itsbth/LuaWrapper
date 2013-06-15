namespace LuaWrapper
{
    public interface IPushable
    {
        int Count { get; }
        void Push(LuaState state);
    }
}
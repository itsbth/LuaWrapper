using System;
using System.Linq;
using LuaWrapper;

namespace CallLuaFunctionTest
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var state = new LuaState();
            state.OpenLibraries();
            state["print"] = new LuaState.Function((s, arguments) =>
                                                       {
                                                           Console.WriteLine(String.Join("\t",
                                                                                         arguments.Select(
                                                                                             i =>
                                                                                             i != null
                                                                                                 ? i.ToString()
                                                                                                 : "nil").
                                                                                             ToArray()));
                                                           return 0;
                                                       });
            state.Get<LuaTable>("io")["write"] =
                new LuaState.Function((s, arguments) =>
                                          {
                                              Console.Write("{0}", arguments[0]);
                                              return 0;
                                          });
            state["foreach"] = new LuaState.Function((s, a) =>
                                                         {
                                                             var function = a.Get<LuaFunction>(1);
                                                             foreach (var argument in a.Get<LuaTable>(0))
                                                             {
                                                                 function.Call(argument.Key, argument.Value);
                                                             }
                                                             return 0;
                                                         });
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(">>> ");
            Console.ForegroundColor = ConsoleColor.Gray;
            string line = Console.ReadLine();
            do
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    object ret = state.DoString(line);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(": {0}", ret);
                    state["_"] = ret;
                    GC.Collect();
                }
                catch (LuaException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("! {0}", e.Message);
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write(">>> ");
                Console.ForegroundColor = ConsoleColor.Gray;
            } while ((line = Console.ReadLine()) != ".quit");
            state.Dispose();
        }
    }
}
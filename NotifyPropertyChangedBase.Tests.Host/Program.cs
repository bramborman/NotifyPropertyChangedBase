using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NotifyPropertyChangedBase.Tests.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.CursorVisible = false;

            Tests tests = new Tests();
            Stopwatch stopwatch = new Stopwatch();

            foreach (MethodInfo methodInfo in typeof(Tests).GetRuntimeMethods().Where(mi => mi.GetCustomAttribute(typeof(TestMethodAttribute)) != null))
            {
                stopwatch.Restart();

                try
                {
                    methodInfo.Invoke(tests, null);
                    stopwatch.Stop();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Success: {methodInfo.Name} ({stopwatch.ElapsedMilliseconds}ms)");
                }
                catch (Exception exception)
                {
                    stopwatch.Stop();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Fail: {methodInfo.Name} ({stopwatch.ElapsedMilliseconds}ms)");

                    StringBuilder stringBuilder = new StringBuilder("\t");

                    // First exception is telling just that invoking method failed
                    exception = exception.InnerException;
                    string stackTrace = exception.StackTrace;

                    while (exception != null)
                    {
                        stringBuilder.AppendLine(exception.Message);

                        exception = exception.InnerException;
                    }

                    stringBuilder.AppendLine(stackTrace);

                    stringBuilder.Replace(Environment.NewLine, Environment.NewLine + '\t');
                    Console.WriteLine(stringBuilder.ToString());
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Environment.NewLine + "Press any key to exit . . .");
            Console.ReadKey(true);
        }
    }
}

// MIT License
//
// Copyright (c) 2017 Marian Dolinský
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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

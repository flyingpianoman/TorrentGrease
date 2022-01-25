using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SpecificationTest.Crosscutting
{
    public static class TestLogger
    {
        public static void LogElapsedTime(Action action, string actionName)
        {
            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();
            LogDebug($"{actionName} took {sw.ElapsedMilliseconds}ms");
        }
        public static async Task LogElapsedTimeAsync(Func<Task> actionAsync, string actionName)
        {
            var sw = Stopwatch.StartNew();
            await actionAsync();
            sw.Stop();
            LogDebug($"{actionName} took {sw.ElapsedMilliseconds}ms");
        }

        public static void LogDebug(string message)
        {
            Debug.WriteLine(message);
        }
    }
}

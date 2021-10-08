using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeskTopWPF.Domain
{
    public static class Extension
    {
        public static IEnumerable<T> SelectManyRecursive<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var result = source.SelectMany(selector);
            if (!result.Any())
            {
                return result.Concat(source);
            }
            return result.Concat(result.SelectManyRecursive(selector));
        }

        public static Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout)
        {
            ManualResetEvent processWaitObject = new ManualResetEvent(false);
            processWaitObject.SafeWaitHandle = new SafeWaitHandle(process.Handle, false);

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            RegisteredWaitHandle registeredProcessWaitHandle = null;
            registeredProcessWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                processWaitObject,
                delegate (object state, bool timedOut)
                {
                    if (!timedOut)
                    {
                        registeredProcessWaitHandle.Unregister(null);
                    }

                    processWaitObject.Dispose();
                    tcs.SetResult(!timedOut);
                },
                null /* state */,
                timeout,
                true /* executeOnlyOnce */);

            return tcs.Task;
        }
    }
}

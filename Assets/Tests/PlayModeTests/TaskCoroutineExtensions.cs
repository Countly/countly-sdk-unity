using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Assets.Tests.PlayModeTests
{
    /// <summary>
    /// Bridges Task-returning async SDK calls into Unity coroutines so the same
    /// [UnityTest] tests run on every supported Unity / Test Framework version
    /// (2020.3 LTS through Unity 6). Neither 'async void' nor 'async Task' [Test]
    /// methods are accepted by both editor lines, but [UnityTest] IEnumerator is.
    /// </summary>
    public static class TaskCoroutineExtensions
    {
        /// <summary>
        /// Yields each frame until the task completes, then rethrows the original
        /// exception (preserving its stack) if the task faulted.
        /// </summary>
        public static IEnumerator AsCoroutine(this Task task)
        {
            while (!task.IsCompleted) {
                yield return null;
            }

            if (task.IsFaulted) {
                Exception ex = (task.Exception != null && task.Exception.InnerExceptions.Count == 1)
                    ? task.Exception.InnerException
                    : task.Exception;
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }
    }
}

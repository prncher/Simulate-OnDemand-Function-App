using System.Reflection;

namespace OnDemandTaskAsynchPatternWithHostService
{
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Invokes an async method via reflection and returns a Task you can await.
        /// Works for both Task and Task<T> return types.
        /// </summary>
        public static async Task<object?> InvokeAsync(this MethodInfo methodInfo, object? obj, params object?[]? parameters)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            // Invoke the method (synchronously starts the Task)
            var result = methodInfo.Invoke(obj, parameters ?? Array.Empty<object?>());

            if (result is Task task)
            {
                // Await the task
                await task.ConfigureAwait(false);

                // If it's Task<T>, get the Result property
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty != null ? resultProperty.GetValue(task) : null;
            }

            // If method is not async, just return the result
            return result;
        }
    }
}

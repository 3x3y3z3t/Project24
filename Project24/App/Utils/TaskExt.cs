/*  App/Utils/TaskExt.cs
 *  Version: v1.0 (2023.09.07)
 *  
 *  Contributor
 *      Arime-chan
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project24.App.Utils
{
    public class TaskExt
    {
        public static async Task WhenAll(params Task[] _tasks)
        {
            var allTasks = Task.WhenAll(_tasks);

            try
            {
                await allTasks;
            }
            catch (Exception)
            {
                throw allTasks.Exception ?? throw new NullReferenceException("TaskExt: allTasks.Exception should not be null.");
            }
        }

        public static async Task<IEnumerable<TResult>> WhenAll<TResult>(params Task<TResult>[] _tasks)
        {
            var allTasks = Task.WhenAll(_tasks);

            try
            {
                return await allTasks;
            }
            catch (Exception)
            {
                throw allTasks.Exception ?? throw new NullReferenceException("TaskExt: allTasks.Exception should not be null.");
            }
        }
    }

}

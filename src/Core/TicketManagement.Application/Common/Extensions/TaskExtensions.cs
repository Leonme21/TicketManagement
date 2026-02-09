namespace TicketManagement.Application.Common.Extensions;

/// <summary>
/// ?? BIG TECH LEVEL: Extension methods for parallel task execution
/// Provides cleaner syntax for executing multiple tasks concurrently
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Executes two tasks in parallel and returns their results as a tuple
    /// </summary>
    public static async Task<(T1, T2)> WhenAll<T1, T2>(this (Task<T1> task1, Task<T2> task2) tasks)
    {
        await Task.WhenAll(tasks.task1, tasks.task2);
        return (tasks.task1.Result, tasks.task2.Result);
    }

    /// <summary>
    /// Executes three tasks in parallel and returns their results as a tuple
    /// </summary>
    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(this (Task<T1> task1, Task<T2> task2, Task<T3> task3) tasks)
    {
        await Task.WhenAll(tasks.task1, tasks.task2, tasks.task3);
        return (tasks.task1.Result, tasks.task2.Result, tasks.task3.Result);
    }

    /// <summary>
    /// Executes four tasks in parallel and returns their results as a tuple
    /// </summary>
    public static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(
        this (Task<T1> task1, Task<T2> task2, Task<T3> task3, Task<T4> task4) tasks)
    {
        await Task.WhenAll(tasks.task1, tasks.task2, tasks.task3, tasks.task4);
        return (tasks.task1.Result, tasks.task2.Result, tasks.task3.Result, tasks.task4.Result);
    }

    /// <summary>
    /// Executes a task with a timeout
    /// </summary>
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, T defaultValue = default!)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        return completedTask == task ? await task : defaultValue;
    }

    /// <summary>
    /// Executes a task and ignores any exceptions
    /// </summary>
    public static async Task<T?> IgnoreExceptions<T>(this Task<T> task) where T : class
    {
        try
        {
            return await task;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Executes a task and returns a default value on exception
    /// </summary>
    public static async Task<T> OnException<T>(this Task<T> task, T defaultValue)
    {
        try
        {
            return await task;
        }
        catch
        {
            return defaultValue;
        }
    }
}

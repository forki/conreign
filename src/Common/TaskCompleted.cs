using System.Threading.Tasks;

internal static class TaskCompleted
{
    public static Task Completed => Task.FromResult<int>(0);
}
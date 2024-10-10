using Microsoft.Build.Framework;

namespace Yamamari.AutoVersion.Extensions;

internal static class ExtendITask
{
    public static void LogInfo(this ITask task, string message)
    {
        var logger = new Microsoft.Build.Utilities.TaskLoggingHelper(task);
        logger.LogMessage(message);
    }
    
    public static void LogWarn(this ITask task, string message)
    {
        var logger = new Microsoft.Build.Utilities.TaskLoggingHelper(task);
        logger.LogWarning(message);
    }
    
    public static void LogFail(this ITask task, string message)
    {
        var logger = new Microsoft.Build.Utilities.TaskLoggingHelper(task);
        logger.LogError(message);
    }
}
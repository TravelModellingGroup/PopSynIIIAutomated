﻿using System;

namespace PopSynIIIAutomated;

/// <summary>
/// Provides calls for the main Use-Cases.
/// </summary>
internal static class Runtime
{
    /// <summary>
    /// Execute a complete run with PopsynIII given configuration.
    /// </summary>
    /// <param name="config">The configuration to execute.</param>
    /// <returns>True if the run completed successfully.</returns>
    public static bool RunAll(Configuration config)
    {
        return Preprocessor.Run(config)
        && PopSynController.Run(config)
        && Postprocessor.Run(config);
    }

    /// <summary>
    /// Just run the PreProcessor without any additional steps.
    /// </summary>
    /// <param name="config">The configuration to use.</param>
    /// <returns>True if the operation succeeds.</returns>
    public static bool RunPreProcessor(Configuration config)
    {
        return Preprocessor.Run(config);
    }

    /// <summary>
    /// Restart the PopSyn run without recomputing the Pre-processor.
    /// </summary>
    /// <param name="config">The configuration to use.</param>
    /// <returns>True if the operation succeeds.</returns>
    public static bool RunWithoutPreProcessor(Configuration config)
    {
        return PopSynController.Run(config)
            && Postprocessor.Run(config);
    }

    /// <summary>
    /// Run the post-processor. This will export the previously computed results.
    /// </summary>
    /// <param name="config">The configuration to use.</param>
    /// <returns>True if the operation succeeds.</returns>
    public static bool RunPostProcessor(Configuration config)
    {
        return Postprocessor.Run(config);
    }

    /// <summary>
    /// Set the function for displaying messages to the user.
    /// </summary>
    public static Action<string>? DisplayToUser;

    /// <summary>
    /// This is called internally to send messages to the user.
    /// </summary>
    /// <param name="message">The message to send to the user.</param>
    internal static void WriteToUser(string message)
    {
        DisplayToUser?.Invoke(message);
    }
}

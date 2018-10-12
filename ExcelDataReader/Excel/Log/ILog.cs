﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Excel.Log
{
/// <summary>
/// Custom interface for logging messages
/// </summary>
public partial interface ILog
{
    /// <summary>
    /// Initializes the instance for the logger name
    /// </summary>
    /// <param name="loggerName">Name of the logger</param>
    void InitializeFor(string loggerName);
    
    /// <summary>
    /// Debug level of the specified message. The other method is preferred since the execution is deferred.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="formatting">The formatting.</param>
    void Debug(string message, params object[] formatting);

    /// <summary>
    /// Info level of the specified message. The other method is preferred since the execution is deferred.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="formatting">The formatting.</param>
    void Info(string message, params object[] formatting);

    /// <summary>
    /// Warn level of the specified message. The other method is preferred since the execution is deferred.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="formatting">The formatting.</param>
    void Warn(string message, params object[] formatting);

    /// <summary>
    /// Error level of the specified message. The other method is preferred since the execution is deferred.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="formatting">The formatting.</param>
    void Error(string message, params object[] formatting);

    /// <summary>
    /// Fatal level of the specified message. The other method is preferred since the execution is deferred.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="formatting">The formatting.</param>
    void Fatal(string message, params object[] formatting);

}

/// <summary>
/// Ensures a default constructor for the logger type
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ILog<T> where T : new()
{
}

}


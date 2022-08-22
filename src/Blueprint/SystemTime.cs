﻿using System;
using System.Threading;

namespace Blueprint;

/// <summary>
/// Provides an abstraction over DateTime, and in particular the current time, to allow easier testing
/// of time-sensitive functions.
/// </summary>
/// <remarks>
/// Any function that requires access to the current time should instead use the <see cref="UtcNow"/>
/// property as tests can change this, unlike if DateTime.Now was used, which can not be mocked out and
/// manipulated.
/// </remarks>
public static class SystemTime
{
    private static readonly AsyncLocal<SystemTimeModifier> _modifier = new AsyncLocal<SystemTimeModifier>();

    /// <summary>
    /// Gets the current UTC date and time, which in a production environment would simply delegate to <see cref="DateTime.UtcNow"/> but
    /// can be controlled in tests using <see cref="PauseForThread" />.
    /// </summary>
    public static DateTime UtcNow => _modifier.Value?.UtcNow ?? DateTime.UtcNow;

    /// <summary>
    /// Gets the current local date and time, which in a production environment would simply delegate to <see cref="DateTime.Now"/> but
    /// can be controlled in tests using <see cref="PauseForThread" />.
    /// </summary>
    public static DateTime Now => _modifier.Value?.Now ?? DateTime.Now;

    /// <summary>
    /// Pauses time for the current thread / async context to make it possible to control time for any client that
    /// uses the <see cref="SystemTime" /> properties to get the current time.
    /// </summary>
    /// <remarks>
    /// This method will set the time to a static value the same as <see cref="DateTime.UtcNow" />
    /// at the time of execution.
    /// </remarks>
    /// <returns>A <see cref="SystemTimeModifier" /> which allows for modification of time.</returns>
    public static SystemTimeModifier PauseForThread()
    {
        _modifier.Value = new SystemTimeModifier();

        return _modifier.Value;
    }

    /// <summary>
    /// Restores <see cref="UtcNow"/> to its default of returning <see cref="DateTime.UtcNow"/>.
    /// Restores <see cref="Now"/> to its default of returning <see cref="DateTime.Now"/>.
    /// </summary>
    public static void ResumeForThread()
    {
        _modifier.Value = null;
    }

    public class SystemTimeModifier : IDisposable
    {
        internal SystemTimeModifier()
        {
            this.UtcNow = SystemTime.UtcNow;
            this.Now = SystemTime.Now;
        }

        /// <summary>
        /// Gets the current date and time, as defined by the current <see cref="getUtcNow"/> method, which in
        /// a production environment would simply delegate to <see cref="DateTime.UtcNow"/>.
        /// </summary>
        public DateTime UtcNow { get; private set; }

        /// <summary>
        /// Gets the current date and time, as defined by the current <see cref="getNow"/> method, which in
        /// a production environment would simply delegate to <see cref="DateTime.Now"/>.
        /// </summary>
        public DateTime Now { get; private set; }

        /// <summary>
        /// Sets both <see cref="UtcNow" /> and <see cref="Now" /> to the given <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date to set</param>
        public void Set(DateTime dateTime)
        {
            this.UtcNow = dateTime;
            this.Now = dateTime;
        }

        /// <summary>
        /// Adds the given <see cref="TimeSpan" /> to both <see cref="UtcNow" /> and <see cref="Now" />.
        /// </summary>
        /// <param name="timeSpan">The amount to progress by.</param>
        public void FastForward(TimeSpan timeSpan)
        {
            this.UtcNow = this.UtcNow.Add(timeSpan);
            this.Now = this.Now.Add(timeSpan);
        }

        /// <summary>
        /// Subtracts the given <see cref="TimeSpan" /> from both <see cref="UtcNow" /> and <see cref="Now" />.
        /// </summary>
        /// <param name="timeSpan">The amount to rewind by.</param>
        public void Rewind(TimeSpan timeSpan)
        {
            this.UtcNow = this.UtcNow.Subtract(timeSpan);
            this.Now = this.Now.Subtract(timeSpan);
        }

        /// <summary>
        /// Sets <see cref="UtcNow" /> to the given <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">The date to set</param>
        public void SetUtcNow(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("DateTime must have kind of UTC", nameof(dateTime));
            }

            this.UtcNow = dateTime;
        }

        /// <summary>
        /// Adds the given <see cref="TimeSpan" /> to <see cref="UtcNow" />.
        /// </summary>
        /// <param name="timeSpan">The amount to progress by.</param>
        public void FastForwardUtc(TimeSpan timeSpan)
        {
            this.UtcNow = this.UtcNow.Add(timeSpan);
        }

        /// <summary>
        /// Subtracts the given <see cref="TimeSpan" /> from <see cref="UtcNow" />.
        /// </summary>
        /// <param name="timeSpan">The amount to rewind by.</param>
        public void RewindUtc(TimeSpan timeSpan)
        {
            this.UtcNow = this.UtcNow.Subtract(timeSpan);
        }

        public void Dispose()
        {
            ResumeForThread();
        }
    }
}
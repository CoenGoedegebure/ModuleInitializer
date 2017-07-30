using System;

namespace Injector
{
    /// <summary>
    /// An exception class to indicate a problem with the module injection
    /// </summary>
    [Serializable]
    public class InjectionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionException"/> class.
        /// </summary>
        public InjectionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InjectionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InjectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionException" /> class.
        /// </summary>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="parameters">The parameters.</param>
        public InjectionException(string formatMessage, params object[] parameters)
            : base(string.Format(formatMessage, parameters))
        {
        }
    }
}

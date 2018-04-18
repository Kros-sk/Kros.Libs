using System;

namespace Kros.KORM.Exceptions
{
    /// <summary>
    /// Exception class for composite primary key.
    /// </summary>
    public class CompositePrimaryKeyException : Exception
    {       

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositePrimaryKeyException" /> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public CompositePrimaryKeyException(string message) :base(message)
        {
        }
    }
}
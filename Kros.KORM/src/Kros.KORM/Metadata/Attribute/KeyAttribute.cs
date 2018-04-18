using System;

namespace Kros.KORM.Metadata.Attribute
{
    /// <summary>
    /// Attribute, which describe property, which are part of primary key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : System.Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
        /// </summary>
        public KeyAttribute()
            : this(null, AutoIncrementMethodType.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
        /// </summary>
        /// <param name="name">The key name.</param>
        public KeyAttribute(string name)
            : this(name, AutoIncrementMethodType.None)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
        /// </summary>
        /// <param name="autoIncrementMethodType">Type of primary key auto increment method.</param>
        public KeyAttribute(AutoIncrementMethodType autoIncrementMethodType)
            : this(null, autoIncrementMethodType)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttribute"/> class.
        /// </summary>
        /// <param name="name">The key name.</param>
        /// <param name="autoIncrementMethodType">Type of primary key auto increment method.</param>
        public KeyAttribute(string name, AutoIncrementMethodType autoIncrementMethodType)
        {
            this.AutoIncrementMethodType = autoIncrementMethodType;
            this.Name = name;
        }

        /// <summary>
        /// Type of primary key auto increment method.
        /// </summary>
        public AutoIncrementMethodType AutoIncrementMethodType { get; private set; }

        /// <summary>
        /// Gets the name of key.
        /// </summary>
        public string Name { get; private set; }
    }
}

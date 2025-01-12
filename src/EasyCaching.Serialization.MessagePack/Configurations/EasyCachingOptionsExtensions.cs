﻿namespace EasyCaching.Serialization.MessagePack
{
    using EasyCaching.Core.Configurations;

    /// <summary>
    /// Easy caching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the message pack.
        /// </summary>
        /// <returns>The message pack.</returns>
        /// <param name="options">Options.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions WithMessagePack(this EasyCachingOptions options, string name = "msgpack")
        {
            options.RegisterExtension(new MessagePackOptionsExtension(name));

            return options;
        }
    }
}

﻿namespace EasyCaching.Serialization.Protobuf
{
    using EasyCaching.Core.Configurations;

    /// <summary>
    /// EasyCaching options extensions.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Withs the protobuf.
        /// </summary>
        /// <returns>The protobuf.</returns>
        /// <param name="options">Options.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions WithProtobuf(this EasyCachingOptions options, string name = "proto")
        {
            options.RegisterExtension(new ProtobufOptionsExtension(name));

            return options;
        }
    }
}

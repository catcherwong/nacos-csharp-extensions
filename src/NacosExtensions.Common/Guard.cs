namespace NacosExtensions.Common
{
    using System;

    public static class Guard
    {
        /// <summary>
        /// Validates that <paramref name="argument"/> is not null , otherwise throws an exception.
        /// </summary>
        /// <param name="argument">Argument.</param>
        /// <param name="argumentName">Argument name.</param>
        /// <exception cref="ArgumentNullException" />
        public static void NotNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}

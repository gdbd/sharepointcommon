// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// General exception, thrown by 'SharepointCommon' in different error situations.
    /// </summary>
    public class SharepointCommonException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SharepointCommonException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SharepointCommonException(string message) : base(message) { }
    }
}

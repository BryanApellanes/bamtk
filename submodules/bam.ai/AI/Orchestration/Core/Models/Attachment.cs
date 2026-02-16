using System;
using System.IO;

namespace Bam.AI.Orchestration.Core.Models
{
    /// <summary>
    /// Concrete implementation of a file attachment.
    /// </summary>
    /// <remarks>
    /// Represents a file attached to a prompt. The content is provided as a stream
    /// to support efficient handling of large files. The stream should be disposed
    /// by the consumer after processing.
    /// 
    /// Thread Safety: Not thread-safe. The underlying stream may not support concurrent access.
    /// </remarks>
    public class Attachment : IAttachment, IDisposable
    {
        /// <summary>
        /// Gets the filename of the attachment.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the MIME type of the attachment.
        /// </summary>
        public string MimeType { get; }

        /// <summary>
        /// Gets the content stream of the attachment.
        /// </summary>
        public Stream Content { get; }

        /// <summary>
        /// Gets the size of the attachment in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        public Attachment(string name, string mimeType, Stream content, long size)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("MimeType cannot be null or empty", nameof(mimeType));

            if (size < 0)
                throw new ArgumentException("Size cannot be negative", nameof(size));

            Name = name;
            MimeType = mimeType;
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Size = size;
        }

        /// <summary>
        /// Disposes the content stream.
        /// </summary>
        public void Dispose()
        {
            Content?.Dispose();
        }
    }
}
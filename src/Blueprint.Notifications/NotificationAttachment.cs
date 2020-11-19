using System.IO;
using System.Net.Mail;

namespace Blueprint.Notifications
{
    /// <summary>
    /// Used for attaching documents or streams of data to an email.
    /// </summary>
    public class NotificationAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachment" /> class.
        /// </summary>
        /// <param name="physicalFilePath">The file path.</param>
        public NotificationAttachment(string physicalFilePath)
        {
            Guard.NotNullOrEmpty(nameof(physicalFilePath), physicalFilePath);

            this.Name = Path.GetFileName(physicalFilePath);
            this.PhysicalFilePath = physicalFilePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachment" /> class.
        /// </summary>
        /// <param name="physicalFilePath">The file path.</param>
        /// <param name="name">The name.</param>
        public NotificationAttachment(string physicalFilePath, string name)
        {
            Guard.NotNullOrEmpty(nameof(physicalFilePath), physicalFilePath);
            Guard.NotNullOrEmpty(nameof(name), name);

            this.Name = name;
            this.PhysicalFilePath = physicalFilePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachment" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="name">The name.</param>
        public NotificationAttachment(Stream stream, string name)
        {
            Guard.NotNull(nameof(stream), stream);
            Guard.NotNullOrEmpty(nameof(name), name);

            this.Name = name;
            this.Stream = stream;
        }

        /// <summary>
        /// Gets the friendly name of the attachment.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the physical file path.
        /// </summary>
        public string PhysicalFilePath { get; }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <value>The stream.</value>
        public Stream Stream { get; }

        public Attachment ToMailAttachment()
        {
            if (this.Stream == null)
            {
                return new Attachment(this.PhysicalFilePath) { Name = this.Name };
            }

            return new Attachment(this.Stream, this.Name);
        }
    }
}

using System.IO;
using System.Net.Mail;
using Blueprint.Core;

namespace Blueprint.Notifications.Notifications
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
            Guard.NotNullOrEmpty("physicalFilePath", physicalFilePath);

            Name = Path.GetFileName(physicalFilePath);
            PhysicalFilePath = physicalFilePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachment" /> class.
        /// </summary>
        /// <param name="physicalFilePath">The file path.</param>
        /// <param name="name">The name.</param>
        public NotificationAttachment(string physicalFilePath, string name)
        {
            Guard.NotNullOrEmpty("physicalFilePath", physicalFilePath);
            Guard.NotNullOrEmpty("name", name);

            Name = name;
            PhysicalFilePath = physicalFilePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationAttachment" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="name">The name.</param>
        public NotificationAttachment(Stream stream, string name)
        {
            Guard.NotNull(nameof(stream), stream);
            Guard.NotNullOrEmpty("name", name);

            Name = name;
            Stream = stream;
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
            if (Stream == null)
            {
                return new Attachment(PhysicalFilePath) { Name = Name };
            }

            return new Attachment(Stream, Name);
        }
    }
}

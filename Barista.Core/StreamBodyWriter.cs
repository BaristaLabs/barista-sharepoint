namespace Barista
{
    using System;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.Xml;

    /// <summary>An abstract base class used to create custom <see cref="T:System.ServiceModel.Channels.BodyWriter" /> classes that can be used to a message body as a stream.</summary>
    public abstract class StreamBodyWriter : BodyWriter
    {
        protected StreamBodyWriter(bool isBuffered)
            : base(isBuffered)
        {
        }

        public static StreamBodyWriter CreateStreamBodyWriter(Action<Stream> streamAction)
        {
            if (streamAction == null)
                throw new ArgumentNullException("streamAction");

            return new StreamBodyWriter.ActionOfStreamBodyWriter(streamAction);
        }

        /// <summary>Override this method to handle writing the message body contents.</summary>
        /// <param name="stream">The stream to write to.</param>
        protected abstract void OnWriteBodyContents(Stream stream);

        /// <summary>Override this method to handle writing the message body contents.</summary>
        /// <param name="writer">The writer to write to.</param>
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            using (StreamBodyWriter.XmlWriterBackedStream xmlWriterBackedStream = new StreamBodyWriter.XmlWriterBackedStream(writer))
            {
                this.OnWriteBodyContents(xmlWriterBackedStream);
            }
        }

        private class ActionOfStreamBodyWriter : StreamBodyWriter
        {
            private readonly Action<Stream> m_actionOfStream;

            public ActionOfStreamBodyWriter(Action<Stream> actionOfStream)
                : base(false)
            {
                this.m_actionOfStream = actionOfStream;
            }

            protected override void OnWriteBodyContents(Stream stream)
            {
                this.m_actionOfStream(stream);
            }
        }

        private class XmlWriterBackedStream : Stream
        {
            private readonly XmlWriter m_writer;

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override long Length
            {
                get
                {
                    throw new InvalidOperationException("Get Length property not supported on XmlWriterBackedStream");
                }
            }

            public override long Position
            {
                get
                {
                    throw new InvalidOperationException("Get Position property not supported on XmlWriterBackedStream");
                }
                set
                {
                    throw new InvalidOperationException("Set Position property not supported on XmlWriterBackedStream");
                }
            }

            public XmlWriterBackedStream(XmlWriter writer)
            {
                if (writer == null)
                    throw new ArgumentNullException("writer");
                this.m_writer = writer;
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw new InvalidOperationException("BeginRead supported on XmlWriterBackedStream");
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                throw new InvalidOperationException("EndRead not supported on XmlWriterBackedStream");
            }

            public override void Flush()
            {
                this.m_writer.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new InvalidOperationException("Read not supported on XmlWriterBackedStream");
            }

            public override int ReadByte()
            {
                throw new InvalidOperationException("ReadByte not supported on XmlWriterBackedStream");
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new InvalidOperationException("Seek not supported on XmlWriterBackedStream");
            }

            public override void SetLength(long value)
            {
                throw new InvalidOperationException("SetLength not supported on XmlWriterBackedStream");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (this.m_writer.WriteState == WriteState.Content)
                {
                    this.m_writer.WriteBase64(buffer, offset, count);
                    return;
                }
                if (this.m_writer.WriteState == WriteState.Start)
                {
                    this.m_writer.WriteStartElement("Binary", string.Empty);
                    this.m_writer.WriteBase64(buffer, offset, count);
                }
            }
        }
    }
}

namespace Barista
{
    using System;
    using System.Xml;
    using System.ServiceModel.Channels;
    using Barista.Extensions;

    public class BaristaStreamMessage : Message
    {
        internal const string StreamElementName = "Binary";

        private BodyWriter m_bodyWriter;

        private readonly MessageHeaders m_headers;

        private readonly MessageProperties m_properties;

        public override MessageHeaders Headers
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Barista Stream Message has been disposed");

                return this.m_headers;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public override bool IsFault
        {
            get
            {
                return false;
            }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Barista Stream Message has been disposed");

                return this.m_properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Barista Stream Message has been disposed");

                return MessageVersion.None;
            }
        }

        public BaristaStreamMessage(BodyWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            this.m_bodyWriter = writer;
            this.m_headers = new MessageHeaders(MessageVersion.None, 1);
            this.m_properties = new MessageProperties();
        }

        public BaristaStreamMessage(MessageHeaders headers, MessageProperties properties, BodyWriter bodyWriter)
        {
            if (bodyWriter == null)
                throw new ArgumentNullException("bodyWriter");

            this.m_headers = new MessageHeaders(headers);
            this.m_properties = new MessageProperties(properties);
            this.m_bodyWriter = bodyWriter;
        }


        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (this.m_bodyWriter.IsBuffered)
            {
                this.m_bodyWriter.WriteBodyContents(writer);
                return;
            }
            writer.WriteString("... stream ...");
        }

        protected override void OnClose()
        {
            Exception exception = null;
            try
            {
                base.OnClose();
            }
            catch (Exception exception2)
            {
                var exception1 = exception2;
                if (exception1.IsFatal())
                {
                    throw;
                }
                exception = exception1;
            }
            try
            {
                if (this.m_properties != null)
                {
                    this.m_properties.Dispose();
                }
            }
            catch (Exception exception4)
            {
                var exception3 = exception4;
                if (exception3.IsFatal())
                {
                    throw;
                }
                if (exception == null)
                {
                    exception = exception3;
                }
            }

            if (exception != null)
            {
                throw exception;
            }

            this.m_bodyWriter = null;
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            var bodyWriter = (!this.m_bodyWriter.IsBuffered ? this.m_bodyWriter.CreateBufferedCopy(maxBufferSize) : this.m_bodyWriter);
            return new BaristaStreamMessage.BaristaStreamMessageBuffer(this.Headers, new MessageProperties(this.Properties), bodyWriter);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.m_bodyWriter.WriteBodyContents(writer);
        }

        private class BaristaStreamMessageBuffer : MessageBuffer
        {
            private BodyWriter m_bufferBodyWriter;

            private bool m_bufferClosed;

            private MessageHeaders m_bufferHeaders;

            private MessageProperties m_bufferProperties;

            private readonly object m_bufferThisLock = new object();

            public override int BufferSize
            {
                get
                {
                    return 0;
                }
            }

            public BaristaStreamMessageBuffer(MessageHeaders headers, MessageProperties properties, BodyWriter bodyWriter)
            {
                this.m_bufferBodyWriter = bodyWriter;
                this.m_bufferHeaders = headers;
                this.m_bufferProperties = properties;
            }

            public override void Close()
            {
                lock (this.m_bufferThisLock)
                {
                    if (!this.m_bufferClosed)
                    {
                        this.m_bufferClosed = true;
                        this.m_bufferBodyWriter = null;
                        this.m_bufferHeaders = null;
                        this.m_bufferProperties = null;
                    }
                }
            }

            public override Message CreateMessage()
            {
                Message httpStreamMessage;
                lock (this.m_bufferThisLock)
                {
                    if (this.m_bufferClosed)
                    {
                        throw new ObjectDisposedException("Barista Message Buffer is closed.");
                    }
                    httpStreamMessage = new BaristaStreamMessage(this.m_bufferHeaders, this.m_bufferProperties, this.m_bufferBodyWriter);
                }
                return httpStreamMessage;
            }
        }
    }
}
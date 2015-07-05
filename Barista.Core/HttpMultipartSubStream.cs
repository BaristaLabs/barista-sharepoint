namespace Barista
{
    using System;
    using System.IO;

    /// <summary>
    /// A decorator stream that sits on top of an existing stream and appears as a unique stream.
    /// </summary>
    public class HttpMultipartSubStream : Stream
    {
        private readonly Stream m_stream;
        private long m_start;
        private readonly long m_end;
        private long m_position;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultipartSubStream"/> class.
        /// </summary>
        /// <param name="stream">The stream to create the sub-stream ontop of.</param>
        /// <param name="start">The start offset on the parent stream where the sub-stream should begin.</param>
        /// <param name="end">The end offset on the parent stream where the sub-stream should end.</param>
        public HttpMultipartSubStream(Stream stream, long start, long end)
        {
            this.m_stream = stream;
            this.m_start = start;
            this.m_position = start;
            this.m_end = end;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns><see langword="true"/> if the stream supports reading; otherwise, <see langword="false"/>.</returns>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns><see langword="true"/> if the stream supports seeking; otherwise, <see langword="false"/>.</returns>
        public override bool CanSeek
        {
            get { return true; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns><see langword="true"/> if the stream supports writing; otherwise, <see langword="false"/>.</returns>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="NotSupportedException">A class derived from Stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Length
        {
            get { return (this.m_end - this.m_start); }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception><filterpriority>1</filterpriority>
        public override long Position
        {
            get { return this.m_position - this.m_start; }
            set { this.m_position = this.Seek(value, SeekOrigin.Begin); }
        }

        private long CalculateSubStreamRelativePosition(SeekOrigin origin, long offset)
        {
            var subStreamRelativePosition = 0L;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    subStreamRelativePosition = this.m_start + offset;
                    break;

                case SeekOrigin.Current:
                    subStreamRelativePosition = this.m_position + offset;
                    break;

                case SeekOrigin.End:
                    subStreamRelativePosition = this.m_end + offset;
                    break;
            }
            return subStreamRelativePosition;
        }

        public void PositionStartAtCurrentLocation()
        {
            this.m_start = this.m_stream.Position;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <remarks>In the <see cref="HttpMultipartSubStream"/> type this method is implemented as no-op.</remarks>
        public override void Flush()
        {
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached. </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count > (this.m_end - this.m_position))
            {
                count = (int)(this.m_end - this.m_position);
            }

            if (count <= 0)
            {
                return 0;
            }

            this.m_stream.Position = this.m_position;

            var bytesReadFromStream =
                this.m_stream.Read(buffer, offset, count);

            this.RepositionAfterRead(bytesReadFromStream);

            return bytesReadFromStream;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        public override int ReadByte()
        {
            if (this.m_position >= this.m_end)
            {
                return -1;
            }

            this.m_stream.Position = this.m_position;

            var byteReadFromStream = this.m_stream.ReadByte();

            this.RepositionAfterRead(1);

            return byteReadFromStream;
        }

        private void RepositionAfterRead(int bytesReadFromStream)
        {
            if (bytesReadFromStream == -1)
            {
                this.m_position = this.m_end;
            }
            else
            {
                this.m_position += bytesReadFromStream;
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>The new position within the current stream.</returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var subStreamRelativePosition =
                this.CalculateSubStreamRelativePosition(origin, offset);

            this.ThrowExceptionIsPositionIsOutOfBounds(subStreamRelativePosition);

            this.m_position = this.m_stream.Seek(subStreamRelativePosition, SeekOrigin.Begin);

            return this.m_position;
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <remarks>This will always throw a <see cref="InvalidOperationException"/> for the <see cref="HttpMultipartSubStream"/> type.</remarks>
        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        /// <remarks>This will always throw a <see cref="InvalidOperationException"/> for the <see cref="HttpMultipartSubStream"/> type.</remarks>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

// ReSharper disable once UnusedParameter.Local
        private void ThrowExceptionIsPositionIsOutOfBounds(long subStreamRelativePosition)
        {
            if (subStreamRelativePosition < 0 || subStreamRelativePosition > this.m_end)
                throw new InvalidOperationException();
        }
    }
}

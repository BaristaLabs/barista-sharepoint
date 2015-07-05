namespace Barista
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Retrieves <see cref="HttpMultipartBoundary"/> instances from a request stream.
    /// </summary>
    public class HttpMultipart
    {
        private const byte Lf = (byte)'\n';
        private readonly HttpMultipartBuffer m_readBuffer;
        private readonly Stream m_requestStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMultipart"/> class.
        /// </summary>
        /// <param name="requestStream">The request stream to parse.</param>
        /// <param name="boundary">The boundary marker to look for.</param>
        public HttpMultipart(Stream requestStream, string boundary)
        {
            this.m_requestStream = requestStream;
            byte[] boundaryAsBytes = GetBoundaryAsBytes(boundary, false);
            byte[] closingBoundaryAsBytes = GetBoundaryAsBytes(boundary, true);
            this.m_readBuffer = new HttpMultipartBuffer(boundaryAsBytes, closingBoundaryAsBytes);
        }

        /// <summary>
        /// Gets the <see cref="HttpMultipartBoundary"/> instances from the request stream.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> instance, containing the found <see cref="HttpMultipartBoundary"/> instances.</returns>
        public IEnumerable<HttpMultipartBoundary> GetBoundaries()
        {
            return
                (from boundaryStream in this.GetBoundarySubStreams()
                 select new HttpMultipartBoundary(boundaryStream)).ToList();
        }

        private IEnumerable<HttpMultipartSubStream> GetBoundarySubStreams()
        {
            var boundarySubStreams = new List<HttpMultipartSubStream>();
            var boundaryStart = this.GetNextBoundaryPosition();

            var found = 0;
            while (MultipartIsNotCompleted(boundaryStart) && found < 1000)
            {
                var boundaryEnd = this.GetNextBoundaryPosition();
                boundarySubStreams.Add(new HttpMultipartSubStream(
                    this.m_requestStream,
                    boundaryStart,
                    this.GetActualEndOfBoundary(boundaryEnd)));

                boundaryStart = boundaryEnd;

                found++;
            }

            return boundarySubStreams;
        }

        private bool MultipartIsNotCompleted(long boundaryPosition)
        {
            return boundaryPosition > -1 && !this.m_readBuffer.IsClosingBoundary;
        }

        //we add two because or the \r\n before the boundary
        private long GetActualEndOfBoundary(long boundaryEnd)
        {
            if (this.CheckIfFoundEndOfStream())
            {
                return this.m_requestStream.Position - (this.m_readBuffer.Length + 2);
            }
            return boundaryEnd - (this.m_readBuffer.Length + 2);
        }

        private bool CheckIfFoundEndOfStream()
        {
            return this.m_requestStream.Position.Equals(this.m_requestStream.Length);
        }

        private static byte[] GetBoundaryAsBytes(string boundary, bool closing)
        {
            var boundaryBuilder = new StringBuilder();

            boundaryBuilder.Append("--");
            boundaryBuilder.Append(boundary);

            if (closing)
            {
                boundaryBuilder.Append("--");
            }
            else
            {
                boundaryBuilder.Append('\r');
                boundaryBuilder.Append('\n');
            }

            var bytes =
                Encoding.ASCII.GetBytes(boundaryBuilder.ToString());

            return bytes;
        }

        private long GetNextBoundaryPosition()
        {
            this.m_readBuffer.Reset();
            while (true)
            {
                var byteReadFromStream = this.m_requestStream.ReadByte();

                if (byteReadFromStream == -1)
                {
                    return -1;
                }

                this.m_readBuffer.Insert((byte)byteReadFromStream);

                if (this.m_readBuffer.IsFull && (this.m_readBuffer.IsBoundary || this.m_readBuffer.IsClosingBoundary))
                {
                    return this.m_requestStream.Position;
                }

                if (byteReadFromStream.Equals(Lf) || this.m_readBuffer.IsFull)
                {
                    this.m_readBuffer.Reset();
                }
            }
        }
    }
}

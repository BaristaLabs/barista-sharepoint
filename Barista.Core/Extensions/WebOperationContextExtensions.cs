namespace Barista.Extensions
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Text;
    using StreamBodyWriter = Barista.StreamBodyWriter;

    public static class WebOperationContextExtensions
    {
        /// <summary>Creates a stream formatted message.</summary>
        /// <returns>A stream formatted message.</returns>
        /// <param name="context"></param>
        /// <param name="streamWriter">The stream writer containg the data to write to the stream.</param>
        /// <param name="contentType">The content type for the message.</param>
        public static Message CreateStreamResponse(this WebOperationContext context, Action<Stream> streamWriter, string contentType)
        {
            if (streamWriter == null)
                throw new ArgumentNullException("streamWriter");

            if (contentType == null)
                throw new ArgumentNullException("contentType");

            Message baristaStreamMessage = new BaristaStreamMessage(StreamBodyWriter.CreateStreamBodyWriter(streamWriter));
            baristaStreamMessage.Properties.Add("WebBodyFormatMessageProperty", new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            context.AddContentType(contentType, null);
            return baristaStreamMessage;
        }

        public static void AddContentType(this WebOperationContext context, string contentType, Encoding encoding)
        {
            if (!string.IsNullOrEmpty(context.OutgoingResponse.ContentType))
                return;

            if (encoding != null)
            {
                contentType = GetContentType(contentType, encoding);
            }
            context.OutgoingResponse.ContentType = contentType;
        }

        internal readonly static CharSetEncoding[] CharSetEncodings;


        static WebOperationContextExtensions()
        {
            CharSetEncodings = new [] { new CharSetEncoding("utf-8", Encoding.UTF8), new CharSetEncoding("utf-16LE", Encoding.Unicode), new CharSetEncoding("utf-16BE", Encoding.BigEndianUnicode), new CharSetEncoding("utf-16", null), new CharSetEncoding(null, null) };
        }

        internal static string EncodingToCharSet(Encoding encoding)
        {
            var webName = encoding.WebName;
            var charSetEncodings = CharSetEncodings;
            for (var i = 0; i < charSetEncodings.Length; i++)
            {
                var encoding1 = charSetEncodings[i].Encoding;
                if (encoding1 != null && encoding1.WebName == webName)
                {
                    return charSetEncodings[i].CharSet;
                }
            }
            return null;
        }

        internal static string GetContentType(string mediaType, Encoding encoding)
        {
            var charSet = EncodingToCharSet(encoding);
            if (string.IsNullOrEmpty(charSet))
            {
                return mediaType;
            }
            var invariantCulture = CultureInfo.InvariantCulture;
            var objArray = new object[] { mediaType, charSet };
            return string.Format(invariantCulture, "{0}; charset={1}", objArray);
        }

        internal class CharSetEncoding
        {
            internal string CharSet;

            internal Encoding Encoding;

            internal CharSetEncoding(string charSet, Encoding enc)
            {
                this.CharSet = charSet;
                this.Encoding = enc;
            }
        }
    }
}

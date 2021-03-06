﻿namespace Barista.Library
{
    using Barista.Extensions;
    using Barista.Helpers;
    using Jurassic;
    using Jurassic.Library;
    using System;
    using System.Linq;
    using System.Text;

    [Serializable]
    public class UriConstructor : ClrFunction
    {
        public UriConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Uri", new UriInstance(engine.Object.InstancePrototype))
        {
            PopulateFunctions();
        }

        [JSConstructorFunction]
        public UriInstance Construct(object uriObject, object type)
        {
            Uri uri = null;
            if (uriObject is string)
            {
                UriKind enumValue;
                uri = type.TryParseEnum(true, UriKind.Absolute, out enumValue)
                  ? new Uri(uriObject as string, enumValue)
                  : new Uri(uriObject as string);
            }

            return new UriInstance(InstancePrototype, uri);
        }

        public UriInstance Construct(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            return new UriInstance(InstancePrototype, uri);
        }

        [JSFunction(Name = "concatUrls")]
        public string ConcatUrls(string firstPart, string secondPart)
        {
            return firstPart.ConcatUrls(secondPart);
        }
    }

    [Serializable]
    public class UriInstance : ObjectInstance
    {
        private readonly Uri m_uri;

        public UriInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public UriInstance(ObjectInstance prototype, Uri uri)
            : this(prototype)
        {
            m_uri = uri;
        }

        public Uri Uri
        {
            get { return m_uri; }
        }

        #region Properties
        [JSProperty(Name = "absolutePath")]
        public string AbsolutePath
        {
            get { return m_uri.AbsolutePath; }
        }

        [JSProperty(Name = "absoluteUri")]
        public string AbsoluteUri
        {
            get { return m_uri.AbsoluteUri; }
        }

        [JSProperty(Name = "authority")]
        public string Authority
        {
            get { return m_uri.Authority; }
        }

        [JSProperty(Name = "dnsSafeHost")]
        public string DnsSafeHost
        {
            get { return m_uri.DnsSafeHost; }
        }

        [JSProperty(Name = "fragment")]
        public string Fragment
        {
            get { return m_uri.Fragment; }
        }

        [JSProperty(Name = "host")]
        public string Host
        {
            get { return m_uri.Host; }
        }

        [JSProperty(Name = "hostNameType")]
        public string HostNameType
        {
            get { return m_uri.HostNameType.ToString(); }
        }

        [JSProperty(Name = "isAbsoluteUri")]
        public bool IsAbsoluteUri
        {
            get { return m_uri.IsAbsoluteUri; }
        }

        [JSProperty(Name = "isDefaultPort")]
        public bool IsDefaultPort
        {
            get { return m_uri.IsDefaultPort; }
        }

        [JSProperty(Name = "isFile")]
        public bool IsFile
        {
            get { return m_uri.IsFile; }
        }

        [JSProperty(Name = "isLoopback")]
        public bool IsLoopback
        {
            get { return m_uri.IsLoopback; }
        }

        [JSProperty(Name = "isUnc")]
        public bool IsUnc
        {
            get { return m_uri.IsUnc; }
        }

        [JSProperty(Name = "localPath")]
        public string LocalPath
        {
            get { return m_uri.LocalPath; }
        }

        [JSProperty(Name = "originalString")]
        public string OriginalString
        {
            get { return m_uri.OriginalString; }
        }

        [JSProperty(Name = "pathAndQuery")]
        public string PathAndQuery
        {
            get { return m_uri.PathAndQuery; }
        }

        [JSProperty(Name = "port")]
        public int Port
        {
            get { return m_uri.Port; }
        }

        [JSProperty(Name = "query")]
        public string Query
        {
            get { return m_uri.Query; }
        }

        [JSProperty(Name = "queryString")]
        public ObjectInstance QueryString
        {
            get
            {
                var result = Engine.Object.Construct();
                var queryString = HttpUtility.ParseQueryString(m_uri.Query, Encoding.UTF8);
                foreach (var key in queryString.Keys.OfType<string>())
                {
                    result.SetPropertyValue(key, queryString[key], false);
                }
                return result;
            }
        }

        [JSProperty(Name = "scheme")]
        public string Scheme
        {
            get { return m_uri.Scheme; }
        }

        [JSProperty(Name = "segments")]
        [JSDoc("ternPropertyType", "[string]")]
        public ArrayInstance Segments
        {
            get
            {
                var result = Engine.Array.Construct();

                foreach (var segment in m_uri.Segments)
                {
                    ArrayInstance.Push(result, segment);
                }
                return result;
            }
        }

        [JSProperty(Name = "userEscaped")]
        public bool UserEscaped
        {
            get { return m_uri.UserEscaped; }
        }

        [JSProperty(Name = "userInfo")]
        public string UserInfo
        {
            get { return m_uri.UserInfo; }
        }
        #endregion

        [JSFunction(Name = "isBaseOf")]
        public bool IsBaseOf(object uri)
        {
            if (uri is UriInstance)
            {
                return m_uri.IsBaseOf((uri as UriInstance).m_uri);
            }

            if (uri is string)
            {
                return m_uri.IsBaseOf(new Uri(uri as string));
            }

            return false;
        }

        [JSFunction(Name = "isWellFormedOriginalString")]
        public bool IsWellFormedOriginalString()
        {
            return m_uri.IsWellFormedOriginalString();
        }

        [JSFunction(Name = "makeRelativeUri")]
        public UriInstance MakeRelativeUri(object uri)
        {
            Uri result = null;

            if (uri is UriInstance)
            {
                result = m_uri.MakeRelativeUri((uri as UriInstance).m_uri);
            }
            else if (uri is string)
            {
                result = m_uri.MakeRelativeUri(new Uri(uri as string));
            }

            if (result == null)
                return null;

            return new UriInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "toString")]
        public override string ToString()
        {
            return m_uri.ToString();
        }
    }
}

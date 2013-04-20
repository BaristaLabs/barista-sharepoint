﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperSocket.SocketBase.Config
{
    /// <summary>
    /// Certificate config model class
    /// </summary>
    [Serializable]
    public class CertificateConfig : ICertificateConfig
    {
        #region ICertificateConfig Members

        /// <summary>
        /// Gets/sets the file path.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets/sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets/sets the the store where certificate locates.
        /// </summary>
        /// <value>
        /// The name of the store.
        /// </value>
        public string StoreName { get; set; }

        /// <summary>
        /// Gets/sets the thumbprint.
        /// </summary>
        public string Thumbprint { get; set; }

        #endregion
    }
}

#define CODE_ANALYSIS
namespace Barista.SharePoint
{
    using Microsoft.BusinessData.Infrastructure.SecureStore;
    using Microsoft.Office.SecureStoreService.Server;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;

    /// <summary>
    /// Helper class to programmatically impersonate a user whose credentials are stored in a SharePoint 2010 secure store service application.
    /// </summary>
    [SuppressMessage("SPCAF.Rules.SecurityGroup", "SPC020204:DoNotCallWindowsIdentityImpersonate", Justification = "The intent of this class is to facilitate impersonation for credentials defined in a secure store application.")]
    public class ImpersonationHelper : IDisposable
    {
        public const string DefaultSharePointSecureStoreProvider = "Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, Microsoft.Office.SecureStoreService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c";

        public delegate void RunAsDelegate();

        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        private const int LOGON32_LOGON_BATCH = 4;

        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_PROVIDER_DEFAULT = 0;

        private const int LOGON32_PROVIDER_WINNT35 = 1;

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3
        }
        // ReSharper restore UnusedMember.Local

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int DuplicateToken(IntPtr ExistingTokenHandle, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, ref IntPtr DuplicateTokenHandle);
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern long RevertToSelf();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern long CloseHandle(IntPtr handle);
        // ReSharper restore InconsistentNaming
        private WindowsImpersonationContext m_impersonationContext;

        #region Properties
        /// <summary>
        /// Gets or sets the type name of the Secure Store Service Provider
        /// </summary>
        /// <remarks>
        /// Usually Microsoft.Office.SecureStoreService.Server.SecureStoreProvider, Microsoft.Office.SecureStoreService, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c
        /// </remarks>
        /// <value>The fully qualified type name of the SSS Provider.</value>
        public string ProviderTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the application id of the Secure Store application to use.
        /// </summary>
        /// <value>The application id defined in the secure store service.</value>
        public string ApplicationId
        {
            get;
            private set;
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Do not allow a new instance of the ImpersonationHelper class without SSS information.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        private ImpersonationHelper()
        {
        }
        // ReSharper restore UnusedMember.Local

        /// <summary>
        /// Initializes a new instance of the ImpersonationHelper class.
        /// </summary>
        public ImpersonationHelper(string providerTypeName, string applicationId)
        {
            if (String.IsNullOrEmpty(providerTypeName))
                throw new ArgumentNullException("providerTypeName");

            if (String.IsNullOrEmpty(applicationId))
                throw new ArgumentNullException("applicationId");

            ProviderTypeName = providerTypeName;
            ApplicationId = applicationId;
        }
        #endregion

        /// <summary>
        /// Performs the impersonation of the user based on the parameters provided in the constructor. 
        /// </summary>
        /// <remarks>
        /// <para>If logon fails using the supplied credentials, an exception is thrown. The exception is thrown because this method is unable to duplicate the logged-on user's token for purposes of impersonation or is unable to create a Windows identity from the user's impersonated token.</para>
        /// <para>For details about the direct cause of the impersonation failure, you can inspect the inner exception.</para>
        /// </remarks>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void ImpersonateUser()
        {
            var hPassword = IntPtr.Zero;
            var hToken = IntPtr.Zero;
            var hTokenDuplicate = IntPtr.Zero;

            try
            {
                var creds = ReadUserCredentialsFromSecureStore(ProviderTypeName, ApplicationId);
                if (creds == null)
                    throw new InvalidOperationException("Could not read user credentials from the specified secure store.");

                if (RevertToSelf() == 0)
                    return;

                var userName = GetString(creds.UserName);
                var password = GetString(creds.Password);

                if (LogonUser(userName, String.Empty, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out hToken))
                {
                    if (DuplicateToken(hToken, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, ref hTokenDuplicate) != 0)
                    {
                        m_impersonationContext = new WindowsIdentity(hTokenDuplicate).Impersonate();

                        if ((m_impersonationContext == null))
                            throw new ImpersonationException(userName);
                    }
                }
                else
                {
                    throw new ImpersonationException(userName);
                }

                if (hPassword.Equals(IntPtr.Zero) == false)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(hPassword);
                }
            }
            finally
            {
                if (hTokenDuplicate.Equals(IntPtr.Zero) == false)
                    CloseHandle(hTokenDuplicate);

                if (hToken.Equals(IntPtr.Zero) == false)
                    CloseHandle(hToken);
            }
        }

        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public ICredentials GetNetworkCredentials()
        {
            var creds = ReadUserCredentialsFromSecureStore(ProviderTypeName, ApplicationId);
            if (creds == null)
                throw new InvalidOperationException("Could not read user credentials from the specified secure store.");

            var userName = GetString(creds.UserName);
            var password = GetString(creds.Password);

            var credentials = new NetworkCredential(userName, password);
            return credentials;
        }

        /// <summary>
        /// Undoes the impersonation of the user, if it is impersonated.
        /// </summary>
        /// <remarks>Use this method to free the objects associated with impersonation.</remarks>
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        public void Undo()
        {
            m_impersonationContext.Undo();
            m_impersonationContext = null;
        }

        /// <summary>
        /// Returns an ICredentials object that represents the credentials stored in the secure store.
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public static ICredentials GetCredentialsFromSecureStore(string applicationId)
        {
            return GetCredentialsFromSecureStore(DefaultSharePointSecureStoreProvider, applicationId);
        }

        /// <summary>
        /// Returns an ICredentials object that represents the credentials stored in the secure store.
        /// </summary>
        /// <param name="providerTypeName"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public static ICredentials GetCredentialsFromSecureStore(string providerTypeName, string applicationId)
        {
            ICredentials credentials;
            using (var impersonationContext = new ImpersonationHelper(providerTypeName, applicationId))
            {
                credentials = impersonationContext.GetNetworkCredentials();
            }
            return credentials;
        }

        public static void InvokeAsUser(string applicationId, Delegate methodToCall)
        {
            InvokeAsUser(DefaultSharePointSecureStoreProvider, applicationId, methodToCall);
        }

        public static void InvokeAsUser(string providerTypeName, string applicationId, Delegate methodToCall)
        {
            InvokeAsUser(providerTypeName, applicationId, methodToCall, new object[] { });
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        public static object InvokeAsUser(string providerTypeName, string applicationId, Delegate methodToCall, params object[] parameters)
        {
            object result;

            using (var impersonationContext = new ImpersonationHelper(providerTypeName, applicationId))
            {
                impersonationContext.ImpersonateUser();
                result = methodToCall.DynamicInvoke(parameters);
            }

            return result;
        }
        // ReSharper restore MethodOverloadWithOptionalParameter

        private sealed class UserCredentials
        {
            public SecureString UserName { get; set; }
            public SecureString Password { get; set; }
        }

        private static ISecureStoreProvider GetSecureStoreProvider(string providerTypeName)
        {
            var providerType = Type.GetType(providerTypeName);

            if (providerType == null)
                return null;

            return Activator.CreateInstance(providerType)
                as ISecureStoreProvider;
        }

        private UserCredentials ReadUserCredentialsFromSecureStore(string providerTypeName, string applicationId)
        {
            var provider = GetSecureStoreProvider(providerTypeName);

            // get the credentials for the user on whose behalf the code
            // is executing
            using (var credentials = provider.GetRestrictedCredentials(applicationId))
            {
                var creds = new UserCredentials();

                // look for username and password in credentials
                foreach (var credential in credentials)
                {
                    switch (credential.CredentialType)
                    {
                        case SecureStoreCredentialType.UserName:
                        case SecureStoreCredentialType.WindowsUserName:
                            creds.UserName = credential.Credential.Copy();
                            break;
                        case SecureStoreCredentialType.Password:
                        case SecureStoreCredentialType.WindowsPassword:
                            creds.Password = credential.Credential.Copy();
                            break;
                    }
                }

                return creds;
            }
        }

        /// <summary>
        /// Secure Store Service returns the credentials as SecureString. 
        /// Need to convert the SecureString into cleartext.
        /// </summary>
        /// <param name="secureString"></param>
        /// <returns></returns>
        private static string GetString(SecureString secureString)
        {
            string str;
            IntPtr pStr = IntPtr.Zero;
            try
            {
                pStr = Marshal.SecureStringToBSTR(secureString);
                str = Marshal.PtrToStringBSTR(pStr);
            }
            finally
            {
                Marshal.FreeBSTR(pStr);
            }
            return str;
        }

        public static SecureString MakeSecureString(string str)
        {
            if (str == null)
            {
                return null;
            }
            SecureString str2 = new SecureString();
            char[] chArray = str.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                str2.AppendChar(chArray[i]);
                chArray[i] = '0';
            }
            return str2;
        }

        #region IDisposable Implementaton

        private bool m_disposed;
        ~ImpersonationHelper()
        {
            Dispose(false);
        }

        /// <summary>
        /// Implementation of the <b>IDisposable</b> interface.
        /// </summary>
        /// <remarks>This method calls <see>Undo</see> if impersonation is still being performed. This method calls the common language runtime version of the Dispose method.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementation of the <b>IDisposable</b> interface.
        /// </summary>
        /// <param name="disposing">If <b>true</b>, the object to be disposed is finalized and collected by the garbage collector; otherwise, <b>false</b>.</param>
        /// <remarks>This method calls Undo if impersonation is still being performed. This method calls the common language runtime version of the Dispose method.</remarks>
        protected void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if ((m_impersonationContext != null))
                    {
                        m_impersonationContext.Undo();
                        m_impersonationContext.Dispose();
                    }
                }
                m_impersonationContext = null;
            }
            m_disposed = true;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// If not using windows authentication, might want to try running under elevated permissions and giving the farm account write access to the SSS.
        /// </summary>
        /// <param name="providerTypeName"></param>
        /// <param name="applicationId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public static void WriteCredentialsToSecureStore(string providerTypeName, string applicationId, string userName, string password)
        {
            SPServiceContext context = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);

            SecureStoreServiceProxy ssp = new SecureStoreServiceProxy();
            ISecureStore iss = ssp.GetSecureStore(context);

            IList<TargetApplicationField> applicationFields = iss.GetUserApplicationFields(applicationId);

            IList<ISecureStoreCredential> creds =
                new List<ISecureStoreCredential>(applicationFields.Count);

            foreach (TargetApplicationField taf in applicationFields)
            {
                switch (taf.CredentialType)
                {
                    case SecureStoreCredentialType.UserName:
                    case SecureStoreCredentialType.WindowsUserName:
                        creds.Add(new SecureStoreCredential(MakeSecureString(userName), taf.CredentialType));
                        break;

                    case SecureStoreCredentialType.Password:
                    case SecureStoreCredentialType.WindowsPassword:
                        creds.Add(new SecureStoreCredential(MakeSecureString(password), taf.CredentialType));
                        break;
                }
            }

            using (SecureStoreCredentialCollection credentials = new SecureStoreCredentialCollection(creds))
            {
                iss.SetCredentials(applicationId, credentials);
            }

        }

        /// <summary>
        /// Returns user credentials from the secure store.
        /// </summary>
        /// <param name="providerTypeName"></param>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public static NetworkCredential ReadCredentialsFromSecureStore(string providerTypeName, string applicationId)
        {
            using (ImpersonationHelper impersonationContext = new ImpersonationHelper(providerTypeName, applicationId))
            {
                var userCredentials = impersonationContext.ReadUserCredentialsFromSecureStore(providerTypeName, applicationId);
                return new NetworkCredential(GetString(userCredentials.UserName), GetString(userCredentials.Password));
            }
        }

        /// <summary>
        /// Executes the specified method using the specified SharePoint 2010 Secure Store provider type name and SharePoint 2010 Secure Store Application Id.
        /// </summary>
        /// <param name="providerTypeName"></param>
        /// <param name="applicationId"></param>
        /// <param name="methodToRunAs"></param>
        public static void RunAs(string providerTypeName, string applicationId, RunAsDelegate methodToRunAs)
        {
            RunAs(providerTypeName, applicationId, methodToRunAs, new object[] { });
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        /// <summary>
        /// Executes the specified method with the specified parameters and returns the result using the specified SharePoint 2010 Secure Store Provider Type Name and SharePoint 2010 Secure Store Application Id.
        /// </summary>
        /// <param name="providerTypeName"></param>
        /// <param name="applicationId"></param>
        /// <param name="methodToRunAs"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object RunAs(string providerTypeName, string applicationId, RunAsDelegate methodToRunAs, params object[] parameters)
        {
            object result;

            using (ImpersonationHelper impersonationContext = new ImpersonationHelper(providerTypeName, applicationId))
            {
                impersonationContext.ImpersonateUser();
                try
                {
                    result = methodToRunAs.DynamicInvoke(parameters);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            return result;
        }
        // ReSharper restore MethodOverloadWithOptionalParameter

        /// <summary>
        /// Executes the specified method under the specified credentials.
        /// </summary>
        /// <param name="methodToRunAs"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void RunAs(RunAsDelegate methodToRunAs, string username, string password)
        {
            string userName;
            string domain;

            if (username.IndexOf('\\') > 0)
            {
                //a domain name was supplied
                string[] usernameArray = username.Split('\\');
                userName = usernameArray[1];
                domain = usernameArray[0];
            }
            else
            {
                //there was no domain name supplied
                userName = username;
                domain = ".";
            }

            RunAs(methodToRunAs, userName, password, domain);
        }

        /// <summary>
        /// Executes the specified method under the specified credentials.
        /// </summary>
        /// <param name="methodToRunAs"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="domain"></param>
        public static void RunAs(RunAsDelegate methodToRunAs, string userName, string password, string domain)
        {
            IntPtr token = IntPtr.Zero;
            IntPtr hTokenDuplicate = IntPtr.Zero;

            try
            {
                if (RevertToSelf() != 0)
                {
                    if (LogonUser(
                        userName,
                        domain,
                        password,
                        LOGON32_LOGON_INTERACTIVE,
                        LOGON32_PROVIDER_DEFAULT,
                        out token))
                    {
                        if (DuplicateToken(token, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, ref hTokenDuplicate) != 0)
                        {
                            WindowsIdentity tempWindowsIdentity = new WindowsIdentity(hTokenDuplicate);
                            using (WindowsImpersonationContext impersonationContext = tempWindowsIdentity.Impersonate())
                            {
                                methodToRunAs();
                                impersonationContext.Undo();
                            }
                        }
                        else
                        {
                            throw new ImpersonationException(userName);
                        }
                    }
                    else
                    {
                        throw new ImpersonationException(userName);
                    }
                }
                else
                {
                    throw new ImpersonationException(userName);
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
                if (hTokenDuplicate != IntPtr.Zero)
                {
                    CloseHandle(hTokenDuplicate);
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// Represents an exception that occurs when Impersonating a user context.
    /// </summary>
    public class ImpersonationException : Exception
    {
        public string UserName
        {
            get;
            set;
        }

        public string Domain
        {
            get;
            set;
        }

        public ImpersonationException(string userName)
            : base(string.Format("Impersonation failure: {0}", userName), new System.ComponentModel.Win32Exception())
        {
            UserName = userName;
        }

        public ImpersonationException(string userName, string message, Exception ex)
            : base(string.Format("Impersonation failure: {1}\\{0} {2}", userName, string.Empty, message), ex)
        {
            UserName = userName;
        }

        public ImpersonationException(string userName, string domain)
            : base(string.Format("Impersonation failure: {1}\\{0}", userName, domain), new System.ComponentModel.Win32Exception())
        {
            UserName = userName;
            Domain = domain;
        }

        public ImpersonationException(string userName, string domain, string message)
            : base(string.Format("Impersonation failure: {1}\\{0} {2}", userName, domain, message), new System.ComponentModel.Win32Exception())
        {
            UserName = userName;
            Domain = domain;
        }

        public ImpersonationException(string userName, string domain, string message, Exception ex)
            : base(string.Format("Impersonation failure: {1}\\{0} {2}", userName, domain, message), ex)
        {
            UserName = userName;
            Domain = domain;
        }
    }
}

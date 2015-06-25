namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPUserTokenConstructor : ClrFunction
    {
        public SPUserTokenConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUserToken", new SPUserTokenInstance(engine.Object.InstancePrototype))
        {
            PopulateFunctions();
        }

        [JSConstructorFunction]
        public SPUserTokenInstance Construct(Base64EncodedByteArrayInstance bytes)
        {
            if (bytes == null)
                throw new JavaScriptException(this.Engine, "Error", "A byte array must be specified as the first argument.");

            return new SPUserTokenInstance(this.InstancePrototype, new SPUserToken(bytes.Data));
        }

        [JSProperty(Name = "systemAccount")]
        public SPUserTokenInstance SystemAccount
        {
            get
            {
                return SPUserToken.SystemAccount == null
                    ? null
                    : new SPUserTokenInstance(this.Engine.Object.InstancePrototype, SPUserToken.SystemAccount);
            }
        }
    }

    [Serializable]
    public class SPUserTokenInstance : ObjectInstance
    {
        private readonly SPUserToken m_userToken;

        public SPUserTokenInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFunctions();
        }

        public SPUserTokenInstance(ObjectInstance prototype, SPUserToken userToken)
            : this(prototype)
        {
            if (userToken == null)
                throw new ArgumentNullException("userToken");

            m_userToken = userToken;
        }

        public SPUserToken SPUserToken
        {
            get
            {
                return m_userToken;
            }
        }

        [JSProperty(Name = "binaryToken")]
        public Base64EncodedByteArrayInstance BinaryToken
        {
            get
            {
                return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, m_userToken.BinaryToken);
            }
        }

        [JSFunction(Name = "compareUser")]
        public bool CompareUser(SPUserTokenInstance userTokenCheck)
        {
            if (userTokenCheck == null)
                throw new JavaScriptException(this.Engine, "Error", "UserToken must be specified as the first argument.");

            return m_userToken.CompareUser(userTokenCheck.SPUserToken);
        }
    }
}

namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPPrincipalInstance : ObjectInstance
    {
        private readonly SPPrincipal m_principal;

        public SPPrincipalInstance(ScriptEngine engine, SPPrincipal principal)
            : base(engine, engine.Object.InstancePrototype)
        {
            this.m_principal = principal;

            this.PopulateFunctions();
        }

        protected SPPrincipalInstance(ObjectInstance prototype, SPPrincipal principal)
            : base(prototype)
        {
            m_principal = principal;
        }

        public SPPrincipal SPPrincipal
        {
            get { return m_principal; }
        }

        [JSProperty(Name = "id")]
        public int Id
        {
            get
            {
                return m_principal.ID;
            }
        }

        [JSProperty(Name = "loginName")]
        public string LoginName
        {
            get
            {
                return m_principal.LoginName;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_principal.Name;
            }
        }

        [JSFunction(Name = "getParentWeb")]
        public SPWebInstance GetParentWeb()
        {
            return m_principal.ParentWeb == null
                 ? null
                 : new SPWebInstance(this.Engine, m_principal.ParentWeb);
        }

        //Roles Property is Deprecated.
    }
}

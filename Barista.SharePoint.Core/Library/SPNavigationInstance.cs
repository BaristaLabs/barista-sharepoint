namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Navigation;

    [Serializable]
    public class SPNavigationConstructor : ClrFunction
    {
        public SPNavigationConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPNavigation", new SPNavigationInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPNavigationInstance Construct()
        {
            return new SPNavigationInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPNavigationInstance : ObjectInstance
    {
        private readonly SPNavigation m_navigation;

        public SPNavigationInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPNavigationInstance(ObjectInstance prototype, SPNavigation navigation)
            : this(prototype)
        {
            if (navigation == null)
                throw new ArgumentNullException("navigation");

            m_navigation = navigation;
        }

        public SPNavigation SPNavigation
        {
            get
            {
                return m_navigation;
            }
        }

        [JSProperty(Name = "globalNodes")]
        public SPNavigationNodeCollectionInstance GlobalNodes
        {
            get
            {
                return m_navigation.GlobalNodes == null
                    ? null
                    : new SPNavigationNodeCollectionInstance(this.Engine.Object.InstancePrototype, m_navigation.GlobalNodes);
            }
        }

        [JSProperty(Name = "home")]
        public SPNavigationNodeInstance Home
        {
            get
            {
                return m_navigation.Home == null
                    ? null
                    : new SPNavigationNodeInstance(this.Engine.Object.InstancePrototype, m_navigation.Home);
            }
        }

        [JSProperty(Name = "quickLaunch")]
        public SPNavigationNodeCollectionInstance QuickLaunch
        {
            get
            {
                return m_navigation.QuickLaunch == null
                    ? null
                    : new SPNavigationNodeCollectionInstance(this.Engine.Object.InstancePrototype, m_navigation.QuickLaunch);
            }
        }

        [JSProperty(Name = "topNavigationBar")]
        public SPNavigationNodeCollectionInstance TopNavigationBar
        {
            get
            {
                return m_navigation.TopNavigationBar == null
                    ? null
                    : new SPNavigationNodeCollectionInstance(this.Engine.Object.InstancePrototype, m_navigation.TopNavigationBar);
            }
        }

        [JSProperty(Name = "useShared")]
        public bool UseShared
        {
            get
            {
                return m_navigation.UseShared;
            }
            set
            {
                m_navigation.UseShared = value;
            }
        }

        [JSFunction(Name = "addToQuickLaunch")]
        public SPNavigationNodeInstance AddToQuickLaunch(SPNavigationNodeInstance node, string heading)
        {
            if (node == null)
                throw new JavaScriptException(this.Engine, "Error", "node must be specified.");

            SPQuickLaunchHeading qlHeading;
            if (!heading.TryParseEnum(true, out qlHeading))
                throw new JavaScriptException(this.Engine, "Error", "SPQuickLaunchHeading must be specified.");

            var result = m_navigation.AddToQuickLaunch(node.SPNavigationNode, qlHeading);
            return result == null
                ? null
                : new SPNavigationNodeInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getNodeById")]
        public SPNavigationNodeInstance GetNodeById(int id)
        {
            var result = m_navigation.GetNodeById(id);
            return result == null
                ? null
                : new SPNavigationNodeInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getNodeByUrl")]
        public SPNavigationNodeInstance GetNodeByUrl(string url)
        {
            var result = m_navigation.GetNodeByUrl(url);
            return result == null
                ? null
                : new SPNavigationNodeInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWeb")]
        public SPWebInstance GetWeb()
        {
            return m_navigation.Web == null
                ? null
                : new SPWebInstance(this.Engine, m_navigation.Web);
        }
    }
}

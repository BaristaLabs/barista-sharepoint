namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.Navigation;

    [Serializable]
    public class SPNavigationNodeConstructor : ClrFunction
    {
        public SPNavigationNodeConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPNavigationNode", new SPNavigationNodeInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPNavigationNodeInstance Construct(string title, string url, object isExternal)
        {
            SPNavigationNode node;
            if (isExternal == Undefined.Value)
                node = new SPNavigationNode(title, url);
            else
                node = new SPNavigationNode(title, url, TypeConverter.ToBoolean(isExternal));

            return new SPNavigationNodeInstance(this.InstancePrototype, node);
        }
    }

    [Serializable]
    public class SPNavigationNodeInstance : ObjectInstance
    {
        private readonly SPNavigationNode m_navigationNode;

        public SPNavigationNodeInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPNavigationNodeInstance(ObjectInstance prototype, SPNavigationNode navigationNode)
            : this(prototype)
        {
            if (navigationNode == null)
                throw new ArgumentNullException("navigationNode");

            m_navigationNode = navigationNode;
        }

        public SPNavigationNode SPNavigationNode
        {
            get
            {
                return m_navigationNode;
            }
        }

        [JSProperty(Name = "children")]
        public SPNavigationNodeCollectionInstance Children
        {
            get
            {
                return m_navigationNode.Children == null
                    ? null
                    : new SPNavigationNodeCollectionInstance(this.Engine.Object.InstancePrototype, m_navigationNode.Children);
            }
        }

        [JSProperty(Name = "id")]
        public int Id
        {
            get
            {
                return m_navigationNode.Id;
            }
        }

        [JSProperty(Name = "isExternal")]
        public bool IsExternal
        {
            get
            {
                return m_navigationNode.IsExternal;
            }
        }

        [JSProperty(Name = "isVisible")]
        public bool IsVisible
        {
            get
            {
                return m_navigationNode.IsVisible;
            }
            set
            {
                m_navigationNode.IsVisible = value;
            }
        }

        [JSProperty(Name = "lastModified")]
        public DateInstance LastModified
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_navigationNode.LastModified);
            }
        }

        [JSProperty(Name = "parentId")]
        public int ParentId
        {
            get
            {
                return m_navigationNode.ParentId;
            }
        }

        [JSProperty(Name = "propertyBag")]
        public HashtableInstance PropertyBag
        {
            get
            {
                return m_navigationNode.Properties == null
                    ? null
                    : new HashtableInstance(this.Engine.Object.InstancePrototype, m_navigationNode.Properties);
            }
        }

        [JSProperty(Name = "targetParentObjectType")]
        public string TargetParentObjectType
        {
            get
            {
                return m_navigationNode.TargetParentObjectType.ToString();
            }
        }

        [JSProperty(Name = "targetSecurityScopeId")]
        public GuidInstance TargetSecurityScopeId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_navigationNode.TargetSecurityScopeId);
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_navigationNode.Title;
            }
            set
            {
                m_navigationNode.Title = value;
            }
        }

        //TitleResouruce

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_navigationNode.Url;
            }
            set
            {
                m_navigationNode.Url = value;
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_navigationNode.Delete();
        }

        [JSFunction(Name = "getNavigation")]
        public SPNavigationInstance GetNavigation()
        {
            return m_navigationNode.Navigation == null
                ? null
                : new SPNavigationInstance(this.Engine.Object.InstancePrototype, m_navigationNode.Navigation);
        }

        [JSFunction(Name = "getParent")]
        public SPNavigationNodeInstance GetParent()
        {
            return m_navigationNode.Parent == null
                ? null
                : new SPNavigationNodeInstance(this.Engine.Object.InstancePrototype, m_navigationNode.Parent);
        }

        [JSFunction(Name = "move")]
        public void Move(SPNavigationNodeCollectionInstance collection, SPNavigationNodeInstance previousSibling)
        {
            if (collection == null)
                throw new JavaScriptException(this.Engine, "Error", "SPNavigationNodeCollection must be specified.");

            if (previousSibling == null)
                throw new JavaScriptException(this.Engine, "Error", "previousSibling must be specified.");

            m_navigationNode.Move(collection.SPNavigationNodeCollection, previousSibling.SPNavigationNode);
        }

        [JSFunction(Name = "moveToFirst")]
        public void MoveToFirst(SPNavigationNodeCollectionInstance collection)
        {
            if (collection == null)
                throw new JavaScriptException(this.Engine, "Error", "SPNavigationNodeCollection must be specified.");

            m_navigationNode.MoveToFirst(collection.SPNavigationNodeCollection);
        }

        [JSFunction(Name = "moveToLast")]
        public void MoveToLast(SPNavigationNodeCollectionInstance collection)
        {
            if (collection == null)
                throw new JavaScriptException(this.Engine, "Error", "SPNavigationNodeCollection must be specified.");

            m_navigationNode.MoveToLast(collection.SPNavigationNodeCollection);
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_navigationNode.Update();
        }
    }
}

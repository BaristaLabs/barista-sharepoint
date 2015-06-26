namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Navigation;

    [Serializable]
    public class SPNavigationNodeCollectionConstructor : ClrFunction
    {
        public SPNavigationNodeCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPNavigationNodeCollection", new SPNavigationNodeCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPNavigationNodeCollectionInstance Construct()
        {
            return new SPNavigationNodeCollectionInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class SPNavigationNodeCollectionInstance : ObjectInstance
    {
        private readonly SPNavigationNodeCollection m_navigationNodeCollection;

        public SPNavigationNodeCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPNavigationNodeCollectionInstance(ObjectInstance prototype, SPNavigationNodeCollection navigationNodeCollection)
            : this(prototype)
        {
            if (navigationNodeCollection == null)
                throw new ArgumentNullException("navigationNodeCollection");

            m_navigationNodeCollection = navigationNodeCollection;
        }

        public SPNavigationNodeCollection SPNavigationNodeCollection
        {
            get
            {
                return m_navigationNodeCollection;
            }
        }

        [JSFunction(Name = "add")]
        public SPNavigationNodeInstance Add(SPNavigationNodeInstance node, SPNavigationNodeInstance previousNode)
        {
            if (node == null)
                throw new JavaScriptException(Engine, "Error", "node must be specified.");

            if (previousNode == null)
                throw new JavaScriptException(Engine, "Error", "previousNode must be specified.");

            var result = m_navigationNodeCollection.Add(node.SPNavigationNode, previousNode.SPNavigationNode);
            return result == null
                ? null
                : new SPNavigationNodeInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "addAsFirst")]
        public SPNavigationNodeInstance AddAsFirst(SPNavigationNodeInstance node)
        {
            if (node == null)
                throw new JavaScriptException(Engine, "Error", "node must be specified.");

            var result = m_navigationNodeCollection.AddAsFirst(node.SPNavigationNode);
            return result == null
                ? null
                : new SPNavigationNodeInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "addAsLast")]
        public SPNavigationNodeInstance AddAsLast(SPNavigationNodeInstance node)
        {
            if (node == null)
                throw new JavaScriptException(Engine, "Error", "node must be specified.");

            var result = m_navigationNodeCollection.AddAsLast(node.SPNavigationNode);
            return result == null
                ? null
                : new SPNavigationNodeInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "delete")]
        public void Delete(SPNavigationNodeInstance node)
        {
            if (node == null)
                throw new JavaScriptException(Engine, "Error", "node must be specified.");

            m_navigationNodeCollection.Delete(node.SPNavigationNode);
        }

        [JSFunction(Name = "getNavigation")]
        public SPNavigationInstance GetNavigation()
        {
            return m_navigationNodeCollection.Navigation == null
                ? null
                : new SPNavigationInstance(Engine.Object.InstancePrototype, m_navigationNodeCollection.Navigation);
        }

        [JSFunction(Name = "getNavigationNodeByIndex")]
        public SPNavigationNodeInstance GetNavigationNodeByIndex(int index)
        {
            var result = m_navigationNodeCollection[index];
            return result == null
                ? null
                : new SPNavigationNodeInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getParent")]
        public SPNavigationNodeInstance GetParent()
        {
            return m_navigationNodeCollection.Parent == null
                ? null
                : new SPNavigationNodeInstance(Engine.Object.InstancePrototype, m_navigationNodeCollection.Parent);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+SPNavigationNote]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var navigationNode in m_navigationNodeCollection.OfType<SPNavigationNode>())
            {
                ArrayInstance.Push(result, new SPNavigationNodeInstance(Engine.Object.InstancePrototype, navigationNode));
            }
            return result;
        }
    }
}

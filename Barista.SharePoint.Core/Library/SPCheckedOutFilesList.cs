namespace Barista.SharePoint.Library
{
    using System.Collections.Generic;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPCheckedOutFilesListConstructor : ClrFunction
    {
        public SPCheckedOutFilesListConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPCheckedOutFilesList", new SPCheckedOutFilesListInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPCheckedOutFilesListInstance Construct()
        {
            return new SPCheckedOutFilesListInstance(Engine, null);
        }
    }

    [Serializable]
    public class SPCheckedOutFilesListInstance : ListInstance<SPCheckedOutFileInstance, SPCheckedOutFile>
    {
        public SPCheckedOutFilesListInstance(ScriptEngine engine, IList<SPCheckedOutFile> checkedOutFilesList)
            : base(new ListInstance<SPCheckedOutFilesListInstance, SPCheckedOutFile>(engine))
        {
            List = checkedOutFilesList;
            PopulateFunctions(GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override SPCheckedOutFileInstance Wrap(SPCheckedOutFile sPCheckedOutFile)
        {
            return sPCheckedOutFile == null
                ? null
                : new SPCheckedOutFileInstance(Engine.Object.InstancePrototype, sPCheckedOutFile);
        }

        protected override SPCheckedOutFile Unwrap(SPCheckedOutFileInstance sPCheckedOutFileInstance)
        {
            return sPCheckedOutFileInstance == null
                ? null
                : sPCheckedOutFileInstance.SPCheckedOutFile;
        }
    }
}

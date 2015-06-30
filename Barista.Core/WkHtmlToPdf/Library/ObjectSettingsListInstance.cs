namespace Barista.WkHtmlToPdf.Library
{
    using System.Collections.Generic;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.TuesPechkin;

    public class ObjectSettingsListInstance : ListInstance<ObjectInstance, ObjectSettings>
    {
        public ObjectSettingsListInstance(ScriptEngine engine, IList<ObjectSettings> settingsList)
            : base(new ListInstance<ObjectSettingsInstance, ObjectSettings>(engine))
        {
            List = settingsList;
            PopulateFunctions(GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public override void Add(ObjectInstance item)
        {
            //don't add null items, which blows up the conversion.
            if (item == null)
                return;

            base.Add(item);
        }

        protected override ObjectInstance Wrap(ObjectSettings objectSettings)
        {
            return objectSettings == null
                ? null
                : new ObjectSettingsInstance(Engine.Object.InstancePrototype, objectSettings);
        }

        protected override ObjectSettings Unwrap(ObjectInstance objectSettings)
        {
            if (objectSettings == null)
                return null;

            var os = objectSettings as ObjectSettingsInstance;
            if (os != null)
                return os.ObjectSettings;

            var os2 = JurassicHelper.Coerce<ObjectSettingsInstance>(Engine, objectSettings);
            return os2.ObjectSettings;
        }
    }
}

namespace Barista.iCal
{
    using Barista.DDay.iCal;
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class TodoConstructor : ClrFunction
    {
        public TodoConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "Todo", new TodoInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public TodoInstance Construct()
        {
            return new TodoInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class TodoInstance : ObjectInstance
    {
        private readonly Todo m_todo;

        public TodoInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public TodoInstance(ObjectInstance prototype, Todo todo)
            : this(prototype)
        {
            if (todo == null)
                throw new ArgumentNullException("todo");

            m_todo = todo;
        }

        public Todo Todo
        {
            get { return m_todo; }
        }

        [JSProperty(Name = "class")]
        public string Class
        {
            get
            {
                return m_todo.Class;
            }
            set
            {
                m_todo.Class = value;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_todo.Description;
            }
            set
            {
                m_todo.Description = value;
            }
        }

        [JSProperty(Name = "due")]
        public object Due
        {
            get
            {
                return new iCalDateTimeInstance(this.Engine.Object.InstancePrototype, (iCalDateTime)m_todo.Due);
            }
            set
            {
                if (value == null)
                {
                    m_todo.Due = null;
                    return;
                }

                if (value is iCalDateTimeInstance)
                    m_todo.Due = ((iCalDateTimeInstance)value).iCalDateTime;
                else if (value is DateTime)
                    m_todo.Due = new iCalDateTime((DateTime)value);
                else if (value is DateInstance)
                    m_todo.Due = new iCalDateTime(((DateInstance)value).Value);
                else
                {
                    var strValue = TypeConverter.ToString(value);
                    var dt = new iCalDateTime(strValue);
                    m_todo.Due = dt;
                }
            }
        }

        [JSProperty(Name = "duration")]
        public object Duration
        {
            get
            {
                return m_todo.Duration.ToString();
            }
            set
            {
                var strValue = TypeConverter.ToString(value);
                TimeSpan ts;
                if (TimeSpan.TryParse(strValue, out ts))
                {
                    m_todo.Duration = ts;
                }
                else
                {
                    throw new JavaScriptException(this.Engine, "Error", "Value could not be converted to a time span:" + strValue);
                }
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_todo.Name;
            }
            set
            {
                m_todo.Name = value;
            }
        }

        [JSProperty(Name = "location")]
        public string Location
        {
            get
            {
                return m_todo.Location;
            }
            set
            {
                m_todo.Location = value;
            }
        }

        [JSProperty(Name = "percentComplete")]
        public int PercentComplete
        {
            get
            {
                return m_todo.PercentComplete;
            }
            set
            {
                m_todo.PercentComplete = value;
            }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get
            {
                return m_todo.Status.ToString();
            }
            set
            {
                TodoStatus status;
                if (value.TryParseEnum(true, out status))
                    m_todo.Status = status;
            }
        }

        [JSProperty(Name = "summary")]
        public string Summary
        {
            get
            {
                return m_todo.Summary;
            }
            set
            {
                m_todo.Summary = value;
            }
        }

        [JSProperty(Name = "uid")]
        public string Uid
        {
            get
            {
                return m_todo.UID;
            }
            set
            {
                m_todo.UID = value;
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_todo.Url.ToString();
            }
            set
            {
                m_todo.Url = value == null ? null : new Uri(value);
            }
        }
    }
}

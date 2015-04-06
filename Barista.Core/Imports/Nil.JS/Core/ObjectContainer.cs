﻿using System;
using System.Collections.Generic;
using Barista.NiL.JS.BaseLibrary;
using Barista.NiL.JS.Core.Modules;
using Barista.NiL.JS.Core.TypeProxing;

namespace Barista.NiL.JS.Core
{
    /// <summary>
    /// Объект-контейнер для внешних объектов. 
    /// Так же используется для типов наследников JSObject, имеющих valueType меньше Object, 
    /// с целью имитировать valueType == Object.
    /// 
    /// Был создан так как вместе с объектом требуется ещё хранить его аттрибуты, 
    /// которые могли разъехаться при переприсваиваниях
    /// </summary>
    internal sealed class ObjectContainer : JSObject
    {
        private object instance;

        [Hidden]
        public override object Value
        {
            get
            {
                return instance;
            }
        }

        [Hidden]
        public ObjectContainer(object instance, JSObject proto)
        {
            __prototype = proto;
            this.instance = instance;
            if (instance is Date)
                valueType = JSObjectType.Date;
            else
                valueType = JSObjectType.Object;
            oValue = this;
            attributes = JSObjectAttributesInternal.SystemObject;
            attributes |= proto.attributes & JSObjectAttributesInternal.Immutable;
        }

        [Hidden]
        public ObjectContainer(object instance)
            : this(instance, TypeProxy.GetPrototype(instance.GetType()))
        {
        }

        [Hidden]
        public override void Assign(JSObject value)
        {
            if ((attributes & JSObjectAttributesInternal.ReadOnly) == 0)
            {
#if DEBUG
                System.Diagnostics.Debugger.Break();
#endif
                throw new InvalidOperationException("Try to assign to " + this.GetType().Name);
            }
        }

        protected internal override JSObject GetMember(JSObject name, bool forWrite, bool own)
        {
            oValue = instance as JSObject ?? this;
            try
            {
                return base.GetMember(name, forWrite, own);
            }
            finally
            {
                oValue = this;
            }
        }

        protected internal override void SetMember(JSObject name, JSObject value, bool strict)
        {
            oValue = instance as JSObject ?? this;
            try
            {
                base.SetMember(name, value, strict);
            }
            finally
            {
                oValue = this;
            }
        }

        protected internal override bool DeleteMember(JSObject name)
        {
            oValue = instance as JSObject ?? this;
            try
            {
                return base.DeleteMember(name);
            }
            finally
            {
                oValue = this;
            }
        }

        protected internal override IEnumerator<string> GetEnumeratorImpl(bool hideNonEnum)
        {
            oValue = instance as JSObject ?? this;
            if (oValue == this)
                return base.GetEnumeratorImpl(hideNonEnum);
            try
            {
                return base.GetEnumerator(hideNonEnum);
            }
            finally
            {
                oValue = this;
            }
        }
    }
}

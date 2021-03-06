﻿namespace Barista.Library
{
    using Barista.Jurassic.Compiler;
    using Jurassic;
    using Jurassic.Library;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web;
    using Barista.Extensions;

    [Serializable]
    public class BaristaGlobal : ObjectInstance
    {
        public BaristaGlobal(ScriptEngine engine)
            : base(engine)
        {
            PopulateFunctions();
        }

        protected BaristaGlobal(ObjectInstance prototype)
            : base(prototype)
        {
            Common = new Common(prototype);
            Environment = new EnvironmentInstance(prototype);
        }

        [JSProperty(Name = "common")]
        public Common Common
        {
            get;
            set;
        }

        [JSProperty(Name = "environment")]
        public EnvironmentInstance Environment
        {
            get;
            set;
        }

        [JSFunction(Name = "equals")]
        public bool JSEquals(object o1, object o2)
        {
            return TypeComparer.Equals(o1, o2);
        }

        [JSFunction(Name = "grabMutex")]
        public MutexInstance GrabMutex(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new JavaScriptException(Engine, "Error", "A mutex name must be specified as the first argument.");

            var mutex = BaristaScriptMutexManager.GrabMutex(name);
            return new MutexInstance(Engine.Object.InstancePrototype, mutex);
        }

        /// <summary>
        /// Returns a JSON object that is a TernJS based type definition of the specified object.
        /// </summary>
        /// <remarks>
        /// If no object (or undefined) is specified, a definition of all bundles will be returned.
        /// </remarks>
        /// <param name="obj"></param>
        /// <returns></returns>
        [JSFunction(Name = "generateTurnTypeDefinition")]
        [JSDoc("Returns a JSON object that is a TernJS based type definition of the specified object. If no object is specified, a definition of all bundes will be returned.")]
        public object GenerateTernTypeDefinition(object obj)
        {
            if (obj == null || obj == Null.Value || obj == Undefined.Value)
            {
                if (Common == null)
                    throw new InvalidOperationException("Internal Error - Could not local Common object.");

                var allGlobals = new Dictionary<string, ObjectInstance>();
                var define = Engine.Object.Construct();

                //Add our special "require" function
                var requireFn = Engine.Object.Construct();
                requireFn.SetPropertyValue("!type", "fn(id: string) -> !custom:baristaRequire", false);
                requireFn.SetPropertyValue("!doc", "To require bundles.", false);
                define.SetPropertyValue("require", requireFn, false);

                foreach (var val in Common.RegisteredBundles.OrderBy(b => b.Key))
                {
                    
                    var bundleResult = Common.Require(val.Value.BundleName, null);

                    if (bundleResult is ObjectInstance)
                    {
                        var broi = bundleResult as ObjectInstance;
                        IDictionary<string, ObjectInstance> globals;
                        var definition = GetTernObjectDefinition(Engine, broi, val.Key.BundleName, out globals);
                        definition.SetPropertyValue("!doc", val.Key, false);

                        define.SetPropertyValue(val.Key.BundleName, definition, false);

                        foreach (var global in globals.Where(global => !allGlobals.ContainsKey(global.Key)))
                            allGlobals.Add(global.Key, global.Value);
                    }
                    else
                    {
                        //TODO: Do something like enumerate the available global types and diff.

                        var emptyDefinition = Engine.Object.Construct();
                        emptyDefinition.SetPropertyValue("!doc", val.Value.BundleDescription, false);

                        define.SetPropertyValue(val.Key.BundleName, emptyDefinition, false);
                    }
                }

                var result = Engine.Object.Construct();
                result.SetPropertyValue("!name", "barista", false);
                result.SetPropertyValue("!define", define, false);

                
                //Add in all properties and functions defined on the global object except for the built-ins.
                /*var globalExclusions = new []
                {
                    "Infinity", "NaN", "undefined", "JSON", "Math",
                    "Array", "Boolean", "Date", "Function", "Number", "Object", "RegExp", "String",
                    "Error", "RangeError", "TypeError", "SyntaxError", "URIError", "EvalError", "ReferenceError"
                };

                //muhaha.
                var thisVar = Engine.GetGlobalValue("this") as ObjectInstance;
                if (thisVar != null) //Woah, this would be wild wouldn't it.
                {
                    foreach (var property in thisVar.Properties)
                    {
                        if (globalExclusions.Contains(property.Name) == false &&
                            result.HasProperty(property.Name) == false)
                        {
                            //GetTernDocumentationObject(Engine, property);
                            result.SetPropertyValue(property.Name, "", false);
                        }
                    }
                }*/

                result.SetPropertyValue("Barista", GetTernDocumentationObject(Engine, typeof(BaristaGlobal)), false);
                var baristaDocObj = Engine.Object.Construct();
                baristaDocObj.SetPropertyValue("!type", "+Barista", false);
                result.SetPropertyValue("barista", baristaDocObj, false);

                result.SetPropertyValue("Guid", GetTernDocumentationObject(Engine, typeof(GuidInstance)), false);
                result.SetPropertyValue("HashTable", GetTernDocumentationObject(Engine, typeof(HashtableInstance)), false);
                result.SetPropertyValue("Uri", GetTernDocumentationObject(Engine, typeof(UriInstance)), false);
                result.SetPropertyValue("NetworkCredential", GetTernDocumentationObject(Engine, typeof(NetworkCredentialInstance)), false);
                result.SetPropertyValue("Base64EncodedByteArray", GetTernDocumentationObject(Engine, typeof(Base64EncodedByteArrayInstance)), false);
                //TODO: Console...


                //Add in all globals that we gathered earlier
                foreach(var global in allGlobals.OrderBy(g => g.Key))
                    result.SetPropertyValue(global.Key, global.Value, false);

                return result;
            }

            var instance = obj as ObjectInstance;

            //Return an empty object if the object isn't an object instance.
            if (instance == null)
                return Engine.Object.Construct();

            var oi = instance;
            return GetTernObjectDefinition(Engine, oi);
        }

        /// <summary>
        /// Returns a JSON object that contains the shape of the specified object with any JSDoc attributes applied.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [JSFunction(Name = "help")]
        public object Help(object obj)
        {
            object result = "Sorry, Dave. I didn't understand what you wanted.";

            if (obj == null || obj == Null.Value || obj == Undefined.Value)
            {
                if (Common != null)
                {
                    return Common.ListBundles();
                }
            }
            else
            {
                var instance = obj as ObjectInstance;
                if (instance == null)
                    return result;

                var oi = instance;
                result = GetObjectInfo(Engine, oi);
            }

            return result;
        }

        [JSFunction(Name = "include")]
        public virtual object Include(string scriptPath)
        {
            scriptPath = HttpContext.Current.Request.MapPath(scriptPath);
            var source = new FileScriptSource(scriptPath, System.Text.Encoding.Unicode);

            return Engine.Evaluate(source);
        }

        /// <summary>
        /// Override of include intended to be used from .net
        /// </summary>
        /// <param name="scriptPath">The path to the code to execute.</param>
        /// <param name="scope">The containing scope.</param>
        /// <param name="thisObject">The value of the "this" object.</param>
        /// <param name="strictMode">Indicates if the statement is being called under strict mode code.</param>
        /// <returns></returns>
        public virtual object Include(string scriptPath, Scope scope, object thisObject, bool strictMode)
        {
            scriptPath = HttpContext.Current.Request.MapPath(scriptPath);
            var source = new FileScriptSource(scriptPath, System.Text.Encoding.Unicode);

            var sourceReader = source.GetReader();
            var code = sourceReader.ReadToEnd();

            return Engine.Eval(code, scope, thisObject, strictMode);
        }

        [JSFunction(Name = "isArray")]
        public bool IsArray(object value)
        {
            return TypeUtilities.IsArray(value);
        }

        [JSFunction(Name = "isDate")]
        public bool IsDate(object value)
        {
            return TypeUtilities.IsDate(value);
        }

        [JSFunction(Name = "isDefined")]
        public bool IsDefined(object value)
        {
            return !TypeUtilities.IsUndefined(value);
        }

        [JSFunction(Name = "isFunction")]
        public bool IsFunction(object value)
        {
            return TypeUtilities.IsFunction(value);
        }

        [JSFunction(Name = "isNumber")]
        public bool IsNumber(object value)
        {
            return TypeUtilities.IsNumeric(value);
        }

        [JSFunction(Name = "isObject")]
        public bool IsObject(object value)
        {
            return TypeUtilities.IsObject(value);
        }

        [JSFunction(Name = "isString")]
        public bool IsString(object value)
        {
            return TypeUtilities.IsString(value);
        }

        [JSFunction(Name = "isUndefined")]
        public bool IsUndefined(object value)
        {
            return TypeUtilities.IsUndefined(value);
        }

        [JSFunction(Name = "lowercase")]
        public string Lowercase(object value)
        {
            return TypeConverter.ToString(value).ToLowerInvariant();
        }

        [JSFunction(Name = "uppercase")]
        public string Uppercase(object value)
        {
            return TypeConverter.ToString(value).ToUpperInvariant();
        }

        [JSFunction(Name = "version")]
        public ObjectInstance Version()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var codeName = "falling-cetacea"; //TODO: Pull this from a custom assembly attribute.

            var result = Engine.Object.Construct();
            result.SetPropertyValue("full", version + " " + codeName, true);
            result.SetPropertyValue("major", version.Major, true);
            result.SetPropertyValue("minor", version.Minor, true);
            result.SetPropertyValue("dot", version.Revision, true);
            result.SetPropertyValue("codeName", codeName, true);

            return result;
        }

        #region Members

        #region Help Generation Methods
        protected static ObjectInstance GetObjectInfo(ScriptEngine engine, ObjectInstance obj)
        {
            var result = engine.Object.Construct();
            var type = obj.GetType();

            var jsDocAttributes = type.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();
            foreach (var attribute in jsDocAttributes)
            {
                string tag = "summary";
                if (String.IsNullOrEmpty(attribute.Tag) == false)
                    tag = attribute.Tag;

                result.SetPropertyValue(tag, attribute.Text, false);
            }

            var resultProperties = engine.Object.Construct();

            var properties = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                                 .Select(mi => new
                                 {
                                     MemberInfo = mi,
                                     PropertyInfo = mi.GetCustomAttributes(typeof(JSPropertyAttribute), true).OfType<JSPropertyAttribute>().FirstOrDefault()
                                 })
                                 .Where(pi => pi.PropertyInfo != null)
                                 .OrderBy(pi => pi.PropertyInfo.Name);

            foreach (var property in properties)
            {
                var doc = GetMemberDocumentationObject(engine, property.MemberInfo);

                resultProperties.SetPropertyValue(property.PropertyInfo.Name, doc, false);
            }

            foreach (var property in obj.Properties)
            {
                if ((property.Value is FunctionInstance) == false && resultProperties.HasProperty(property.Name) == false)
                    resultProperties.SetPropertyValue(property.Name, "", false);
            }

            var resultFunctions = engine.Object.Construct();

            var functions = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                           .Select(mi => new
                           {
                               MemberInfo = mi,
                               FunctionInfo = mi.GetCustomAttributes(typeof(JSFunctionAttribute), true).OfType<JSFunctionAttribute>().FirstOrDefault()
                           })
                           .Where(pi => pi.FunctionInfo != null)
                           .OrderBy(pi => pi.FunctionInfo.Name);

            foreach (var function in functions)
            {
                var doc = GetMemberDocumentationObject(engine, function.MemberInfo);

                resultFunctions.SetPropertyValue(function.FunctionInfo.Name, doc, false);
            }

            foreach (var property in obj.Properties)
            {
                if (property.Value is FunctionInstance && resultFunctions.HasProperty(property.Name) == false)
                    resultFunctions.SetPropertyValue(property.Name, "", false);
            }

            result.SetPropertyValue("properties", resultProperties, false);
            result.SetPropertyValue("functions", resultFunctions, false);

            return result;
        }

        protected static ObjectInstance GetMemberDocumentationObject(ScriptEngine engine, MemberInfo member)
        {
            var jsDocAttributes = member.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();

            var doc = engine.Object.Construct();
            foreach (var attribute in jsDocAttributes)
            {
                var tag = "summary";
                if (string.IsNullOrEmpty(attribute.Tag) == false)
                    tag = attribute.Tag;

                doc.SetPropertyValue(tag, attribute.Text, false);
            }

            if (member is MethodInfo)
            {
                var methodInfo = member as MethodInfo;
                var methodParams = engine.Array.Construct();
                foreach (var parameter in methodInfo.GetParameters().OrderBy(p => p.Position))
                {
                    var parameterDoc = engine.Object.Construct();

                    var propertyJSDocAttributes =
                      parameter.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();

                    foreach (var attribute in propertyJSDocAttributes)
                    {
                        var tag = "param";
                        if (string.IsNullOrEmpty(attribute.Tag) == false)
                            tag = attribute.Tag;

                        parameterDoc.SetPropertyValue(tag, attribute.Text, false);
                    }

                    parameterDoc.SetPropertyValue("name", parameter.Name, false);
                    parameterDoc.SetPropertyValue("type", parameter.ParameterType.ToString().Replace("System.", ""), false);


                    ArrayInstance.Push(methodParams, parameterDoc);
                }

                doc.SetPropertyValue("params", methodParams, false);
                if (methodInfo.ReturnType != typeof(void))
                    doc.SetPropertyValue("returns", methodInfo.ReturnType.ToString().Replace("System.", ""), false);
            }
            else if (member is PropertyInfo)
            {
                var propertyInfo = member as PropertyInfo;
                doc.SetPropertyValue("type", GetTypeString(propertyInfo.PropertyType), false);
                doc.SetPropertyValue("hasGetter", propertyInfo.CanRead, false);
                doc.SetPropertyValue("hasSetter", propertyInfo.CanWrite, false);
            }
            else if (member is FieldInfo)
            {
                var fieldInfo = member as FieldInfo;
                doc.SetPropertyValue("type", GetTypeString(fieldInfo.FieldType), false);
            }

            return doc;
        }

        protected static string GetTypeString(Type type)
        {
            var result = type.ToString();
            switch (result)
            {
                case "Jurassic.Library.DateInstance":
                    result = "Date";
                    break;
                case "Jurassic.Library.ArrayInstance":
                    result = "Array";
                    break;
                case "Jurassic.Library.ObjectInstance":
                    result = "Object";
                    break;
                default:
                    result = result.Replace("System.", "");
                    result = result.Replace("Barista.Library.", "");
                    break;
            }

            return result;
        }
        #endregion

        #region Tern Documentation Generation
        protected static ObjectInstance GetTernObjectDefinition(ScriptEngine engine, ObjectInstance obj)
        {
            IDictionary<string, ObjectInstance> ignore;
            return GetTernObjectDefinition(engine, obj, null, out ignore);
        }

        protected static ObjectInstance GetTernObjectDefinition(ScriptEngine engine, ObjectInstance obj, string prefix, out IDictionary<string, ObjectInstance> globals)
        {
            //Get the statically defined property and function defintions defined on the object.

            var customTypes = new List<Type>();
            var result = engine.Object.Construct();
            var type = obj.GetType();

            if (type.IsSubclassOf(typeof(ObjectInstance)))
            {
                IList<Type> propertyTypes;
                GetTernPropertyDefinitionsForType(engine, type, result, out propertyTypes);

                foreach (var propertyType in propertyTypes)
                    if (!customTypes.Contains(propertyType))
                        customTypes.Add(propertyType);

                IList<Type> functionTypes;
                GetTernFunctionDefinitionsForType(engine, type, result, prefix, out functionTypes);

                foreach (var functionType in functionTypes)
                    if (!customTypes.Contains(functionType))
                        customTypes.Add(functionType);
            }

            //Get the dynamically defined property and function definitions defined on the object;
            var builtIns = new[] { "prototype", "hasOwnProperty", "isPrototypeOf", "propertyIsEnumerable", "toLocaleString", "toString", "valueOf" };

            // ReSharper disable once TooWideLocalVariableScope
            // ReSharper disable once RedundantAssignment
            IList<Type> objectFunctions = null;

            foreach (var property in obj.Properties)
            {
                //Don't document built-in properties, or properties already defined (statically)
                if (builtIns.Any(p => p.Equals(property.Name, StringComparison.Ordinal)) || result.HasProperty(property.Name))
                    continue;

                var valueType = property.Value.GetType();

                //if the property value is a functioninstance, get the doc object from the function's instance prototype.
                var pfunc = property.Value as FunctionInstance;
                if (pfunc != null)
                {
                    if (pfunc.InstancePrototype.GetType() != typeof(ObjectInstance))
                        valueType = pfunc.InstancePrototype.GetType();

                    var docObj = GetTernDocumentationObject(engine, valueType, prefix, out objectFunctions);

                    result.SetPropertyValue(property.Name, docObj, false);
                }
                else
                {
                    //If it's a property, get the documentation for the type, and reference it.
                    var typeName = GetTernTypeString(valueType, false, null, out objectFunctions);
                    if (!result.HasProperty(typeName))
                    {
                        var typeDocObj = GetTernDocumentationObject(engine, valueType, prefix, out objectFunctions);
                        result.SetPropertyValue(typeName, typeDocObj, false);
                    }

                    if (!prefix.IsNullOrWhiteSpace())
                        typeName = prefix + "." + typeName;

                    var refDoc = engine.Object.Construct();
                    refDoc.SetPropertyValue("!type", "+" + typeName, false);

                    result.SetPropertyValue(property.Name, refDoc, false);
                }
                
                
            }

            //Output Custom Global Types we discovered while enumerating properties and functions as globals.
            globals = new Dictionary<string, ObjectInstance>();
            while (customTypes.Any())
            {
                var customType = customTypes.First();
                var customTypeName = customType.Name;
                if (customType.IsGenericType)
                    customTypeName = customTypeName.Substring(0, customTypeName.LastIndexOf("`", StringComparison.Ordinal));

                var ternTypeName = GenerateTernCustomTypeString(customTypeName);
                if (globals.ContainsKey(ternTypeName))
                {
                    customTypes.Remove(customType);
                    continue;
                }

                IList<Type> subTypes;
                var typeDefinition = GetTernDocumentationObject(engine, customType, out subTypes);
                customTypes.AddRange(subTypes);

                globals.Add(ternTypeName, typeDefinition);

                customTypes.Remove(customType);
            }

            //Output doc and url metadata
            AssociateTernJsDocMetadata(type, result);
            return result;
        }

        protected static ObjectInstance GetTernDocumentationObject(ScriptEngine engine, MemberInfo member)
        {
            IList<Type> ignoreTypes;
            return GetTernDocumentationObject(engine, member, null, out ignoreTypes);
        }

        protected static ObjectInstance GetTernDocumentationObject(ScriptEngine engine, MemberInfo member, string prefix)
        {
            IList<Type> ignoreTypes;
            return GetTernDocumentationObject(engine, member, prefix, out ignoreTypes);
        }

        protected static ObjectInstance GetTernDocumentationObject(ScriptEngine engine, MemberInfo member, out IList<Type> customTypes)
        {
            IList<Type> innerTypes;
            var result = GetTernDocumentationObject(engine, member, null, out innerTypes);
            customTypes = innerTypes;
            return result;
        }

        protected static ObjectInstance GetTernDocumentationObject(ScriptEngine engine, MemberInfo member, string prefix, out IList<Type> customTypes)
        {
            customTypes = new List<Type>();

            var doc = engine.Object.Construct();
            
            if (member is Type)
            {
                var type = member as Type;

                //TODO: generate constructors.
                var ternConstructorDefinition = "fn(?)";

                var jsDocAttributes = type.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();
                var docAttributes = jsDocAttributes as JSDocAttribute[] ?? jsDocAttributes.ToArray();

                var ternConstructorTypeJsDocAttribute = docAttributes.FirstOrDefault(da => string.Equals(da.Tag, "ternConstructorDefinition", StringComparison.InvariantCultureIgnoreCase));
                if (ternConstructorTypeJsDocAttribute != null)
                    ternConstructorDefinition = ternConstructorTypeJsDocAttribute.Text;

                doc.SetPropertyValue("!type", ternConstructorDefinition, false);
                var prototype = engine.Object.Construct();

                IList<Type> propertyTypes;
                GetTernPropertyDefinitionsForType(engine, type, prototype, out propertyTypes);

                foreach (var propertyType in propertyTypes)
                    if (!customTypes.Contains(propertyType))
                        customTypes.Add(propertyType);

                IList<Type> functionTypes;
                GetTernFunctionDefinitionsForType(engine, type, prototype, prefix, out functionTypes);

                foreach (var functionType in functionTypes)
                    if (!customTypes.Contains(functionType))
                        customTypes.Add(functionType);
                
                doc.SetPropertyValue("prototype", prototype, false);
                
                //By convention, try to find an associated xxxConstructor object and reflect on it's methods/properties.
                if (type.AssemblyQualifiedName != null)
                {
                    var prototypeTypeName = type.AssemblyQualifiedName.Replace("Instance", "Constructor");
                    var prototypeType = Type.GetType(prototypeTypeName, false, true);
                    if (prototypeType != null)
                    {
                        IList<Type> prototypePropertyTypes;
                        GetTernPropertyDefinitionsForType(engine, prototypeType, doc, out prototypePropertyTypes);

                        foreach (var propertyType in prototypePropertyTypes)
                            if (!customTypes.Contains(propertyType))
                                customTypes.Add(propertyType);

                        IList<Type> prototypeFunctionTypes;
                        GetTernFunctionDefinitionsForType(engine, type, prototype, prefix, out prototypeFunctionTypes);

                        foreach (var functionType in prototypeFunctionTypes)
                            if (!customTypes.Contains(functionType))
                                customTypes.Add(functionType);
                    }
                }
                
                AssociateTernJsDocMetadata(member, doc);
            }
            else if (member is MethodInfo)
            {
                var methodInfoStringBuilder = new StringBuilder("fn(");

                var methodInfo = member as MethodInfo;
                var parameters = new List<string>();
                foreach (var parameter in methodInfo.GetParameters().OrderBy(p => p.Position))
                {
                    IList<Type> parameterTypes;
                    var parameterType = GetTernTypeString(parameter.ParameterType, prefix, out parameterTypes);

                    parameters.Add(parameter.Name + ": " + parameterType);

                    foreach (var type in parameterTypes)
                        if (!customTypes.Contains(type))
                            customTypes.Add(type);
                }

                methodInfoStringBuilder.Append(string.Join(", ", parameters.ToArray()));
                methodInfoStringBuilder.Append(")");

                if (methodInfo.ReturnType != typeof(void))
                {
                    IList<Type> methodReturnTypes;
                    var returnType = GetTernTypeString(methodInfo.ReturnType, out methodReturnTypes);

                    //If the method defines a JSDocAttribute with a "ternReturnType" tag, use that instead of the return type.
                    //For example, this allows return types of Array to specify their expected value type. [+SPWeb]
                    var jsDocAttributes = methodInfo.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();
                    var docAttributes = jsDocAttributes as JSDocAttribute[] ?? jsDocAttributes.ToArray();

                    var ternReturnTypeJsDocAttribute = docAttributes.FirstOrDefault(da => string.Equals(da.Tag, "ternReturnType", StringComparison.InvariantCultureIgnoreCase));
                    if (ternReturnTypeJsDocAttribute != null)
                        returnType = ternReturnTypeJsDocAttribute.Text;

                    methodInfoStringBuilder.Append(" -> " + returnType);

                    foreach (var type in methodReturnTypes)
                        if (!customTypes.Contains(type))
                            customTypes.Add(type);
                }
                    
                doc.SetPropertyValue("!type", methodInfoStringBuilder.ToString(), false);
            }
            else if (member is PropertyInfo)
            {
                var propertyInfo = member as PropertyInfo;

                IList<Type> propertyTypes;
                var propertyType = GetTernTypeString(propertyInfo.PropertyType, out propertyTypes);
                //If the method defines a JSDocAttribute with a "ternReturnType" tag, use that instead of the return type.
                //For example, this allows return types of Array to specify their expected value type. [+SPWeb]
                var jsDocAttributes = propertyInfo.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();
                var docAttributes = jsDocAttributes as JSDocAttribute[] ?? jsDocAttributes.ToArray();

                var ternPropertyTypeJsDocAttribute = docAttributes.FirstOrDefault(da => string.Equals(da.Tag, "ternPropertyType", StringComparison.InvariantCultureIgnoreCase));
                if (ternPropertyTypeJsDocAttribute != null)
                    propertyType = ternPropertyTypeJsDocAttribute.Text;

                doc.SetPropertyValue("!type", propertyType, false);

                foreach (var type in propertyTypes)
                    if (!customTypes.Contains(type))
                        customTypes.Add(type);
            }
            else if (member is FieldInfo)
            {
                var fieldInfo = member as FieldInfo;

                IList<Type> fieldTypes;
                doc.SetPropertyValue("!type", GetTernTypeString(fieldInfo.FieldType, out fieldTypes), false);

                foreach (var type in fieldTypes)
                    if (!customTypes.Contains(type))
                        customTypes.Add(type);
            }

            AssociateTernJsDocMetadata(member, doc);
            return doc;
        }

        protected static void GetTernPropertyDefinitionsForType(ScriptEngine engine, Type type, ObjectInstance doc, out IList<Type> propertyTypes)
        {
            propertyTypes = new List<Type>();

            //Properties...
            var properties = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .Select(mi => new
                {
                    MemberInfo = mi,
                    PropertyInfo =
                        mi.GetCustomAttributes(typeof(JSPropertyAttribute), true)
                            .OfType<JSPropertyAttribute>()
                            .FirstOrDefault()
                })
                .Where(pi => pi.PropertyInfo != null)
                .OrderBy(pi => pi.PropertyInfo.Name);

            foreach (var property in properties)
            {
                IList<Type> nestedPropertyTypes;
                var propertyDoc = GetTernDocumentationObject(engine, property.MemberInfo, out nestedPropertyTypes);
                doc.SetPropertyValue(property.PropertyInfo.Name, propertyDoc, false);

                foreach(var nestedPropertyType in nestedPropertyTypes)
                    if (!propertyTypes.Contains(nestedPropertyType))
                        propertyTypes.Add(nestedPropertyType);
            }
        }

        protected static void GetTernFunctionDefinitionsForType(ScriptEngine engine, Type type, ObjectInstance doc, string prefix, out IList<Type> functionTypes)
        {
            functionTypes = new List<Type>();

            //Functions...
            var functions = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                .Select(mi => new
                {
                    MemberInfo = mi,
                    FunctionInfo =
                        mi.GetCustomAttributes(typeof(JSFunctionAttribute), true)
                            .OfType<JSFunctionAttribute>()
                            .FirstOrDefault()
                })
                .Where(pi => pi.FunctionInfo != null)
                .OrderBy(pi => pi.FunctionInfo.Name);

            foreach (var function in functions)
            {
                IList<Type> nestedFunctionTypes;

                var functionDoc = GetTernDocumentationObject(engine, function.MemberInfo, prefix, out nestedFunctionTypes);
                doc.SetPropertyValue(function.FunctionInfo.Name, functionDoc, false);

                foreach (var nestedFunctionType in nestedFunctionTypes)
                    if (!functionTypes.Contains(nestedFunctionType))
                        functionTypes.Add(nestedFunctionType);
            }
        }

        /// <summary>
        /// For the given memberinfo, gets the defined JSDoc attributes and adds TernJS related metadata to the specified object instance.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="obj"></param>
        protected static void AssociateTernJsDocMetadata(MemberInfo member, ObjectInstance obj)
        {
            var jsDocAttributes = member.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();
            var docAttributes = jsDocAttributes as JSDocAttribute[] ?? jsDocAttributes.ToArray();
            
            //Pull the jsdoc attribute as the documentation
            var summaryJsDocAttribute = docAttributes.FirstOrDefault(da => da.Tag.IsNullOrWhiteSpace());

            if (summaryJsDocAttribute != null)
                obj.SetPropertyValue("!doc", summaryJsDocAttribute.Text, false);

            //Pull a link jsdoc attribute, if it exists.
            var linkJSDocAttribute = docAttributes.FirstOrDefault(da => string.Equals(da.Tag, "link", StringComparison.InvariantCultureIgnoreCase));

            if (linkJSDocAttribute != null)
                obj.SetPropertyValue("!url", linkJSDocAttribute.Text, false);
        }

        protected static string GetTernTypeString(Type type, out IList<Type> customTypes)
        {
            IList<Type> types;
            var result = GetTernTypeString(type, true, null, out types);
            customTypes = types;
            return result;
        }

        protected static string GetTernTypeString(Type type, string prefix, out IList<Type> customTypes)
        {
            IList<Type> types;
            var result = GetTernTypeString(type, true, prefix, out types);
            customTypes = types;
            return result;
        }

        protected static string GetTernTypeString(Type type, bool isReference, string prefix, out IList<Type> customTypes)
        {
            customTypes = new List<Type>();

            string result;
            if (type == typeof (Boolean) || type == typeof (BooleanInstance))
                result = "bool";
            else if (type == typeof (string) || type == typeof (ConcatenatedString) || type == typeof (StringInstance))
                result = "string";
            else if (type == typeof (FunctionInstance))
                result = "fn()";
            else if (type == typeof (DateTime) || type == typeof (DateInstance))
                result = "+Date";
            else if (type == typeof (Array) || type == typeof (Object[]) || type == typeof (ArrayInstance))
                result = "[?]"; //TODO: Specify array type - [+SPContentType] for instance.
            else if (type == typeof (Object) || type == typeof (ObjectInstance))
                result = "?";
            else if (type == typeof (Double) || type == typeof (float) || type == typeof (Byte) ||
                     type == typeof (IntPtr) ||
                     type == typeof (Int16) || type == typeof (Int32) || type == typeof (Int64) ||
                     type == typeof (UIntPtr) ||
                     type == typeof (UInt16) || type == typeof (UInt32) || type == typeof (UInt64) ||
                     type == typeof (BigInteger))
                result = "number";
            else
            {
                var typeName = type.Name;
                if (type.IsGenericType)
                    typeName = typeName.Substring(0, typeName.LastIndexOf("`", StringComparison.Ordinal));

                result = GenerateTernCustomTypeString(typeName);

                //TODO: fix this so that globals don't get prefixed.
                if (!prefix.IsNullOrWhiteSpace() && result != "Base64EncodedByteArray")
                    result = prefix + "." + result;

                if (isReference)
                    result = "+" + result;

                if (!customTypes.Contains(type))
                    customTypes.Add(type);
            }
            
            return result;
        }

        protected static string GenerateTernCustomTypeString(string typeName)
        {
            if (typeName.EndsWith("Instance"))
                typeName = typeName.Remove(typeName.Length - 8);

            return typeName;
        }

        #endregion

        
        #endregion
    }
}

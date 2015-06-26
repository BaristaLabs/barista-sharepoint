(function (mod) {
    if (typeof exports == "object" && typeof module == "object") // CommonJS
        return mod(require("../lib/infer"), require("../lib/tern"), require);
    if (typeof define == "function" && define.amd) // AMD
        return define(["../lib/infer", "../lib/tern"], mod);
    mod(tern, tern);
})(function (infer, tern, require) {
    "use strict";

    function resolvePath(base, path) {
        if (path[0] == "/") return path;
        var slash = base.lastIndexOf("/"), m;
        if (slash >= 0) path = base.slice(0, slash + 1) + path;
        while (m = /[^\/]*[^\/\.][^\/]*\/\.\.\//.exec(path))
            path = path.slice(0, m.index) + path.slice(m.index + m[0].length);
        return path.replace(/(^|[^\.])\.\//g, "$1");
    }

    function relativePath(from, to) {
        if (from[from.length - 1] != "/") from += "/";
        if (to.indexOf(from) == 0) return to.slice(from.length);
        else return to;
    }

    function getModule(data, name) {
        return data.modules[name] || (data.modules[name] = new infer.AVal);
    }

    var WG_DEFAULT_EXPORT = 95;

    function buildWrappingScope(parent, origin, node) {
        var scope = new infer.Scope(parent);
        scope.originNode = node;
        infer.cx().definitions.barista.require.propagate(scope.defProp("require"));
        //var module = new infer.Obj(infer.cx().definitions.node.Module.getProp("prototype").getType());
        //module.propagate(scope.defProp("module"));
        //var exports = new infer.Obj(true, "exports");
        //module.origin = exports.origin = origin;
        //module.originNode = exports.originNode = scope.originNode;
        //exports.propagate(scope.defProp("exports"));
        //var moduleExports = scope.exports = module.defProp("exports");
        //exports.propagate(moduleExports, WG_DEFAULT_EXPORT);
        return scope;
    }

    function resolveModule(server, name, _parent) {
        server.addFile(name, null, server._node.currentOrigin);
        return getModule(server._node, name);
    }

    // Assume node.js & access to local file system
    if (require) (function () {
        var fs = require("fs"), module_ = require("module"), path = require("path");

        relativePath = path.relative;

        resolveModule = function (server, name, parent) {
            var data = server._node;
            if (data.options.dontLoad == true ||
                data.options.dontLoad && new RegExp(data.options.dontLoad).test(name) ||
                data.options.load && !new RegExp(data.options.load).test(name))
                return infer.ANull;

            if (data.modules[name]) return data.modules[name];

            var currentModule = {
                id: parent,
                paths: module_._nodeModulePaths(path.dirname(parent))
            };
            try {
                var file = module_._resolveFilename(name, currentModule);
            } catch (e) { return infer.ANull; }

            var norm = normPath(file);
            if (data.modules[norm]) return data.modules[norm];

            if (fs.existsSync(file) && /^(\.js)?$/.test(path.extname(file)))
                server.addFile(relativePath(server.options.projectDir, file), null, data.currentOrigin);
            return data.modules[norm] = new infer.AVal;
        };
    })();

    function normPath(name) { return name.replace(/\\/g, "/"); }

    function resolveProjectPath(server, pth) {
        return resolvePath(normPath(server.options.projectDir || "") + "/", normPath(pth));
    }

    infer.registerFunction("baristaRequire", function (_self, _args, argNodes) {
        if (!argNodes || !argNodes.length || argNodes[0].type != "Literal" || typeof argNodes[0].value != "string")
            return infer.ANull;
        var cx = infer.cx(), server = cx.parent, data = server._node, name = argNodes[0].value;
        var locals = cx.definitions.barista;
        var result;

        if (locals[name]) {
            result = locals[name];
        } else if (name in data.modules) {
            result = data.modules[name];
        } else if (data.options.modules && data.options.modules.hasOwnProperty(name)) {
            var scope = buildWrappingScope(cx.topScope, name);
            infer.def.load(data.options.modules[name], scope);
            result = data.modules[name] = scope.exports;
        } else {
            // data.currentFile is only available while analyzing a file; at query
            // time, determine the calling file from the caller's AST.
            var currentFile = data.currentFile || resolveProjectPath(server, argNodes[0].sourceFile.name);

            var relative = /^\.{0,2}\//.test(name);
            if (relative) {
                if (!currentFile) return argNodes[0].required || infer.ANull;
                name = resolvePath(currentFile, name);
            }
            result = resolveModule(server, name, currentFile);
        }
        return argNodes[0].required = result;
    });

    function preCondenseReach(state) {
        var mods = infer.cx().parent._node.modules;
        var node = state.roots["!node"] = new infer.Obj(null);
        for (var name in mods) {
            var mod = mods[name];
            var id = mod.origin || name;
            var prop = node.defProp(id.replace(/\./g, "`"));
            mod.propagate(prop);
            prop.origin = mod.origin;
        }
    }

    function postLoadDef(data) {
        var cx = infer.cx(), mods = cx.definitions[data["!name"]]["!node"];
        var data = cx.parent._node;
        if (mods) for (var name in mods.props) {
            var origin = name.replace(/`/g, ".");
            var mod = getModule(data, origin);
            mod.origin = origin;
            mods.props[name].propagate(mod);
        }
    }

    function findTypeAt(_file, _pos, expr, type) {
        var isStringLiteral = expr.node.type === "Literal" &&
           typeof expr.node.value === "string";
        var isRequireArg = !!expr.node.required;

        if (isStringLiteral && isRequireArg) {
            // The `type` is a value shared for all string literals.
            // We must create a copy before modifying `origin` and `originNode`.
            // Otherwise all string literals would point to the last jump location
            type = Object.create(type);

            // Provide a custom origin location pointing to the require()d file
            var exportedType;
            if (expr.node.required && (exportedType = expr.node.required.getType())) {
                type.origin = exportedType.origin;
                type.originNode = exportedType.originNode;
            }
        }

        return type;
    }

    tern.registerPlugin("barista", function (server, options) {
        server._node = {
            modules: Object.create(null),
            options: options || {},
            currentFile: null,
            currentRequires: [],
            currentOrigin: null,
            server: server
        };

        server.on("beforeLoad", function (file) {
            this._node.currentFile = resolveProjectPath(server, file.name);
            this._node.currentOrigin = file.name;
            this._node.currentRequires = [];
            file.scope = buildWrappingScope(file.scope, this._node.currentOrigin, file.ast);
        });

        server.on("afterLoad", function (file) {
            var mod = getModule(this._node, this._node.currentFile);
            mod.origin = this._node.currentOrigin;
            //file.scope.exports.propagate(mod);
            this._node.currentFile = null;
            this._node.currentOrigin = null;
        });

        server.on("reset", function () {
            this._node.modules = Object.create(null);
        });

        return {
            defs: defs,
            passes: {
                preCondenseReach: preCondenseReach,
                postLoadDef: postLoadDef,
                completion: findCompletions,
                typeAt: findTypeAt
            }
        };
    });

    // Completes CommonJS module names in strings passed to require
    function findCompletions(file, query) {
        var wordEnd = tern.resolvePos(file, query.end);
        var callExpr = infer.findExpressionAround(file.ast, null, wordEnd, file.scope, "CallExpression");
        if (!callExpr) return;
        var callNode = callExpr.node;
        if (callNode.callee.type != "Identifier" || callNode.callee.name != "require" ||
            callNode.arguments.length < 1) return;
        var argNode = callNode.arguments[0];
        if (argNode.type != "Literal" || typeof argNode.value != "string" ||
            argNode.start > wordEnd || argNode.end < wordEnd) return;

        var word = argNode.raw.slice(1, wordEnd - argNode.start), quote = argNode.raw.charAt(0);
        if (word && word.charAt(word.length - 1) == quote)
            word = word.slice(0, word.length - 1);
        var completions = completeModuleName(query, file, word);
        if (argNode.end == wordEnd + 1 && file.text.charAt(wordEnd) == quote)
            ++wordEnd;
        return {
            start: tern.outputPos(query, file, argNode.start),
            end: tern.outputPos(query, file, wordEnd),
            isProperty: false,
            completions: completions.map(function (rec) {
                var name = typeof rec == "string" ? rec : rec.name;
                var string = JSON.stringify(name);
                if (quote == "'") string = quote + string.slice(1, string.length - 1).replace(/'/g, "\\'") + quote;
                if (typeof rec == "string") return string;
                rec.displayName = name;
                rec.name = string;
                return rec;
            })
        };
    }

    function completeModuleName(query, file, word) {
        var completions = [];
        var cx = infer.cx(), server = cx.parent, data = server._node;
        var currentFile = data.currentFile || resolveProjectPath(server, file.name);
        var wrapAsObjs = query.types || query.depths || query.docs || query.urls || query.origins;

        function gather(modules) {
            for (var name in modules) {
                if (name == currentFile) continue;

                var moduleName = resolveModulePath(name, currentFile);
                if (moduleName &&
                    !(query.filter !== false && word &&
                      (query.caseInsensitive ? moduleName.toLowerCase() : moduleName).indexOf(word) !== 0)) {
                    var rec = wrapAsObjs ? { name: moduleName } : moduleName;
                    completions.push(rec);

                    if (query.types || query.docs || query.urls || query.origins) {
                        var val = modules[name];
                        infer.resetGuessing();
                        var type = val.getType();
                        rec.guess = infer.didGuess();
                        if (query.types)
                            rec.type = infer.toString(val);
                        if (query.docs)
                            maybeSet(rec, "doc", val.doc || type && type.doc);
                        if (query.urls)
                            maybeSet(rec, "url", val.url || type && type.url);
                        if (query.origins)
                            maybeSet(rec, "origin", val.origin || type && type.origin);
                    }
                }
            }
        }

        if (query.caseInsensitive) word = word.toLowerCase();
        gather(cx.definitions.barista);
        //gather(data.modules);
        return completions;
    }

    /**
     * Resolve the module path of the given module name by using the current file.
     */
    function resolveModulePath(name, currentFile) {

        function startsWith(str, prefix) {
            return str.slice(0, prefix.length) == prefix;
        }

        function endsWith(str, suffix) {
            return str.slice(-suffix.length) == suffix;
        }

        if (name.indexOf('/') == -1) return name;
        // module name has '/', compute the module path
        var modulePath = normPath(relativePath(currentFile + '/..', name));
        if (startsWith(modulePath, 'node_modules')) {
            // module name starts with node_modules, remove it
            modulePath = modulePath.substring('node_modules'.length + 1, modulePath.length);
            if (endsWith(modulePath, 'index.js')) {
                // module name ends with index.js, remove it.
                modulePath = modulePath.substring(0, modulePath.length - 'index.js'.length - 1);
            }
        } else if (!startsWith(modulePath, '../')) {
            // module name is not inside node_modules and there is not ../, add ./
            modulePath = './' + modulePath;
        }
        if (endsWith(modulePath, '.js')) {
            // remove js extension
            modulePath = modulePath.substring(0, modulePath.length - '.js'.length);
        }
        return modulePath;
    }

    function maybeSet(obj, prop, val) {
        if (val != null) obj[prop] = val;
    }

    tern.defineQueryType("node_exports", {
        takesFile: true,
        run: function (server, query, file) {
            function describe(aval) {
                var target = {}, type = aval.getType(false);
                target.type = infer.toString(aval, 3);
                var doc = aval.doc || (type && type.doc), url = aval.url || (type && type.url);
                if (doc) target.doc = doc;
                if (url) target.url = url;
                var span = tern.getSpan(aval) || (type && tern.getSpan(type));
                if (span) tern.storeSpan(server, query, span, target);
                return target;
            }

            var known = server._node.modules[resolveProjectPath(server, file.name)];
            if (!known) return {};
            var type = known.getObjType(false);
            var resp = describe(known);
            if (type instanceof infer.Obj) {
                var props = resp.props = {};
                for (var prop in type.props)
                    props[prop] = describe(type.props[prop]);
            }
            return resp;
        }
    });

    var defs = {
        "!name": "barista",
        "!define": {
            "require": {
                "!type": "fn(id: string) -> !custom:baristaRequire",
                "!doc": "To require bundles."
            },
            "Active Directory": {
                "currentDomain": {
                    "!type": "string",
                    "!doc": "Gets the current domain, if the current machine is not joined to a domain, null is returned."
                },
                "ldapPath": {
                    "!type": "string",
                    "!doc": "Gets or sets the current ldap path that will be used."
                },
                "getADGroup": {
                    "!type": "fn(groupName: string) -> +ADGroup",
                    "!doc": "Returns an object representating the specified group."
                },
                "getADGroupByDistinguishedName": {
                    "!type": "fn(distinguishedName: ?) -> ?",
                    "!doc": "Returns an object representating the specified group."
                },
                "getADUser": {
                    "!type": "fn(loginName: ?) -> ?",
                    "!doc": "Returns an object representating the specified user. If no login name is specified, returns the current user."
                },
                "getADUserByDistinguishedName": {
                    "!type": "fn(distinguishedName: ?) -> ?",
                    "!doc": "Returns an object representating the specified user."
                },
                "searchAllDirectoryEntries": {
                    "!type": "fn(searchText: string, maxResults: number, principalType: string) -> [?]",
                    "!doc": "Searches all directory entries for the specified search text, optionally indicating a maximium number of results and to limit to the specified principal type."
                },
                "searchAllGroups": {
                    "!type": "fn(searchText: string, maxResults: number) -> [+ADGroup]",
                    "!doc": "Searches all groups for the specified search text, optionally indicating a maximium number of results."
                },
                "searchAllUsers": {
                    "!type": "fn(searchText: string, maxResults: number) -> [+ADUser]",
                    "!doc": "Searches all users for the specified search text contained within a user's firstname, lastname, displayname, email or logon name. Optionally indicating a maximium number of results."
                },
                "searchAllUsersByLogonAndEmail": {
                    "!type": "fn(searchText: string, maxResults: number) -> [?]",
                    "!doc": "Searches all users for the specified search text contained within a user's email or logon. Optionally indicating a maximium number of results."
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Active Directory"
            },
            "Barista Search Index": {
                "defaultMaxResults": {
                    "!type": "number",
                    "!doc": "Gets or sets the maximum number of results to return. Initial value is 10000."
                },
                "indexName": {
                    "!type": "string",
                    "!doc": "Gets or sets the name of index."
                },
                "createBooleanQuery": {
                    "!type": "fn() -> +BooleanQuery"
                },
                "createDoubleRangeQuery": {
                    "!type": "fn(fieldName: string, min: ?, max: ?, minInclusive: bool, maxInclusive: bool) -> +NumericRangeQueryInstance`1[Double]"
                },
                "createFloatRangeQuery": {
                    "!type": "fn(fieldName: string, min: ?, max: ?, minInclusive: bool, maxInclusive: bool) -> +NumericRangeQueryInstance`1[Single]"
                },
                "createFuzzyQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +FuzzyQuery"
                },
                "createIntRangeQuery": {
                    "!type": "fn(fieldName: string, min: ?, max: ?, minInclusive: bool, maxInclusive: bool) -> +NumericRangeQueryInstance`1[Int32]"
                },
                "createMatchAllDocsQuery": {
                    "!type": "fn() -> +GenericQuery"
                },
                "createMultiFieldQueryParserQuery": {
                    "!type": "fn(fieldNames: [?], query: string) -> +MultiFieldQueryParserQuery"
                },
                "createODataQuery": {
                    "!type": "fn(query: string, defaultField: string, allowLeadingWildcard: bool) -> +ODataQuery"
                },
                "createPhraseQuery": {
                    "!type": "fn() -> +PhraseQuery"
                },
                "createPrefixFilter": {
                    "!type": "fn(fieldName: string, text: string) -> +PrefixFilter"
                },
                "createPrefixQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +PrefixQuery"
                },
                "createQueryParserQuery": {
                    "!type": "fn(query: string, defaultField: string, allowLeadingWildcard: bool) -> +QueryParserQuery"
                },
                "createQueryWrapperFilter": {
                    "!type": "fn(query: ?) -> +QueryWrapperFilter"
                },
                "createRegexQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +RegexQuery"
                },
                "createSort": {
                    "!type": "fn() -> +Sort"
                },
                "createTermQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +TermQuery"
                },
                "createTermRangeQuery": {
                    "!type": "fn(fieldName: string, lowerTerm: string, upperTerm: string, includeLower: bool, includeUpper: bool) -> +TermRangeQuery"
                },
                "createTermsFilter": {
                    "!type": "fn(fieldName: ?, text: ?) -> +TermsFilter"
                },
                "createWildcardQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +WildcardQuery"
                },
                "deleteAllDocuments": {
                    "!type": "fn()"
                },
                "deleteDocuments": {
                    "!type": "fn(documentIds: ?)"
                },
                "doesIndexExist": {
                    "!type": "fn() -> bool"
                },
                "explain": {
                    "!type": "fn(query: ?, docId: ?) -> +Explanation"
                },
                "facetedSearch": {
                    "!type": "fn(query: ?, maxResults: ?, groupByFields: ?) -> [+FacetedSearchResult]"
                },
                "getFieldNames": {
                    "!type": "fn() -> [string]"
                },
                "highlight": {
                    "!type": "fn(query: ?, docId: ?, fieldName: ?, fragCharSize: ?) -> string"
                },
                "index": {
                    "!type": "fn(documentObject: ?) -> +SearchService"
                },
                "retrieve": {
                    "!type": "fn(documentId: string) -> +JsonDocument"
                },
                "search": {
                    "!type": "fn(query: ?, maxResults: ?) -> [+SearchResult]"
                },
                "searchResultCount": {
                    "!type": "fn(query: ?, maxResults: ?) -> number"
                },
                "setFieldOptions": {
                    "!type": "fn(fieldOptions: ?) -> +SearchService"
                },
                "shutdown": {
                    "!type": "fn() -> +SearchService"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Barista Search Index"
            },
            "Deferred": {
                "!doc": "Deferred Bundle. Contains functionality to perform multi-threaded, async tasks."
            },
            "Diagnostics": {
                "!doc": "Diagnostics Bundle. Provides a mechanism to interact with system performance counters and other diagnostic tools"
            },
            "Document": {
                "csv2Json": {
                    "!type": "fn(csv: ?, csvOptions: ?) -> ?"
                },
                "html2Pdf": {
                    "!type": "fn(html: string, pdfAttachments: ?) -> +Base64EncodedByteArray"
                },
                "json2Csv": {
                    "!type": "fn(array: ?, csvOptions: ?) -> ?"
                },
                "json2Xml": {
                    "!type": "fn(jsonObject: ?) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "xml2Json": {
                    "!type": "fn(xml: ?) -> ?"
                },
                "!doc": "Document"
            },
            "Document Store": {
                "containerTitle": {
                    "!type": "string"
                },
                "addEntityComment": {
                    "!type": "fn(entityId: ?, comment: string) -> +Comment"
                },
                "addPrincipalRoleToEntity": {
                    "!type": "fn(entityId: ?, principalName: string, principalType: string, roleName: string) -> +PrincipalRoleInfo"
                },
                "cloneEntity": {
                    "!type": "fn(entityId: ?, sourcePath: string, targetPath: string, newTitle: string) -> +Entity"
                },
                "countEntities": {
                    "!type": "fn(filterCriteria: ?) -> number"
                },
                "createContainer": {
                    "!type": "fn(containerTitle: string, description: string) -> +Container",
                    "!doc": "Creates a new container in the repository with the specified title and description."
                },
                "createEntity": {
                    "!type": "fn(args: [?]) -> +Entity",
                    "!doc": "Creates a new entity.\nEx: createEntity([path], title, entityNamespace, data])"
                },
                "createEntityPart": {
                    "!type": "fn(entityId: ?, partName: ?, category: ?, data: ?) -> +EntityPart"
                },
                "createFolder": {
                    "!type": "fn(path: ?) -> +Folder"
                },
                "createOrUpdateEntityPart": {
                    "!type": "fn(entityId: ?, partName: ?, category: ?, data: ?) -> +EntityPart"
                },
                "deleteAttachment": {
                    "!type": "fn(entityId: ?, fileName: string) -> bool"
                },
                "deleteContainer": {
                    "!type": "fn(containerTitle: string)",
                    "!doc": "Deletes the specified container."
                },
                "deleteEntity": {
                    "!type": "fn(entityId: ?) -> bool"
                },
                "deleteEntityPart": {
                    "!type": "fn(entityId: ?, partName: string) -> bool"
                },
                "deleteFolder": {
                    "!type": "fn(path: ?)"
                },
                "downloadAttachment": {
                    "!type": "fn(entityId: ?, fileName: string) -> +Base64EncodedByteArray"
                },
                "exportEntity": {
                    "!type": "fn(entityId: ?) -> +Base64EncodedByteArray"
                },
                "getAttachment": {
                    "!type": "fn(entityId: ?, fileName: string) -> +Attachment"
                },
                "getContainer": {
                    "!type": "fn(containerTitle: string) -> +Container",
                    "!doc": "Returns the container from the repository with the specified title."
                },
                "getEntity": {
                    "!type": "fn(entityId: ?, path: ?) -> ?"
                },
                "getEntityPart": {
                    "!type": "fn(entityId: ?, path: string, partName: ?) -> ?",
                    "!doc": "Gets the entity part with the specified partName. Path argument is optional."
                },
                "getEntityPermissions": {
                    "!type": "fn(entityId: ?) -> +PermissionsInfo"
                },
                "getEntitySet": {
                    "!type": "fn(entityId: ?, path: ?) -> +EntitySet"
                },
                "hasEntityPart": {
                    "!type": "fn(entityId: ?, partName: string) -> bool"
                },
                "importEntity": {
                    "!type": "fn(path: string, entityId: ?, namespace: string, archiveData: +Base64EncodedByteArray) -> +Entity"
                },
                "listAttachments": {
                    "!type": "fn(entityId: ?) -> [+Attachment]"
                },
                "listContainers": {
                    "!type": "fn() -> [+Container]",
                    "!doc": "Lists all containers contained within the repository."
                },
                "listEntities": {
                    "!type": "fn(filterCriteria: ?) -> [+Entity]",
                    "!doc": "Lists the entities according to the specified criteria. If filterCriteria argument is null, returns all objects, if filterCriteria object is a string, restricts to the folder with the specified name. If an object, uses the following properties: path, includeData, namespace, namespaceMatchType, queryPairs, skip, top."
                },
                "listEntityComments": {
                    "!type": "fn(entityId: ?, path: ?) -> [+Comment]"
                },
                "listEntityParts": {
                    "!type": "fn(entityId: ?) -> ?"
                },
                "listFolders": {
                    "!type": "fn(path: ?) -> [+Folder]"
                },
                "moveEntity": {
                    "!type": "fn(entityId: ?, destinationPath: string) -> bool"
                },
                "removePrincipalRoleFromEntity": {
                    "!type": "fn(entityId: ?, principalName: string, principalType: string, roleName: string) -> bool"
                },
                "resetEntityPermissions": {
                    "!type": "fn(entityId: ?) -> +PermissionsInfo"
                },
                "single": {
                    "!type": "fn(filterCriteria: ?) -> ?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "updateEntity": {
                    "!type": "fn(entityId: ?, eTag: ?, data: ?) -> ?"
                },
                "updateEntityNamespace": {
                    "!type": "fn(entityId: ?, newNamespace: ?) -> ?"
                },
                "updateEntityPart": {
                    "!type": "fn(entityId: ?, partName: ?, eTag: ?, data: ?) -> +EntityPart"
                },
                "uploadAttachment": {
                    "!type": "fn(entityId: ?, fileName: string, attachment: +Base64EncodedByteArray) -> +Attachment"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Document Store"
            },
            "iCal": {
                "createCalendar": {
                    "!type": "fn() -> +iCalendar"
                },
                "loadCalendar": {
                    "!type": "fn(data: +Base64EncodedByteArray) -> +iCalendarCollection"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "iCal"
            },
            "Json Data": {
                "!doc": "Json Data Bundle. Provides behavior to assist with manipulating json. Currently adds:\njsonDataHandler: a mechanism to Diff/Merge Json objects.\nautomapper: a mechanism to perform json object-to-object mapping.\nJsonSchema: a top-level native JsonSchema validator.\nJsonMergeSettings: Supports object.merge().\nobject.merge(): a native mechanism to merge json objects.\nobject.selectToken(): a native mechnism for XPath like quering.\nobject.selectTokens(): a native mechnism for XPath like quering.\nobject.isValid(schema): validates that the object is valid according to the instance of the JsonSchema.\nobject.isValid2(schema): validates that the object is valid according to the instance of the JsonSchema. Returns a collection of errors\n"
            },
            "K2": {
                "servicesBaseUrl": {
                    "!type": "string"
                },
                "openProcessInstance": {
                    "!type": "fn(processInstanceId: string, includeDataFields: bool, includeXmlFields: bool) -> +ProcessInstance"
                },
                "openWorklist": {
                    "!type": "fn(includeActivityData: bool, includeActivityXml: bool, includeProcessData: bool, includeProcessXml: bool) -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "K2"
            },
            "Linq": {
                "!doc": "Linq Bundle. Adds objects to allow javascript arrays to be queried via linq-like syntax. (See http://linqjs.codeplex.com/)"
            },
            "Lo-Dash": {
                "!doc": "LoDash Bundle. A utility library delivering consistency, customization, performance, & extras. Based off of Underscore.js. (See http://lodash.com/)"
            },
            "Moment": {
                "!doc": "Moment Bundle. Includes a library that provides extra date/time methods. (See http://momentjs.com/)"
            },
            "Mustache": {
                "render": {
                    "!type": "fn(template: string, data: ?) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Mustache"
            },
            "SharePoint": {
                "currentContext": {
                    "!type": "+SPContext",
                    "!doc": "Gets the current context of the request. Equivalent to SPContext.Current in the server object model."
                },
                "farm": {
                    "!type": "+SPFarm",
                    "!doc": "Gets the local farm instance. Equivalent to SPFarm.Local in the server object model."
                },
                "secureStore": {
                    "!type": "+SPSecureStore",
                    "!doc": "Gets a reference to the secure store service."
                },
                "server": {
                    "!type": "+SPServer",
                    "!doc": "Gets the local server instance. Equivalent to SPFarm.Server in the server object model."
                },
                "beginMonitoredScope": {
                    "!type": "fn(name: string) -> +SPMonitoredScope",
                    "!doc": "Starts a new monitored scope that can be used to profile script executino."
                },
                "endMonitoredScope": {
                    "!type": "fn(monitoredScope: ?) -> ?",
                    "!doc": "Ends a previously created monitored scope that can be used to profile script execution."
                },
                "exists": {
                    "!type": "fn(fileUrl: string) -> bool",
                    "!doc": "Returns a value that indicates if a file exists at the specified url."
                },
                "getCurrentUser": {
                    "!type": "fn() -> +SPUser",
                    "!doc": "Gets the current user. Equivalent to SPContext.Current.Web.CurrentUser"
                },
                "getResponseUrl": {
                    "!type": "fn() -> string",
                    "!doc": "Gets the url that corresponds to the incoming request for the current zone."
                },
                "loadFileAsByteArray": {
                    "!type": "fn(fileUrl: string) -> +Base64EncodedByteArray",
                    "!doc": "Loads the file at the specified url as a byte array."
                },
                "loadFileAsJSON": {
                    "!type": "fn(fileUrl: string) -> ?",
                    "!doc": "Loads the file at the specified url as a JSON Object."
                },
                "loadFileAsString": {
                    "!type": "fn(fileUrl: string) -> string",
                    "!doc": "Loads the file at the specified url as a string."
                },
                "sendEmail": {
                    "!type": "fn(to: string, cc: string, bcc: string, from: string, subject: string, messageBody: string, appendFooter: bool) -> bool",
                    "!doc": "Sends an email."
                },
                "toggleListEvents": {
                    "!type": "fn(enabled: bool)",
                    "!doc": "Indicates whether event firing is enabled (true) or disabled (false)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "write": {
                    "!type": "fn(fileUrl: string, contents: ?) -> +SPFile",
                    "!doc": "Writes the specified contents to the file located at the specified url"
                },
                "!doc": "SharePoint"
            },
            "SharePoint Content Migration": {
                "export": {
                    "!type": "fn(exportSettings: ?, dropLocation: ?) -> [string]"
                },
                "exportFile": {
                    "!type": "fn(file: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, dropLocation: ?) -> [string]",
                    "!doc": "Exports the specified file."
                },
                "exportFolder": {
                    "!type": "fn(folder: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, dropLocation: ?) -> [string]"
                },
                "exportList": {
                    "!type": "fn(list: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, dropLocation: ?) -> [string]"
                },
                "exportListItem": {
                    "!type": "fn(listItem: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, dropLocation: ?) -> [string]"
                },
                "exportSite": {
                    "!type": "fn(site: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, dropLocation: ?) -> [string]"
                },
                "exportWeb": {
                    "!type": "fn(web: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, dropLocation: ?) -> [string]"
                },
                "import": {
                    "!type": "fn(importSettings: ?, dropLocation: ?)"
                },
                "importToSite": {
                    "!type": "fn(site: ?, fileLocation: ?, baseFileName: ?, logFilePath: ?, isDropFileLocation: ?)",
                    "!doc": "Provides an alternative interface for import."
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "SharePoint Content Migration"
            },
            "SharePoint Publishing": {
                "!doc": "SharePoint Publishing Bundle. Provides top-level objects to interact with the SharePoint Publishing Infrastructure"
            },
            "SharePoint Search": {
                "!doc": "SharePoint Search Bundle. Provides top-level objects to interact with SharePoint Search"
            },
            "SharePoint Taxonomy": {
                "!doc": "SharePoint Taxonomy Bundle. Includes basic functionality for enterprise metadata management. Examples include types for managing terms, term sets, groups, keywords, term stores, and metadata service applications.\r\n This Bundle adds a top-level prototype named 'TaxonomySession' which can be instantiated with a site."
            },
            "SharePoint Workflow": {
                "!doc": "SharePoint Workflow Bundle. Provides top-level objects to interact with SharePoint Workflows and Workflow Tasks."
            },
            "Simple Inheritance": {
                "!doc": "Includes functionality for simple inheritance."
            },
            "Smtp": {
                "!doc": "Includes functionality to interact with Smtp severs."
            },
            "Sql Data": {
                "!doc": "Provides access to SQL Server Databases."
            },
            "State Machine": {
                "!doc": "State Machine Bundle. Provides a mechanism to create finite state machines. (See https://github.com/jakesgordon/javascript-state-machine)"
            },
            "String": {
                "name": {},
                "length": {},
                "prototype": {},
                "VERSION": {},
                "TMPL_OPEN": {},
                "TMPL_CLOSE": {},
                "ENTITIES": {},
                "apply": {
                    "!type": "fn(thisObj: ?, arguments: ?) -> ?"
                },
                "bind": {
                    "!type": "fn(boundThis: ?, boundArguments: [?]) -> fn()"
                },
                "call": {
                    "!type": "fn(thisObj: ?, arguments: [?]) -> ?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "extendPrototype": {},
                "restorePrototype": {},
                "!doc": "String"
            },
            "Sucralose": {
                "!doc": "Sucralose Bundle. Includes a library that extends native objects with helpful methods similar to Sugar, however this bundle is implemented with native code."
            },
            "Sugar": {
                "!doc": "SugarJS Bundle. Includes a library that extends native objects with helpful methods. (See http://sugarjs.com/)"
            },
            "Team Foundation Server": {
                "credential": {
                    "!type": "?"
                },
                "uri": {
                    "!type": "?"
                },
                "allowUntrustedCertificates": {
                    "!type": "fn()"
                },
                "getTeamProjectCollection": {
                    "!type": "fn(name: ?) -> ?",
                    "!doc": "Returns the Team Project Collection with the specified display name. If no name is specified, gets the default project collection for the specified url."
                },
                "listTeamProjectCollections": {
                    "!type": "fn() -> [+TfsTeamProjectCollection]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Team Foundation Server"
            },
            "Unified Logging Service": {
                "getLocalLogEntries": {
                    "!type": "fn(guid: ?, daysToLook: number) -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Unified Logging Service"
            },
            "Unit Testing": {
                "!doc": "Unit Testing Bundle. Adds components that facilitate unit testing of scripts."
            },
            "Utility": {
                "deserializeObjectFromByteArray": {
                    "!type": "fn(serializedObject: ?) -> ?"
                },
                "getCurrentCorrelationId": {
                    "!type": "fn() -> string"
                },
                "getExtensionFromFileName": {
                    "!type": "fn(fileName: string) -> string"
                },
                "getMimeTypeFromFileName": {
                    "!type": "fn(fileName: string) -> string"
                },
                "randomString": {
                    "!type": "fn(size: number, allowNumbers: bool, allowUpperCase: bool, allowLowerCase: bool, allowSpecialChars: bool, allowWhitespace: bool) -> string"
                },
                "replaceJsonReferences": {
                    "!type": "fn(o: ?) -> ?"
                },
                "serializeObjectToByteArray": {
                    "!type": "fn(objectToSerialize: ?) -> ?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Utility"
            },
            "Web": {
                "request": {
                    "!type": "+HttpRequest"
                },
                "response": {
                    "!type": "+HttpResponse"
                },
                "addItemToCache": {
                    "!type": "fn(cacheKey: string, item: ?, absoluteExpiration: ?, slidingExpiration: ?)"
                },
                "ajax": {
                    "!type": "fn(url: string, settings: ?) -> ?"
                },
                "getItemFromCache": {
                    "!type": "fn(cacheKey: string) -> ?"
                },
                "getItemsInCache": {
                    "!type": "fn() -> ?"
                },
                "parseQueryString": {
                    "!type": "fn(query: ?) -> ?"
                },
                "removeItemFromCache": {
                    "!type": "fn(cacheKey: string) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Web"
            },
            "Web Administration": {
                "applicationPools": {
                    "!type": "[+ApplicationPool]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Web Administration"
            },
            "Web Optimization": {
                "bundle": {
                    "!type": "fn(bundleDefinitionXml: string, fileName: string, update: ?, minify: ?) -> ?",
                    "!doc": "Using a xml bundle definition, combines the specified files and returns the bundle as an object."
                },
                "clearCache": {
                    "!type": "fn()",
                    "!doc": "Clears all cached files."
                },
                "gzip": {
                    "!type": "fn(obj: ?, fileName: ?, mimeType: ?) -> +Base64EncodedByteArray"
                },
                "hasBundleChangedSince": {
                    "!type": "fn(bundleDefinitionXml: string, date: ?) -> bool",
                    "!doc": "Using a xml bundle definition, determines if the contents of the bundle have changed since the specfied date."
                },
                "minifyCss": {
                    "!type": "fn(css: string) -> string",
                    "!doc": "Returns a minified representation of the css string passed as the first argument."
                },
                "minifyJs": {
                    "!type": "fn(javascript: string) -> string",
                    "!doc": "Returns a minified representation of the javascript string passed as the first argument."
                },
                "replaceRelativeUrlsWithAbsoluteInCss": {
                    "!type": "fn(css: string, cssFilePath: string) -> string",
                    "!doc": "Returns the specified CSS file with the included url values to be absolute urls."
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "!doc": "Web Optimization"
            }
        },
        "barista": {
            "!type": "fn(?)",
            "prototype": {
                "common": {
                    "!type": "+Common"
                },
                "environment": {
                    "!type": "+Environment"
                },
                "equals": {
                    "!type": "fn(o1: ?, o2: ?) -> bool"
                },
                "generateTurnTypeDefinition": {
                    "!type": "fn(obj: ?) -> ?",
                    "!doc": "Returns a JSON object that is a TernJS based type definition of the specified object. If no object is specified, a definition of all bundes will be returned."
                },
                "grabMutex": {
                    "!type": "fn(name: string) -> +Mutex"
                },
                "help": {
                    "!type": "fn(obj: ?) -> ?"
                },
                "include": {
                    "!type": "fn(scriptPath: string) -> ?"
                },
                "isArray": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isDate": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isDefined": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isFunction": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isNumber": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isObject": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isString": {
                    "!type": "fn(value: ?) -> bool"
                },
                "isUndefined": {
                    "!type": "fn(value: ?) -> bool"
                },
                "lowercase": {
                    "!type": "fn(value: ?) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "uppercase": {
                    "!type": "fn(value: ?) -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "version": {
                    "!type": "fn() -> ?"
                }
            },
            "common": {
                "!type": "+Common"
            },
            "environment": {
                "!type": "+Environment"
            }
        },
        "Guid": {
            "!type": "fn(?)",
            "prototype": {
                "toByteArray": {
                    "!type": "fn() -> +Base64EncodedByteArray"
                },
                "toJSON": {
                    "!type": "fn(thisObject: ?, key: string) -> ?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "HashTable": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "isFixedSize": {
                    "!type": "bool"
                },
                "isReadOnly": {
                    "!type": "bool"
                },
                "isSynchronized": {
                    "!type": "bool"
                },
                "add": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "clear": {
                    "!type": "fn()"
                },
                "clone": {
                    "!type": "fn() -> +Hashtable"
                },
                "contains": {
                    "!type": "fn(key: ?) -> bool"
                },
                "containsKey": {
                    "!type": "fn(key: ?) -> bool"
                },
                "containsValue": {
                    "!type": "fn(value: ?) -> bool"
                },
                "getKeys": {
                    "!type": "fn() -> [string]"
                },
                "getValueByKey": {
                    "!type": "fn(key: ?) -> ?"
                },
                "getValues": {
                    "!type": "fn() -> [?]"
                },
                "remove": {
                    "!type": "fn(key: ?)"
                },
                "setValueByKey": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toObject": {
                    "!type": "fn() -> ?"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Uri": {
            "!type": "fn(?)",
            "prototype": {
                "absolutePath": {
                    "!type": "string"
                },
                "absoluteUri": {
                    "!type": "string"
                },
                "authority": {
                    "!type": "string"
                },
                "dnsSafeHost": {
                    "!type": "string"
                },
                "fragment": {
                    "!type": "string"
                },
                "host": {
                    "!type": "string"
                },
                "hostNameType": {
                    "!type": "string"
                },
                "isAbsoluteUri": {
                    "!type": "bool"
                },
                "isDefaultPort": {
                    "!type": "bool"
                },
                "isFile": {
                    "!type": "bool"
                },
                "isLoopback": {
                    "!type": "bool"
                },
                "isUnc": {
                    "!type": "bool"
                },
                "localPath": {
                    "!type": "string"
                },
                "originalString": {
                    "!type": "string"
                },
                "pathAndQuery": {
                    "!type": "string"
                },
                "port": {
                    "!type": "number"
                },
                "query": {
                    "!type": "string"
                },
                "queryString": {
                    "!type": "?"
                },
                "scheme": {
                    "!type": "string"
                },
                "segments": {
                    "!type": "[string]"
                },
                "userEscaped": {
                    "!type": "bool"
                },
                "userInfo": {
                    "!type": "string"
                },
                "isBaseOf": {
                    "!type": "fn(uri: ?) -> bool"
                },
                "isWellFormedOriginalString": {
                    "!type": "fn() -> bool"
                },
                "makeRelativeUri": {
                    "!type": "fn(uri: ?) -> +Uri"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "NetworkCredential": {
            "!type": "fn(?)",
            "prototype": {
                "domain": {
                    "!type": "string"
                },
                "password": {
                    "!type": "string"
                },
                "userName": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Base64EncodedByteArray": {
            "!type": "fn(?)",
            "prototype": {
                "fileName": {
                    "!type": "string"
                },
                "length": {
                    "!type": "number"
                },
                "mimeType": {
                    "!type": "string"
                },
                "append": {
                    "!type": "fn(data: string)"
                },
                "getByteAt": {
                    "!type": "fn(index: number) -> string"
                },
                "setByteAt": {
                    "!type": "fn(index: number, data: string)"
                },
                "toAsciiString": {
                    "!type": "fn() -> string"
                },
                "toBase64String": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "toUnicodeString": {
                    "!type": "fn() -> string"
                },
                "toUtf8String": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "ADGroup": {
            "!type": "fn(?)",
            "prototype": {
                "displayName": {
                    "!type": "string"
                },
                "members": {
                    "!type": "[+ADUser]"
                },
                "name": {
                    "!type": "string"
                },
                "rawSid": {
                    "!type": "?"
                },
                "sId": {
                    "!type": "?"
                },
                "expandGroups": {
                    "!type": "fn() -> [+ADGroup]"
                },
                "expandUsers": {
                    "!type": "fn() -> [+ADUser]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Attachment": {
            "!type": "fn(?)",
            "prototype": {
                "category": {
                    "!type": "string"
                },
                "created": {
                    "!type": "+Date"
                },
                "createdBy": {
                    "!type": "?"
                },
                "eTag": {
                    "!type": "string"
                },
                "fileName": {
                    "!type": "string"
                },
                "mimeType": {
                    "!type": "string"
                },
                "modified": {
                    "!type": "+Date"
                },
                "modifiedBy": {
                    "!type": "?"
                },
                "path": {
                    "!type": "string"
                },
                "size": {
                    "!type": "number"
                },
                "timeLastModified": {
                    "!type": "+Date"
                },
                "url": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "BooleanQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "minimimumNumberShouldMatch": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn(searchQuery: ?, occur: ?) -> +BooleanQuery"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "CalendarProperty": {
            "!type": "fn(?)",
            "prototype": {
                "name": {
                    "!type": "string"
                },
                "value": {
                    "!type": "string"
                },
                "addParameter": {
                    "!type": "fn(name: string, value: string)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "CalendarPropertyList": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(calendarProperty: +CalendarProperty)"
                },
                "clear": {
                    "!type": "fn()"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "ClientInfo": {
            "!type": "fn(?)",
            "prototype": {
                "device": {
                    "!type": "+Device"
                },
                "os": {
                    "!type": "+OS"
                },
                "userAgent": {
                    "!type": "+UserAgent"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Comment": {
            "!type": "fn(?)",
            "prototype": {
                "commentText": {
                    "!type": "string"
                },
                "id": {
                    "!type": "number"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Container": {
            "!type": "fn(?)",
            "prototype": {
                "created": {
                    "!type": "+Date"
                },
                "createdBy": {
                    "!type": "?"
                },
                "description": {
                    "!type": "string"
                },
                "entityCount": {
                    "!type": "number"
                },
                "id": {
                    "!type": "+Guid"
                },
                "modified": {
                    "!type": "+Date"
                },
                "modifiedBy": {
                    "!type": "?"
                },
                "title": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Device": {
            "!type": "fn(?)",
            "prototype": {
                "family": {
                    "!type": "string"
                },
                "isSpider": {
                    "!type": "bool"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "DiffResult": {
            "!type": "fn(?)",
            "prototype": {
                "added": {
                    "!type": "[?]"
                },
                "changed": {
                    "!type": "[?]"
                },
                "identical": {
                    "!type": "[?]"
                },
                "removed": {
                    "!type": "[?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Entity": {
            "!type": "fn(?)",
            "prototype": {
                "contentsETag": {
                    "!type": "string"
                },
                "contentsModified": {
                    "!type": "+Date"
                },
                "created": {
                    "!type": "+Date"
                },
                "createdBy": {
                    "!type": "?"
                },
                "data": {
                    "!type": "?"
                },
                "description": {
                    "!type": "string"
                },
                "eTag": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "modified": {
                    "!type": "+Date"
                },
                "modifiedBy": {
                    "!type": "?"
                },
                "namespace": {
                    "!type": "?"
                },
                "path": {
                    "!type": "string"
                },
                "title": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "EntityPart": {
            "!type": "fn(?)",
            "prototype": {
                "category": {
                    "!type": "string"
                },
                "created": {
                    "!type": "+Date"
                },
                "createdBy": {
                    "!type": "?"
                },
                "data": {
                    "!type": "?"
                },
                "entityId": {
                    "!type": "+Guid"
                },
                "eTag": {
                    "!type": "string"
                },
                "modified": {
                    "!type": "+Date"
                },
                "modifiedBy": {
                    "!type": "?"
                },
                "name": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "EntitySet": {
            "!type": "fn(?)",
            "prototype": {
                "entity": {
                    "!type": "+Entity"
                },
                "entityParts": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Event": {
            "!type": "fn(?)",
            "prototype": {
                "categories": {
                    "!type": "[string]"
                },
                "comments": {
                    "!type": "[string]"
                },
                "description": {
                    "!type": "string"
                },
                "duration": {
                    "!type": "?"
                },
                "isAllDay": {
                    "!type": "bool"
                },
                "location": {
                    "!type": "string"
                },
                "organizer": {
                    "!type": "?"
                },
                "priority": {
                    "!type": "number"
                },
                "properties": {
                    "!type": "+CalendarPropertyList"
                },
                "resources": {
                    "!type": "[string]"
                },
                "start": {
                    "!type": "?"
                },
                "status": {
                    "!type": "string"
                },
                "summary": {
                    "!type": "string"
                },
                "uid": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "getCalendar": {
                    "!type": "fn() -> +iCalendar"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Explanation": {
            "!type": "fn(?)",
            "prototype": {
                "description": {
                    "!type": "string"
                },
                "details": {
                    "!type": "[+Explanation]"
                },
                "explanationHtml": {
                    "!type": "string"
                },
                "isMatch": {
                    "!type": "bool"
                },
                "value": {
                    "!type": "number"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Folder": {
            "!type": "fn(?)",
            "prototype": {
                "created": {
                    "!type": "+Date"
                },
                "createdBy": {
                    "!type": "?"
                },
                "entityCount": {
                    "!type": "number"
                },
                "fullPath": {
                    "!type": "string"
                },
                "modified": {
                    "!type": "+Date"
                },
                "modifiedBy": {
                    "!type": "?"
                },
                "name": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "FuzzyQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "GenericQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Hashtable": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "isFixedSize": {
                    "!type": "bool"
                },
                "isReadOnly": {
                    "!type": "bool"
                },
                "isSynchronized": {
                    "!type": "bool"
                },
                "add": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "clear": {
                    "!type": "fn()"
                },
                "clone": {
                    "!type": "fn() -> +Hashtable"
                },
                "contains": {
                    "!type": "fn(key: ?) -> bool"
                },
                "containsKey": {
                    "!type": "fn(key: ?) -> bool"
                },
                "containsValue": {
                    "!type": "fn(value: ?) -> bool"
                },
                "getKeys": {
                    "!type": "fn() -> [string]"
                },
                "getValueByKey": {
                    "!type": "fn(key: ?) -> ?"
                },
                "getValues": {
                    "!type": "fn() -> [?]"
                },
                "remove": {
                    "!type": "fn(key: ?)"
                },
                "setValueByKey": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toObject": {
                    "!type": "fn() -> ?"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "HttpRequest": {
            "!type": "fn(?)",
            "prototype": {
                "accept": {
                    "!type": "[string]"
                },
                "body": {
                    "!type": "+Base64EncodedByteArray"
                },
                "clientInfo": {
                    "!type": "+ClientInfo"
                },
                "contentType": {
                    "!type": "string"
                },
                "extendedProperties": {
                    "!type": "+Hashtable"
                },
                "filenames": {
                    "!type": "[string]"
                },
                "files": {
                    "!type": "?"
                },
                "form": {
                    "!type": "?"
                },
                "headers": {
                    "!type": "?"
                },
                "location": {
                    "!type": "string"
                },
                "method": {
                    "!type": "string"
                },
                "queryString": {
                    "!type": "?"
                },
                "rawUrl": {
                    "!type": "string"
                },
                "referrer": {
                    "!type": "string"
                },
                "referrerLocation": {
                    "!type": "string"
                },
                "restUrl": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "userAgent": {
                    "!type": "string"
                },
                "getBodyObject": {
                    "!type": "fn() -> ?"
                },
                "getBodyString": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "HttpResponse": {
            "!type": "fn(?)",
            "prototype": {
                "autoDetectContentType": {
                    "!type": "bool"
                },
                "body": {
                    "!type": "string"
                },
                "contentType": {
                    "!type": "string"
                },
                "expires": {
                    "!type": "number"
                },
                "isRaw": {
                    "!type": "bool"
                },
                "lastModified": {
                    "!type": "+Date"
                },
                "redirectLocation": {
                    "!type": "string"
                },
                "statusCode": {
                    "!type": "number"
                },
                "getHeaders": {
                    "!type": "fn() -> ?"
                },
                "setHeader": {
                    "!type": "fn(name: string, value: string) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "iCalendar": {
            "!type": "fn(?)",
            "prototype": {
                "name": {
                    "!type": "string"
                },
                "addLocalTimeZone": {
                    "!type": "fn() -> +TimeZone"
                },
                "createEvent": {
                    "!type": "fn() -> +Event"
                },
                "createTodo": {
                    "!type": "fn() -> +Todo"
                },
                "getBytes": {
                    "!type": "fn(fileName: ?) -> +Base64EncodedByteArray"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "iCalendarCollection": {
            "!type": "fn(?)",
            "prototype": {
                "toArray": {
                    "!type": "fn() -> [+iCalendar]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "JsonDocument": {
            "!type": "fn(?)",
            "prototype": {
                "data": {
                    "!type": "?"
                },
                "documentId": {
                    "!type": "string"
                },
                "metadata": {
                    "!type": "?"
                },
                "toJSON": {
                    "!type": "fn(thisObject: ?, key: string) -> ?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "MultiFieldQueryParserQuery": {
            "!type": "fn(?)",
            "prototype": {
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "NumericRangeQueryInstance`1[Double]": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "NumericRangeQueryInstance`1[Int32]": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "NumericRangeQueryInstance`1[Single]": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "ODataQuery": {
            "!type": "fn(?)",
            "prototype": {
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "OS": {
            "!type": "fn(?)",
            "prototype": {
                "family": {
                    "!type": "string"
                },
                "major": {
                    "!type": "string"
                },
                "minor": {
                    "!type": "string"
                },
                "patch": {
                    "!type": "string"
                },
                "patchMinor": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "PermissionsInfo": {
            "!type": "fn(?)",
            "prototype": {
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "principals": {
                    "!type": "[+PrincipalRoleInfo]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "PhraseQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "slop": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn(fieldName: string, text: string) -> +PhraseQuery"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "PrefixFilter": {
            "!type": "fn(?)",
            "prototype": {
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "PrefixQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Principal": {
            "!type": "fn(?)",
            "prototype": {
                "email": {
                    "!type": "string"
                },
                "loginName": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "type": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "PrincipalRoleInfo": {
            "!type": "fn(?)",
            "prototype": {
                "principal": {
                    "!type": "+Principal"
                },
                "roles": {
                    "!type": "[+Role]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "ProcessInstance": {
            "!type": "fn(?)",
            "prototype": {
                "description": {
                    "!type": "string"
                },
                "expectedDuration": {
                    "!type": "number"
                },
                "folder": {
                    "!type": "string"
                },
                "folio": {
                    "!type": "string"
                },
                "fullName": {
                    "!type": "string"
                },
                "guid": {
                    "!type": "string"
                },
                "Id": {
                    "!type": "number"
                },
                "metadata": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "priority": {
                    "!type": "number"
                },
                "startDate": {
                    "!type": "+Date"
                },
                "status": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "QueryParserQuery": {
            "!type": "fn(?)",
            "prototype": {
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "QueryWrapperFilter": {
            "!type": "fn(?)",
            "prototype": {
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "RegexQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SearchService": {
            "!type": "fn(?)",
            "prototype": {
                "defaultMaxResults": {
                    "!type": "number",
                    "!doc": "Gets or sets the maximum number of results to return. Initial value is 10000."
                },
                "indexName": {
                    "!type": "string",
                    "!doc": "Gets or sets the name of index."
                },
                "createBooleanQuery": {
                    "!type": "fn() -> +BooleanQuery"
                },
                "createDoubleRangeQuery": {
                    "!type": "fn(fieldName: string, min: ?, max: ?, minInclusive: bool, maxInclusive: bool) -> +NumericRangeQueryInstance`1[Double]"
                },
                "createFloatRangeQuery": {
                    "!type": "fn(fieldName: string, min: ?, max: ?, minInclusive: bool, maxInclusive: bool) -> +NumericRangeQueryInstance`1[Single]"
                },
                "createFuzzyQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +FuzzyQuery"
                },
                "createIntRangeQuery": {
                    "!type": "fn(fieldName: string, min: ?, max: ?, minInclusive: bool, maxInclusive: bool) -> +NumericRangeQueryInstance`1[Int32]"
                },
                "createMatchAllDocsQuery": {
                    "!type": "fn() -> +GenericQuery"
                },
                "createMultiFieldQueryParserQuery": {
                    "!type": "fn(fieldNames: [?], query: string) -> +MultiFieldQueryParserQuery"
                },
                "createODataQuery": {
                    "!type": "fn(query: string, defaultField: string, allowLeadingWildcard: bool) -> +ODataQuery"
                },
                "createPhraseQuery": {
                    "!type": "fn() -> +PhraseQuery"
                },
                "createPrefixFilter": {
                    "!type": "fn(fieldName: string, text: string) -> +PrefixFilter"
                },
                "createPrefixQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +PrefixQuery"
                },
                "createQueryParserQuery": {
                    "!type": "fn(query: string, defaultField: string, allowLeadingWildcard: bool) -> +QueryParserQuery"
                },
                "createQueryWrapperFilter": {
                    "!type": "fn(query: ?) -> +QueryWrapperFilter"
                },
                "createRegexQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +RegexQuery"
                },
                "createSort": {
                    "!type": "fn() -> +Sort"
                },
                "createTermQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +TermQuery"
                },
                "createTermRangeQuery": {
                    "!type": "fn(fieldName: string, lowerTerm: string, upperTerm: string, includeLower: bool, includeUpper: bool) -> +TermRangeQuery"
                },
                "createTermsFilter": {
                    "!type": "fn(fieldName: ?, text: ?) -> +TermsFilter"
                },
                "createWildcardQuery": {
                    "!type": "fn(fieldName: string, text: string) -> +WildcardQuery"
                },
                "deleteAllDocuments": {
                    "!type": "fn()"
                },
                "deleteDocuments": {
                    "!type": "fn(documentIds: ?)"
                },
                "doesIndexExist": {
                    "!type": "fn() -> bool"
                },
                "explain": {
                    "!type": "fn(query: ?, docId: ?) -> +Explanation"
                },
                "facetedSearch": {
                    "!type": "fn(query: ?, maxResults: ?, groupByFields: ?) -> [+FacetedSearchResult]"
                },
                "getFieldNames": {
                    "!type": "fn() -> [string]"
                },
                "highlight": {
                    "!type": "fn(query: ?, docId: ?, fieldName: ?, fragCharSize: ?) -> string"
                },
                "index": {
                    "!type": "fn(documentObject: ?) -> +SearchService"
                },
                "retrieve": {
                    "!type": "fn(documentId: string) -> +JsonDocument"
                },
                "search": {
                    "!type": "fn(query: ?, maxResults: ?) -> [+SearchResult]"
                },
                "searchResultCount": {
                    "!type": "fn(query: ?, maxResults: ?) -> number"
                },
                "setFieldOptions": {
                    "!type": "fn(fieldOptions: ?) -> +SearchService"
                },
                "shutdown": {
                    "!type": "fn() -> +SearchService"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Sort": {
            "!type": "fn(?)",
            "prototype": {
                "addSortField": {
                    "!type": "fn(fieldName: string, reverse: ?, fieldType: ?) -> +Sort",
                    "!doc": "Adds a sort field to the sort object"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            },
            "!doc": "Represents a sort operator used with the Search function of the Barista Search Index bundle"
        },
        "SPAlert": {
            "!type": "fn(?)",
            "prototype": {
                "alertFrequency": {
                    "!type": "string"
                },
                "alertTemplate": {
                    "!type": "+SPAlertTemplate"
                },
                "alertTemplateName": {
                    "!type": "string"
                },
                "alertTime": {
                    "!type": "+Date"
                },
                "alertType": {
                    "!type": "string"
                },
                "alwaysNotify": {
                    "!type": "bool"
                },
                "deliveryChannels": {
                    "!type": "string"
                },
                "dynamicRecipient": {
                    "!type": "string"
                },
                "eventType": {
                    "!type": "string"
                },
                "eventTypeBitmask": {
                    "!type": "number"
                },
                "filter": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "itemId": {
                    "!type": "number"
                },
                "listId": {
                    "!type": "+Guid"
                },
                "listUrl": {
                    "!type": "string"
                },
                "matchId": {
                    "!type": "+Guid"
                },
                "propertyBag": {
                    "!type": "+SPPropertyBag"
                },
                "status": {
                    "!type": "string"
                },
                "title": {
                    "!type": "string"
                },
                "user": {
                    "!type": "+SPUser"
                },
                "userId": {
                    "!type": "number"
                },
                "getList": {
                    "!type": "fn() -> +SPList"
                },
                "getListItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "setList": {
                    "!type": "fn(list: +SPList)"
                },
                "setListItem": {
                    "!type": "fn(listItem: +SPListItem)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn(bSendMail: ?)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPAlertCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn() -> +SPAlert"
                },
                "deleteById": {
                    "!type": "fn(id: ?)"
                },
                "deleteByIndex": {
                    "!type": "fn(index: number)"
                },
                "getAlertById": {
                    "!type": "fn(id: ?) -> +SPAlert"
                },
                "getAlertByIndex": {
                    "!type": "fn(index: number) -> +SPAlert"
                },
                "getSite": {
                    "!type": "fn() -> +SPSite"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toArray": {
                    "!type": "fn() -> [+SPAlert]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPAlertTemplate": {
            "!type": "fn(?)",
            "prototype": {
                "displayName": {
                    "!type": "string"
                },
                "id": {
                    "!type": "?"
                },
                "name": {
                    "!type": "string"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "status": {
                    "!type": "string"
                },
                "xml": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getLocalizedXml": {
                    "!type": "fn(lcid: number) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPAlternateUrl": {
            "!type": "fn(?)",
            "prototype": {
                "contextUri": {
                    "!type": "+Uri"
                },
                "incomingUrl": {
                    "!type": "string"
                },
                "uri": {
                    "!type": "+Uri"
                },
                "urlZone": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPAlternateUrlCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getAlternateUrlByIndex": {
                    "!type": "fn(index: number) -> +SPAlternateUrl"
                },
                "getAlternateUrlByName": {
                    "!type": "fn(incomingUrl: string) -> +SPAlternateUrl"
                },
                "getAlternateUrlByUrl": {
                    "!type": "fn(url: ?) -> +SPAlternateUrl"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPApplicationPool": {
            "!type": "fn(?)",
            "prototype": {
                "displayName": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "isCredentialDeploymentEnabled": {
                    "!type": "bool"
                },
                "isCredentialUpdateEnabled": {
                    "!type": "bool"
                },
                "managedAccount": {
                    "!type": "+SPManagedAccount"
                },
                "name": {
                    "!type": "string"
                },
                "typeName": {
                    "!type": "string"
                },
                "username": {
                    "!type": "string"
                },
                "version": {
                    "!type": "number"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deploy": {
                    "!type": "fn()"
                },
                "getFarm": {
                    "!type": "fn() -> +SPFarm"
                },
                "provision": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "uncache": {
                    "!type": "fn()"
                },
                "unprovision": {
                    "!type": "fn()"
                },
                "update": {
                    "!type": "fn(ensure: ?)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPAudit": {
            "!type": "fn(?)",
            "prototype": {
                "auditFlags": {
                    "!type": "?"
                },
                "effectiveAuditMask": {
                    "!type": "?"
                },
                "useAuditFlagCache": {
                    "!type": "?"
                },
                "deleteEntries": {
                    "!type": "fn(deleteEndDate: +Date) -> number"
                },
                "getEntries": {
                    "!type": "fn(query: ?) -> +SPAuditEntryCollection"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "trimAuditLog": {
                    "!type": "fn(deleteEndDate: +Date)"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "writeAuditEvent": {
                    "!type": "fn(eventName: string, eventSource: string, arg3: ?, arg4: ?) -> bool"
                },
                "writeAuditEvent2": {
                    "!type": "fn(eventType: string, eventSource: string, xmlData: string) -> bool"
                },
                "writeAuditEventUnlimitedData": {
                    "!type": "fn(eventName: string, eventSource: string, xmlData: string) -> bool"
                },
                "writeAuditEventUnlimitedData2": {
                    "!type": "fn(eventType: string, eventSource: string, xmlData: string) -> bool"
                }
            }
        },
        "SPAuditEntry": {
            "!type": "fn(?)",
            "prototype": {
                "docLocation": {
                    "!type": "string"
                },
                "event": {
                    "!type": "string"
                },
                "eventData": {
                    "!type": "string"
                },
                "eventName": {
                    "!type": "string"
                },
                "eventSource": {
                    "!type": "string"
                },
                "itemId": {
                    "!type": "+Guid"
                },
                "itemType": {
                    "!type": "string"
                },
                "locationType": {
                    "!type": "string"
                },
                "machineIp": {
                    "!type": "string"
                },
                "machineName": {
                    "!type": "string"
                },
                "occurred": {
                    "!type": "+Date"
                },
                "siteId": {
                    "!type": "+Guid"
                },
                "sourceName": {
                    "!type": "string"
                },
                "userId": {
                    "!type": "number"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPAuditEntryCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getEntryByIndex": {
                    "!type": "fn(index: number) -> +SPAuditEntry"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPCheckedOutFile": {
            "!type": "fn(?)",
            "prototype": {
                "checkedOutBy": {
                    "!type": "+SPUser"
                },
                "checkedOutByEmail": {
                    "!type": "string"
                },
                "checkedOutById": {
                    "!type": "number"
                },
                "checkedOutByName": {
                    "!type": "string"
                },
                "dirName": {
                    "!type": "string"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "leafName": {
                    "!type": "string"
                },
                "length": {
                    "!type": "number"
                },
                "listItemId": {
                    "!type": "number"
                },
                "timeLastModified": {
                    "!type": "+Date"
                },
                "url": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "takeOverCheckOut": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPCheckedOutFilesList": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(item: +SPCheckedOutFile)"
                },
                "clear": {
                    "!type": "fn()"
                },
                "contains": {
                    "!type": "fn(item: +SPCheckedOutFile) -> bool"
                },
                "getItemByIndex": {
                    "!type": "fn(index: number) -> +SPCheckedOutFile"
                },
                "indexOf": {
                    "!type": "fn(item: +SPCheckedOutFile) -> number"
                },
                "insert": {
                    "!type": "fn(index: number, item: +SPCheckedOutFile)"
                },
                "remove": {
                    "!type": "fn(item: +SPCheckedOutFile) -> bool"
                },
                "removeAt": {
                    "!type": "fn(index: number)"
                },
                "reverse": {
                    "!type": "fn()"
                },
                "setItemByIndex": {
                    "!type": "fn(index: number, item: +SPCheckedOutFile)"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPContentDatabase": {
            "!type": "fn(?)",
            "prototype": {
                "canRenameOnRestore": {
                    "!type": "bool"
                },
                "canSelectForBackup": {
                    "!type": "bool"
                },
                "canSelectForRestore": {
                    "!type": "bool"
                },
                "canUpgrade": {
                    "!type": "bool"
                },
                "currentSiteCount": {
                    "!type": "number"
                },
                "databaseConnectionString": {
                    "!type": "string"
                },
                "diskSizeRequired": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "exists": {
                    "!type": "bool"
                },
                "existsInFarm": {
                    "!type": "bool"
                },
                "failoverServer": {
                    "!type": "+SPServer"
                },
                "id": {
                    "!type": "?"
                },
                "includeInVssBackup": {
                    "!type": "bool"
                },
                "isAttachedToFarm": {
                    "!type": "bool"
                },
                "isBackwardsCompatible": {
                    "!type": "?"
                },
                "isReadOnly": {
                    "!type": "bool"
                },
                "legacyDatabaseConnectionString": {
                    "!type": "string"
                },
                "maximumSiteCount": {
                    "!type": "number"
                },
                "name": {
                    "!type": "string"
                },
                "needsUpgrade": {
                    "!type": "bool"
                },
                "needsUpgradeIncludeChildren": {
                    "!type": "bool"
                },
                "normalizedDataSource": {
                    "!type": "string"
                },
                "password": {
                    "!type": "string"
                },
                "schemaVersionXml": {
                    "!type": "string"
                },
                "server": {
                    "!type": "string"
                },
                "snapshots": {
                    "!type": "?"
                },
                "status": {
                    "!type": "string"
                },
                "typeName": {
                    "!type": "string"
                },
                "userName": {
                    "!type": "string"
                },
                "warningSiteCount": {
                    "!type": "number"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getFarm": {
                    "!type": "fn() -> +SPFarm"
                },
                "getSites": {
                    "!type": "fn() -> +SPSiteCollection"
                },
                "getWebApplication": {
                    "!type": "fn() -> +SPWebApplication"
                },
                "invalidate": {
                    "!type": "fn()"
                },
                "move": {
                    "!type": "fn(destinationDatabase: +SPContentDatabase, sitesToMove: [?]) -> ?"
                },
                "repair": {
                    "!type": "fn(deleteCorruption: bool) -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn(ensure: ?)"
                },
                "upgrade": {
                    "!type": "fn(recursively: ?)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPContentDatabaseCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getContentDatabaseByGuid": {
                    "!type": "fn(id: ?) -> +SPContentDatabase"
                },
                "getContentDatabaseByIndex": {
                    "!type": "fn(index: number) -> +SPContentDatabase"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPContentType": {
            "!type": "fn(?)",
            "prototype": {
                "description": {
                    "!type": "string"
                },
                "displayFormTemplateName": {
                    "!type": "string"
                },
                "displayFormUrl": {
                    "!type": "string"
                },
                "documentTemplate": {
                    "!type": "string"
                },
                "documentTemplateUrl": {
                    "!type": "string"
                },
                "editFormTemplateName": {
                    "!type": "string"
                },
                "editFormUrl": {
                    "!type": "string"
                },
                "eventReceivers": {
                    "!type": "+SPEventReceiverDefinitionCollection"
                },
                "featureId": {
                    "!type": "string"
                },
                "fieldLinks": {
                    "!type": "+SPFieldLinkCollection"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "group": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "+SPContentTypeId"
                },
                "mobileDisplayFormUrl": {
                    "!type": "string"
                },
                "mobileEditFormUrl": {
                    "!type": "string"
                },
                "mobileNewFormUrl": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "newDocumentControl": {
                    "!type": "string"
                },
                "newFormTemplateName": {
                    "!type": "string"
                },
                "newFormUrl": {
                    "!type": "string"
                },
                "readOnly": {
                    "!type": "bool"
                },
                "requireClientRenderingOnNew": {
                    "!type": "bool"
                },
                "scope": {
                    "!type": "string"
                },
                "sealed": {
                    "!type": "bool"
                },
                "version": {
                    "!type": "number"
                },
                "workflowAssociations": {
                    "!type": "+SPWorkflowAssociationCollection"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getParent": {
                    "!type": "fn() -> +SPContentType"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getResourceFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "getSchemaXmlWithResourceTokens": {
                    "!type": "fn() -> string"
                },
                "setSchemaXmlWithResourceTokens": {
                    "!type": "fn(xml: string)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn(updateChildren: bool, throwOnSealedOrReadOnly: bool)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPContentTypeCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(contentType: +SPContentType) -> +SPContentType"
                },
                "bestMatch": {
                    "!type": "fn(contentTypeId: ?) -> +SPContentTypeId"
                },
                "delete": {
                    "!type": "fn(contentTypeId: ?)"
                },
                "getContentTypeById": {
                    "!type": "fn(contentTypeId: ?) -> +SPContentType"
                },
                "getContentTypeByIndex": {
                    "!type": "fn(index: number) -> +SPContentType"
                },
                "getContentTypeByName": {
                    "!type": "fn(name: string) -> +SPContentType"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPContentTypeId": {
            "!type": "fn(?)",
            "prototype": {
                "getParent": {
                    "!type": "fn() -> +SPContentTypeId"
                },
                "isChildOf": {
                    "!type": "fn(id: ?) -> bool"
                },
                "isParentOf": {
                    "!type": "fn(id: ?) -> bool"
                },
                "toJSON": {
                    "!type": "fn(thisObject: ?, key: string) -> ?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPContext": {
            "!type": "fn(?)",
            "prototype": {
                "list": {
                    "!type": "+SPList"
                },
                "listItem": {
                    "!type": "+SPListItem"
                },
                "serverVersion": {
                    "!type": "string"
                },
                "site": {
                    "!type": "+SPSite"
                },
                "view": {
                    "!type": "+SPView"
                },
                "web": {
                    "!type": "+SPWeb"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPDeletedSite": {
            "!type": "fn(?)",
            "prototype": {
                "databaseId": {
                    "!type": "+Guid"
                },
                "deletionTime": {
                    "!type": "+Date"
                },
                "path": {
                    "!type": "string"
                },
                "siteId": {
                    "!type": "+Guid"
                },
                "siteSubscriptionId": {
                    "!type": "+Guid"
                },
                "webApplicationId": {
                    "!type": "+Guid"
                },
                "delete": {
                    "!type": "fn()"
                },
                "restore": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPDeletedSiteCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "rowLimit": {
                    "!type": "number"
                },
                "getDeletedSiteByIndex": {
                    "!type": "fn(index: number) -> +SPDeletedSite"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPDocTemplate": {
            "!type": "fn(?)",
            "prototype": {
                "defaultTemplate": {
                    "!type": "bool"
                },
                "description": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "type": {
                    "!type": "number"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPDocTemplateCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getDocTemplateByIndex": {
                    "!type": "fn(index: number) -> +SPDocTemplate"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPDocumentLibrary": {
            "!type": "fn(?)",
            "prototype": {
                "allowContentTypes": {
                    "!type": "bool"
                },
                "allowDeletion": {
                    "!type": "bool"
                },
                "allowEveryoneViewItems": {
                    "!type": "bool"
                },
                "allowMultiResponses": {
                    "!type": "bool"
                },
                "allowRssFeeds": {
                    "!type": "bool"
                },
                "allRolesForCurrentUser": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "author": {
                    "!type": "+SPUser"
                },
                "baseTemplate": {
                    "!type": "number"
                },
                "browserFileHandling": {
                    "!type": "string"
                },
                "calculationOptions": {
                    "!type": "string"
                },
                "canReceiveEmail": {
                    "!type": "string"
                },
                "contentTypes": {
                    "!type": "+SPContentTypeCollection"
                },
                "contentTypesEnabled": {
                    "!type": "bool"
                },
                "created": {
                    "!type": "+Date"
                },
                "defaultDisplayFormUrl": {
                    "!type": "string"
                },
                "defaultEditFormUrl": {
                    "!type": "string"
                },
                "defaultItemOpen": {
                    "!type": "string"
                },
                "defaultItemOpenUseListSetting": {
                    "!type": "bool"
                },
                "defaultNewFormUrl": {
                    "!type": "string"
                },
                "defaultView": {
                    "!type": "+SPView"
                },
                "defaultViewUrl": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "direction": {
                    "!type": "string"
                },
                "disableGridEditing": {
                    "!type": "bool"
                },
                "documentTemplateUrl": {
                    "!type": "string"
                },
                "draftVersionVisibility": {
                    "!type": "string"
                },
                "emailAlias": {
                    "!type": "string"
                },
                "enableAssignToEmail": {
                    "!type": "bool"
                },
                "enableAttachments": {
                    "!type": "bool"
                },
                "enableDeployWithDependentList": {
                    "!type": "bool"
                },
                "enableFolderCreation": {
                    "!type": "bool"
                },
                "enableMinorVersions": {
                    "!type": "bool"
                },
                "enableModeration": {
                    "!type": "bool"
                },
                "enablePeopleSelector": {
                    "!type": "bool"
                },
                "enableResourceSelector": {
                    "!type": "bool"
                },
                "enableSchemaCaching": {
                    "!type": "bool"
                },
                "enableSyndication": {
                    "!type": "bool"
                },
                "enableThrottling": {
                    "!type": "bool"
                },
                "enableVersioning": {
                    "!type": "bool"
                },
                "enforceDataValidation": {
                    "!type": "bool"
                },
                "eventReceivers": {
                    "!type": "+SPEventReceiverDefinitionCollection"
                },
                "excludeFromOfflineClient": {
                    "!type": "bool"
                },
                "excludeFromTemplate": {
                    "!type": "bool"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "forceCheckout": {
                    "!type": "bool"
                },
                "hasExternalDataSource": {
                    "!type": "bool"
                },
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "+Guid"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "isApplicationList": {
                    "!type": "bool"
                },
                "isCatalog": {
                    "!type": "bool"
                },
                "isSiteAssetsLibrary": {
                    "!type": "?"
                },
                "isThrottled": {
                    "!type": "bool"
                },
                "itemCount": {
                    "!type": "number"
                },
                "items": {
                    "!type": "+SPListItemCollection"
                },
                "lastItemDeletedDate": {
                    "!type": "+Date"
                },
                "lastItemModifiedDate": {
                    "!type": "+Date"
                },
                "listViewWebPartKey": {
                    "!type": "string"
                },
                "majorVersionLimit": {
                    "!type": "number"
                },
                "majorWithMinorVersionsLimit": {
                    "!type": "number"
                },
                "multipleDataList": {
                    "!type": "bool"
                },
                "navigateForFormsPages": {
                    "!type": "bool"
                },
                "noCrawl": {
                    "!type": "bool"
                },
                "onQuickLaunch": {
                    "!type": "bool"
                },
                "ordered": {
                    "!type": "bool"
                },
                "parentWebUrl": {
                    "!type": "string"
                },
                "readSecurity": {
                    "!type": "number"
                },
                "requestAccessEnabled": {
                    "!type": "bool"
                },
                "restrictedTemplateList": {
                    "!type": "bool"
                },
                "roleAssignments": {
                    "!type": "+SPRoleAssignmentCollection"
                },
                "rootFolder": {
                    "!type": "+SPFolder"
                },
                "rootWebOnly": {
                    "!type": "bool"
                },
                "sendToLocationName": {
                    "!type": "string"
                },
                "sendToLocationUrl": {
                    "!type": "string"
                },
                "serverRelativeDocumentTemplateUrl": {
                    "!type": "string"
                },
                "serverTemplateCanCreateFolders": {
                    "!type": "bool"
                },
                "serverTemplateId": {
                    "!type": "?"
                },
                "showUser": {
                    "!type": "bool"
                },
                "templateFeatureId": {
                    "!type": "string"
                },
                "thumbnailsEnabled": {
                    "!type": "bool"
                },
                "thumbnailSize": {
                    "!type": "number"
                },
                "title": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "useFormsForDisplay": {
                    "!type": "?"
                },
                "validationFormula": {
                    "!type": "string"
                },
                "validationMessage": {
                    "!type": "string"
                },
                "version": {
                    "!type": "number"
                },
                "webImageHeight": {
                    "!type": "number"
                },
                "webImageWidth": {
                    "!type": "number"
                },
                "workflowAssociations": {
                    "!type": "+SPWorkflowAssociationCollection"
                },
                "writeSecurity": {
                    "!type": "number"
                },
                "addBaristaRemoteItemEventReceiver": {
                    "!type": "fn(eventReceiverType: string, targetUrl: string) -> +SPEventReceiverDefinition",
                    "!doc": "Adds an BaristaRemoteItemEventReceiver to the list. Note -- existing item event receivers are not modified. Update existing event receivers via the eventreceiver property"
                },
                "addContentType": {
                    "!type": "fn(contentType: ?) -> +SPContentType"
                },
                "addEventReceiver": {
                    "!type": "fn(eventReceiverType: string, assembly: string, className: string) -> +SPEventReceiverDefinition"
                },
                "addFile": {
                    "!type": "fn(url: string, data: ?, overwrite: ?) -> +SPFile"
                },
                "addGroup": {
                    "!type": "fn(group: ?, role: ?)"
                },
                "addItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "addItemToFolder": {
                    "!type": "fn(folderUrl: string) -> +SPListItem"
                },
                "addOrUpdateBaristaItemEventReceiver": {
                    "!type": "fn(eventReceiverType: string, code: ?) -> +SPEventReceiverDefinition",
                    "!doc": "Adds an BaristaItemEventReceiver to the list. Note -- existing item event receivers are not modified. Update existing event receivers via the eventreceiver property"
                },
                "addUser": {
                    "!type": "fn(user: ?, role: ?)"
                },
                "breakRoleInheritance": {
                    "!type": "fn(copyRoleAssignments: bool, clearSubscopes: ?)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(permissions: string) -> bool"
                },
                "ensureContentType": {
                    "!type": "fn(contentType: ?) -> +SPContentType"
                },
                "getCheckedOutFiles": {
                    "!type": "fn() -> +SPCheckedOutFilesList"
                },
                "getContentTypeById": {
                    "!type": "fn(contentType: ?) -> +SPContentType"
                },
                "getContentTypes": {
                    "!type": "fn() -> [+SPContentType]"
                },
                "getEventReceivers": {
                    "!type": "fn() -> [+SPEventReceiver]"
                },
                "getItemById": {
                    "!type": "fn(id: number) -> +SPListItem"
                },
                "getItems": {
                    "!type": "fn() -> [+SPListItem]"
                },
                "getItemsByQuery": {
                    "!type": "fn(query: ?) -> [+SPListItem]"
                },
                "getItemsByView": {
                    "!type": "fn(view: ?) -> [+SPListItem]"
                },
                "getItemsCollection": {
                    "!type": "fn(fields: ?) -> +SPListItemCollection"
                },
                "getItemsInFolder": {
                    "!type": "fn(view: +SPView, folder: +SPFolder) -> +SPListItemCollection"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "getViews": {
                    "!type": "fn() -> [+SPView]"
                },
                "recycle": {
                    "!type": "fn() -> string"
                },
                "removeContentType": {
                    "!type": "fn(contentType: ?)"
                },
                "removeGroup": {
                    "!type": "fn(group: ?)"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "resetRoleInheritance": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPDocumentSet": {
            "!type": "fn(?)",
            "prototype": {
                "item": {
                    "!type": "+SPListItem"
                },
                "welcomePageUrl": {
                    "!type": "string"
                },
                "export": {
                    "!type": "fn() -> +Base64EncodedByteArray"
                },
                "getContentType": {
                    "!type": "fn() -> +SPContentType"
                },
                "getFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getParentFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPEventReceiverDefinition": {
            "!type": "fn(?)",
            "prototype": {
                "assembly": {
                    "!type": "string"
                },
                "class": {
                    "!type": "string"
                },
                "contextCollectionId": {
                    "!type": "+Guid"
                },
                "contextEventType": {
                    "!type": "+Guid"
                },
                "contextId": {
                    "!type": "+Guid"
                },
                "contextItemId": {
                    "!type": "number"
                },
                "contextItemUrl": {
                    "!type": "string"
                },
                "contextObjectId": {
                    "!type": "+Guid"
                },
                "contextType": {
                    "!type": "+Guid"
                },
                "credential": {
                    "!type": "number"
                },
                "data": {
                    "!type": "string"
                },
                "filter": {
                    "!type": "string"
                },
                "hostId": {
                    "!type": "+Guid"
                },
                "hostType": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "name": {
                    "!type": "string"
                },
                "parentHostId": {
                    "!type": "+Guid"
                },
                "parentHostType": {
                    "!type": "string"
                },
                "sequenceNumber": {
                    "!type": "number"
                },
                "siteId": {
                    "!type": "+Guid"
                },
                "synchronization": {
                    "!type": "string"
                },
                "type": {
                    "!type": "string"
                },
                "webId": {
                    "!type": "+Guid"
                },
                "delete": {
                    "!type": "fn()"
                },
                "fireContextEvent": {
                    "!type": "fn(instance: +SPSite)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPEventReceiverDefinitionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "hostId": {
                    "!type": "+Guid"
                },
                "hostType": {
                    "!type": "string"
                },
                "itemId": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(id: ?, contextList: ?) -> +SPEventReceiverDefinition"
                },
                "add2": {
                    "!type": "fn(receiverType: string, assembly: string, className: string)"
                },
                "doesEventReceiverDefinitionExist": {
                    "!type": "fn(eventReceiverId: ?) -> bool"
                },
                "getEventReceiverById": {
                    "!type": "fn(eventReceiverId: ?) -> +SPEventReceiverDefinition"
                },
                "getEventReceiverByIndex": {
                    "!type": "fn(index: number) -> +SPEventReceiverDefinition"
                },
                "getSite": {
                    "!type": "fn() -> +SPSite"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFarm": {
            "!type": "fn(?)",
            "prototype": {
                "farmManagedAccounts": {
                    "!type": "+SPFarmManagedAccountCollection"
                },
                "featureDefinitions": {
                    "!type": "+SPFeatureDefinitionCollection"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "servers": {
                    "!type": "+SPServerCollection"
                },
                "services": {
                    "!type": "+SPServiceCollection"
                },
                "extractFarmSolutionByName": {
                    "!type": "fn(solutionName: string) -> +Base64EncodedByteArray"
                },
                "getFarmKeyValue": {
                    "!type": "fn(key: string) -> ?"
                },
                "getServiceApplicationById": {
                    "!type": "fn(id: ?) -> ?"
                },
                "setFarmKeyValue": {
                    "!type": "fn(key: string, value: ?)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFarmManagedAccountCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getManagedAccountByUserName": {
                    "!type": "fn(userName: string) -> +SPManagedAccount"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFeature": {
            "!type": "fn(?)",
            "prototype": {
                "definitionId": {
                    "!type": "string"
                },
                "featureDefinitionScope": {
                    "!type": "string"
                },
                "version": {
                    "!type": "string"
                },
                "getDefinition": {
                    "!type": "fn() -> +SPFeatureDefinition"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFeatureCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(featureId: ?, force: ?, featureDefinitionScope: ?)"
                },
                "getFeatureById": {
                    "!type": "fn(featureId: ?) -> +SPFeature"
                },
                "remove": {
                    "!type": "fn(featureId: ?, force: ?)"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFeatureDefinition": {
            "!type": "fn(?)",
            "prototype": {
                "activateOnDefault": {
                    "!type": "bool"
                },
                "activationDependencies": {
                    "!type": "[?]"
                },
                "alwaysForceInstall": {
                    "!type": "bool"
                },
                "autoActivateInCentralAdmin": {
                    "!type": "bool"
                },
                "defaultResourceFile": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "string"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "receiverAssembly": {
                    "!type": "string"
                },
                "receiverClass": {
                    "!type": "string"
                },
                "requireResources": {
                    "!type": "bool"
                },
                "rootDirectory": {
                    "!type": "string"
                },
                "scope": {
                    "!type": "string"
                },
                "solutionId": {
                    "!type": "string"
                },
                "status": {
                    "!type": "string"
                },
                "typeName": {
                    "!type": "string"
                },
                "uiVersion": {
                    "!type": "string"
                },
                "version": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFeatureDefinitionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "add": {
                    "!type": "fn(relativePathToFeatureManifest: string, solutionId: ?, force: ?) -> +SPFeatureDefinition"
                },
                "add2": {
                    "!type": "fn(featureDefinition: +SPFeatureDefinition) -> +SPFeatureDefinition"
                },
                "ensure": {
                    "!type": "fn(featureDefinition: +SPFeatureDefinition) -> +SPFeatureDefinition"
                },
                "getByFeatureId": {
                    "!type": "fn(featureId: ?) -> +SPFeatureDefinition"
                },
                "getByFeatureName": {
                    "!type": "fn(featureName: string) -> +SPFeatureDefinition"
                },
                "removeById": {
                    "!type": "fn(id: ?, force: ?)"
                },
                "removeByPath": {
                    "!type": "fn(pathToFeatureManifest: string, force: ?)"
                },
                "scanForFeatures": {
                    "!type": "fn(solutionId: ?, scanOnly: bool, force: bool) -> [?]"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPField": {
            "!type": "fn(?)",
            "prototype": {
                "aggregationFunction": {
                    "!type": "string"
                },
                "allowDeletion": {
                    "!type": "?"
                },
                "authoringInfo": {
                    "!type": "string"
                },
                "canBeDeleted": {
                    "!type": "bool"
                },
                "canBeDisplayedInEditForm": {
                    "!type": "bool"
                },
                "canToggleHidden": {
                    "!type": "bool"
                },
                "defaultFormula": {
                    "!type": "string"
                },
                "defaultListField": {
                    "!type": "bool"
                },
                "defaultValue": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "direction": {
                    "!type": "string"
                },
                "displaySize": {
                    "!type": "string"
                },
                "enforceUniqueValues": {
                    "!type": "bool"
                },
                "fieldReferences": {
                    "!type": "[?]"
                },
                "fieldTypeDefinition": {
                    "!type": "string"
                },
                "fieldValueType": {
                    "!type": "?"
                },
                "filterable": {
                    "!type": "bool"
                },
                "filterableNoRecurrence": {
                    "!type": "bool"
                },
                "fromBaseType": {
                    "!type": "bool"
                },
                "group": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "+Guid"
                },
                "imeMode": {
                    "!type": "string"
                },
                "indexable": {
                    "!type": "bool"
                },
                "indexed": {
                    "!type": "bool"
                },
                "internalName": {
                    "!type": "string"
                },
                "jumpToField": {
                    "!type": "string"
                },
                "linkToItem": {
                    "!type": "bool"
                },
                "listItemMenu": {
                    "!type": "bool"
                },
                "noCrawl": {
                    "!type": "bool"
                },
                "piAttribute": {
                    "!type": "string"
                },
                "piTarget": {
                    "!type": "string"
                },
                "primaryPIAttribute": {
                    "!type": "string"
                },
                "primaryPITarget": {
                    "!type": "string"
                },
                "pushChangesToLists": {
                    "!type": "bool"
                },
                "readOnlyField": {
                    "!type": "bool"
                },
                "relatedField": {
                    "!type": "string"
                },
                "reorderable": {
                    "!type": "bool"
                },
                "required": {
                    "!type": "bool"
                },
                "schemaXml": {
                    "!type": "string"
                },
                "schemaXmlWithResourceTokens": {
                    "!type": "string"
                },
                "scope": {
                    "!type": "string"
                },
                "sealed": {
                    "!type": "bool"
                },
                "showInDisplayForm": {
                    "!type": "?"
                },
                "showInEditForm": {
                    "!type": "?"
                },
                "showInListSettings": {
                    "!type": "?"
                },
                "showInNewForm": {
                    "!type": "?"
                },
                "showInVersionHistory": {
                    "!type": "bool"
                },
                "showInViewForms": {
                    "!type": "?"
                },
                "sortable": {
                    "!type": "bool"
                },
                "sourceId": {
                    "!type": "string"
                },
                "staticName": {
                    "!type": "string"
                },
                "title": {
                    "!type": "string"
                },
                "translationXml": {
                    "!type": "string"
                },
                "typeAsDisplayName": {
                    "!type": "string"
                },
                "typeAsString": {
                    "!type": "string"
                },
                "typeShortDescription": {
                    "!type": "string"
                },
                "usedInWebContentTypes": {
                    "!type": "bool"
                },
                "validationEcmaScript": {
                    "!type": "string"
                },
                "validationFormula": {
                    "!type": "string"
                },
                "validationMessage": {
                    "!type": "string"
                },
                "version": {
                    "!type": "number"
                },
                "xPath": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getFieldValueAsHtml": {
                    "!type": "fn(value: ?) -> string"
                },
                "getFieldValueAsText": {
                    "!type": "fn(value: ?) -> string"
                },
                "getFieldValueForEdit": {
                    "!type": "fn(value: ?) -> string"
                },
                "getProperty": {
                    "!type": "fn(propertyName: string) -> string"
                },
                "parseAndSetValue": {
                    "!type": "fn(listItem: +SPListItem, value: string)"
                },
                "setCustomProperty": {
                    "!type": "fn(propertyName: string, propertyValue: string)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn(pushChangesToLists: ?)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFieldCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "addDependentLookup": {
                    "!type": "fn(displayName: string, guid: ?) -> string"
                },
                "addField": {
                    "!type": "fn(field: +SPField) -> string"
                },
                "addFieldAsXml": {
                    "!type": "fn(xml: string) -> string"
                },
                "addLookup": {
                    "!type": "fn(displayName: string, lookupListId: ?, lookupWebId: ?, isRequired: ?) -> string"
                },
                "addNewField": {
                    "!type": "fn(displayName: string, fieldType: string, required: bool) -> string"
                },
                "addNewFieldEx": {
                    "!type": "fn(displayName: string, fieldType: string, required: bool, compactName: bool, choices: [?]) -> string"
                },
                "containsField": {
                    "!type": "fn(name: string) -> bool"
                },
                "containsFieldWithStaticName": {
                    "!type": "fn(staticName: string) -> bool"
                },
                "createNewField": {
                    "!type": "fn(typeName: string, displayName: string) -> +SPField"
                },
                "delete": {
                    "!type": "fn(name: string)"
                },
                "getField": {
                    "!type": "fn(name: string) -> +SPField"
                },
                "getFieldByDisplayName": {
                    "!type": "fn(displayName: string) -> +SPField"
                },
                "getFieldByGuid": {
                    "!type": "fn(guid: ?) -> +SPField"
                },
                "getFieldByIndex": {
                    "!type": "fn(index: number) -> +SPField"
                },
                "getFieldByInternalName": {
                    "!type": "fn(name: string) -> +SPField"
                },
                "getList": {
                    "!type": "fn() -> +SPList"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "tryGetFieldByStaticName": {
                    "!type": "fn(staticName: string) -> ?"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFieldLink": {
            "!type": "fn(?)",
            "prototype": {
                "aggregationFunction": {
                    "!type": "string"
                },
                "customization": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "Id": {
                    "!type": "+Guid"
                },
                "name": {
                    "!type": "string"
                },
                "piAttribute": {
                    "!type": "string"
                },
                "piTarget": {
                    "!type": "string"
                },
                "primaryPIAttribute": {
                    "!type": "string"
                },
                "primaryPITarget": {
                    "!type": "string"
                },
                "readOnly": {
                    "!type": "bool"
                },
                "required": {
                    "!type": "bool"
                },
                "schemaXml": {
                    "!type": "string"
                },
                "showInDisplayForm": {
                    "!type": "bool"
                },
                "xPath": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFieldLinkCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(fieldLink: +SPFieldLink)"
                },
                "delete": {
                    "!type": "fn(fieldLink: ?)"
                },
                "getFieldLinkById": {
                    "!type": "fn(id: ?) -> +SPFieldLink"
                },
                "getFieldLinkByIndex": {
                    "!type": "fn(index: number) -> +SPFieldLink"
                },
                "getFieldLinkByName": {
                    "!type": "fn(name: string) -> +SPFieldLink"
                },
                "reorder": {
                    "!type": "fn(internalNames: [?])"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFile": {
            "!type": "fn(?)",
            "prototype": {
                "allProperties": {
                    "!type": "?"
                },
                "author": {
                    "!type": "+SPUser",
                    "!doc": "Gets the author (original creator) of the file."
                },
                "checkedOutByUser": {
                    "!type": "+SPUser",
                    "!doc": "Gets the login name of the user who the file is checked out to."
                },
                "checkedOutDate": {
                    "!type": "+Date"
                },
                "checkInComment": {
                    "!type": "string"
                },
                "checkOutType": {
                    "!type": "string",
                    "!doc": "Gets the current level of check out of the file."
                },
                "contentType": {
                    "!type": "string"
                },
                "customizedPageStatus": {
                    "!type": "string"
                },
                "eTag": {
                    "!type": "string"
                },
                "eventReceivers": {
                    "!type": "+SPEventReceiverDefinitionCollection"
                },
                "exists": {
                    "!type": "bool",
                    "!doc": "Returns a value that indicates if the file exists."
                },
                "iconUrl": {
                    "!type": "string"
                },
                "inDocumentLibrary": {
                    "!type": "bool"
                },
                "isConvertedFile": {
                    "!type": "bool"
                },
                "isSharedAccessRequested": {
                    "!type": "bool"
                },
                "length": {
                    "!type": "number",
                    "!doc": "Gets size of the file in bytes."
                },
                "level": {
                    "!type": "string"
                },
                "listRelativeUrl": {
                    "!type": "string",
                    "!doc": "Gets the list relative url of the file"
                },
                "lockedByUser": {
                    "!type": "+SPUser"
                },
                "lockedDate": {
                    "!type": "+Date"
                },
                "lockExpires": {
                    "!type": "+Date"
                },
                "lockId": {
                    "!type": "string"
                },
                "lockType": {
                    "!type": "string"
                },
                "majorVersion": {
                    "!type": "number"
                },
                "minorVersion": {
                    "!type": "number"
                },
                "modifiedBy": {
                    "!type": "+SPUser"
                },
                "name": {
                    "!type": "string"
                },
                "parentFolderName": {
                    "!type": "string"
                },
                "progId": {
                    "!type": "string"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "requiresCheckout": {
                    "!type": "bool"
                },
                "serverRedirected": {
                    "!type": "bool"
                },
                "serverRelativeUrl": {
                    "!type": "string",
                    "!doc": "Gets the server relative url of the file"
                },
                "sourceLeafName": {
                    "!type": "string"
                },
                "sourceUIVersion": {
                    "!type": "number"
                },
                "timeCreated": {
                    "!type": "+Date"
                },
                "timeLastModified": {
                    "!type": "+Date"
                },
                "title": {
                    "!type": "string"
                },
                "uiVersion": {
                    "!type": "number"
                },
                "uiVersionLabel": {
                    "!type": "string"
                },
                "uniqueId": {
                    "!type": "+Guid"
                },
                "url": {
                    "!type": "string",
                    "!doc": "Gets the absolute url of the file"
                },
                "addProperty": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "approve": {
                    "!type": "fn(comment: string)",
                    "!doc": "Approves the file submitted for content approval with the specified comment."
                },
                "canOpenFile": {
                    "!type": "fn(checkCanGetFileSource: bool) -> string"
                },
                "checkIn": {
                    "!type": "fn(comment: string, checkInType: ?)",
                    "!doc": "Checks the file in. The first argument is a (string) comment, the second is an optional (string) value of one of these values: MajorCheckIn, MinorCheckIn, OverwriteCheckIn"
                },
                "checkOut": {
                    "!type": "fn()",
                    "!doc": "Sets the checkout state of the file as checked out to the current user."
                },
                "convert": {
                    "!type": "fn(options: ?) -> ?"
                },
                "convertLock": {
                    "!type": "fn(fromType: string, toType: string, fromLockId: string, toLockId: string, newTimeout: string)"
                },
                "copyTo": {
                    "!type": "fn(newUrl: string, overwrite: bool)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deleteAllPersonalizations": {
                    "!type": "fn(userId: number)"
                },
                "deleteAllPersonalizationsAllUsers": {
                    "!type": "fn()"
                },
                "deleteProperty": {
                    "!type": "fn(key: ?)"
                },
                "deny": {
                    "!type": "fn(comment: string)",
                    "!doc": "Denies approval for a file that was submitted for content approval."
                },
                "getConversionState": {
                    "!type": "fn(converterId: ?, workItemId: ?) -> string"
                },
                "getConvertedFile": {
                    "!type": "fn(converterId: ?) -> +SPFile"
                },
                "getDocumentLibrary": {
                    "!type": "fn() -> +SPList"
                },
                "getLimitedWebPartManager": {
                    "!type": "fn(personalizationScope: string) -> +SPLimitedWebPartManager"
                },
                "getListItem": {
                    "!type": "fn(fields: ?) -> +SPListItem"
                },
                "getListItemAllFields": {
                    "!type": "fn() -> +SPListItem"
                },
                "getParentFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getPermissions": {
                    "!type": "fn() -> +SPSecurableObject"
                },
                "getProperty": {
                    "!type": "fn(key: ?) -> ?"
                },
                "getVersionHistory": {
                    "!type": "fn() -> +SPFileVersionCollection"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "lock": {
                    "!type": "fn(lockType: string, lockId: string, timeout: string)"
                },
                "moveTo": {
                    "!type": "fn(newUrl: string, overwrite: bool)"
                },
                "openBinary": {
                    "!type": "fn(openOptions: string) -> +Base64EncodedByteArray",
                    "!doc": "Returns a Base-64 Encoded byte array of the contents of the file."
                },
                "publish": {
                    "!type": "fn(comment: string)",
                    "!doc": "Publishes the file."
                },
                "recycle": {
                    "!type": "fn() -> +Guid",
                    "!doc": "Moves the file to the recycle bin."
                },
                "refreshLock": {
                    "!type": "fn(lockId: string, timeout: string)"
                },
                "releaseLock": {
                    "!type": "fn(lockId: string)"
                },
                "removeSharedAccessRequest": {
                    "!type": "fn()"
                },
                "replaceLink": {
                    "!type": "fn(oldUrl: string, newUrl: string)"
                },
                "revertContentStream": {
                    "!type": "fn()"
                },
                "revertToLastApprovedVersion": {
                    "!type": "fn() -> string",
                    "!doc": ""
                },
                "saveBinary": {
                    "!type": "fn(data: +Base64EncodedByteArray)",
                    "!doc": "Updates the file with the contents of the specified argument."
                },
                "scheduleEnd": {
                    "!type": "fn(endDate: +Date)"
                },
                "scheduleStart": {
                    "!type": "fn(startDate: +Date, setModerationStatus: ?, approvalComment: ?)"
                },
                "setProperty": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "takeOffline": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "undoCheckOut": {
                    "!type": "fn()",
                    "!doc": "Un-checkouts the file."
                },
                "unPublish": {
                    "!type": "fn(comment: string)",
                    "!doc": "Unpublishes the file."
                },
                "update": {
                    "!type": "fn()",
                    "!doc": "Updates the file with any changes."
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFileCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn(args: [?]) -> +SPFile"
                },
                "delete": {
                    "!type": "fn(urlOfFile: string)"
                },
                "getFileByIndex": {
                    "!type": "fn(index: number) -> +SPFile"
                },
                "getFolderByUrl": {
                    "!type": "fn(urlOfFile: string) -> +SPFile"
                },
                "getParentFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFileVersionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "delete": {
                    "!type": "fn(index: number)"
                },
                "deleteAll": {
                    "!type": "fn()"
                },
                "deleteAllMinorVersions": {
                    "!type": "fn()"
                },
                "deleteByLabel": {
                    "!type": "fn(label: string)"
                },
                "getFile": {
                    "!type": "fn() -> +SPFile"
                },
                "getVersionFromId": {
                    "!type": "fn(id: number) -> ?"
                },
                "getVersionFromLabel": {
                    "!type": "fn(versionLabel: string) -> ?"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "recycleAll": {
                    "!type": "fn()"
                },
                "recycleAllMinorVersions": {
                    "!type": "fn()"
                },
                "restore": {
                    "!type": "fn(index: number)"
                },
                "restoreById": {
                    "!type": "fn(versionId: number, bypassSharedLockId: ?)"
                },
                "restoreByLabel": {
                    "!type": "fn(versionLabel: string)"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFolder": {
            "!type": "fn(?)",
            "prototype": {
                "allProperties": {
                    "!type": "?"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "containingDocumentLibrary": {
                    "!type": "+Guid"
                },
                "contentTypeOrder": {
                    "!type": "?"
                },
                "exists": {
                    "!type": "bool"
                },
                "files": {
                    "!type": "+SPFileCollection"
                },
                "itemCount": {
                    "!type": "?"
                },
                "name": {
                    "!type": "string"
                },
                "parentListId": {
                    "!type": "+Guid"
                },
                "progId": {
                    "!type": "?"
                },
                "propertyBag": {
                    "!type": "?"
                },
                "requiresCheckout": {
                    "!type": "bool"
                },
                "serverRelativeUrl": {
                    "!type": "string"
                },
                "subFolders": {
                    "!type": "+SPFolderCollection"
                },
                "uniqueContentTypeOrder": {
                    "!type": "?"
                },
                "uniqueId": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "welcomePage": {
                    "!type": "?"
                },
                "addDocumentSet": {
                    "!type": "fn(name: string, contentType: ?, properties: ?, provisionDefaultContent: bool) -> +SPDocumentSet"
                },
                "addFile": {
                    "!type": "fn(file: ?, overwrite: bool) -> +SPFile"
                },
                "addFileByUrl": {
                    "!type": "fn(url: string, data: ?, overwrite: bool) -> +SPFile"
                },
                "addProperty": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "addSubFolder": {
                    "!type": "fn(url: string) -> +SPFolder"
                },
                "copyTo": {
                    "!type": "fn(strNewUrl: string)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deleteProperty": {
                    "!type": "fn(key: ?)"
                },
                "deletePropertyBagValue": {
                    "!type": "fn(key: string)"
                },
                "diff": {
                    "!type": "fn(targetFolder: +SPFolder, recursive: ?) -> +DiffResult"
                },
                "diffWithZip": {
                    "!type": "fn(target: ?) -> +DiffResult"
                },
                "ensureSubFolderExists": {
                    "!type": "fn(folderName: string) -> +SPFolder"
                },
                "getDocumentLibrary": {
                    "!type": "fn() -> +SPDocumentLibrary"
                },
                "getFiles": {
                    "!type": "fn(recursive: ?) -> [+SPFile]"
                },
                "getItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "getItems": {
                    "!type": "fn(scope: ?) -> [?]"
                },
                "getParentFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getPermissions": {
                    "!type": "fn() -> +SPSecurableObject"
                },
                "getPropertyBagValue": {
                    "!type": "fn(key: ?) -> ?"
                },
                "getPropertyBagValueAsString": {
                    "!type": "fn(key: string) -> string"
                },
                "getSubFolders": {
                    "!type": "fn() -> [?]"
                },
                "moveTo": {
                    "!type": "fn(strNewUrl: string)"
                },
                "recycle": {
                    "!type": "fn() -> string"
                },
                "setProperty": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "setPropertyBagValue": {
                    "!type": "fn(key: string, value: string)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPFolderCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn(strUrl: string) -> +SPFolder"
                },
                "delete": {
                    "!type": "fn(strUrl: string)"
                },
                "getFolderByIndex": {
                    "!type": "fn(index: number) -> +SPFolder"
                },
                "getFolderByUrl": {
                    "!type": "fn(urlOfFolder: string) -> +SPFolder"
                },
                "getParentFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPGroup": {
            "!type": "fn(?)",
            "prototype": {
                "allowMembersEditMembership": {
                    "!type": "bool"
                },
                "allowRequestToJoinLeave": {
                    "!type": "bool"
                },
                "autoAcceptRequestToJoinLeave": {
                    "!type": "bool"
                },
                "canCurrentUserEditMembership": {
                    "!type": "bool"
                },
                "canCurrentUserManageGroup": {
                    "!type": "bool"
                },
                "canCurrentUserViewMembership": {
                    "!type": "bool"
                },
                "containsCurrentUser": {
                    "!type": "bool"
                },
                "description": {
                    "!type": "string"
                },
                "distributionGroupAlias": {
                    "!type": "string"
                },
                "distributionGroupEmail": {
                    "!type": "string"
                },
                "distributionGroupErrorMessage": {
                    "!type": "string"
                },
                "explicitlyContainsCurrentUser": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "number"
                },
                "loginName": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "onlyAllowMembersViewMembership": {
                    "!type": "bool"
                },
                "requestToJoinLeaveEmailSetting": {
                    "!type": "string"
                },
                "users": {
                    "!type": "+SPUserCollection"
                },
                "addUser": {
                    "!type": "fn(user: ?)"
                },
                "clearDistributionGroupErrorMessage": {
                    "!type": "fn()"
                },
                "createDistributionGroup": {
                    "!type": "fn(dlAlias: string)"
                },
                "deleteDistributionGroup": {
                    "!type": "fn()"
                },
                "getDistributionGroupArchives": {
                    "!type": "fn() -> [?]"
                },
                "getOwner": {
                    "!type": "fn() -> ?"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "isUserMemberOfGroup": {
                    "!type": "fn(user: ?) -> bool"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "renameDistributionGroup": {
                    "!type": "fn(newAlias: string)"
                },
                "resynchronizeDistributionGroup": {
                    "!type": "fn()"
                },
                "setDistributionGroupArchives": {
                    "!type": "fn(lists: [?])"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "updateDistributionGroupStatus": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPGroupCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getGroupById": {
                    "!type": "fn(id: number) -> +SPGroup"
                },
                "getGroupByIndex": {
                    "!type": "fn(index: number) -> +SPGroup"
                },
                "getGroupByName": {
                    "!type": "fn(name: string) -> +SPGroup"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPGroupList": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(item: +SPGroup)"
                },
                "clear": {
                    "!type": "fn()"
                },
                "contains": {
                    "!type": "fn(item: +SPGroup) -> bool"
                },
                "getItemByIndex": {
                    "!type": "fn(index: number) -> +SPGroup"
                },
                "indexOf": {
                    "!type": "fn(item: +SPGroup) -> number"
                },
                "insert": {
                    "!type": "fn(index: number, item: +SPGroup)"
                },
                "remove": {
                    "!type": "fn(item: +SPGroup) -> bool"
                },
                "removeAt": {
                    "!type": "fn(index: number)"
                },
                "reverse": {
                    "!type": "fn()"
                },
                "setItemByIndex": {
                    "!type": "fn(index: number, item: +SPGroup)"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPLimitedWebPartManager": {
            "!type": "fn(?)",
            "prototype": {
                "hasPersonalizedParts": {
                    "!type": "bool"
                },
                "scope": {
                    "!type": "string"
                },
                "serverRelativeUrl": {
                    "!type": "string"
                },
                "webPartConnections": {
                    "!type": "+SPWebPartConnectionCollection"
                },
                "webParts": {
                    "!type": "+SPWebPartConnectionCollection"
                },
                "addWebPart": {
                    "!type": "fn(webPart: +SPWebPart, zoneId: string, zoneIndex: number)"
                },
                "closeWebPart": {
                    "!type": "fn(webPart: +SPWebPart)"
                },
                "deleteWebPart": {
                    "!type": "fn(webPart: +SPWebPart)"
                },
                "disconnectWebParts": {
                    "!type": "fn(connection: +SPWebPartConnection)"
                },
                "exportWebPart": {
                    "!type": "fn(webPart: +SPWebPart) -> +Base64EncodedByteArray"
                },
                "getStorageKey": {
                    "!type": "fn(webPart: +SPWebPart) -> +Guid"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getZoneId": {
                    "!type": "fn(webPart: +SPWebPart) -> string"
                },
                "importWebPart": {
                    "!type": "fn(webPart: +Base64EncodedByteArray) -> +SPWebPart"
                },
                "moveWebPart": {
                    "!type": "fn(webPart: +SPWebPart, zoneId: string, zoneIndex: number)"
                },
                "openWebPart": {
                    "!type": "fn(webPart: +SPWebPart)"
                },
                "resetAllPersonalizationState": {
                    "!type": "fn()"
                },
                "resetPersonalizationState": {
                    "!type": "fn(webPart: +SPWebPart)"
                },
                "saveChanges": {
                    "!type": "fn(webPart: +SPWebPart)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPList": {
            "!type": "fn(?)",
            "prototype": {
                "allowContentTypes": {
                    "!type": "bool"
                },
                "allowDeletion": {
                    "!type": "bool"
                },
                "allowEveryoneViewItems": {
                    "!type": "bool"
                },
                "allowMultiResponses": {
                    "!type": "bool"
                },
                "allowRssFeeds": {
                    "!type": "bool"
                },
                "allRolesForCurrentUser": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "author": {
                    "!type": "+SPUser"
                },
                "baseTemplate": {
                    "!type": "number"
                },
                "browserFileHandling": {
                    "!type": "string"
                },
                "calculationOptions": {
                    "!type": "string"
                },
                "canReceiveEmail": {
                    "!type": "string"
                },
                "contentTypes": {
                    "!type": "+SPContentTypeCollection"
                },
                "contentTypesEnabled": {
                    "!type": "bool"
                },
                "created": {
                    "!type": "+Date"
                },
                "defaultDisplayFormUrl": {
                    "!type": "string"
                },
                "defaultEditFormUrl": {
                    "!type": "string"
                },
                "defaultItemOpen": {
                    "!type": "string"
                },
                "defaultItemOpenUseListSetting": {
                    "!type": "bool"
                },
                "defaultNewFormUrl": {
                    "!type": "string"
                },
                "defaultView": {
                    "!type": "+SPView"
                },
                "defaultViewUrl": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "direction": {
                    "!type": "string"
                },
                "disableGridEditing": {
                    "!type": "bool"
                },
                "draftVersionVisibility": {
                    "!type": "string"
                },
                "emailAlias": {
                    "!type": "string"
                },
                "enableAssignToEmail": {
                    "!type": "bool"
                },
                "enableAttachments": {
                    "!type": "bool"
                },
                "enableDeployWithDependentList": {
                    "!type": "bool"
                },
                "enableFolderCreation": {
                    "!type": "bool"
                },
                "enableMinorVersions": {
                    "!type": "bool"
                },
                "enableModeration": {
                    "!type": "bool"
                },
                "enablePeopleSelector": {
                    "!type": "bool"
                },
                "enableResourceSelector": {
                    "!type": "bool"
                },
                "enableSchemaCaching": {
                    "!type": "bool"
                },
                "enableSyndication": {
                    "!type": "bool"
                },
                "enableThrottling": {
                    "!type": "bool"
                },
                "enableVersioning": {
                    "!type": "bool"
                },
                "enforceDataValidation": {
                    "!type": "bool"
                },
                "eventReceivers": {
                    "!type": "+SPEventReceiverDefinitionCollection"
                },
                "excludeFromOfflineClient": {
                    "!type": "bool"
                },
                "excludeFromTemplate": {
                    "!type": "bool"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "forceCheckout": {
                    "!type": "bool"
                },
                "hasExternalDataSource": {
                    "!type": "bool"
                },
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "+Guid"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "isApplicationList": {
                    "!type": "bool"
                },
                "isSiteAssetsLibrary": {
                    "!type": "?"
                },
                "isThrottled": {
                    "!type": "bool"
                },
                "itemCount": {
                    "!type": "number"
                },
                "items": {
                    "!type": "+SPListItemCollection"
                },
                "lastItemDeletedDate": {
                    "!type": "+Date"
                },
                "lastItemModifiedDate": {
                    "!type": "+Date"
                },
                "listViewWebPartKey": {
                    "!type": "string"
                },
                "majorVersionLimit": {
                    "!type": "number"
                },
                "majorWithMinorVersionsLimit": {
                    "!type": "number"
                },
                "multipleDataList": {
                    "!type": "bool"
                },
                "navigateForFormsPages": {
                    "!type": "bool"
                },
                "noCrawl": {
                    "!type": "bool"
                },
                "onQuickLaunch": {
                    "!type": "bool"
                },
                "ordered": {
                    "!type": "bool"
                },
                "parentWebUrl": {
                    "!type": "string"
                },
                "readSecurity": {
                    "!type": "number"
                },
                "requestAccessEnabled": {
                    "!type": "bool"
                },
                "restrictedTemplateList": {
                    "!type": "bool"
                },
                "roleAssignments": {
                    "!type": "+SPRoleAssignmentCollection"
                },
                "rootFolder": {
                    "!type": "+SPFolder"
                },
                "rootWebOnly": {
                    "!type": "bool"
                },
                "sendToLocationName": {
                    "!type": "string"
                },
                "sendToLocationUrl": {
                    "!type": "string"
                },
                "serverTemplateCanCreateFolders": {
                    "!type": "bool"
                },
                "serverTemplateId": {
                    "!type": "?"
                },
                "showUser": {
                    "!type": "bool"
                },
                "templateFeatureId": {
                    "!type": "string"
                },
                "title": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "useFormsForDisplay": {
                    "!type": "?"
                },
                "validationFormula": {
                    "!type": "string"
                },
                "validationMessage": {
                    "!type": "string"
                },
                "version": {
                    "!type": "number"
                },
                "workflowAssociations": {
                    "!type": "+SPWorkflowAssociationCollection"
                },
                "writeSecurity": {
                    "!type": "number"
                },
                "addBaristaRemoteItemEventReceiver": {
                    "!type": "fn(eventReceiverType: string, targetUrl: string) -> +SPEventReceiverDefinition",
                    "!doc": "Adds an BaristaRemoteItemEventReceiver to the list. Note -- existing item event receivers are not modified. Update existing event receivers via the eventreceiver property"
                },
                "addContentType": {
                    "!type": "fn(contentType: ?) -> +SPContentType"
                },
                "addEventReceiver": {
                    "!type": "fn(eventReceiverType: string, assembly: string, className: string) -> +SPEventReceiverDefinition"
                },
                "addFile": {
                    "!type": "fn(url: string, data: ?, overwrite: ?) -> +SPFile"
                },
                "addGroup": {
                    "!type": "fn(group: ?, role: ?)"
                },
                "addItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "addItemToFolder": {
                    "!type": "fn(folderUrl: string) -> +SPListItem"
                },
                "addOrUpdateBaristaItemEventReceiver": {
                    "!type": "fn(eventReceiverType: string, code: ?) -> +SPEventReceiverDefinition",
                    "!doc": "Adds an BaristaItemEventReceiver to the list. Note -- existing item event receivers are not modified. Update existing event receivers via the eventreceiver property"
                },
                "addUser": {
                    "!type": "fn(user: ?, role: ?)"
                },
                "breakRoleInheritance": {
                    "!type": "fn(copyRoleAssignments: bool, clearSubscopes: ?)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(permissions: string) -> bool"
                },
                "ensureContentType": {
                    "!type": "fn(contentType: ?) -> +SPContentType"
                },
                "getContentTypeById": {
                    "!type": "fn(contentType: ?) -> +SPContentType"
                },
                "getContentTypes": {
                    "!type": "fn() -> [+SPContentType]"
                },
                "getEventReceivers": {
                    "!type": "fn() -> [+SPEventReceiver]"
                },
                "getItemById": {
                    "!type": "fn(id: number) -> +SPListItem"
                },
                "getItems": {
                    "!type": "fn() -> [+SPListItem]"
                },
                "getItemsByQuery": {
                    "!type": "fn(query: ?) -> [+SPListItem]"
                },
                "getItemsByView": {
                    "!type": "fn(view: ?) -> [+SPListItem]"
                },
                "getItemsCollection": {
                    "!type": "fn(fields: ?) -> +SPListItemCollection"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "getViews": {
                    "!type": "fn() -> [+SPView]"
                },
                "recycle": {
                    "!type": "fn() -> string"
                },
                "removeContentType": {
                    "!type": "fn(contentType: ?)"
                },
                "removeGroup": {
                    "!type": "fn(group: ?)"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "resetRoleInheritance": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "includeMobileDefaultViewUrl": {
                    "!type": "bool"
                },
                "includeRootFolder": {
                    "!type": "bool"
                },
                "listsForCurrentUser": {
                    "!type": "bool"
                },
                "addList": {
                    "!type": "fn(listCreationInfo: ?) -> +SPList"
                },
                "delete": {
                    "!type": "fn(guid: ?)"
                },
                "ensureSiteAssetsLibrary": {
                    "!type": "fn() -> +SPList"
                },
                "ensureSitePagesLibrary": {
                    "!type": "fn() -> +SPList"
                },
                "getListByGuid": {
                    "!type": "fn(guid: ?, fetchMetadata: ?, fetchSecurityData: ?, fetchRelatedFields: ?) -> +SPList"
                },
                "getListByIndex": {
                    "!type": "fn(index: number) -> +SPList"
                },
                "getListByListName": {
                    "!type": "fn(listName: string) -> +SPList"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "tryGetList": {
                    "!type": "fn(listName: string) -> +SPList"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListItem": {
            "!type": "fn(?)",
            "prototype": {
                "allRolesForCurrentUser": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "attachments": {
                    "!type": "?"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "contentTypeId": {
                    "!type": "+SPContentTypeId"
                },
                "copySource": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "fieldValues": {
                    "!type": "?"
                },
                "fieldValuesAsHtml": {
                    "!type": "?"
                },
                "fieldValuesAsText": {
                    "!type": "?"
                },
                "fieldValuesForEdit": {
                    "!type": "?"
                },
                "fileSystemObjectType": {
                    "!type": "string"
                },
                "hasPublishedVersion": {
                    "!type": "bool"
                },
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "iconOverlay": {
                    "!type": "?"
                },
                "id": {
                    "!type": "number"
                },
                "level": {
                    "!type": "string"
                },
                "missingRequiredFields": {
                    "!type": "bool"
                },
                "moderationInformation": {
                    "!type": "+SPModerationInformation"
                },
                "name": {
                    "!type": "string"
                },
                "progId": {
                    "!type": "string"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "recurrenceId": {
                    "!type": "string"
                },
                "roleAssignments": {
                    "!type": "+SPRoleAssignmentCollection"
                },
                "serverRedirected": {
                    "!type": "bool"
                },
                "sortType": {
                    "!type": "string"
                },
                "tasks": {
                    "!type": "+SPWorkflowTaskCollection"
                },
                "title": {
                    "!type": "string"
                },
                "uniqueId": {
                    "!type": "+Guid"
                },
                "url": {
                    "!type": "string"
                },
                "workflows": {
                    "!type": "+SPWorkflowCollection"
                },
                "addGroup": {
                    "!type": "fn(group: ?, role: ?)"
                },
                "addUser": {
                    "!type": "fn(user: ?, role: ?)"
                },
                "breakRoleInheritance": {
                    "!type": "fn(copyRoleAssignments: bool, clearSubscopes: ?)"
                },
                "copyFrom": {
                    "!type": "fn(sourceUrl: string)"
                },
                "copyTo": {
                    "!type": "fn(destinationUrl: string)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(permissions: string) -> bool"
                },
                "ensureWorkflowInformation": {
                    "!type": "fn(retrieveAssociations: ?, retrieveWorkflows: ?)"
                },
                "getContentType": {
                    "!type": "fn() -> +SPContentType"
                },
                "getFile": {
                    "!type": "fn() -> +SPFile"
                },
                "getFileContentsAsJson": {
                    "!type": "fn() -> ?"
                },
                "getFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getFormattedValue": {
                    "!type": "fn(fieldName: string) -> string"
                },
                "getListItems": {
                    "!type": "fn() -> +SPListItemCollection"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "getProperty": {
                    "!type": "fn(key: string) -> ?"
                },
                "getVersions": {
                    "!type": "fn() -> +SPListItemVersionCollection"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "parseAndSetValue": {
                    "!type": "fn(fieldName: string, value: string)"
                },
                "recycle": {
                    "!type": "fn() -> string"
                },
                "removeGroup": {
                    "!type": "fn(group: ?)"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "replaceLink": {
                    "!type": "fn(oldUrl: string, newUrl: string)"
                },
                "resetRoleInheritance": {
                    "!type": "fn()"
                },
                "setFieldValue": {
                    "!type": "fn(fieldName: string, fieldValue: string)"
                },
                "setFieldValues": {
                    "!type": "fn(fieldValues: ?)"
                },
                "setProperty": {
                    "!type": "fn(key: string, value: ?)"
                },
                "setTaxonomyFieldValue": {
                    "!type": "fn(fieldName: string, fieldValue: +Term)"
                },
                "systemUpdate": {
                    "!type": "fn(incrementListItemVersion: bool)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "unlinkFromCopySource": {
                    "!type": "fn()"
                },
                "update": {
                    "!type": "fn()"
                },
                "updateOverwriteVersion": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListItemCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "listItemCollectionPosition": {
                    "!type": "+SPListItemCollectionPosition"
                },
                "numberOfFields": {
                    "!type": "number"
                },
                "queryFieldNames": {
                    "!type": "[string]"
                },
                "add": {
                    "!type": "fn() -> +SPListItem"
                },
                "add2": {
                    "!type": "fn(folderUrl: string, underlyingObjectType: string, leafName: ?) -> +SPListItem"
                },
                "beginLoadData": {
                    "!type": "fn()"
                },
                "delete": {
                    "!type": "fn(index: number)"
                },
                "deleteItemById": {
                    "!type": "fn(id: number)"
                },
                "endLoadData": {
                    "!type": "fn()"
                },
                "getItemByGuid": {
                    "!type": "fn(guid: ?) -> +SPListItem"
                },
                "getItemById": {
                    "!type": "fn(id: number) -> +SPListItem"
                },
                "getItemByIndex": {
                    "!type": "fn(index: number) -> +SPListItem"
                },
                "getList": {
                    "!type": "fn() -> +SPList"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "getXmlDataSchema": {
                    "!type": "fn() -> string"
                },
                "toArray": {
                    "!type": "fn() -> [+SPListItem]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListItemCollectionPosition": {
            "!type": "fn(?)",
            "prototype": {
                "pagingInfo": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListItemVersion": {
            "!type": "fn(?)",
            "prototype": {
                "created": {
                    "!type": "+Date"
                },
                "createdBy": {
                    "!type": "+SPUser"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "isCurrentVersion": {
                    "!type": "bool"
                },
                "level": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "versionId": {
                    "!type": "number"
                },
                "versionLabel": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getFieldValueByFieldName": {
                    "!type": "fn(fieldName: string) -> ?"
                },
                "getListItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "recycle": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListItemVersionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "deleteAll": {
                    "!type": "fn()"
                },
                "getListItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "getVersionFromId": {
                    "!type": "fn(versionId: number) -> +SPListItemVersion"
                },
                "getVersionFromIndex": {
                    "!type": "fn(index: number) -> +SPListItemVersion"
                },
                "getVersionFromLabel": {
                    "!type": "fn(versionLabel: string) -> +SPListItemVersion"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "recycleAll": {
                    "!type": "fn()"
                },
                "restore": {
                    "!type": "fn(index: number)"
                },
                "restoreById": {
                    "!type": "fn(id: number)"
                },
                "restoreByLabel": {
                    "!type": "fn(versionLabel: string)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListTemplate": {
            "!type": "fn(?)",
            "prototype": {
                "allowsFolderCreation": {
                    "!type": "bool"
                },
                "baseType": {
                    "!type": "string"
                },
                "categoryType": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "documentTemplate": {
                    "!type": "string"
                },
                "editPage": {
                    "!type": "string"
                },
                "featureId": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "internalName": {
                    "!type": "string"
                },
                "isCustomTemplate": {
                    "!type": "bool"
                },
                "name": {
                    "!type": "string"
                },
                "newPage": {
                    "!type": "string"
                },
                "onQuickLaunch": {
                    "!type": "bool"
                },
                "type": {
                    "!type": "string"
                },
                "type_Client": {
                    "!type": "number"
                },
                "unique": {
                    "!type": "bool"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPListTemplateCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getListTemplateByIndex": {
                    "!type": "fn(index: number) -> +SPListTemplate"
                },
                "getListTemplateByName": {
                    "!type": "fn(name: string) -> +SPListTemplate"
                },
                "getSchemaXml": {
                    "!type": "fn() -> string"
                },
                "toArray": {
                    "!type": "fn() -> [+SPListTemplate]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPManagedAccount": {
            "!type": "fn(?)",
            "prototype": {
                "automaticChange": {
                    "!type": "bool"
                },
                "canChangePassword": {
                    "!type": "bool"
                },
                "displayName": {
                    "!type": "string"
                },
                "enableEmailBeforePasswordChange": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "+Guid"
                },
                "passwordExpiration": {
                    "!type": "+Date"
                },
                "passwordLastChanged": {
                    "!type": "+Date"
                },
                "sid": {
                    "!type": "string"
                },
                "typeName": {
                    "!type": "string"
                },
                "username": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPModerationInformation": {
            "!type": "fn(?)",
            "prototype": {
                "comment": {
                    "!type": "string"
                },
                "status": {
                    "!type": "string",
                    "!doc": "Gets or sets the moderation status. Possible values are: Approved, Denied, Draft, Pending, Scheduled"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPMonitoredScope": {
            "!type": "fn(?)",
            "prototype": {
                "elapsedTime": {
                    "!type": "?"
                },
                "endTime": {
                    "!type": "?"
                },
                "id": {
                    "!type": "+Guid"
                },
                "name": {
                    "!type": "string"
                },
                "startTime": {
                    "!type": "?"
                },
                "dispose": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPNavigation": {
            "!type": "fn(?)",
            "prototype": {
                "globalNodes": {
                    "!type": "+SPNavigationNodeCollection"
                },
                "home": {
                    "!type": "+SPNavigationNode"
                },
                "quickLaunch": {
                    "!type": "+SPNavigationNodeCollection"
                },
                "topNavigationBar": {
                    "!type": "+SPNavigationNodeCollection"
                },
                "useShared": {
                    "!type": "bool"
                },
                "addToQuickLaunch": {
                    "!type": "fn(node: +SPNavigationNode, heading: string) -> +SPNavigationNode"
                },
                "getNodeById": {
                    "!type": "fn(id: number) -> +SPNavigationNode"
                },
                "getNodeByUrl": {
                    "!type": "fn(url: string) -> +SPNavigationNode"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPNavigationNode": {
            "!type": "fn(?)",
            "prototype": {
                "children": {
                    "!type": "+SPNavigationNodeCollection"
                },
                "id": {
                    "!type": "number"
                },
                "isExternal": {
                    "!type": "bool"
                },
                "isVisible": {
                    "!type": "bool"
                },
                "lastModified": {
                    "!type": "+Date"
                },
                "parentId": {
                    "!type": "number"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "targetParentObjectType": {
                    "!type": "string"
                },
                "targetSecurityScopeId": {
                    "!type": "+Guid"
                },
                "title": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getNavigation": {
                    "!type": "fn() -> +SPNavigation"
                },
                "getParent": {
                    "!type": "fn() -> +SPNavigationNode"
                },
                "move": {
                    "!type": "fn(collection: +SPNavigationNodeCollection, previousSibling: +SPNavigationNode)"
                },
                "moveToFirst": {
                    "!type": "fn(collection: +SPNavigationNodeCollection)"
                },
                "moveToLast": {
                    "!type": "fn(collection: +SPNavigationNodeCollection)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPNavigationNodeCollection": {
            "!type": "fn(?)",
            "prototype": {
                "add": {
                    "!type": "fn(node: +SPNavigationNode, previousNode: +SPNavigationNode) -> +SPNavigationNode"
                },
                "addAsFirst": {
                    "!type": "fn(node: +SPNavigationNode) -> +SPNavigationNode"
                },
                "addAsLast": {
                    "!type": "fn(node: +SPNavigationNode) -> +SPNavigationNode"
                },
                "delete": {
                    "!type": "fn(node: +SPNavigationNode)"
                },
                "getNavigation": {
                    "!type": "fn() -> +SPNavigation"
                },
                "getNavigationNodeByIndex": {
                    "!type": "fn(index: number) -> +SPNavigationNode"
                },
                "getParent": {
                    "!type": "fn() -> +SPNavigationNode"
                },
                "toArray": {
                    "!type": "fn() -> [+SPNavigationNote]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPPrincipal": {
            "!type": "fn(?)",
            "prototype": {
                "id": {
                    "!type": "number"
                },
                "loginName": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPPropertyBag": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "isSynchronized": {
                    "!type": "bool"
                },
                "add": {
                    "!type": "fn(key: string, value: string)"
                },
                "containsKey": {
                    "!type": "fn(key: string) -> bool"
                },
                "containsValue": {
                    "!type": "fn(value: string) -> bool"
                },
                "getKeys": {
                    "!type": "fn() -> [string]"
                },
                "getValueByKey": {
                    "!type": "fn(key: string) -> string"
                },
                "getValues": {
                    "!type": "fn() -> [string]"
                },
                "remove": {
                    "!type": "fn(key: string)"
                },
                "setValueByKey": {
                    "!type": "fn(key: string, value: string)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toObject": {
                    "!type": "fn() -> ?"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPQuota": {
            "!type": "fn(?)",
            "prototype": {
                "invitedUserMaximumLevel": {
                    "!type": "number"
                },
                "quotaId": {
                    "!type": "number"
                },
                "storageMaximumLevel": {
                    "!type": "number"
                },
                "storageWarningLevel": {
                    "!type": "number"
                },
                "userCodeMaximumLevel": {
                    "!type": "number"
                },
                "userCodeWarningLevel": {
                    "!type": "number"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRecycleBinItem": {
            "!type": "fn(?)",
            "prototype": {
                "author": {
                    "!type": "+SPUser"
                },
                "authorEmail": {
                    "!type": "string"
                },
                "authorId": {
                    "!type": "number"
                },
                "authorName": {
                    "!type": "string"
                },
                "deletedBy": {
                    "!type": "+SPUser"
                },
                "deletedByEmail": {
                    "!type": "string"
                },
                "deletedById": {
                    "!type": "number"
                },
                "deletedByName": {
                    "!type": "string"
                },
                "deletedDate": {
                    "!type": "+Date"
                },
                "dirName": {
                    "!type": "string",
                    "!doc": "Gets the site-relative url of the list or folder that originally contained the item."
                },
                "id": {
                    "!type": "string"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "itemState": {
                    "!type": "string"
                },
                "itemType": {
                    "!type": "string"
                },
                "leafName": {
                    "!type": "string"
                },
                "progId": {
                    "!type": "string"
                },
                "size": {
                    "!type": "number"
                },
                "title": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "moveToSecondStage": {
                    "!type": "fn()"
                },
                "restore": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRecycleBinItemCollection": {
            "!type": "fn(?)",
            "prototype": {
                "binType": {
                    "!type": "string"
                },
                "count": {
                    "!type": "?"
                },
                "lastProcessedId": {
                    "!type": "string"
                },
                "deleteAll": {
                    "!type": "fn()"
                },
                "getItemById": {
                    "!type": "fn(id: ?) -> +SPRecycleBinItem"
                },
                "moveAllToSecondStage": {
                    "!type": "fn()"
                },
                "restoreAll": {
                    "!type": "fn()"
                },
                "toArray": {
                    "!type": "fn() -> [+SPRecycleBinItem]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRoleAssignment": {
            "!type": "fn(?)",
            "prototype": {
                "member": {
                    "!type": "?"
                },
                "roleDefinitionBindings": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "getParentSecurableObject": {
                    "!type": "fn() -> +SPSecurableObject"
                },
                "importRoleDefinitionBindings": {
                    "!type": "fn(roleDefinitionBindings: +SPRoleDefinitionBindingCollection)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRoleAssignmentCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "id": {
                    "!type": "+Guid"
                },
                "addPrincipal": {
                    "!type": "fn(principal: +SPPrincipal)"
                },
                "addRoleAssignment": {
                    "!type": "fn(roleAssignment: +SPRoleAssignment)"
                },
                "addToCurrentScopeOnly": {
                    "!type": "fn(roleAssignment: +SPRoleAssignment)"
                },
                "getAssignmentByPrincipal": {
                    "!type": "fn(principal: +SPPrincipal) -> +SPRoleAssignment"
                },
                "getParentSecurableObject": {
                    "!type": "fn() -> +SPSecurableObject"
                },
                "getRoleAssignmentByIndex": {
                    "!type": "fn(index: number) -> +SPRoleAssignment"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "removeById": {
                    "!type": "fn(id: number)"
                },
                "removeByIndex": {
                    "!type": "fn(index: number)"
                },
                "removeFromCurrentScopeOnly": {
                    "!type": "fn(principal: +SPPrincipal)"
                },
                "removePrincipal": {
                    "!type": "fn(principal: +SPPrincipal)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPRoleAssignment]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRoleDefinition": {
            "!type": "fn(?)",
            "prototype": {
                "basePermissions": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "number"
                },
                "name": {
                    "!type": "string"
                },
                "order": {
                    "!type": "number"
                },
                "type": {
                    "!type": "string",
                    "!doc": "Gets the type of the role definition."
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRoleDefinitionBindingCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(roleDefinition: +SPRoleDefinition)"
                },
                "contains": {
                    "!type": "fn(roleDefinition: +SPRoleDefinition) -> bool"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "removeAll": {
                    "!type": "fn(index: number)"
                },
                "removeByIndex": {
                    "!type": "fn(index: number)"
                },
                "removeRoleDefinition": {
                    "!type": "fn(roleDefinition: +SPRoleDefinition)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPRoleDefinition]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPRoleDefinitionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(role: +SPRoleDefinition)"
                },
                "breakInheritance": {
                    "!type": "fn(copyRoleDefinitions: bool, keepRoleAssignments: bool)"
                },
                "deleteById": {
                    "!type": "fn(id: number)"
                },
                "deleteByIndex": {
                    "!type": "fn(index: number)"
                },
                "deleteByRoleName": {
                    "!type": "fn(roleName: string)"
                },
                "getById": {
                    "!type": "fn(id: number) -> +SPRoleDefinition"
                },
                "getByType": {
                    "!type": "fn(roleType: string) -> +SPRoleDefinition"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "toArray": {
                    "!type": "fn() -> [+SPRoleDefinition]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPSecurableObject": {
            "!type": "fn(?)",
            "prototype": {
                "allRolesForCurrentUser": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "roleAssignments": {
                    "!type": "+SPRoleAssignmentCollection"
                },
                "addGroup": {
                    "!type": "fn(group: ?, role: ?)"
                },
                "addUser": {
                    "!type": "fn(user: ?, role: ?)"
                },
                "breakRoleInheritance": {
                    "!type": "fn(copyRoleAssignments: bool, clearSubscopes: ?)"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(permissions: string) -> bool"
                },
                "removeGroup": {
                    "!type": "fn(group: ?)"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "resetRoleInheritance": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPSecureStore": {
            "!type": "fn(?)",
            "prototype": {
                "callAsUser": {
                    "!type": "fn(applicationId: string, function: fn(), thisObj: ?, arguments: [?]) -> ?",
                    "!doc": "Calls the specified function using the credentials stored under the specified application id."
                },
                "evalAsUser": {
                    "!type": "fn(applicationId: string, script: string) -> ?",
                    "!doc": "Evaluates the specified script using the credentials stored under the specified application id."
                },
                "execAsUser": {
                    "!type": "fn(applicationId: string, script: string)",
                    "!doc": "Executes the specified script using the credentials stored under the specified application id."
                },
                "getCredential": {
                    "!type": "fn(applicationId: string) -> ?",
                    "!doc": "Gets a credential object that represents the credentials stored with the specified application id."
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPServer": {
            "!type": "fn(?)",
            "prototype": {
                "address": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "role": {
                    "!type": "string"
                },
                "getServiceInstances": {
                    "!type": "fn() -> [+SPService]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPServerCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getServerById": {
                    "!type": "fn(id: ?) -> +SPServer"
                },
                "getServerByName": {
                    "!type": "fn(name: string) -> +SPServer"
                },
                "toArray": {
                    "!type": "fn() -> [+SPServer]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPService": {
            "!type": "fn(?)",
            "prototype": {
                "applications": {
                    "!type": "+SPServiceApplicationCollection"
                },
                "displayName": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "instances": {
                    "!type": "+SPServiceInstanceDependencyCollection"
                },
                "name": {
                    "!type": "string"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "status": {
                    "!type": "string"
                },
                "typeName": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPServiceApplicationCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getServiceApplicationById": {
                    "!type": "fn(id: ?) -> ?"
                },
                "getServiceApplicationByName": {
                    "!type": "fn(name: string) -> ?"
                },
                "toArray": {
                    "!type": "fn() -> [?]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPServiceCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getServiceById": {
                    "!type": "fn(id: ?) -> +SPService"
                },
                "getServiceByName": {
                    "!type": "fn(name: string) -> +SPService"
                },
                "toArray": {
                    "!type": "fn() -> [+SPService]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPServiceInstance": {
            "!type": "fn(?)",
            "prototype": {
                "description": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "+Guid"
                },
                "manageLinkUrl": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "provisionLinkUrl": {
                    "!type": "string"
                },
                "server": {
                    "!type": "?"
                },
                "status": {
                    "!type": "string"
                },
                "systemService": {
                    "!type": "bool"
                },
                "typeName": {
                    "!type": "string"
                },
                "unprovisionLinkUrl": {
                    "!type": "string"
                },
                "version": {
                    "!type": "number"
                },
                "getFarm": {
                    "!type": "fn() -> +SPFarm"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPServiceInstanceDependencyCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getServiceInstanceByGuid": {
                    "!type": "fn(id: ?) -> +SPServiceInstance"
                },
                "getServiceInstanceByName": {
                    "!type": "fn(name: string) -> +SPServiceInstance"
                },
                "toArray": {
                    "!type": "fn() -> [+SPService]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPSite": {
            "!type": "fn(?)",
            "prototype": {
                "allowDesigner": {
                    "!type": "bool"
                },
                "allowMasterPageEditing": {
                    "!type": "bool"
                },
                "allowRevertFromTemplate": {
                    "!type": "bool"
                },
                "allowRssFeeds": {
                    "!type": "bool"
                },
                "allowUnsafeUpdates": {
                    "!type": "bool"
                },
                "allWebs": {
                    "!type": "?"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "auditLogTrimmingCallout": {
                    "!type": "string"
                },
                "auditLogTrimmingRetention": {
                    "!type": "number"
                },
                "averageResourceUsage": {
                    "!type": "number"
                },
                "browserDocumentsEnabled": {
                    "!type": "bool"
                },
                "catchAccessDeniedException": {
                    "!type": "bool"
                },
                "certificationDate": {
                    "!type": "+Date"
                },
                "contentDatabase": {
                    "!type": "+SPContentDatabase"
                },
                "currentResourceUsage": {
                    "!type": "number"
                },
                "deadWebNotificationCount": {
                    "!type": "number"
                },
                "eventReceivers": {
                    "!type": "+SPEventReceiverDefinitionCollection"
                },
                "featureDefinitions": {
                    "!type": "+SPFeatureDefinitionCollection"
                },
                "features": {
                    "!type": "+SPFeatureCollection"
                },
                "hostHeaderIsSiteName": {
                    "!type": "bool"
                },
                "hostName": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "iisAllowsAnonymous": {
                    "!type": "bool"
                },
                "impersonating": {
                    "!type": "bool"
                },
                "lastContentModifiedDate": {
                    "!type": "+Date"
                },
                "lastSecurityModifiedDate": {
                    "!type": "+Date"
                },
                "lockIssue": {
                    "!type": "string"
                },
                "maxItemsPerThrottledOperation": {
                    "!type": "string"
                },
                "owner": {
                    "!type": "?"
                },
                "port": {
                    "!type": "number"
                },
                "portalName": {
                    "!type": "string"
                },
                "portalUrl": {
                    "!type": "string"
                },
                "protocol": {
                    "!type": "string"
                },
                "quota": {
                    "!type": "+SPQuota"
                },
                "readLocked": {
                    "!type": "bool"
                },
                "readOnly": {
                    "!type": "bool"
                },
                "recycleBin": {
                    "!type": "+SPRecycleBinItemCollection"
                },
                "resourceQuotaExceeded": {
                    "!type": "bool"
                },
                "resourceQuotaExceededNotificationSent": {
                    "!type": "bool"
                },
                "resourceQuotaWarningNotificationSent": {
                    "!type": "bool"
                },
                "rootWeb": {
                    "!type": "+SPWeb"
                },
                "searchServiceInstance": {
                    "!type": "+SPServiceInstance"
                },
                "secondaryContact": {
                    "!type": "?"
                },
                "serverRelativeUrl": {
                    "!type": "string"
                },
                "showUrlStructure": {
                    "!type": "bool"
                },
                "solutions": {
                    "!type": "+SPUserSolutionCollection"
                },
                "syndicationEnabled": {
                    "!type": "bool"
                },
                "systemAccount": {
                    "!type": "+SPUser"
                },
                "trimAuditLog": {
                    "!type": "bool"
                },
                "uiVersionConfigurationEnabled": {
                    "!type": "bool"
                },
                "url": {
                    "!type": "string"
                },
                "usage": {
                    "!type": "?"
                },
                "userCodeEnabled": {
                    "!type": "?"
                },
                "userCustomActions": {
                    "!type": "+SPUserCustomActionCollection"
                },
                "userDefinedWorkflowsEnabled": {
                    "!type": "bool"
                },
                "userToken": {
                    "!type": "+SPUserToken"
                },
                "warningNotificationSent": {
                    "!type": "bool"
                },
                "workflowManager": {
                    "!type": "+SPWorkflowManager"
                },
                "writeLocked": {
                    "!type": "bool"
                },
                "zone": {
                    "!type": "?"
                },
                "activateFeature": {
                    "!type": "fn(feature: ?, force: ?) -> +SPFeature"
                },
                "confirmUsage": {
                    "!type": "fn() -> bool"
                },
                "createWeb": {
                    "!type": "fn(webCreationInfo: ?) -> +SPWeb"
                },
                "deactivateFeature": {
                    "!type": "fn(feature: ?)"
                },
                "delete": {
                    "!type": "fn(deleteADAccounts: ?, gradualDelete: ?)"
                },
                "dispose": {
                    "!type": "fn()"
                },
                "fileExists": {
                    "!type": "fn(fileUrl: string) -> bool",
                    "!doc": "Returns a value that indicates if a file exists at the specified url."
                },
                "getAllWebs": {
                    "!type": "fn() -> [+SPWeb]"
                },
                "getCatalog": {
                    "!type": "fn(typeCatalog: string) -> +SPList"
                },
                "getContentDatabase": {
                    "!type": "fn() -> +SPContentDatabase"
                },
                "getFeatureDefinitions": {
                    "!type": "fn() -> [+SPFeatureDefinition]"
                },
                "getPermissions": {
                    "!type": "fn() -> +SPSecurableObject"
                },
                "getRecycleBin": {
                    "!type": "fn() -> +SPRecycleBinItemCollection"
                },
                "getTaxonomySession": {
                    "!type": "fn() -> +TaxonomySession"
                },
                "getUsageInfo": {
                    "!type": "fn() -> +UsageInfo"
                },
                "getWebApplication": {
                    "!type": "fn() -> +SPWebApplication"
                },
                "getWebTemplates": {
                    "!type": "fn(language: ?) -> [+SPWebTemplate]"
                },
                "loadFileAsByteArray": {
                    "!type": "fn(fileUrl: string) -> +Base64EncodedByteArray",
                    "!doc": "Loads the file at the specified url as a byte array."
                },
                "loadFileAsString": {
                    "!type": "fn(fileUrl: string) -> string",
                    "!doc": "Loads the file at the specified url as a string."
                },
                "makeFullUrl": {
                    "!type": "fn(strUrl: string) -> string"
                },
                "openWeb": {
                    "!type": "fn(url: ?, requireExactUrl: ?) -> +SPWeb"
                },
                "openWebById": {
                    "!type": "fn(guid: ?) -> +SPWeb"
                },
                "recalculateStorageUsed": {
                    "!type": "fn()"
                },
                "refreshEmailEnabledObjects": {
                    "!type": "fn()"
                },
                "rename": {
                    "!type": "fn(uri: ?)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "updateValidationKey": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "visualUpgradeWebs": {
                    "!type": "fn()"
                },
                "write": {
                    "!type": "fn(fileUrl: string, contents: ?) -> +SPFile",
                    "!doc": "Writes the specified contents to the file located at the specified url"
                }
            }
        },
        "SPSiteCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "names": {
                    "!type": "[string]"
                },
                "add": {
                    "!type": "fn(siteUrl: string, ownerLogin: string, ownerEmail: string) -> +SPSite"
                },
                "backup": {
                    "!type": "fn(siteUrl: string, fileName: string, overwrite: bool)"
                },
                "delete": {
                    "!type": "fn(siteUrl: string, deleteADAccounts: ?, gradualDelete: ?)"
                },
                "getSiteByIndex": {
                    "!type": "fn(index: number) -> +SPSite"
                },
                "getSiteByName": {
                    "!type": "fn(name: string) -> +SPSite"
                },
                "getWebApplication": {
                    "!type": "fn() -> +SPWebApplication"
                },
                "restore": {
                    "!type": "fn(siteUrl: string, fileName: string, overwrite: bool, hostHeaderAsSiteName: ?)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPSite]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPSiteDataQuery": {
            "!type": "fn(?)",
            "prototype": {
                "lists": {
                    "!type": "string"
                },
                "query": {
                    "!type": "string"
                },
                "queryThrottleMode": {
                    "!type": "string"
                },
                "rowLimit": {
                    "!type": "string"
                },
                "viewFields": {
                    "!type": "string"
                },
                "webs": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUser": {
            "!type": "fn(?)",
            "prototype": {
                "alerts": {
                    "!type": "+SPAlertCollection"
                },
                "email": {
                    "!type": "string"
                },
                "groups": {
                    "!type": "+SPGroupCollection"
                },
                "id": {
                    "!type": "number"
                },
                "isApplicationPrincipal": {
                    "!type": "bool"
                },
                "isDomainGroup": {
                    "!type": "bool"
                },
                "isSiteAdmin": {
                    "!type": "bool"
                },
                "isSiteAuditor": {
                    "!type": "bool"
                },
                "loginName": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "notes": {
                    "!type": "string"
                },
                "sid": {
                    "!type": "string"
                },
                "userToken": {
                    "!type": "+SPUserToken"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUserCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn(loginName: string, email: string, name: string, notes: string)"
                },
                "getSchemaXmlEx": {
                    "!type": "fn() -> string"
                },
                "getUserByEmail": {
                    "!type": "fn(emailAddress: string) -> +SPUser"
                },
                "getUserById": {
                    "!type": "fn(id: number) -> +SPUser"
                },
                "getUserByIndex": {
                    "!type": "fn(index: number) -> +SPUser"
                },
                "getUserByLogonName": {
                    "!type": "fn(loginName: string) -> +SPUser"
                },
                "getUsersByLogonName": {
                    "!type": "fn(loginNames: [?]) -> +SPUserCollection"
                },
                "getViewSchemaXmlEx": {
                    "!type": "fn() -> string"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "getXmlEx": {
                    "!type": "fn() -> string"
                },
                "remove": {
                    "!type": "fn(user: ?)"
                },
                "removeById": {
                    "!type": "fn(loginNames: [?])"
                },
                "removeUsersByLoginName": {
                    "!type": "fn(id: number)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPUser]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUserCustomAction": {
            "!type": "fn(?)",
            "prototype": {
                "commandUIExtension": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "group": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "location": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "registrationId": {
                    "!type": "string"
                },
                "registrationType": {
                    "!type": "string"
                },
                "rights": {
                    "!type": "string"
                },
                "scope": {
                    "!type": "string"
                },
                "scriptBlock": {
                    "!type": "string"
                },
                "scriptSrc": {
                    "!type": "string"
                },
                "sequence": {
                    "!type": "number"
                },
                "title": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "versionOfUserCustomAction": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUserCustomActionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "scope": {
                    "!type": "string"
                },
                "add": {
                    "!type": "fn() -> +SPUserCustomAction"
                },
                "clear": {
                    "!type": "fn()"
                },
                "getUserCustomActionById": {
                    "!type": "fn(id: ?) -> +SPUserCustomAction"
                },
                "toArray": {
                    "!type": "fn() -> [+SPUserCustomAction]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUserSolution": {
            "!type": "fn(?)",
            "prototype": {
                "hasAssemblies": {
                    "!type": "bool"
                },
                "name": {
                    "!type": "string"
                },
                "signature": {
                    "!type": "string"
                },
                "solutionId": {
                    "!type": "+Guid"
                },
                "status": {
                    "!type": "string"
                },
                "dispose": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUserSolutionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(solutionGalleryItemId: number) -> +SPUserSolution"
                },
                "getBySolutionId": {
                    "!type": "fn(solutionId: ?) -> +SPUserSolution"
                },
                "remove": {
                    "!type": "fn(solution: +SPUserSolution)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPUserSolution]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPUserToken": {
            "!type": "fn(?)",
            "prototype": {
                "binaryToken": {
                    "!type": "+Base64EncodedByteArray"
                },
                "compareUser": {
                    "!type": "fn(userTokenCheck: +SPUserToken) -> bool"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            },
            "systemAccount": {
                "!type": "+SPUserToken"
            }
        },
        "SPView": {
            "!type": "fn(?)",
            "prototype": {
                "aggregations": {
                    "!type": "string"
                },
                "aggregationsStatus": {
                    "!type": "string"
                },
                "baseViewId": {
                    "!type": "string"
                },
                "contentTypeId": {
                    "!type": "+SPContentTypeId"
                },
                "defaultView": {
                    "!type": "bool"
                },
                "defaultViewForContentType": {
                    "!type": "bool"
                },
                "editorModified": {
                    "!type": "bool"
                },
                "formats": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "string"
                },
                "imageUrl": {
                    "!type": "string"
                },
                "includeRootFolder": {
                    "!type": "bool"
                },
                "method": {
                    "!type": "string"
                },
                "mobileDefaultView": {
                    "!type": "bool"
                },
                "mobileView": {
                    "!type": "bool"
                },
                "moderationType": {
                    "!type": "string"
                },
                "orderedView": {
                    "!type": "bool"
                },
                "paged": {
                    "!type": "bool"
                },
                "personalView": {
                    "!type": "bool"
                },
                "readOnlyView": {
                    "!type": "bool"
                },
                "requiresClientIntegration": {
                    "!type": "bool"
                },
                "rowLimit": {
                    "!type": "string"
                },
                "scope": {
                    "!type": "string"
                },
                "serverRelativeUrl": {
                    "!type": "string"
                },
                "styleId": {
                    "!type": "string"
                },
                "threaded": {
                    "!type": "bool"
                },
                "title": {
                    "!type": "string"
                },
                "toolbar": {
                    "!type": "string"
                },
                "toolbarTemplateName": {
                    "!type": "string"
                },
                "viewData": {
                    "!type": "string"
                },
                "viewFields": {
                    "!type": "[string]"
                },
                "viewJoins": {
                    "!type": "string"
                },
                "viewProjectedFields": {
                    "!type": "string"
                },
                "viewQuery": {
                    "!type": "string"
                },
                "viewType": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getHtmlSchemaXml": {
                    "!type": "fn() -> string"
                },
                "renderAsHtml": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWeb": {
            "!type": "fn(webUrl: string) -> +SPWeb",
            "prototype": {
                "alerts": {
                    "!type": "+SPAlertCollection"
                },
                "allowAnonymousAccess": {
                    "!type": "bool"
                },
                "allowAutomaticAspxPageIndexing": {
                    "!type": "bool"
                },
                "allowDesignerForCurrentUser": {
                    "!type": "bool",
                    "!doc": "Gets a Boolean value that specifies whether the current user is allowed to use the designer for this website. The default value is false."
                },
                "allowMasterPageEditingForCurrentUser": {
                    "!type": "bool"
                },
                "allowRevertFromTemplateForCurrentUser": {
                    "!type": "bool"
                },
                "allowRssFeeds": {
                    "!type": "bool"
                },
                "allowUnsafeUpdates": {
                    "!type": "bool"
                },
                "allProperties": {
                    "!type": "+Hashtable"
                },
                "allRolesForCurrentUser": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "allUsers": {
                    "!type": "+SPUserCollection"
                },
                "allWebTemplatesAllowed": {
                    "!type": "bool"
                },
                "alternateCssUrl": {
                    "!type": "string"
                },
                "alternateHeader": {
                    "!type": "string"
                },
                "aspxPageIndexed": {
                    "!type": "bool"
                },
                "aspxPageIndexMode": {
                    "!type": "string"
                },
                "associatedGroups": {
                    "!type": "+SPGroupList"
                },
                "associatedMemberGroup": {
                    "!type": "+SPGroup"
                },
                "associatedOwnerGroup": {
                    "!type": "+SPGroup"
                },
                "associatedVisitorGroup": {
                    "!type": "+SPGroup"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "author": {
                    "!type": "+SPUser"
                },
                "availableContentTypes": {
                    "!type": "+SPContentTypeCollection"
                },
                "availableFields": {
                    "!type": "+SPFieldCollection"
                },
                "cacheAllSchema": {
                    "!type": "bool"
                },
                "clientTag": {
                    "!type": "number"
                },
                "configuration": {
                    "!type": "number"
                },
                "contentTypes": {
                    "!type": "+SPContentTypeCollection"
                },
                "created": {
                    "!type": "+Date"
                },
                "currencyLocaleId": {
                    "!type": "number"
                },
                "currentUser": {
                    "!type": "+SPUser"
                },
                "customJavaScriptFileUrl": {
                    "!type": "string"
                },
                "customMasterUrl": {
                    "!type": "string"
                },
                "customUploadPage": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "docTemplates": {
                    "!type": "+SPDocTemplateCollection"
                },
                "effectivePresenceEnabled": {
                    "!type": "bool"
                },
                "eventReceivers": {
                    "!type": "+SPEventReceiverDefinitionCollection"
                },
                "excludeFromOfflineClient": {
                    "!type": "bool"
                },
                "executeUrl": {
                    "!type": "string"
                },
                "exists": {
                    "!type": "bool"
                },
                "features": {
                    "!type": "+SPFeatureCollection"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "fileDialogPostProcessorId": {
                    "!type": "+Guid"
                },
                "files": {
                    "!type": "+SPFileCollection"
                },
                "folders": {
                    "!type": "+SPFolderCollection"
                },
                "groups": {
                    "!type": "+SPGroupCollection"
                },
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "hasUniqueRoleDefinitions": {
                    "!type": "bool"
                },
                "id": {
                    "!type": "string"
                },
                "includeSupportingFolders": {
                    "!type": "bool"
                },
                "isADAccountCreationMode": {
                    "!type": "bool"
                },
                "isADEmailEnabled": {
                    "!type": "bool"
                },
                "isMultilingual": {
                    "!type": "bool"
                },
                "isRootWeb": {
                    "!type": "bool"
                },
                "language": {
                    "!type": "string"
                },
                "lastItemModifiedDate": {
                    "!type": "+Date"
                },
                "lists": {
                    "!type": "+SPListCollection"
                },
                "listTemplates": {
                    "!type": "+SPListTemplateCollection"
                },
                "masterPageReferenceEnabled": {
                    "!type": "bool"
                },
                "masterUrl": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "navigation": {
                    "!type": "+SPNavigation"
                },
                "noCrawl": {
                    "!type": "bool"
                },
                "overwriteTranslationsOnChange": {
                    "!type": "bool"
                },
                "parentWebId": {
                    "!type": "+Guid"
                },
                "parserEnabled": {
                    "!type": "bool"
                },
                "portalMember": {
                    "!type": "bool"
                },
                "portalName": {
                    "!type": "string"
                },
                "portalSubscriptionUrl": {
                    "!type": "string"
                },
                "portalUrl": {
                    "!type": "string"
                },
                "presenceEnabled": {
                    "!type": "bool"
                },
                "propertyBag": {
                    "!type": "+SPPropertyBag"
                },
                "provisioned": {
                    "!type": "bool"
                },
                "quickLaunchEnabled": {
                    "!type": "bool"
                },
                "recycleBin": {
                    "!type": "+SPRecycleBinItemCollection"
                },
                "recycleBinEnabled": {
                    "!type": "bool"
                },
                "requestAccessEmail": {
                    "!type": "?"
                },
                "requestAccessEnabled": {
                    "!type": "bool"
                },
                "roleAssignments": {
                    "!type": "+SPRoleAssignmentCollection"
                },
                "roleDefinitions": {
                    "!type": "+SPRoleDefinitionCollection"
                },
                "rootFolder": {
                    "!type": "+SPFolder"
                },
                "serverRelativeUrl": {
                    "!type": "string"
                },
                "showUrlStructureForCurrentUser": {
                    "!type": "bool"
                },
                "siteAdministrators": {
                    "!type": "?"
                },
                "siteGroups": {
                    "!type": "+SPGroupCollection"
                },
                "siteLogoDescription": {
                    "!type": "string"
                },
                "siteLogoUrl": {
                    "!type": "string"
                },
                "siteUserInfoList": {
                    "!type": "+SPList"
                },
                "siteUsers": {
                    "!type": "+SPUserCollection"
                },
                "syndicationEnabled": {
                    "!type": "bool"
                },
                "theme": {
                    "!type": "string"
                },
                "themeCssFolderUrl": {
                    "!type": "string"
                },
                "themeCssUrl": {
                    "!type": "string"
                },
                "title": {
                    "!type": "string"
                },
                "treeViewEnabled": {
                    "!type": "bool"
                },
                "uiVersion": {
                    "!type": "number"
                },
                "uiVersionConfigurationEnabled": {
                    "!type": "bool"
                },
                "url": {
                    "!type": "string"
                },
                "userCustomActions": {
                    "!type": "+SPUserCustomActionCollection"
                },
                "userIsSiteAdmin": {
                    "!type": "bool"
                },
                "userIsWebAdmin": {
                    "!type": "bool"
                },
                "users": {
                    "!type": "+SPUserCollection"
                },
                "webs": {
                    "!type": "+SPWebCollection"
                },
                "webTemplate": {
                    "!type": "string"
                },
                "webTemplateId": {
                    "!type": "number"
                },
                "workflowAssociations": {
                    "!type": "+SPWorkflowAssociationCollection"
                },
                "workflows": {
                    "!type": "+SPWorkflowCollection"
                },
                "workflowTemplates": {
                    "!type": "+SPWorkflowTemplateCollection"
                },
                "activateFeature": {
                    "!type": "fn(feature: ?, force: ?) -> +SPFeature"
                },
                "addApplicationPrincipal": {
                    "!type": "fn(logonName: string, allowBrowseUerInfo: bool, requireRequestToken: bool) -> +SPUser"
                },
                "addFileByUrl": {
                    "!type": "fn(url: string, data: ?, overwrite: bool) -> +SPFile"
                },
                "addGroup": {
                    "!type": "fn(group: ?, role: ?)"
                },
                "addProperty": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "addUser": {
                    "!type": "fn(user: ?, role: ?)"
                },
                "allowAllWebTemplates": {
                    "!type": "fn()"
                },
                "applyTheme": {
                    "!type": "fn(newTheme: string)"
                },
                "applyWebTemplate": {
                    "!type": "fn(webTemplate: ?)"
                },
                "breakRoleInheritance": {
                    "!type": "fn(copyRoleAssignments: bool, clearSubscopes: ?)"
                },
                "close": {
                    "!type": "fn()"
                },
                "createDefaultAssociatedGroups": {
                    "!type": "fn(userLogin: string, userLogin2: string, groupNameSeed: string)"
                },
                "createList": {
                    "!type": "fn(listCreationInfo: ?) -> +SPList"
                },
                "customizeCss": {
                    "!type": "fn(cssFile: string)"
                },
                "deactivateFeature": {
                    "!type": "fn(feature: ?)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deleteFileIfExists": {
                    "!type": "fn(serverRelativeUrl: string) -> bool"
                },
                "deleteFolderIfExists": {
                    "!type": "fn(serverRelativeUrl: string) -> bool"
                },
                "deleteProperty": {
                    "!type": "fn(key: ?)"
                },
                "dispose": {
                    "!type": "fn()"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(permissions: string) -> bool"
                },
                "ensureUser": {
                    "!type": "fn(logonName: string) -> +SPUser"
                },
                "fileExists": {
                    "!type": "fn(fileUrl: string) -> bool",
                    "!doc": "Returns a value that indicates if a file exists at the specified url."
                },
                "getAvailableContentTypes": {
                    "!type": "fn() -> [+SPContentType]"
                },
                "getCatalog": {
                    "!type": "fn(typeCatalog: string) -> +SPList"
                },
                "getContentTypes": {
                    "!type": "fn() -> [+SPContentType]"
                },
                "getDocTemplates": {
                    "!type": "fn() -> [+SPDocTemplate]"
                },
                "getFileAsString": {
                    "!type": "fn(url: string) -> string"
                },
                "getFileById": {
                    "!type": "fn(id: ?) -> +SPFile"
                },
                "getFileByServerRelativeUrl": {
                    "!type": "fn(serverRelativeUrl: string) -> +SPFile"
                },
                "getFileOrFolderObject": {
                    "!type": "fn(strUrl: string) -> ?"
                },
                "getFiles": {
                    "!type": "fn() -> [+SPFile]"
                },
                "getFolderByServerRelativeUrl": {
                    "!type": "fn(serverRelativeUrl: string) -> +SPFolder"
                },
                "getFolders": {
                    "!type": "fn() -> [+SPFolder]"
                },
                "getLimitedWebPartManager": {
                    "!type": "fn(fullOrRelativeUrl: string, personalizationScope: string) -> +SPLimitedWebPartManager"
                },
                "getList": {
                    "!type": "fn(strUrl: string) -> +SPList"
                },
                "getListByServerRelativeUrl": {
                    "!type": "fn(serverRelativeUrl: string) -> ?"
                },
                "getListByTitle": {
                    "!type": "fn(listTitle: string) -> +SPList"
                },
                "getListFromUrl": {
                    "!type": "fn(strUrl: string) -> +SPList"
                },
                "getListFromWebPartPageUrl": {
                    "!type": "fn(pageUrl: string) -> +SPList"
                },
                "getListItem": {
                    "!type": "fn(strUrl: string) -> +SPListItem"
                },
                "getListItemFields": {
                    "!type": "fn(strUrl: string, fields: [?]) -> +SPListItem"
                },
                "getLists": {
                    "!type": "fn() -> [+SPList]"
                },
                "getListsOfType": {
                    "!type": "fn(baseType: string) -> +SPListCollection"
                },
                "getListTemplates": {
                    "!type": "fn() -> [+SPListTemplate]"
                },
                "getProperty": {
                    "!type": "fn(key: ?) -> ?"
                },
                "getSiteData": {
                    "!type": "fn(query: +SPSiteDataQuery) -> ?"
                },
                "getSiteUserInfoList": {
                    "!type": "fn() -> +SPList"
                },
                "getSubwebsForCurrentUser": {
                    "!type": "fn(webTemplateFilter: ?, configurationFilter: ?) -> +SPWebCollection"
                },
                "getUser": {
                    "!type": "fn(loginName: string) -> ?"
                },
                "getUserEffectivePermissions": {
                    "!type": "fn(userName: string) -> string"
                },
                "getUserToken": {
                    "!type": "fn(userName: string) -> +SPUserToken"
                },
                "getViewFromUrl": {
                    "!type": "fn(listUrl: string) -> +SPView"
                },
                "getWebs": {
                    "!type": "fn() -> [+SPWeb]"
                },
                "getWebsAndListsWithUniquePermissions": {
                    "!type": "fn() -> [+SPWebListInfo]"
                },
                "isCurrentUserMemberOfGroup": {
                    "!type": "fn(groupId: number) -> bool"
                },
                "loadFileAsByteArray": {
                    "!type": "fn(fileUrl: string) -> +Base64EncodedByteArray",
                    "!doc": "Loads the file at the specified url as a byte array."
                },
                "loadFileAsJSON": {
                    "!type": "fn(fileUrl: string) -> ?",
                    "!doc": "Loads the file at the specified url as a JSON Object."
                },
                "loadFileAsString": {
                    "!type": "fn(fileUrl: string) -> string",
                    "!doc": "Loads the file at the specified url as a string."
                },
                "mapToIcon": {
                    "!type": "fn(fileName: string, progId: string, iconSize: string) -> string"
                },
                "processBatchData": {
                    "!type": "fn(strBatchData: string) -> string"
                },
                "recalculateWebFineGrainedPermissions": {
                    "!type": "fn()"
                },
                "recycle": {
                    "!type": "fn()"
                },
                "removeGroup": {
                    "!type": "fn(group: ?)"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "resetRoleInheritance": {
                    "!type": "fn()"
                },
                "revertAllDocumentContentStreams": {
                    "!type": "fn()"
                },
                "revertCss": {
                    "!type": "fn(cssFile: string)"
                },
                "saveAsTemplate": {
                    "!type": "fn(strTemplateName: string, strTemplateTitle: string, strTemplateDescription: string, bSaveData: bool)"
                },
                "setProperty": {
                    "!type": "fn(key: ?, value: ?)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn()"
                },
                "validateFormDigest": {
                    "!type": "fn() -> bool"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                },
                "write": {
                    "!type": "fn(fileUrl: string, contents: ?) -> +SPFile",
                    "!doc": "Writes the specified contents to the file located at the specified url"
                }
            },
            "!doc": "Represents a SharePoint website."
        },
        "SPWebApplication": {
            "!type": "fn(?)",
            "prototype": {
                "alertsEnabled": {
                    "!type": "bool"
                },
                "alertsLimited": {
                    "!type": "bool"
                },
                "alertsMaximum": {
                    "!type": "number"
                },
                "alertsMaximumQuerySet": {
                    "!type": "number"
                },
                "allowAccessToWebPartCatalog": {
                    "!type": "bool"
                },
                "allowContributorsToEditScriptableParts": {
                    "!type": "bool"
                },
                "allowDesigner": {
                    "!type": "bool"
                },
                "allowedInlineDownloadedMimeTypes": {
                    "!type": "[string]"
                },
                "allowHighCharacterListFolderNames": {
                    "!type": "bool"
                },
                "allowMasterPageEditing": {
                    "!type": "bool"
                },
                "allowOMCodeOverrideThrottleSettings": {
                    "!type": "bool"
                },
                "allowPartToPartCommunication": {
                    "!type": "bool"
                },
                "allowRevertFromTemplate": {
                    "!type": "bool"
                },
                "allowSilverlightPrompt": {
                    "!type": "bool"
                },
                "alternateUrls": {
                    "!type": "+SPAlternateUrlCollection"
                },
                "alwaysProcessDocuments": {
                    "!type": "bool"
                },
                "automaticallyDeleteUnusedSiteCollections": {
                    "!type": "bool"
                },
                "browserCEIPEnabled": {
                    "!type": "bool"
                },
                "canRenameOnRestore": {
                    "!type": "bool"
                },
                "displayName": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "incomingEmailServerAddress": {
                    "!type": "string"
                },
                "maximumFileSize": {
                    "!type": "number"
                },
                "name": {
                    "!type": "string"
                },
                "outboundMailServiceInstance": {
                    "!type": "+SPServiceInstance"
                },
                "typeName": {
                    "!type": "string"
                },
                "useClaimsAuthentication": {
                    "!type": "bool"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getApplicationPool": {
                    "!type": "fn() -> +SPApplicationPool"
                },
                "getContentDatabases": {
                    "!type": "fn() -> +SPContentDatabaseCollection"
                },
                "getDeletedSites": {
                    "!type": "fn() -> +SPDeletedSiteCollection"
                },
                "getDeletedSitesByGuid": {
                    "!type": "fn(id: ?) -> +SPDeletedSiteCollection"
                },
                "getDeletedSitesByPath": {
                    "!type": "fn(sitePath: string) -> +SPDeletedSiteCollection"
                },
                "getFeatures": {
                    "!type": "fn() -> [+SPFeature]"
                },
                "getSites": {
                    "!type": "fn() -> +SPSiteCollection"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn(ensure: ?)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWebCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "?"
                },
                "names": {
                    "!type": "?"
                },
                "websInfo": {
                    "!type": "?"
                },
                "add": {
                    "!type": "fn(strWebUrl: string) -> +SPWeb"
                },
                "add2": {
                    "!type": "fn(strWebUrl: string, strTitle: string, strDescription: string, nLcid: number, webTemplate: ?, useUniquePermissions: bool, bConvertIfThere: bool) -> +SPWeb"
                },
                "delete": {
                    "!type": "fn(strWebUrl: string)"
                },
                "getWebByGuid": {
                    "!type": "fn(id: ?) -> +SPWeb"
                },
                "getWebByIndex": {
                    "!type": "fn(index: number) -> +SPWeb"
                },
                "getWebByName": {
                    "!type": "fn(name: string) -> +SPWeb"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWeb]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWebPart": {
            "!type": "fn(?)",
            "prototype": {
                "allowClose": {
                    "!type": "bool"
                },
                "allowConnect": {
                    "!type": "bool"
                },
                "allowEdit": {
                    "!type": "bool"
                },
                "allowHide": {
                    "!type": "bool"
                },
                "allowMinimize": {
                    "!type": "bool"
                },
                "allowZoneChange": {
                    "!type": "bool"
                },
                "authorizationFilter": {
                    "!type": "string"
                },
                "catalogIconImageUrl": {
                    "!type": "string"
                },
                "chromeState": {
                    "!type": "string"
                },
                "chromeType": {
                    "!type": "string"
                },
                "connectErrorMessage": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "displayTitle": {
                    "!type": "string"
                },
                "exportMode": {
                    "!type": "string"
                },
                "hasSharedData": {
                    "!type": "bool"
                },
                "hasUserData": {
                    "!type": "bool"
                },
                "height": {
                    "!type": "string"
                },
                "helpMode": {
                    "!type": "string"
                },
                "helpUrl": {
                    "!type": "string"
                },
                "hidden": {
                    "!type": "bool"
                },
                "importErrorMessage": {
                    "!type": "string"
                },
                "isClosed": {
                    "!type": "bool"
                },
                "isShared": {
                    "!type": "bool"
                },
                "isStandalone": {
                    "!type": "bool"
                },
                "isStatic": {
                    "!type": "bool"
                },
                "subtitle": {
                    "!type": "string"
                },
                "title": {
                    "!type": "string"
                },
                "titleIconImageUrl": {
                    "!type": "string"
                },
                "titleUrl": {
                    "!type": "string"
                },
                "width": {
                    "!type": "string"
                },
                "zone": {
                    "!type": "+WebPartZoneBase"
                },
                "zoneIndex": {
                    "!type": "number"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWebPartConnection": {
            "!type": "fn(?)",
            "prototype": {
                "consumer": {
                    "!type": "+SPWebPart"
                },
                "consumerConnectionPointId": {
                    "!type": "string"
                },
                "consumerId": {
                    "!type": "string"
                },
                "crossPageConnectionId": {
                    "!type": "string"
                },
                "crossPageSchema": {
                    "!type": "string"
                },
                "id": {
                    "!type": "string"
                },
                "isActive": {
                    "!type": "bool"
                },
                "isEnabled": {
                    "!type": "bool"
                },
                "isShared": {
                    "!type": "bool"
                },
                "isStatic": {
                    "!type": "bool"
                },
                "provider": {
                    "!type": "+SPWebPart"
                },
                "providerConnectionPointId": {
                    "!type": "string"
                },
                "providerID": {
                    "!type": "string"
                },
                "sourcePageUrl": {
                    "!type": "string"
                },
                "targetPageUrl": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWebPartConnectionCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "isReadOnly": {
                    "!type": "bool"
                },
                "add": {
                    "!type": "fn(value: +SPWebPartConnection) -> number"
                },
                "clear": {
                    "!type": "fn()"
                },
                "contains": {
                    "!type": "fn(value: +SPWebPartConnection) -> bool"
                },
                "getConnectionById": {
                    "!type": "fn(id: string) -> +SPWebPartConnection"
                },
                "getConnectionByIndex": {
                    "!type": "fn(index: number) -> +SPWebPartConnection"
                },
                "indexOf": {
                    "!type": "fn(value: +SPWebPartConnection) -> number"
                },
                "insert": {
                    "!type": "fn(index: number, value: +SPWebPartConnection)"
                },
                "remove": {
                    "!type": "fn(value: +SPWebPartConnection)"
                },
                "removeAt": {
                    "!type": "fn(index: number)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWebPartConnection]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflow": {
            "!type": "fn(?)",
            "prototype": {
                "associationId": {
                    "!type": "+Guid"
                },
                "author": {
                    "!type": "number"
                },
                "authorUser": {
                    "!type": "+SPUser"
                },
                "created": {
                    "!type": "+Date"
                },
                "hasNewEvents": {
                    "!type": "bool"
                },
                "historyListId": {
                    "!type": "+Guid"
                },
                "instanceId": {
                    "!type": "+Guid"
                },
                "internalState": {
                    "!type": "string"
                },
                "isCompleted": {
                    "!type": "bool"
                },
                "isLocked": {
                    "!type": "bool"
                },
                "itemGuid": {
                    "!type": "+Guid"
                },
                "itemId": {
                    "!type": "number"
                },
                "itemName": {
                    "!type": "string"
                },
                "listId": {
                    "!type": "+Guid"
                },
                "modifications": {
                    "!type": "+SPWorkflowModificationCollection"
                },
                "modified": {
                    "!type": "+Date"
                },
                "ownerUser": {
                    "!type": "+SPUser"
                },
                "siteId": {
                    "!type": "+Guid"
                },
                "statusUrl": {
                    "!type": "string"
                },
                "taskListId": {
                    "!type": "+Guid"
                },
                "tasks": {
                    "!type": "+SPWorkflowTaskCollection"
                },
                "visibleParentItem": {
                    "!type": "bool"
                },
                "webId": {
                    "!type": "+Guid"
                },
                "compareTo": {
                    "!type": "fn(workflow: +SPWorkflow) -> number"
                },
                "createHistoryDurationEvent": {
                    "!type": "fn(eventId: number, groupId: ?, user: ?, duration: string, outcome: string, description: string, otherData: string)"
                },
                "createHistoryEvent": {
                    "!type": "fn(eventId: number, groupId: ?, user: ?, outcome: string, description: string, otherData: string)"
                },
                "getActivityDetails": {
                    "!type": "fn() -> [string]"
                },
                "getHistoryList": {
                    "!type": "fn() -> +SPList"
                },
                "getParentAssociation": {
                    "!type": "fn() -> +SPWorkflowAssociation"
                },
                "getParentItem": {
                    "!type": "fn() -> +SPListItem"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getTaskFilter": {
                    "!type": "fn() -> +SPWorkflowFilter"
                },
                "getTaskList": {
                    "!type": "fn() -> +SPList"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowAssociation": {
            "!type": "fn(?)",
            "prototype": {
                "allowAsyncManualStart": {
                    "!type": "bool"
                },
                "allowManual": {
                    "!type": "bool"
                },
                "associationData": {
                    "!type": "string"
                },
                "author": {
                    "!type": "number"
                },
                "autoCleanupDays": {
                    "!type": "number"
                },
                "autoStartChange": {
                    "!type": "bool"
                },
                "autoStartCreate": {
                    "!type": "bool"
                },
                "baseId": {
                    "!type": "+Guid"
                },
                "baseTemplate": {
                    "!type": "+SPWorkflowTemplate"
                },
                "compressInstanceData": {
                    "!type": "bool"
                },
                "contentTypePushDown": {
                    "!type": "bool"
                },
                "created": {
                    "!type": "+Date"
                },
                "description": {
                    "!type": "string"
                },
                "enabled": {
                    "!type": "bool"
                },
                "globallyEnabled": {
                    "!type": "bool"
                },
                "historyListId": {
                    "!type": "+Guid"
                },
                "historyListTitle": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "instantiationUrl": {
                    "!type": "string"
                },
                "internalName": {
                    "!type": "string"
                },
                "isDeclarative": {
                    "!type": "bool"
                },
                "lockItem": {
                    "!type": "bool"
                },
                "markedForDelete": {
                    "!type": "bool"
                },
                "modificationUrl": {
                    "!type": "string"
                },
                "modified": {
                    "!type": "+Date"
                },
                "name": {
                    "!type": "string"
                },
                "parentAssociationId": {
                    "!type": "+Guid"
                },
                "parentContentType": {
                    "!type": "+SPContentType"
                },
                "permissionsManual": {
                    "!type": "string"
                },
                "runningInstances": {
                    "!type": "number"
                },
                "siteId": {
                    "!type": "+Guid"
                },
                "siteOverQuota": {
                    "!type": "bool"
                },
                "siteWriteLocked": {
                    "!type": "bool"
                },
                "statusColumn": {
                    "!type": "bool"
                },
                "statusUrl": {
                    "!type": "string"
                },
                "taskListContentTypeId": {
                    "!type": "+SPContentTypeId"
                },
                "taskListId": {
                    "!type": "+Guid"
                },
                "taskListTitle": {
                    "!type": "string"
                },
                "webId": {
                    "!type": "+Guid"
                },
                "exportToXml": {
                    "!type": "fn() -> string"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "getParentSite": {
                    "!type": "fn() -> +SPSite"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getPropertyByName": {
                    "!type": "fn(property: string) -> ?"
                },
                "getSoapXml": {
                    "!type": "fn() -> string"
                },
                "setHistoryList": {
                    "!type": "fn(list: +SPList)"
                },
                "setPropertyByName": {
                    "!type": "fn(property: string, obj: ?)"
                },
                "setTaskList": {
                    "!type": "fn(list: +SPList)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowAssociationCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "add": {
                    "!type": "fn(workflowAssociation: +SPWorkflowAssociation) -> +SPWorkflowAssociation"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "getParentSite": {
                    "!type": "fn() -> +SPSite"
                },
                "getParentWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getSoapXml": {
                    "!type": "fn() -> string"
                },
                "getWorkflowAssociationByBaseId": {
                    "!type": "fn(baseTemplateId: ?) -> +SPWorkflowAssociation"
                },
                "getWorkflowAssociationById": {
                    "!type": "fn(id: ?) -> +SPWorkflowAssociation"
                },
                "getWorkflowAssociationByIndex": {
                    "!type": "fn(index: number) -> +SPWorkflowAssociation"
                },
                "getWorkflowAssociationByName": {
                    "!type": "fn(name: string) -> +SPWorkflowAssociation"
                },
                "remove": {
                    "!type": "fn(workflowAssociation: +SPWorkflowAssociation)"
                },
                "removeById": {
                    "!type": "fn(associationId: ?)"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWorkflowAssociation]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "update": {
                    "!type": "fn(workflowAssociation: +SPWorkflowAssociation)"
                },
                "updateAssociationsToLatestVersion": {
                    "!type": "fn() -> bool"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "exclusiveFilterStates": {
                    "!type": "string"
                },
                "inclusiveFilterStates": {
                    "!type": "string"
                },
                "getInstanceIds": {
                    "!type": "fn() -> [+Guid]"
                },
                "getWorkflowById": {
                    "!type": "fn(id: ?) -> +SPWorkflow"
                },
                "getWorkflowByIndex": {
                    "!type": "fn(index: number) -> +SPWorkflow"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWorkflow]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowFilter": {
            "!type": "fn(?)",
            "prototype": {
                "assignedTo": {
                    "!type": "string"
                },
                "exclusiveFilterStates": {
                    "!type": "string"
                },
                "inclusiveFilterStates": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowManager": {
            "!type": "fn(?)",
            "prototype": {
                "shuttingDown": {
                    "!type": "bool"
                },
                "countWorkflowAssociations": {
                    "!type": "fn(template: +SPWorkflowTemplate, site: +SPSite) -> number"
                },
                "countWorkflowsByAssociation": {
                    "!type": "fn(association: +SPWorkflowAssociation) -> number"
                },
                "countWorkflowsByTemplate": {
                    "!type": "fn(template: +SPWorkflowTemplate, site: +SPSite) -> number"
                },
                "countWorkflowsInContentType": {
                    "!type": "fn(ct: +SPContentType) -> ?"
                },
                "countWorkflowsInList": {
                    "!type": "fn(list: +SPList) -> ?"
                },
                "countWorkflowsInWeb": {
                    "!type": "fn(web: +SPWeb) -> ?"
                },
                "dispose": {
                    "!type": "fn()"
                },
                "getItemActiveWorkflows": {
                    "!type": "fn(item: +SPListItem) -> +SPWorkflowCollection"
                },
                "getItemTasks": {
                    "!type": "fn(item: +SPListItem, filter: ?) -> +SPWorkflowTaskCollection"
                },
                "getItemWorkflows": {
                    "!type": "fn(item: +SPListItem, filter: ?) -> +SPWorkflowCollection"
                },
                "getWorkflowAvailableRunCount": {
                    "!type": "fn(listItem: +SPListItem, guidOrWorkflow: ?, filter: ?) -> +SPWorkflowTaskCollection"
                },
                "getWorkflowTemplatesByCategory": {
                    "!type": "fn(web: +SPWeb, strReqCaegs: string) -> +SPWorkflowTemplateCollection"
                },
                "removeWorkflowFromListItem": {
                    "!type": "fn(workflow: +SPWorkflow)"
                },
                "startWorkflow": {
                    "!type": "fn(listItem: +SPListItem, association: +SPWorkflowAssociation, eventData: ?, isAutoStart: ?) -> +SPWorkflow"
                },
                "startWorkflowFromContext": {
                    "!type": "fn(context: ?, association: +SPWorkflowAssociation, eventData: ?, runOptions: ?) -> +SPWorkflow"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowModification": {
            "!type": "fn(?)",
            "prototype": {
                "contextData": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "nameFormatData": {
                    "!type": "string"
                },
                "typeId": {
                    "!type": "+Guid"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowModificationCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getWorkflowModificationById": {
                    "!type": "fn(id: ?) -> +SPWorkflowModification"
                },
                "getWorkflowModificationByIndex": {
                    "!type": "fn(index: number) -> +SPWorkflowModification"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWorkflowModification]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowTask": {
            "!type": "fn(?)",
            "prototype": {
                "allRolesForCurrentUser": {
                    "!type": "+SPRoleDefinitionBindingCollection"
                },
                "attachments": {
                    "!type": "?"
                },
                "audit": {
                    "!type": "+SPAudit"
                },
                "contentTypeId": {
                    "!type": "+SPContentTypeId"
                },
                "copySource": {
                    "!type": "string"
                },
                "displayName": {
                    "!type": "string"
                },
                "fields": {
                    "!type": "+SPFieldCollection"
                },
                "fieldValues": {
                    "!type": "?"
                },
                "fieldValuesAsHtml": {
                    "!type": "?"
                },
                "fieldValuesAsText": {
                    "!type": "?"
                },
                "fieldValuesForEdit": {
                    "!type": "?"
                },
                "fileSystemObjectType": {
                    "!type": "string"
                },
                "hasPublishedVersion": {
                    "!type": "bool"
                },
                "hasUniqueRoleAssignments": {
                    "!type": "bool"
                },
                "iconOverlay": {
                    "!type": "?"
                },
                "id": {
                    "!type": "number"
                },
                "level": {
                    "!type": "string"
                },
                "missingRequiredFields": {
                    "!type": "bool"
                },
                "moderationInformation": {
                    "!type": "+SPModerationInformation"
                },
                "name": {
                    "!type": "string"
                },
                "progId": {
                    "!type": "string"
                },
                "propertyBag": {
                    "!type": "+Hashtable"
                },
                "recurrenceId": {
                    "!type": "string"
                },
                "roleAssignments": {
                    "!type": "+SPRoleAssignmentCollection"
                },
                "serverRedirected": {
                    "!type": "bool"
                },
                "sortType": {
                    "!type": "string"
                },
                "tasks": {
                    "!type": "+SPWorkflowTaskCollection"
                },
                "title": {
                    "!type": "string"
                },
                "uniqueId": {
                    "!type": "+Guid"
                },
                "url": {
                    "!type": "string"
                },
                "workflowId": {
                    "!type": "+Guid"
                },
                "workflows": {
                    "!type": "+SPWorkflowCollection"
                },
                "xml": {
                    "!type": "string"
                },
                "addGroup": {
                    "!type": "fn(group: ?, role: ?)"
                },
                "addUser": {
                    "!type": "fn(user: ?, role: ?)"
                },
                "breakRoleInheritance": {
                    "!type": "fn(copyRoleAssignments: bool, clearSubscopes: ?)"
                },
                "copyFrom": {
                    "!type": "fn(sourceUrl: string)"
                },
                "copyTo": {
                    "!type": "fn(destinationUrl: string)"
                },
                "delete": {
                    "!type": "fn()"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(permissions: string) -> bool"
                },
                "ensureWorkflowInformation": {
                    "!type": "fn(retrieveAssociations: ?, retrieveWorkflows: ?)"
                },
                "getContentType": {
                    "!type": "fn() -> +SPContentType"
                },
                "getFile": {
                    "!type": "fn() -> +SPFile"
                },
                "getFileContentsAsJson": {
                    "!type": "fn() -> ?"
                },
                "getFolder": {
                    "!type": "fn() -> +SPFolder"
                },
                "getFormattedValue": {
                    "!type": "fn(fieldName: string) -> string"
                },
                "getListItems": {
                    "!type": "fn() -> +SPListItemCollection"
                },
                "getParentList": {
                    "!type": "fn() -> +SPList"
                },
                "getProperty": {
                    "!type": "fn(key: string) -> ?"
                },
                "getVersions": {
                    "!type": "fn() -> +SPListItemVersionCollection"
                },
                "getWeb": {
                    "!type": "fn() -> +SPWeb"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "parseAndSetValue": {
                    "!type": "fn(fieldName: string, value: string)"
                },
                "recycle": {
                    "!type": "fn() -> string"
                },
                "removeGroup": {
                    "!type": "fn(group: ?)"
                },
                "removeUser": {
                    "!type": "fn(user: ?)"
                },
                "replaceLink": {
                    "!type": "fn(oldUrl: string, newUrl: string)"
                },
                "resetRoleInheritance": {
                    "!type": "fn()"
                },
                "setFieldValue": {
                    "!type": "fn(fieldName: string, fieldValue: string)"
                },
                "setFieldValues": {
                    "!type": "fn(fieldValues: ?)"
                },
                "setProperty": {
                    "!type": "fn(key: string, value: ?)"
                },
                "setTaxonomyFieldValue": {
                    "!type": "fn(fieldName: string, fieldValue: +Term)"
                },
                "systemUpdate": {
                    "!type": "fn(incrementListItemVersion: bool)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "unlinkFromCopySource": {
                    "!type": "fn()"
                },
                "update": {
                    "!type": "fn()"
                },
                "updateOverwriteVersion": {
                    "!type": "fn()"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowTaskCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "delete": {
                    "!type": "fn(index: number)"
                },
                "getTaskById": {
                    "!type": "fn(id: ?) -> +SPWorkflowTask"
                },
                "getTaskByIndex": {
                    "!type": "fn(index: number) -> +SPWorkflowTask"
                },
                "getXml": {
                    "!type": "fn() -> string"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWorkflowTask]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowTemplate": {
            "!type": "fn(?)",
            "prototype": {
                "allowAsyncManualStart": {
                    "!type": "bool"
                },
                "allowDefaultContentApproval": {
                    "!type": "bool"
                },
                "allowManual": {
                    "!type": "bool"
                },
                "associationData": {
                    "!type": "string"
                },
                "associationUrl": {
                    "!type": "string"
                },
                "autoCleanupDays": {
                    "!type": "number"
                },
                "autoStartChange": {
                    "!type": "bool"
                },
                "autoStartCreate": {
                    "!type": "bool"
                },
                "baseId": {
                    "!type": "+Guid"
                },
                "compressInstanceData": {
                    "!type": "bool"
                },
                "description": {
                    "!type": "string"
                },
                "id": {
                    "!type": "+Guid"
                },
                "instantiationUrl": {
                    "!type": "string"
                },
                "isDeclarative": {
                    "!type": "bool"
                },
                "modificationUrl": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "permissionsManual": {
                    "!type": "string"
                },
                "statusColumn": {
                    "!type": "bool"
                },
                "statusUrl": {
                    "!type": "string"
                },
                "taskListContentTypeId": {
                    "!type": "+SPContentTypeId"
                },
                "xml": {
                    "!type": "string"
                },
                "clone": {
                    "!type": "fn() -> +SPWorkflowTemplate"
                },
                "getPropertyByName": {
                    "!type": "fn(property: string) -> ?"
                },
                "getStatusChoices": {
                    "!type": "fn(web: +SPWeb) -> [string]"
                },
                "setPropertyByName": {
                    "!type": "fn(property: string, obj: ?)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "SPWorkflowTemplateCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getTemplateByBaseId": {
                    "!type": "fn(baseTemplateId: ?) -> +SPWorkflowTemplate"
                },
                "getTemplateById": {
                    "!type": "fn(id: ?) -> +SPWorkflowTemplate"
                },
                "getTemplateByIndex": {
                    "!type": "fn(index: number) -> +SPWorkflowTemplate"
                },
                "getTemplateByName": {
                    "!type": "fn(name: string) -> +SPWorkflowTemplate"
                },
                "toArray": {
                    "!type": "fn() -> [+SPWorkflowTemplate]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TaxonomySession": {
            "!type": "fn(?)",
            "prototype": {
                "offlineTermStoreNames": {
                    "!type": "[string]"
                },
                "termStores": {
                    "!type": "+TermStoreCollection"
                },
                "getDefaultKeywordsTermStore": {
                    "!type": "fn() -> +TermStore"
                },
                "getDefaultSiteCollectionTermStore": {
                    "!type": "fn() -> +TermStore"
                },
                "getTerm": {
                    "!type": "fn(termId: ?) -> +Term",
                    "!doc": "Gets a Term object that is based on Term IDs. If the current Term belongs to multiple TermSet objects, it will arbitrarily return the Term from one of the TermSet objects."
                },
                "getTerms": {
                    "!type": "fn(termLabel: string, arg2: ?, arg3: ?) -> +TermCollection",
                    "!doc": "Gets Term objects for the current TaxonomySession object."
                },
                "getTermSets": {
                    "!type": "fn(termSetName: string, lcid: number) -> +TermSetCollection"
                },
                "getTermSetsFromLabels": {
                    "!type": "fn(termLabels: [?], lcid: ?) -> +TermSetCollection"
                },
                "getTermsEx": {
                    "!type": "fn(termLabel: string, defaultLabelOnly: bool, stringMatchOption: string, resultCollectionSize: number, trimUnavailable: bool) -> +TermCollection"
                },
                "getTermsEx2": {
                    "!type": "fn(termLabel: string, lcid: number, defaultLabelOnly: bool, stringMatchOption: string, resultCollectionSize: number, trimUnavailable: bool, trimDeprecated: bool) -> +TermCollection"
                },
                "getTermsFromIds": {
                    "!type": "fn(ids: [?]) -> +TermCollection"
                },
                "getTermsInDefaultLanguage": {
                    "!type": "fn(termLabel: string, defaultLabelOnly: bool, stringMatchOption: string, resultCollectionSize: number, trimUnavailable: bool, trimDeprecated: bool) -> +TermCollection"
                },
                "getTermsInWorkingLocale": {
                    "!type": "fn(termLabel: string, defaultLabelOnly: bool, stringMatchOption: string, resultCollectionSize: number, trimUnavailable: bool, trimDeprecated: bool) -> +TermCollection"
                },
                "getTermsWithCustomProperty": {
                    "!type": "fn(customPropertyName: string, customPropertyValue: string, stringMatchOption: string, resultCollectionSize: number, trimUnavailable: bool) -> +TermCollection"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Term": {
            "!type": "fn(?)",
            "prototype": {
                "createdDate": {
                    "!type": "+Date"
                },
                "customProperties": {
                    "!type": "?"
                },
                "customSortOrder": {
                    "!type": "string"
                },
                "id": {
                    "!type": "string"
                },
                "isAvailableForTagging": {
                    "!type": "bool"
                },
                "isDeprecated": {
                    "!type": "bool"
                },
                "isKeyword": {
                    "!type": "bool"
                },
                "isReused": {
                    "!type": "bool"
                },
                "isRoot": {
                    "!type": "bool"
                },
                "isSourceTerm": {
                    "!type": "bool"
                },
                "lastModifiedDate": {
                    "!type": "+Date"
                },
                "name": {
                    "!type": "string"
                },
                "owner": {
                    "!type": "string"
                },
                "termsCount": {
                    "!type": "number"
                },
                "copy": {
                    "!type": "fn(doCopyChildren: bool) -> +Term"
                },
                "createLabel": {
                    "!type": "fn(labelName: string, lcid: number, isDefault: bool) -> +TermLabel"
                },
                "createTerm": {
                    "!type": "fn(name: string, lcid: number) -> +Term"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deleteCustomProperty": {
                    "!type": "fn(name: string)"
                },
                "deprecate": {
                    "!type": "fn(doDeprecate: bool)"
                },
                "DoesUserHavePermissions": {
                    "!type": "fn(rights: string) -> bool"
                },
                "getAllLabels": {
                    "!type": "fn(lcid: number) -> [+TermLabel]"
                },
                "getDefaultLabel": {
                    "!type": "fn(lcid: number) -> string"
                },
                "getDescription": {
                    "!type": "fn() -> string"
                },
                "getIsDescendantOf": {
                    "!type": "fn(term: +Term) -> bool"
                },
                "getLabels": {
                    "!type": "fn(lcid: number) -> [+TermLabel]"
                },
                "getParent": {
                    "!type": "fn() -> +Term"
                },
                "getPath": {
                    "!type": "fn() -> string"
                },
                "getSourceTerm": {
                    "!type": "fn() -> +Term"
                },
                "getTerms": {
                    "!type": "fn(pagingLimit: number) -> [+Term]"
                },
                "GetTermSet": {
                    "!type": "fn() -> +TermSet"
                },
                "GetTermSets": {
                    "!type": "fn() -> [+TermSet]"
                },
                "getTermStore": {
                    "!type": "fn() -> +TermStore"
                },
                "merge": {
                    "!type": "fn(term: +Term) -> +Term"
                },
                "move": {
                    "!type": "fn(newParentTerm: +Term)"
                },
                "reassignSourceTerm": {
                    "!type": "fn(reusedTerm: +Term)"
                },
                "reuseTerm": {
                    "!type": "fn(sourceTerm: +Term, reuseBranch: bool) -> +Term"
                },
                "setCustomProperty": {
                    "!type": "fn(name: string, value: string)"
                },
                "setDescription": {
                    "!type": "fn(description: string, lcid: number)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "pagingLimit": {
                    "!type": "number"
                },
                "getTermById": {
                    "!type": "fn(id: ?) -> +Term"
                },
                "getTermByIndex": {
                    "!type": "fn(index: number) -> +Term"
                },
                "getTermByName": {
                    "!type": "fn(name: string) -> +Term"
                },
                "toArray": {
                    "!type": "fn() -> [+Term]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermGroup": {
            "!type": "fn(?)",
            "prototype": {
                "createdDate": {
                    "!type": "+Date"
                },
                "id": {
                    "!type": "string"
                },
                "isSiteCollectionGroup": {
                    "!type": "bool"
                },
                "isSystemGroup": {
                    "!type": "bool"
                },
                "lastModifiedDate": {
                    "!type": "+Date"
                },
                "name": {
                    "!type": "string"
                },
                "addContributor": {
                    "!type": "fn(principalName: string)"
                },
                "addGroupManager": {
                    "!type": "fn(principalName: string)"
                },
                "addSiteCollectionAccess": {
                    "!type": "fn(siteCollectionId: ?)"
                },
                "createTermSet": {
                    "!type": "fn(name: string) -> +TermSet"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deleteContributor": {
                    "!type": "fn(principalName: string)"
                },
                "deleteGroupManager": {
                    "!type": "fn(principalName: string)"
                },
                "deleteSiteCollectionAccess": {
                    "!type": "fn(siteCollectionId: ?)"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(rights: string) -> bool"
                },
                "export": {
                    "!type": "fn() -> string"
                },
                "getTermSets": {
                    "!type": "fn() -> [+TermSet]"
                },
                "getTermStore": {
                    "!type": "fn() -> +TermStore"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermLabel": {
            "!type": "fn(?)",
            "prototype": {
                "isDefaultForLanguage": {
                    "!type": "bool"
                },
                "language": {
                    "!type": "number"
                },
                "value": {
                    "!type": "string"
                },
                "delete": {
                    "!type": "fn()"
                },
                "getTerm": {
                    "!type": "fn() -> +Term"
                },
                "setAsDefaultForLanguage": {
                    "!type": "fn()"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermRangeQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermSet": {
            "!type": "fn(?)",
            "prototype": {
                "contact": {
                    "!type": "string"
                },
                "createdDate": {
                    "!type": "+Date"
                },
                "customSortOrder": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "id": {
                    "!type": "string"
                },
                "isAvailableForTagging": {
                    "!type": "bool"
                },
                "isOpenForTermCreation": {
                    "!type": "bool"
                },
                "lastModifiedDate": {
                    "!type": "+Date"
                },
                "name": {
                    "!type": "string"
                },
                "owner": {
                    "!type": "string"
                },
                "stakeholders": {
                    "!type": "[string]"
                },
                "addStakeholder": {
                    "!type": "fn(stakeHolderName: string)"
                },
                "copy": {
                    "!type": "fn() -> +TermSet"
                },
                "createTerm": {
                    "!type": "fn(name: string, lcid: number) -> +Term"
                },
                "createTermWithId": {
                    "!type": "fn(name: string, lcid: number, newTermId: ?) -> +Term"
                },
                "delete": {
                    "!type": "fn()"
                },
                "deleteStakeholder": {
                    "!type": "fn(stakeholderName: string)"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(rights: string) -> bool"
                },
                "export": {
                    "!type": "fn() -> string"
                },
                "getAllTerms": {
                    "!type": "fn() -> [+Term]"
                },
                "getGroup": {
                    "!type": "fn() -> +TermGroup"
                },
                "getTerm": {
                    "!type": "fn(termId: ?) -> +Term"
                },
                "getTerms": {
                    "!type": "fn() -> [+Term]"
                },
                "getTermStore": {
                    "!type": "fn() -> +TermStore"
                },
                "move": {
                    "!type": "fn(targetGroup: +TermGroup)"
                },
                "reuseTerm": {
                    "!type": "fn(term: +Term, reuseBranch: bool) -> +Term"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermSetCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getTermSetById": {
                    "!type": "fn(id: ?) -> +TermSet"
                },
                "getTermSetByIndex": {
                    "!type": "fn(index: number) -> +TermSet"
                },
                "getTermSetByName": {
                    "!type": "fn(name: string) -> +TermSet"
                },
                "toArray": {
                    "!type": "fn() -> [+TermSet]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermsFilter": {
            "!type": "fn(?)",
            "prototype": {
                "addTerm": {
                    "!type": "fn(fieldName: string, text: string) -> +TermsFilter"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermStore": {
            "!type": "fn(?)",
            "prototype": {
                "defaultLanguage": {
                    "!type": "number"
                },
                "id": {
                    "!type": "string"
                },
                "isOnline": {
                    "!type": "bool"
                },
                "languages": {
                    "!type": "[number]"
                },
                "name": {
                    "!type": "string"
                },
                "workingLanguage": {
                    "!type": "number"
                },
                "addLanguage": {
                    "!type": "fn(lcid: number)"
                },
                "addTermStoreAdministrator": {
                    "!type": "fn(principalName: string)"
                },
                "commitAll": {
                    "!type": "fn()"
                },
                "createGroup": {
                    "!type": "fn(name: string) -> +TermGroup"
                },
                "deleteLanguage": {
                    "!type": "fn(lcid: number)"
                },
                "deleteTermStoreAdministrator": {
                    "!type": "fn(principalName: string)"
                },
                "doesUserHavePermissions": {
                    "!type": "fn(rights: string) -> bool"
                },
                "getGroup": {
                    "!type": "fn(id: ?) -> +TermGroup"
                },
                "getGroups": {
                    "!type": "fn() -> [+TermGroup]"
                },
                "getSiteCollectionGroup": {
                    "!type": "fn(site: +SPSite) -> +TermGroup"
                },
                "getSystemGroup": {
                    "!type": "fn(site: +SPSite) -> +TermGroup"
                },
                "resyncHiddenList": {
                    "!type": "fn(site: +SPSite)"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "updateUsedTermsOnSite": {
                    "!type": "fn(site: +SPSite)"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TermStoreCollection": {
            "!type": "fn(?)",
            "prototype": {
                "count": {
                    "!type": "number"
                },
                "getTermStoreById": {
                    "!type": "fn(id: ?) -> +TermStore"
                },
                "getTermStoreByIndex": {
                    "!type": "fn(index: number) -> +TermStore"
                },
                "getTermStoreByName": {
                    "!type": "fn(name: string) -> +TermStore"
                },
                "toArray": {
                    "!type": "fn() -> [+TermStore]"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "TimeZone": {
            "!type": "fn(?)",
            "prototype": {
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "Todo": {
            "!type": "fn(?)",
            "prototype": {
                "class": {
                    "!type": "string"
                },
                "description": {
                    "!type": "string"
                },
                "due": {
                    "!type": "?"
                },
                "duration": {
                    "!type": "?"
                },
                "location": {
                    "!type": "string"
                },
                "name": {
                    "!type": "string"
                },
                "percentComplete": {
                    "!type": "number"
                },
                "status": {
                    "!type": "string"
                },
                "summary": {
                    "!type": "string"
                },
                "uid": {
                    "!type": "string"
                },
                "url": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "UsageInfo": {
            "!type": "fn(?)",
            "prototype": {
                "bandwidth": {
                    "!type": "number"
                },
                "discussionStorage": {
                    "!type": "number"
                },
                "hits": {
                    "!type": "number"
                },
                "storage": {
                    "!type": "number"
                },
                "visits": {
                    "!type": "number"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "UserAgent": {
            "!type": "fn(?)",
            "prototype": {
                "family": {
                    "!type": "string"
                },
                "major": {
                    "!type": "string"
                },
                "minor": {
                    "!type": "string"
                },
                "patch": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "WebPartZoneBase": {
            "!type": "fn(?)",
            "prototype": {
                "emptyZoneText": {
                    "!type": "string"
                },
                "headerText": {
                    "!type": "string"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        "WildcardQuery": {
            "!type": "fn(?)",
            "prototype": {
                "boost": {
                    "!type": "?"
                },
                "toLocaleString": {
                    "!type": "fn() -> string"
                },
                "toString": {
                    "!type": "fn() -> string"
                },
                "ToString": {
                    "!type": "fn() -> string"
                },
                "valueOf": {
                    "!type": "fn() -> ?"
                }
            }
        },
        console: {
            log: {
                "!type": "fn(text: string)",
                "!doc": "Prints to stdout with newline. This function can take multiple arguments in a printf()-like way."
            },
            info: {
                "!type": "fn(text: string)",
                "!doc": "Same as console.log."
            },
            debug: {
                "!type": "fn(text: string)",
                "!doc": "Same as console.log but prints to debug."
            },
            error: {
                "!type": "fn(text: string)",
                "!doc": "Same as console.log but prints to stderr."
            },
            warn: {
                "!type": "fn(text: string)",
                "!doc": "Same as console.error."
            },
            time: {
                "!type": "fn(label: string)",
                "!doc": "Mark a time."
            },
            timeEnd: {
                "!type": "fn(label: string)",
                "!doc": "Finish timer, record output."
            },
            group: {
                "!type": "fn(label: string)",
                "!doc": "Begin a group"
            },
            endGroup: {
                "!type": "fn()",
                "!doc": "Ends a group"
            },
            assert: {
                "!type": "fn(expression: bool)",
                "!doc": "Same as assert.ok() where if the expression evaluates as false throw an AssertionError with message."
            },
            clear: {
                "!type": "fn()",
                "!doc": "Clears the stdout."
            },
            "!url": "http://nodejs.org/api/globals.html#globals_console",
            "!doc": "Used to print to stdout and stderr."
        },
    };
});
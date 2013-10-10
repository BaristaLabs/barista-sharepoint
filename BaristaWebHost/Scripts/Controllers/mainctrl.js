var globalJSHintOptions = {
    latedef: true
};

var filterJSHintErrors = function (code, errors) {
    var lastExprError = Enumerable.From(errors)
        .Where(function (e) { return e.message === "Expected an assignment or function call and instead saw an expression."; })
        .LastOrDefault();

    var lineCount = code.split(/\r\n|\r|\n/).length;

    if (lastExprError != null && lastExprError.from.line == lineCount - 1) {
        errors = errors.exclude(lastExprError);
    } else {
        errors.push({
            from: {
                ch: 0,
                line: lineCount - 1
            },
            message: "Ensure that a correct result is being returned by the Barista script.",
            severity: "warning",
            to: {
                ch: 0,
                line: lineCount - 1
            }
        });
    }

    return errors;
};

baristaFiddle.controller("MainCtrl", function ($scope, $window, $timeout, $http, $localStorage, $modal, select) {
    $scope.title = "Barista Fiddle";
    $scope.cm = null;

    $scope.isScriptResultLoading = false;
    $scope.response = {
        responseMode: ""
    };

    $scope.snapOptions = {
        touchToDrag: false,
        maxPosition: 250
    };

    $scope.editorOptions = {
        mode: "javascript",
        indentUnit: 2,
        lineNumbers: true,
        
        lint: {
            async: true,
            getAnnotations: function(cm, updateLinting) {
                var validator = cm.getHelper(null, "lint");
                var code = cm.getValue().trim();

                var result = validator(code, globalJSHintOptions);

                result = filterJSHintErrors(code, result);
                
                updateLinting(cm, result);
            }
        },
        matchBrackets: true,
        styleActiveLine: true,
        highlightSelectionMatches: { showToken: /\w/ },
        foldGutter: {
            rangeFinder: new CodeMirror.fold.combine(CodeMirror.fold.brace, CodeMirror.fold.comment)
        },
        gutters: ["CodeMirror-foldgutter", "CodeMirror-lint-markers", "CodeMirror-linenumbers"],
        extraKeys: {
            "Ctrl-Enter": function(cm) {
                $scope.$apply(function() {
                    $scope.evalFiddleScript();
                });
            },
            "Ctrl-G": function(cm) {
                var dialog = $modal.open({
                    templateUrl: "Views/goto.html",
                    controller: GotoCtrl,
                    resolve: {
                        currentCursorPosition: function() {
                            return cm.getCursor();
                        }
                    }
                });
                
                dialog.result.then(function (position) {
                    cm.setCursor({ line: position.line, ch: 0 });
                    cm.focus();
                });
            },
            "Shift-Ctrl-C": function (cm) {
                //Comment the current selection
                cm.lineComment(cm.getCursor("start"), cm.getCursor("end"));
            },
            "Shift-Ctrl-U": function (cm) {
                //Uncomment the current selection
                cm.uncomment(cm.getCursor("start"), cm.getCursor("end"));
            },
            "Ctrl-S": function (cm) {
                //Save...
            },
            "Ctrl-M": function (cm) {
                //Toggle all folding
                var code = cm.getValue().trim();
                var lineCount = code.split(/\r\n|\r|\n/).length;

                for (var i = 1; i < lineCount + 1; i++)
                    cm.foldCode(i);
            },
            "Ctrl-Up": function (cm) {
                //Cycle Up...
            },
            "Ctrl-Down": function (cm) {
                //Cycle Down...
            },
            "Ctrl-Q": function (cm) {
                //Toggle folding at cursor
                cm.foldCode(cm.getCursor());
            },
            "Ctrl-Space": function (cm) {
                var hintFunction = function (cm, cb, options) {
                    var result = CodeMirror.hint.javascript(cm, options);

                    //TODO: Build in mechanism to getBarista methods.

                    cb(result);
                };

                CodeMirror.showHint(cm, hintFunction, {
                    async: true,
                    completeSingle: true
                });
            }
        },
        theme: "neat",
    };

    $scope.resultOptions = {
        mode: {name: "javascript", json: true},
        readOnly: true,
        highlightSelectionMatches: { showToken: /\w/ },
        foldGutter: {
            rangeFinder: new CodeMirror.fold.combine(CodeMirror.fold.brace, CodeMirror.fold.comment)
        },
        gutters: ["CodeMirror-foldgutter"],
        lineWrapping: true,
        extraKeys: {
            "Ctrl-M": function (cm) {
                //Toggle all folding
                var code = cm.getValue().trim();
                var lineCount = code.split(/\r\n|\r|\n/).length;

                for (var i = 1; i < lineCount + 1; i++)
                    cm.foldCode(i);
            }
        },
        theme: "neat"
    };

    $scope.verticalSplitterOptions = {
        orientation: "vertical",
        panes: [
                    { collapsible: false }
        ]
    };

    $scope.horizontalSplitterOptions = {
        panes: [
                    { collapsible: true, size: "50%" },
                    { collapsible: true }
        ]
    };

    $scope.$storage = $localStorage.$default({
        code: '"Hello, Barista!";',
        evalOptions: {}
    });

    $scope.evalFiddleScript = function () {

        var code = $scope.$storage.code

        if (typeof($scope.$storage.evalOptions) === "undefined")
            $scope.$storage.evalOptions = {};

        var url = "Barista/v1/Barista.svc/eval";
        var evalOptions = $scope.$storage.evalOptions;

        var seperator = "?";

        if (evalOptions.forceStrict == true) {
            url += seperator + "Barista_ForceStrict=1;";
            seperator = "&";
        }

        if (evalOptions.instanceMode != null) {
            url += seperator + "Barista_InstanceMode=" + encodeURIComponent(baristaInstanceMode);
            seperator = "&";
        }

        if (evalOptions.instanceName != null) {
            url += seperator + "Barista_InstanceName=" + encodeURIComponent(baristaInstanceName);
            seperator = "&";
        }

        if (evalOptions.instanceAbsoluteExpiration != null) {
            url += seperator + "Barista_InstanceAbsoluteExpiration=" + encodeURIComponent(baristaInstanceAbsoluteExpiration);
            seperator = "&";
        }

        if (evalOptions.instanceSlidingExpiration != null) {
            url += seperator + "Barista_InstanceSlidingExpiration=" + encodeURIComponent(baristaInstanceSlidingExpiration);
            seperator = "&";
        }

        if (evalOptions.instanceInitializationCode != null) {
            url += seperator + "Barista_InstanceInitializationCode=" + encodeURIComponent(baristaInstanceInitializationCode);
        }

        $scope.isScriptResultLoading = true;
        $scope.response.responseMode = "";

        $http({
                method: 'POST',
                contentType: "application/json; charset=utf-8",
                url: url,
                data: JSON.stringify({ code: code })
            })
            .success(function(data, status, header) {
                $scope.isScriptResultLoading = false;

                $scope.response = {
                    contentType: header()['content-type'].toLowerCase(),
                    status: status,
                    localStatus: "success",
                    header: header,
                    data: data
                };
            })
            .error(function(data, status, header) {
                $scope.isScriptResultLoading = false;

                $scope.response = {
                    contentType: typeof(header()['content-type']) === "undefined" ? "" : header()['content-type'].toLowerCase(),
                    status: status,
                    localStatus: "error",
                    header: header,
                    data: data
                };
            });
    };

    $scope.showEvalOptions = function(e) {
        $modal.open({            
            templateUrl: "Views/evaloptions.html"
        });
    };

    $scope.saveFiddleScript = function (e) {
    };

    $scope.tidyUp = function () {
        $scope.$storage.code = js_beautify($scope.$storage.code);
    };

    $scope.jsHint = function () {

        $modal.open({
            templateUrl: "Views/jshintresults.html",
            controller: JSHintCtrl,
            resolve: {
                code: function () {
                    return $scope.$storage.code;
                }
            }
        });
    };

    $scope.getCodeMirror = function () {
        var cm = jQuery('#code-mirror')[0].nextSibling.CodeMirror;
        return cm;
    };

    $scope.maximizeCodeMirror = function() {
        $timeout(function() {

            var height = jQuery("#code-mirror-pane").height();

            var cm = jQuery('#code-mirror')[0].nextSibling.CodeMirror;
            if (cm != null) {
                cm.setSize(null, height + "px");
                cm.refresh();
            } else {
                $scope.maximizeCodeMirror();
            }
        }, false, 250);
    };

    $scope.maximizeResultsCodeMirror = function() {
        $timeout(function() {

            var height = jQuery("#results-pane").height();

            var cm = jQuery('#code-mirror-result')[0].nextSibling.CodeMirror;
            if (cm != null) {
                cm.setSize(null, height + "px");
                cm.refresh();
            } else {
                $scope.maximizeResultsCodeMirror();
            }
        }, false, 250);
    };



    $window.onresize = function () {
        $scope.maximizeCodeMirror();
        $scope.maximizeResultsCodeMirror
        $scope.$apply();
    };

    $scope.$watch('response', function(newResponse, oldResponse) {
        if (newResponse.localStatus == "success") {
            if (newResponse.contentType != null && newResponse.contentType.indexOf("text/html") == 0) {
                newResponse.formattedData = newResponse.data;

                var htmlScriptResultContents = jQuery("#htmlScriptResult").contents();
                htmlScriptResultContents.find('html').html(newResponse.formattedData);
                newResponse.responseMode = "html";
            } else {
                newResponse.formattedData = JSON.stringify(newResponse.data, null, 4);
                newResponse.responseMode = "json";
                $scope.maximizeResultsCodeMirror();
            }
        } else {
            if (typeof(newResponse.contentType) !== "undefined" && newResponse.contentType.indexOf("text/html") == 0) {
                newResponse.formattedData = newResponse.data;

                var htmlScriptResultContents = jQuery("#htmlScriptResult").contents();
                htmlScriptResultContents.find('html').html(newResponse.formattedData);
                newResponse.responseMode = "html";
            } else {
                newResponse.formattedData = jQuery(newResponse.data).children(".intro").text();
                newResponse.responseMode = "json";
                $scope.maximizeResultsCodeMirror();
            }
        }
    });

    $scope.maximizeCodeMirror();
    $scope.maximizeResultsCodeMirror();
});

var JSHintCtrl = function ($scope, $modalInstance, code) {

    $scope.ok = function () {
        $modalInstance.close();
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

    $scope.$watch("code", function() {

        var code = $scope.code.trim();

        JSHINT(code, globalJSHintOptions);

        $scope.jshintResults = JSHINT.data();

        if (typeof ($scope.jshintResults.errors) == "undefined") {
            $scope.noErrors = true;
        }
        else {
            $scope.jshintResults.errors = filterJSHintErrors(code, $scope.jshintResults.errors);
        }
    });

    $scope.code = code;
};

var GotoCtrl = function($scope, $modalInstance, focus, select, currentCursorPosition) {
    $scope.cursorPosition = currentCursorPosition;
    $scope.cursorPosition.line = $scope.cursorPosition.line + 1;

    $scope.ok = function () {
        var val = $scope.cursorPosition;
        val.line = val.line - 1;
        
        $modalInstance.close(val);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

    select('selectLineNumber');
}
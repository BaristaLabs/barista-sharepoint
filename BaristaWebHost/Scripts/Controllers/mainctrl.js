baristaFiddle.controller("MainCtrl", function ($scope, $window, $timeout, $http) {
    $scope.title = "Barista Fiddle";
    $scope.cm = null;

    $scope.isScriptResultLoading = false;
    $scope.hasScriptResult = false;
    $scope.isHtmlScriptResult = false;

    $scope.editorOptions = {
        lineNumbers: true,
        extraKeys: {
            "Ctrl-Enter": "run",
            "Ctrl-Space": "autocomplete"
        },
        theme: "neat",
        indentUnit: 4,
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

    $scope.executeFiddleScript = function () {

        var cm = $scope.getCodeMirror();
        if (cm == null)
            return;

        var code = cm.getValue();

        store.set("BaristaCode", code);

        var url = "Barista/v1/Barista.svc/eval";

        var seperator = "?";

        if ($scope.forceStrict == true) {
            url += seperator + "Barista_ForceStrict=1;";
            seperator = "&";
        }

        if ($scope.instanceMode != null) {
            url += seperator + "Barista_InstanceMode=" + encodeURIComponent(baristaInstanceMode);
            seperator = "&";
        }

        if ($scope.instanceName != null) {
            url += seperator + "Barista_InstanceName=" + encodeURIComponent(baristaInstanceName);
            seperator = "&";
        }

        if ($scope.instanceAbsoluteExpiration != null) {
            url += seperator + "Barista_InstanceAbsoluteExpiration=" + encodeURIComponent(baristaInstanceAbsoluteExpiration);
            seperator = "&";
        }

        if ($scope.instanceSlidingExpiration != null) {
            url += seperator + "Barista_InstanceSlidingExpiration=" + encodeURIComponent(baristaInstanceSlidingExpiration);
            seperator = "&";
        }

        if ($scope.instanceInitializationCode != null) {
            url += seperator + "Barista_InstanceInitializationCode=" + encodeURIComponent(baristaInstanceInitializationCode);
        }

        $scope.isScriptResultLoading = true;
        $scope.hasScriptResult = false;

        $http({
                method: 'POST',
                contentType: "application/json; charset=utf-8",
                url: url,
                data: JSON.stringify({ code: code })
            })
            .success(function(data, textStatus, header) {
                $scope.isScriptResultLoading = false;
                $scope.hasScriptResult = true;

                var contentType = header()['content-type'];
               
                if (contentType != null && contentType.indexOf("text/html") == 0) {
                    $scope.isHtmlScriptResult = true;
                    $scope.scriptResult = data
                    $("#htmlResult").contents().find('html').html($scope.scriptResult);
                } else {
                    $scope.isHtmlScriptResult = false;
                    var dataText = JSON.stringify(data, null, 4);
                    $scope.scriptResult = dataText;
                }
            })
            .error(function(data, status, header) {
                $scope.isScriptResultLoading = false;
                $scope.hasScriptResult = true;

                var contentType = header()['content-type'];

                if (contentType.indexOf("text/html") == 0) {
                    $scope.isHtmlScriptResult = true;
                    $scope.scriptResult = data
                    $("#htmlScriptResult").contents().find('html').html($scope.scriptResult);
                } else {
                    $scope.isHtmlScriptResult = false;
                    $scope.scriptResult = $(jqXhr.responseText).children(".intro").text();
                }
            });
    };

    $scope.saveFiddleScript = function(e) {
        store.set("BaristaCode", $scope.fiddleScript);
        console.log($scope.fiddleScript);
    };

    $scope.tidyUp = function() {
        var sobeautiful = js_beautify($scope.fiddleScript);
        cm.setValue(sobeautiful);
    };

    $scope.jsLint = function () {
        var code = $scope.fiddleScript;

        JSLINT(code, {
            sloppy: true
        });

        var jslintResults = JSLINT.data();

        if (typeof (jslintResults.errors) == "undefined") {
            $("#jslintResults").html("Your JS code is valid.");
        }
        else {
            $("#jslintResults").html(jsLintResultTemplate(jslintResults));
        }

        $("#jslintResultDialog").modal("show");
    };

    $scope.fiddleScript = "";

    $scope.fiddleScript = store.get("BaristaCode");

    $scope.maximizeSplitter = function (kendoEvent) {
        $timeout(function() {
            var height = jQuery(window).height() - 40;
            var width = jQuery(window).width();
            $("#horizontalSplitter")
                .css("height", height + "px")
                .css("width", width + "px")
                .trigger("resize");
        }, false);
    };

    $scope.getCodeMirror = function () {
        var cm = jQuery('#code-mirror')[0].nextSibling.CodeMirror;
        return cm;
    };

    $scope.maximizeCodeMirror = function () {
        $timeout(function () {
            var height = jQuery(window).height() - 40;
            var cm = jQuery('#code-mirror')[0].nextSibling.CodeMirror;
            if (cm != null) {
                cm.setSize(null, height + "px");
                cm.refresh();
            } else {
                $scope.maximizeCodeMirror();
            }
        }, false, 250);
    }

    $window.onresize = function () {
        $scope.$apply();
    };

    $scope.maximizeSplitter();
    $scope.maximizeCodeMirror();
});
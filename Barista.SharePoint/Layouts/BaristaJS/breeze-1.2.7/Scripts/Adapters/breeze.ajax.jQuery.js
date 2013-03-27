﻿// needs JQuery
(function(factory) {
    // Module systems magic dance.

    if (typeof require === "function" && typeof exports === "object" && typeof module === "object") {
        // CommonJS or Node: hard-coded dependency on "breeze"
        factory(require("breeze"));
    } else if (typeof define === "function" && define["amd"]) {
        // AMD anonymous module with hard-coded dependency on "breeze"
        if (breeze) {
            factory(breeze);
        } else {
            define(["breeze"], factory);
        }
    } else {
        // <script> tag: use the global `breeze` object
        factory(breeze);
    }
}(function(breeze) {
    var core = breeze.core;
    
    var jQuery;
    
    var ctor = function () {
        this.name = "jQuery";
        this.defaultSettings = { };
    };

    ctor.prototype.initialize = function () {
        jQuery = core.requireLib("jQuery", "needed for 'ajax_jQuery' pluggin");
    };

    ctor.prototype.ajax = function (settings) {
        if (! core.isEmpty(this.defaultSettings)) {
            var compositeSettings = core.extend({}, this.defaultSettings);
            core.extend(compositeSettings, settings);
            jQuery.ajax(compositeSettings);
        } else {
            jQuery.ajax(settings);
        }
    };

    
    // last param is true because for now we only have one impl.
    breeze.config.registerAdapter("ajax", ctor);
    
}));

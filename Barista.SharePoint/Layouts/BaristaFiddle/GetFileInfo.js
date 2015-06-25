///Summary:
///Returns the document library and the file information for the specified file.
var web = require("Web");
var sp = require("SharePoint");

var serverRelativeUrl = web.request.queryString["f"];

var jsFile = sp.currentContext.web.getFileByServerRelativeUrl(serverRelativeUrl);
var docLib = sp.currentContext.web.getListByServerRelativeUrl(serverRelativeUrl);

result = {};

if (docLib != null) {
    result["docLibInfo"] = {
        type: "docLib",
        text: docLib.title,
        spriteCssClass: "icon-book",
        enableModeration: docLib.enableModeration,
        enableVersioning: docLib.enableVersioning,
        requireCheckout: docLib.forceCheckout,
        draftVersionVisibility: docLib.draftVersionVisibility,
        docLibUrl: docLib.url
    };
}

if (jsFile != null && jsFile.exists == true) {
    result["fileInfo"] = {
        type: "file",
        text: jsFile.name,
        spriteCssClass: "icon-file",
        checkOutLevel: jsFile.level,
        checkOutType: jsFile.checkOutType,
        checkedOutByUser: jsFile.checkedOutByUser,
        eTag: jsFile.eTag,
        fileUrl: jsFile.url
    };
}

result;
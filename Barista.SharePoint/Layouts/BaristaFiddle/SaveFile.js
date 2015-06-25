///Summary:
///Adds the contents of the specified file to the specified location, overwriting the existing file.
var sp = require("SharePoint");
var web = require("Web");

var serverRelativeUrl = web.request.queryString["f"];
var fileContents = web.request.body;

var result = {};
if (serverRelativeUrl.length >= 0 && fileContents.length > 0) {
    sp.currentContext.web.allowUnsafeUpdates = true;
    var jsFile = sp.currentContext.web.addFileByUrl(serverRelativeUrl, fileContents, true);
    var docLib = jsFile.getDocumentLibrary();
    result = {
        docLibInfo: {
            type: "docLib",
            text: docLib.title,
            spriteCssClass: "icon-book",
            enableModeration: docLib.enableModeration,
            enableVersioning: docLib.enableVersioning,
            requireCheckout: docLib.forceCheckout,
            draftVersionVisibility: docLib.draftVersionVisibility,
            docLibUrl: docLib.url
        },
        fileInfo: {
            type: "file",
            text: jsFile.name,
            spriteCssClass: "icon-file",
            checkOutLevel: jsFile.level,
            checkOutType: jsFile.checkOutType,
            checkedOutByUser: jsFile.checkedOutByUser,
            eTag: jsFile.eTag,
            fileUrl: jsFile.url
        }
    };
}

result;
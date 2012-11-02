///Summary:
///Returns the document library and the file information for the specified file.
var web = require("Web");
var sp = require("SharePoint");

var serverRelativeUrl = web.request.queryString["f"];

var jsFile = sp.currentContext.web.getFileByServerRelativeUrl(serverRelativeUrl);
sp.currentContext.web.allowUnsafeUpdates = true;
jsFile.checkOut();
true;
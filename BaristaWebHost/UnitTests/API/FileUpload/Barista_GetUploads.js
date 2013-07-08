var sp = require("SharePoint");
var web = require("Web");

var list = new SPList("/Lists/BaristaUnitTests");
var uploadsFolder = list.rootFolder.ensureSubFolderExists("Uploads");

var scriptPath = "/Content/Barista_DeleteUpload.js";

var result = new Array();
uploadsFolder.getFiles().forEach(function (file) {
    result.push({
        "url": file.url,
        "thumbnail_url": file.url,
        "name": file.name,
        "type": file.contentType,
        "size": file.length,
        "delete_url": "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(scriptPath) + "&f=" + encodeURIComponent(file.name),
        "delete_type": "GET",
        "request": web.request
    });
});


result;
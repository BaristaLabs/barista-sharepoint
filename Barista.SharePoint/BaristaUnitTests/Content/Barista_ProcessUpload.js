var sp = require("SharePoint");

var list = new SPList("/Lists/BaristaUnitTests");
var uploadsFolder = list.rootFolder.ensureSubFolderExists("Uploads");
var file = web.request.files["files[]"];
var uploadedFile = uploadsFolder.addFile(file, true);

var scriptPath = "/Content/Barista_DeleteUpload.js";

var result = new Array();
result.push({
    "url": uploadedFile.url,
    "thumbnail_url": uploadedFile.url,
    "name": uploadedFile.name,
    "type": uploadedFile.contentType,
    "size": uploadedFile.length,
    "delete_url": "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(scriptPath) + "&f=" + encodeURIComponent(uploadedFile.name),
    "delete_type": "GET",
});

result;
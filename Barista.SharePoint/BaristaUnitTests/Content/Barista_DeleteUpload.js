var sp = require("SharePoint");

var list = new SPList("/Lists/BaristaUnitTests");
var uploadsFolder = list.rootFolder.ensureSubFolderExists("Uploads");

var file = Enumerable.From(uploadsFolder.getFiles())
                     .Where(function(f) { return f.name == web.request.queryString.f; })
                     .FirstOrDefault();

var result = false;
if (file != null) {
    var fileParent = file.getParentWeb();

    fileParent.allowUnsafeUpdates = true;
    file.delete();
    fileParent.allowUnsafeUpdates = false;
    result = true;
}


result;
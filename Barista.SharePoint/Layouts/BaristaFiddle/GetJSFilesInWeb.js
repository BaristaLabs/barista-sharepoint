///Summary:
///Get all .js files in all document libraries in the current web.
///Project the files into a hierarchical data structure of 
///DocumentLibs -> Folders -> Files where folders are flattened to be
///their relative url.
var sp = require("SharePoint");
require("Linq");

var camlBuilder = new SPCamlQueryBuilder();
var caml = camlBuilder.Where()
                      .TextField('FileLeafRef').Contains('.js')
                      .GroupBy('FileDirRef')
                      .OrderBy('FileLeafRef')
                      .ToString();
var result = [];

//Get all document libraries in the current web.
var lists = sp.currentContext.web.getLists();
var docLibs = Enumerable.From(lists).Where(function (l) {
    return l.baseTemplate == 101;
}).ToArray();


//Iterate through each document set
docLibs.forEach(function (docLib) {
    //Execute a caml query to get all .js files.
    var query = new SPCamlQuery();
    query.query = caml;
    query.viewAttributes = "Scope='RecursiveAll'";
    jsListItems = docLib.getItemsByQuery(query);

    var resultFolders = [];
    var lastFolder = null;

    //For each list item returned, add it to a corresponding parent folder object
    jsListItems.forEach(function (jsListItem) {
        var jsFile = jsListItem.getFile();

        var shortParentFolderName = jsFile.parentFolderName.replace(docLib.rootFolder.url, "");
        if (shortParentFolderName.indexOf("/") == 0)
            shortParentFolderName = shortParentFolderName.substring(1);

        shortParentFolderName = shortParentFolderName.trimLeft("/");

        //Expect that list items are ordered by folder.
        if (shortParentFolderName !== "" && (lastFolder === null || lastFolder.text != shortParentFolderName)) {

            lastFolder = {
                type: "folder",
                text: shortParentFolderName,
                spriteCssClass: "icon-folder-open",
                items: []
            };

            resultFolders.push(lastFolder);
        }

        var fileInfo = {
            type: "file",
            text: jsFile.name,
            spriteCssClass: "icon-file",
            checkOutLevel: jsFile.level,
            checkOutType: jsFile.checkOutType,
            checkedOutByUser: jsFile.checkedOutByUser,
            eTag: jsFile.eTag,
            fileUrl: jsFile.url
        };

        if (shortParentFolderName === "") {
            resultFolders.push(fileInfo);
        }
        else {
            lastFolder.items.push(fileInfo);
        }
    });

    //If the doclib contained .js files, add it to the result.
    if (resultFolders.length > 0) {
        result.push({
            type: "docLib",
            text: docLib.title,
            spriteCssClass: "icon-book",
            enableModeration: docLib.enableModeration,
            enableVersioning: docLib.enableVersioning,
            requireCheckout: docLib.forceCheckout,
            draftVersionVisibility: docLib.draftVersionVisibility,
            docLibUrl: docLib.url,
            items: resultFolders
        });
    }
});

//Return the result set.
result;
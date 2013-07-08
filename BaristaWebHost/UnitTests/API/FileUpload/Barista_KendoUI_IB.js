var sp = require("SharePoint");var web = require("Web");require("Linq");

var domainModel = {
    //Read the files in the specified path and return an array of JSON objects.    getFilesAndFoldersInPath: function(targetPath) {
        var uploadsFolder = sp.currentContext.web.getFolderByServerRelativeUrl(targetPath);

        var result = [];

        //Read the folders from the uploads folder and transform into model that KendoUI expects.
        uploadsFolder.getSubFolders().forEach(function(folder) {
            if (folder.name == "Forms" || folder.name == "_t" || folder.name == "_w")
                return;
            result.push({
                "name": folder.name,
                "type": "d",
            });
        });

        //Read the files from the uploads folder and transform into a model that KendoUI expects;
        uploadsFolder.getFiles().forEach(function(file) {
            result.push({
                "name": file.name,
                "type": "f",
                "size": file.length,
            });
        });

        return result;
    },    //Read the file associated with the POST and store them in the target folder.
    uploadFileToPath: function(targetPath, file) {
        var uploadsFolder = sp.currentContext.web.getFolderByServerRelativeUrl(targetPath);
        var uploadedFile = uploadsFolder.addFile(file, true);

        var result = {
            "name": uploadedFile.name,
            "type": "f",
            "size": uploadedFile.length
        };

        return result;
    },    //Create a new folder in the specified path.
    createFolder: function(targetPath, newFolderName) {
        var uploadsFolder = sp.currentContext.web.getFolderByServerRelativeUrl(targetPath);

        var newFolder = uploadsFolder.addSubFolder(newFolderName);
        var result = {
            "name": newFolder.name,
            "type": "d",
        };
        return result;
    },
    deleteFile: function(targetPath, fileName) {
        var uploadsFolder = sp.currentContext.web.getFolderByServerRelativeUrl(targetPath);

        //Iterate through the files in the folder until we find a match.
        //TODO: This is a common pattern and could be made easier/more efficient with a Barista function to do the same.
        var file = Enumerable.From(uploadsFolder.getFiles())
            .Where(function(f) { return f.name == fileName; })
            .FirstOrDefault();

        var result = false;

        //If we found a file, delete it.
        if (file != null) {
            var fileParent = file.getParentWeb();

            fileParent.allowUnsafeUpdates = true;
            file.delete();
            fileParent.allowUnsafeUpdates = false;
            result = true;
        }

        return result;
    },    //Deletes the specified folder in the specified path
    deleteFolder: function(path, folderName) {
        var uploadsFolder = sp.currentContext.web.getFolderByServerRelativeUrl(path);

        //Iterate through the subfolders in the folder until we find a match.
        //TODO: This is a common pattern and could be made easier/more efficient with a Barista function to do the same.
        var folder = Enumerable.From(uploadsFolder.getSubFolders())
            .Where(function(f) { return f.name == folderName; })
            .FirstOrDefault();

        var result = false;

        //If we found a folder, delete it
        //TODO: implement folder.getParentWeb
        if (folder != null) {
            var folderParent = web;

            folderParent.allowUnsafeUpdates = true;
            folder.delete();
            folderParent.allowUnsafeUpdates = false;
            result = true;
        }

        return result;
    }
};//Perform the appropriate operation based on the "o" query string.var operation = web.request.queryString["o"],	uploadsFolderPath,	result;switch (operation) {
    case "read":        uploadsFolderPath = web.request.queryString["path"];        result = domainModel.getFilesAndFoldersInPath(uploadsFolderPath);        break;    case "upload":        uploadsFolderPath = web.request.form["path"];        var file = web.request.files["file"];        result = domainModel.uploadFileToPath(uploadsFolderPath, file);        break;    case "create":        var uploadsFolderPath = web.request.form["path"];        var newFolderName = web.request.form["name"];        result = domainModel.createFolder(uploadsFolderPath, newFolderName);        break;    case "destroy":        uploadsFolderPath = web.request.form["path"];        var fileOrFolderNameToDelete = web.request.form["name"];        //Delete the specified file        if (web.request.form["type"] == "f") {
            result = domainModel.deleteFile(uploadsFolderPath, fileOrFolderNameToDelete);
        }            //Delete the specified folder        else if (web.request.form["type"] == "d") {
            result = domainModel.deleteFolder(uploadsFolderPath, fileOrFolderNameToDelete);
        }        break;
}//Return the result.result;
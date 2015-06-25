require("Deferred");
var sp = require("SharePoint");
var util = require("Utility");
require("Linq");
var ds = require("Document Store");
require("Deferred");

var containerTitle = util.randomString(15, false, true, true, false, false);

ds.createContainer(containerTitle);
ds.containerTitle = containerTitle;

var result = {};
try {
    result.containers = ds.listContainers();
    var dsList = sp.currentContext.web.getListByTitle(containerTitle);

    dsList.addEventReceiver("ItemAdded",
                            "Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52",
                            "Barista.SharePoint.DocumentStore.SPDocumentStoreEventReceiver.SPDocumentStoreEventReceiver"
                           );

    dsList.addEventReceiver("ItemUpdated",
                            "Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52",
                            "Barista.SharePoint.DocumentStore.SPDocumentStoreEventReceiver.SPDocumentStoreEventReceiver"
                           );

    dsList.addEventReceiver("ItemDeleted",
                            "Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52",
                            "Barista.SharePoint.DocumentStore.SPDocumentStoreEventReceiver.SPDocumentStoreEventReceiver"
                           );

    dsList.addEventReceiver("ItemFileMoved",
                            "Barista.SharePoint, Version=1.0.0.0, Culture=neutral, PublicKeyToken=a2d8064cb9226f52",
                            "Barista.SharePoint.DocumentStore.SPDocumentStoreEventReceiver.SPDocumentStoreEventReceiver"
                           );

    dsList.update();

    //Add an entity.
    var entity = ds.createEntity("", "http://test12345", { Pine: "Tree" });

    //Update it.
    ds.updateEntity(entity.id, "", { Forrest: "Gump" });

    //Add an entity part.
    ds.createEntityPart(entity.id, "My Test Part", { Hello: "World" });

    //Update the entity part.
    ds.updateEntityPart(entity.id, "My Test Part", "", { Goodbye: "Moon" });

    //Delete the entity part.
    ds.deleteEntityPart(entity.id, "My Test Part");

    //Add an attachment.
    var data = Base64EncodedByteArray.CreateFromString("Test 12345");
    ds.uploadAttachment(entity.id, "Test12345.txt", data);

    //Update the attachment
    data = Base64EncodedByteArray.CreateFromString("54321 tseT");
    ds.uploadAttachment(entity.id, "Test12345.txt", data);

    //We don't need no steenkin' attachment
    ds.deleteAttachment(entity.id, "Test12345.txt");

    //Move it somewhere else.
    ds.createFolder("SomeFolder");
    ds.moveEntity(entity.id, "SomeFolder");

    //Delete the entity.
    ds.deleteEntity(entity.id);

}
finally {
    ds.deleteContainer(containerTitle);
}

result.existsAfterDelete = Enumerable.From(ds.listContainers).Any(function (x) {
    return x.title === containerTitle;
});


result;
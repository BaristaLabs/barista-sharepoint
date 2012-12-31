require("Deferred");
var sp = require("SharePoint");
var util = require("Utility");
require("Linq");

var listTemplates = sp.currentContext.web.getListTemplates();

var docLibTemplate = Enumerable.From(listTemplates).Where(function (x) {
    return x.name == "Document Library";
})
                                        .FirstOrDefault();

var url = util.randomString(15, false, true, true, false, false);

sp.currentContext.web.allowUnsafeUpdates = true;

var newList = sp.currentContext.web.createList({
    title: 'Document Library For Testing',
    description: 'This is a test, this is only a test',
    url: url,
    listTemplate: docLibTemplate
});
try {
    var result = {
        url: newList.url,
        title: newList.title,
        listTemplate: newList.serverTemplateId,
        beforeEventReceivers: newList.getEventReceivers(),
        beforeProperties: newList.rootFolder.allProperties
    };

    newList.addBaristaEventReceiver("ItemAdded");
    newList.update();

    result.afterEventReceivers = newList.getEventReceivers();

    //Set the list property bag setting to the code thing.
    var code = "var sp = require(\"SharePoint\");\
        var list = new SPList(\"" + url + "\");\
		list.rootFolder.setPropertyBagValue(\"Event Receiver Executed\", true);\
        list.rootFolder.setPropertyBagValue(\"Event Receiver Execution Stamp\", new Date());\
		list.rootFolder.update();\
	";

    newList.rootFolder.setPropertyBagValue("BaristaItemEventReceiver_Code", code);
    newList.rootFolder.update();

    //Add a new item to the list.
    var item = newList.addFile("Testing.json", "Test12345");
    delay(2000); // wait a spell...
    result.afterProperties = newList.rootFolder.allProperties;
}
finally {
    newList.delete();
}

result.existsAfterDelete = Enumerable.From(sp.currentContext.web.getLists()).Any(function (x) {
    return x.url == result.url;
});

result;
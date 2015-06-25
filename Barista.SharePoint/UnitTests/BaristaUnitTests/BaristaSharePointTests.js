module("Barista SharePoint");

asyncTest("Retrieve webs in the current context.", function () {
    expect(1);

    var code = "var sp = require(\"SharePoint\");\
    sp.currentContext.site.getAllWebs();";

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(code));

    request.done(function (data) {
        ok(data != null && data.length > 0, "A collection of webs was returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Assert correlation id matches header", function () {
    expect(1);
    var code = "var util = require('Utility');\
    var correlationId = util.getCurrentCorrelationId();\
    correlationId;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: Barista.getBaristaServiceUrl(),
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        var spRequestGuid = jqXHR.getResponseHeader("SPRequestGuid");
        var items = spRequestGuid.split(",");
        equal(data, items[0], "Correlation Ids Matched.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Retrieve lists in the current context.", function () {
    expect(2);

    var code = "var sp = require(\"SharePoint\");\
    sp.currentContext.web.getLists();";

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(code));

    request.done(function (data) {
        ok(data != null && data.length > 0, "A collection of lists was returned.");
        ok(Enumerable.From(data)
                    .Any(function (x) { return x.title == "Documents"; }), "Documents list was contained in the results.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create List Title PDF Report.", function () {
    expect(2);

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_SPListTitlesToPDF.js");

    request.done(function (data, textStatus, jqXHR) {
        ok(data.indexOf("%PDF-1.4") == 0, "Script was run and a result that contained a PDF 1.4 header was returned.");
        deepEqual(jqXHR.getResponseHeader("Content-Type"), "application/pdf", "Response content type was application/pdf");
        start();
    });

    request.fail(function (jqXHR, textStatus) {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create List Item", function () {
    expect(5);

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_SPListItemRetrieval.js");

    request.done(function (data) {
        ok(data != null && data.length > 0, "A collection of list items was returned.");
        ok(typeof (data[0].fieldValues["FileLeafRef"]) === "string", "List item fields can be accesssed via property accessors");
        ok(data[0].fieldValues["FileLeafRef"].length > 0, "List item title was set.");
        ok(typeof (data[0].fieldValues["Author"]) === "object", "Creator field was set and an object");
        ok(data[0].fieldValues["Author"].loginName.length > 0, "Creator has a login name.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Enumerate List Items", function () {
    expect(5);

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_SPListItemRetrieval.js");

    request.done(function (data) {
        ok(data != null && data.length > 0, "A collection of list items was returned.");
        ok(typeof (data[0].fieldValues["FileLeafRef"]) === "string", "List item fields can be accesssed via property accessors");
        ok(data[0].fieldValues["FileLeafRef"].length > 0, "List item title was set.");
        ok(typeof (data[0].fieldValues["Author"]) === "object", "Creator field was set and an object");
        ok(data[0].fieldValues["Author"].loginName.length > 0, "Creator has a login name.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Enumerate List Items Through View", function () {
    expect(5);

    var request = jQuery.ajax(getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_SPListItemRetrievalThroughView.js");

    request.done(function (data) {
        ok(data != null && data.length > 0, "A collection of list items was returned.");
        ok(typeof (data[0].fieldValues["_ModerationStatus"]) === "string", "List item fields can be accesssed via property accessors");
        ok(data[0].fieldValues["_ModerationStatus"].length > 0, "List item approval status was set.");
        ok(typeof (data[0].fieldValues["Editor"]) === "object", "Modified By field was set and an object");
        ok(data[0].fieldValues["Editor"].loginName.length > 0, "Modifier has a login name.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Enumerate Views On List", function () {
    expect(2);

    var code = "\
    var sp = require(\"SharePoint\");\
    var list = new SPList(\"/Lists/BaristaUnitTests\"); \
    list.getViews(); \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data) {
        ok(data != null && data.length > 0, "A collection of views was returned.");
        ok(typeof (data[0].title) === "string", "View fields can be accesssed via property accessors");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Send Email to Current User", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var currentUser = sp.getCurrentUser(); \
    sp.sendEmail(currentUser.email, '', '', 'BaristaUnitTests@treasury.gov', 'SharePoint Email Unit Test', 'This is a test'); \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data) {
        ok(data == true, "An email was sent to the current user.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create a zip file of files that start with the letter 'B'", function () {
    expect(2);

    var code = "\
    var sp = require(\"SharePoint\");\
    var web = require(\"Web\");\
    var doc = require(\"Document\");\
    var Linq = require(\"Linq\");\
    var list = new SPList(\"/Lists/BaristaUnitTests\"); \
    var files = list.rootFolder.getFiles(); \
    var bFiles = Enumerable.From(files) \
                           .Where(function(f) { return f.name.indexOf('B') == 0; }) \
                           .ToArray(); \
    var zip = new ZipFile(); \
    bFiles.forEach(function(f) { \
        zip.addFile(f.name, f.openBinary()); \
    }); \
    web.response.contentType=\"application/zip\"; \
    zip.finish(); \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.indexOf("PK") == 0, "Expression was run and a result that contained a zip header was returned.");
        deepEqual(jqXHR.getResponseHeader("Content-Type"), "application/zip", "Response content type was application/zip");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Return the total size of the files contained in the list, recursively.", function () {
    expect(2);

    var code = "\
    var sp = require(\"SharePoint\");\
    require(\"Linq\");\
    var list = new SPList(\"/Lists/BaristaUnitTests\"); \
    var files = list.rootFolder.getFiles(true); \
    var result = {};\
    result.fileCount = files.length.toString();\
    result.totalSize = Enumerable.From(files) \
                                 .Sum(function(f) { return f.length; }); \
    result; \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.totalSize > 0, "Total Size was greater than 0");
        ok(parseInt(data.fileCount) > 0, "Total number of files was greater than 0");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Return the content types in the current web", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var w = sp.currentContext.web; \
    var contentTypes = w.getContentTypes(); \
    contentTypes; \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.length > 0, "A number of content types were returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Return the feature definitions in the current site", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var w = sp.currentContext.site; \
    var features = w.getFeatureDefinitions(); \
    features; \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.length > 0, "A number of feature definitions were returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});


asyncTest("Return the features in the current web", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var w = sp.currentContext.web; \
    var features = w.features; \
    features; \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.length > 0, "A number of features were returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Caml Query Builder", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var camlBuilder = new SPCamlQueryBuilder(); \
    var caml = camlBuilder.Where() \
        .TextField('Email').EqualTo('support@google.com') \
        .Or() \
        .TextField('Email').EqualTo('plus@google.com') \
        .Or() \
        .TextField('Title').BeginsWith('[Google]') \
        .Or() \
        .TextField('Content').Contains('Google') \
        .ToString(); \
    caml; \
    ";

    var expected = "<Where>\
  <Or>\
    <Eq>\
      <FieldRef Name=\"Email\" />\
      <Value Type=\"Text\">support@google.com</Value>\
    </Eq>\
    <Or>\
      <Eq>\
        <FieldRef Name=\"Email\" />\
        <Value Type=\"Text\">plus@google.com</Value>\
      </Eq>\
      <Or>\
        <BeginsWith>\
          <FieldRef Name=\"Title\" />\
          <Value Type=\"Text\">[Google]</Value>\
        </BeginsWith>\
        <Contains>\
          <FieldRef Name=\"Content\" />\
          <Value Type=\"Text\">Google</Value>\
        </Contains>\
      </Or>\
    </Or>\
  </Or>\
</Where>";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        expected = expected.replace(/(\r\n|\n|\r)/gm, "");
        data = data.replace(/(\r\n|\n|\r)/gm, "");
        equal(data, expected, "Created a CAML query that matched the expected value.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Caml Query Builder - Membership", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var camlBuilder = new SPCamlQueryBuilder(); \
    var caml = camlBuilder.Where() \
        .IntegerField(\"AssignedTo\").EqualTo(\"{UserID}\") \
        .Or() \
        .UserField(\"AssignedTo\").Membership.CurrentUserGroups() \
        .GroupBy(\"ProductTitle\") \
        .OrderBy(\"Priority\").ThenBy(\"Title\") \
        .ToString(); \
    caml; \
    ";

    var expected = "<Where>\
  <Or>\
    <Eq>\
      <FieldRef Name=\"AssignedTo\" />\
      <Value Type=\"Integer\"><UserID/></Value>\
    </Eq>\
    <Membership Type=\"CurrentUserGroups\">\
      <FieldRef Name=\"AssignedTo\" />\
    </Membership>\
  </Or>\
</Where>\
<GroupBy>\
  <FieldRef Name=\"ProductTitle\" />\
</GroupBy>\
<OrderBy>\
  <FieldRef Name=\"Priority\" />\
  <FieldRef Name=\"Title\" />\
</OrderBy>";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        expected = expected.replace(/(\r\n|\n|\r)/gm, "");
        data = data.replace(/(\r\n|\n|\r)/gm, "");
        equal(data, expected, "Created a CAML query that matched the expected value.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Caml Query Builder - In", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var camlBuilder = new SPCamlQueryBuilder(); \
    var caml = camlBuilder.Where()\
        .LookupIdField(\"Category\").In([2, 3, 10])\
        .And()\
        .DateField(\"ExpirationDate\").LessThanOrEqualTo(\"{Now}\")\
        .OrderByDesc(\"ExpirationDate\")\
        .ToString();\
    caml; \
    ";

    var expected = "<Where>\
  <And>\
    <In>\
      <FieldRef Name=\"Category\" LookupId=\"True\" />\
      <Values>\
        <Value Type=\"Integer\">2</Value>\
        <Value Type=\"Integer\">3</Value>\
        <Value Type=\"Integer\">10</Value>\
      </Values>\
    </In>\
    <Leq>\
      <FieldRef Name=\"ExpirationDate\" />\
      <Value Type=\"Date\"><Now/></Value>\
    </Leq>\
  </And>\
</Where><OrderBy>\
  <FieldRef Name=\"ExpirationDate\" Ascending=\"False\" />\
</OrderBy>";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        expected = expected.replace(/(\r\n|\n|\r)/gm, "");
        data = data.replace(/(\r\n|\n|\r)/gm, "");
        equal(data, expected, "Created a CAML query that matched the expected value.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Caml Query Builder - DateRangesOverlap", function () {
    expect(1);

    var code = "\
    var sp = require(\"SharePoint\");\
    var camlBuilder = new SPCamlQueryBuilder(); \
    var caml = camlBuilder.Where()\
                          .DateRangesOverlap(\"EventDate\", \"EndDate\", \"RecurrenceID\", \"{Today}\")\
                          .And()\
                          .TextField(\"BroadcastTo\").Membership.CurrentUserGroups()\
                          .Or()\
                          .LookupIdField(\"BroadcastTo\").EqualTo(\"{UserID}\")\
                          .OrderByDesc(\"EventDate\")\
                          .ToString();\
    caml; \
    ";

    var expected = "<Where>\
  <And>\
    <DateRangesOverlap>\
      <FieldRef Name=\"EventDate\" />\
      <FieldRef Name=\"EndDate\" />\
      <FieldRef Name=\"RecurrenceID\" />\
      <Value Type=\"DateTime\"><Today/></Value>\
    </DateRangesOverlap>\
    <Or>\
      <Membership Type=\"CurrentUserGroups\">\
        <FieldRef Name=\"BroadcastTo\" />\
      </Membership>\
      <Eq>\
        <FieldRef Name=\"BroadcastTo\" LookupId=\"True\" />\
        <Value Type=\"Integer\"><UserID/></Value>\
      </Eq>\
    </Or>\
  </And>\
</Where>\
<OrderBy>\
  <FieldRef Name=\"EventDate\" Ascending=\"False\" />\
</OrderBy>";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        expected = expected.replace(/(\r\n|\n|\r)/gm, "");
        data = data.replace(/(\r\n|\n|\r)/gm, "");
        equal(data, expected, "Created a CAML query that matched the expected value.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create a zip file of files that start with the letter 'B' using a Caml Query", function () {
    expect(2);

    var code = "\
    var sp = require(\"SharePoint\");\
    var doc = require(\"Document\");\
    var web = require(\"Web\");\
    var camlBuilder = new SPCamlQueryBuilder(); \
    var caml = camlBuilder.Where() \
        .TextField('FileLeafRef').BeginsWith('B') \
        .ToString(); \
    var list = new SPList(\"/Lists/BaristaUnitTests\"); \
    var bListItems = list.getItemsByQuery(caml); \
    var zip = new ZipFile(); \
    bListItems.forEach(function(item) { \
        var f = item.getFile();\
        zip.addFile(f.name, f.openBinary()); \
    }); \
    web.response.contentType=\"application/zip\"; \
    zip.finish(); \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data.indexOf("PK") == 0, "Expression was run and a result that contained a zip header was returned.");
        deepEqual(jqXHR.getResponseHeader("Content-Type"), "application/zip", "Response content type was application/zip");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});


asyncTest("Get Web Templates on current site.", function () {
    expect(2);

    var code = "\
    var sp = require(\"SharePoint\");\
    var webTemplates = sp.currentContext.site.getWebTemplates();\
    webTemplates; \
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null && data.length > 0, "A collection of web templates was returned.");
        ok(Enumerable.From(data)
                    .Any(function (x) { return x.title == "Wiki Site"; }), "'Wiki Site'web template was contained in the results.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create and then remove a Wiki site to the current site collection.", function () {
    expect(4);

    var code = "\
    var sp = require(\"SharePoint\");\
    var util = require(\"Utility\");\
    require(\"Linq\");\
    var webTemplates = sp.currentContext.site.getWebTemplates();\
    var wikiTemplate = Enumerable.From(webTemplates)\
                                 .Where(function(x) { return x.title == \"Wiki Site\"; })\
                                 .FirstOrDefault();\
    var url = util.randomString(15, false, true, true, false, false);\
    url = encodeURIComponent(url);\
    sp.currentContext.site.allowUnsafeUpdates = true;\
    var newWeb = sp.currentContext.site.createWeb({ title: 'Wiki Site For Testing', description: 'This is a test, this is only a test', url: url, webTemplate: wikiTemplate});\
    var result = { url: newWeb.url, title: newWeb.title, webTemplate: newWeb.webTemplate };\
    newWeb.delete();\
    result.existsAfterDelete = Enumerable.From(sp.currentContext.site.getAllWebs())\
                                         .Any(function(x) { return x.url == result.url; });\
    result;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        equal("Wiki Site For Testing", data.title, "A result was returned, and the created site had the correct title.");
        equal("WIKI", data.webTemplate, "The created site was a wiki site.");
        ok(data.url.indexOf("http://") == 0, "The created site had a url.");
        equal(false, data.existsAfterDelete, "The site was deleted.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Create and then remove a Document Library to the current web.", function () {
    expect(4);

    var code = "\
    var sp = require(\"SharePoint\");\
    var util = require(\"Utility\");\
    require(\"Linq\");\
    var listTemplates = sp.currentContext.web.getListTemplates();\
    var docLibTemplate = Enumerable.From(listTemplates)\
                                   .Where(function(x) { return x.name == \"Document Library\"; })\
                                   .FirstOrDefault();\
    var url = util.randomString(15, false, true, true, false, false);\
    sp.currentContext.web.allowUnsafeUpdates = true;\
    var newList = sp.currentContext.web.createList({ title: 'Document Library For Testing', description: 'This is a test, this is only a test', url: url, listTemplate: docLibTemplate});\
    var result = { url: newList.url, title: newList.title, listTemplate: newList.serverTemplateId };\
    newList.delete();\
    result.existsAfterDelete = Enumerable.From(sp.currentContext.web.getLists())\
                                         .Any(function(x) { return x.url == result.url; });\
    result;\
    ";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        equal("Document Library For Testing", data.title, "A result was returned, and the created list had the correct title.");
        equal("101", data.listTemplate, "The created list was a document library");
        ok(data.url.indexOf("http://") == 0, "The created list had a url.");
        equal(false, data.existsAfterDelete, "The list was deleted.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Recursively retrieve all SPWebs in the current site", function () {
    expect(1);

    var request = jQuery.ajax({
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_RecursiveWebsRetrieval.js"
    });

    request.done(function (data) {
        ok(data.length > 0, "Expression was run and a correct result returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});
asyncTest("Test Templates", function () {
    expect(2);

    var code = "\
    var sp = require(\"SharePoint\");\
    var util = require(\"Utility\");\
    var Mustache = require(\"Mustache\");\
    require(\"Linq\");\
    var template = sp.loadFileAsString('/Lists/BaristaUnitTests/Content/DemoTemplate.htm');\
    var json = {\
  \"header\": \"Colors\",\
  \"items\": [\
      {\"name\": \"red\", \"first\": true, \"url\": \"#Red\"},\
      {\"name\": \"green\", \"link\": true, \"url\": \"#Green\"},\
      {\"name\": \"blue\", \"link\": true, \"url\": \"#Blue\"}\
  ],\
  \"empty\": false\
};\
    var html = Mustache.render(template, json);\
    html;\
    ";

    var expected = "<h1>Colors</h1>\
    <li><strong>red</strong></li>\
    <li><a href=\"#Green\">green</a></li>\
    <li><a href=\"#Blue\">blue</a></li>";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        ok(data != null, "A result was returned from the service.");
        expected = expected.replace(/(\r\n|\n|\r)/gm, "");
        data = data.replace(/(\r\n|\n|\r)/gm, "");
        equal(data, expected, "The correct rendered template was returned.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Assert that DateTime values retrieved from list item fields are in the correct time zone.", function () {
    expect(1);

    var code = "var sp = require(\"SharePoint\");\
var util = require(\"Utility\");\
require(\"Linq\");\
var url = util.randomString(15, false, true, true, false, false);\
var newList = sp.currentContext.web.createList({\
    title: 'List For Testing ' + util.randomString(5, false, true, true, false, false),\
    description: 'This is a test, this is only a test',\
    url: url,\
    listTemplate: 100\
});\
var item = newList.addItem();\
item.update();\
var result = item.fieldValues[\"Modified\"];\
newList.delete();\
result;";

    var request = jQuery.ajax({
        type: 'POST',
        contentType: "application/json; charset=utf-8",
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval",
        data: JSON.stringify({ code: code })
    });

    request.done(function (data, textStatus, jqXHR) {
        equal(new Date(data).getHours(), (new Date()).getHours(), "The hour component of the date returned matches the local date.");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});

asyncTest("Assert that a Barista Event Receiver is able to be set on a list, and that the script is executed.", function () {
    expect(2);

    var request = jQuery.ajax({
        url: getDomain() + "/_vti_bin/Barista/v1/Barista.svc/eval?c=" + encodeURIComponent(getContentPath()) + "Barista_SPListEventReceiver.js"
    });

    request.done(function (data) {
        ok(typeof data["afterProperties"] !== "undefined", "Expression was run and a result returned.");
        ok(data["afterProperties"]["Event Receiver Executed"] === "true", "Event Receiver Fired");
        start();
    });

    request.fail(function () {
        ok(1 == 0, "Call to service failed.");
        start();
    });
});
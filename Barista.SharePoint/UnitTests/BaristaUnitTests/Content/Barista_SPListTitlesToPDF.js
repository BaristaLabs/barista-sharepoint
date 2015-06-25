var sp = require("SharePoint");
var doc = require("Document");

var innerHtml = "";
var lists = sp.currentContext.web.getLists();
for (i in lists) {
    innerHtml += "<div style='color: blue;'>" + lists[i].title + "</div>"
}

doc.html2Pdf("<html><body>" + innerHtml + "</body></html>");
var sp = require("SharePoint");

var list = new SPList("/Lists/BaristaUnitTests");

list.getItemsByView("All Documents");
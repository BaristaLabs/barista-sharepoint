var sp = require("SharePoint");

var list = new SPList("/Lists/BaristaUnitTests");

var listItems = list.getItems();

listItems;
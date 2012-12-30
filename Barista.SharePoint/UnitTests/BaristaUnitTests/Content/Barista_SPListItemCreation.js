var sp = require("SharePoint");

//Create a new list
var list = new SPList("/Lists/BaristaUnitTests");

var listItems = list.getItems();

listItems;
local progHelper = require("mods").requireFromPlugin("libraries.progHelper")

return {
	name = "progHelper/onlyLoadIfFlagTrigger",
	placements = {
		name = "default",
		data = {
			flag = "",
			invert = false,
			entityTypes = ""
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"flag",
		"invert",
		"entityTypes",
	},
	fieldInformation = {
        entityTypes = {
            fieldType = "list",
            elementSeparator = ",",
            elementDefault = "",
            elementOptions = {
                options = function() return progHelper.getMapSIDs() end,
                searchable = true
            }
        }
    }
}
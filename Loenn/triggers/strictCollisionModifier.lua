local progHelper = require("mods").requireFromPlugin("libraries.progHelper")

return {
	name = "progHelper/strictCollisionModifier",
	placements = {
		name = "default",
		data = { entityTypes = "" }
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"entityTypes"
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
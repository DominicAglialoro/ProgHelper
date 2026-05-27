local progHelper = require("mods").requireFromPlugin("libraries.progHelper")

return {
	name = "progHelper/colliderResizer",
	placements = {
		name = "default",
		alternativeName = "colliderEnlarger",
		data = {
			hitboxLeft = 0,
			hitboxRight = 0,
			hitboxTop = 0,
			hitboxBottom = 0,
			circleRadius = 0,
			resizePickupCollider = false,
			entityTypes = ""
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"hitboxLeft",
		"hitboxRight",
		"hitboxTop",
		"hitboxBottom",
		"circleRadius",
		"entityTypes",
		"resizePickupCollider"
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
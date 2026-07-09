local progHelper = require("mods").requireFromPlugin("libraries.progHelper")

return {
	name = "progHelper/clipPreventionTrigger",
	placements = {
		name = "default",
		data = {
			right = true,
			left = true,
			up = true,
			down = true,
			applyToEntities = false,
			applyToPickupColliders = false,
			entityTypes = ""
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"left",
		"right",
		"up",
		"down",
		"applyToEntities",
		"applyToPickupColliders",
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
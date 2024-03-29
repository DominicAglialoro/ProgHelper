local fakeTilesHelper = require("helpers.fake_tiles")

return {
	name = "progHelper/adjustableFallingBlock",
	placements = {
		name = "default",
		data = {
			tiletype = "3",
			climbFall = true,
			checkAttached = false,
			delay = 0.2,
			playerWait = 0.4,
			behind = false,
			width = 8,
			height = 8
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"delay",
		"playerWait",
		"tiletype",
		"climbFall",
		"checkAttached",
		"behind"
	},
	fieldInformation = fakeTilesHelper.getFieldInformation("tiletype"),
	sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false),
	depth = function(room, entity)
		return entity.behind and 5000 or 0
	end
}
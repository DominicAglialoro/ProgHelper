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
	fieldInformation = fakeTilesHelper.getFieldInformation("tiletype"),
	sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false),
	depth = 0
}
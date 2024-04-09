local fakeTilesHelper = require("helpers.fake_tiles")

return {
	name = "progHelper/crumbleBlockOnJump",
	placements = {
		name = "default",
		data = {
			tiletype = "3",
			delay = 0.2,
			breakSound = "event:/new_content/game/10_farewell/quake_rockbreak",
			triggerOnLean = true,
			permanent = false,
			blendIn = false,
			destroyStaticMovers = false,
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
		"tiletype",
		"breakSound",
		"triggerOnLean",
		"permanent",
		"blendIn",
		"destroyStaticMovers"
	},
	fieldInformation = fakeTilesHelper.getFieldInformation("tiletype"),
	sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false),
	depth = function(room, entity)
		return entity.blendIn and -10501 or -12999
	end
}
local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")

return {
	name = "progHelper/linearIntroCrusher",
	placements = {
		name = "default",
		data = {
			tiletype = "3",
			flags = "",
			width = 8,
			height = 8,
			delay = 1.2,
			speed = 2,
			easingPeriod = 0
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"delay",
		"speed",
		"easingPeriod",
		"tiletype",
		"flags"
	},
	fieldInformation = fakeTilesHelper.getFieldInformation("tiletype"),
	sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false),
	nodeSprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false),
	nodeLineRenderType = "line",
	nodeLimits = { 1, 1 },
	nodeRectangle = function(room, entity, node)
		return utils.rectangle(node.x or 0, node.y or 0, entity.width or 8, entity.height or 8)
	end,
	depth = -10501
}
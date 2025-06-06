local waterfallHelper = require("helpers.waterfalls")

return {
	name = "progHelper/waterfallBg",
	placements = { name = "default" },
	depth = 9000,
	sprite = function(room, entity)
		return waterfallHelper.getWaterfallSprites(room, entity)
	end,
	rectangle = waterfallHelper.getWaterfallRectangle
}
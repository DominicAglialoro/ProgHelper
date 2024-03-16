return {
	name = "progHelper/tilePulseSource",
	placements = {
		name = "default",
		data = { bg = false }
	},
	rectangle = function(room, entity, viewport)
		return {
			x = entity.x,
			y = entity.y,
			width = 8,
			height = 8
		}
	end,
	fillColor = { 0.25, 0.25, 0.25, 0.8 },
	borderColor = { 0.0, 0.0, 0.0, 0.0 },
	depth = -10001
}
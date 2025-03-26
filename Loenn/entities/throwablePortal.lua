local drawableSprite = require("structs.drawable_sprite")

return {
	name = "progHelper/throwablePortal",
	placements = {
		name = "default",
		alternativeName = "atom"
	},
	sprite = function(room, entity)
		local sprite = drawableSprite.fromTexture("loenn/progHelper/throwablePortal", entity)

		sprite.y -= 5

		return sprite
	end,
	depth = 100
}
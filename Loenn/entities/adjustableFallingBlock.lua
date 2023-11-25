local fakeTilesHelper = require("helpers.fake_tiles")

local adjustableFallingBlock = {}

adjustableFallingBlock.depth = 0
adjustableFallingBlock.name = "progHelper/adjustableFallingBlock"
adjustableFallingBlock.placements = {
	name = "block",
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
}

adjustableFallingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
adjustableFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)

return adjustableFallingBlock
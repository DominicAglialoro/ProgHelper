local fakeTilesHelper = require("helpers.fake_tiles")
local utils = require("utils")

local linearIntroCrusher = {}

linearIntroCrusher.depth = 0
linearIntroCrusher.name = "progHelper/linearIntroCrusher"
linearIntroCrusher.placements = {
    name = "block",
    data = {
        tiletype = "3",
        flags = "",
        width = 8,
        height = 8,
        delay = 1.2,
        speed = 2,
		easingPeriod = 0
    }
}

linearIntroCrusher.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")
linearIntroCrusher.nodeLineRenderType = "line"
linearIntroCrusher.nodeLimits = { 1, 1 }
linearIntroCrusher.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
linearIntroCrusher.nodeSprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)

function linearIntroCrusher.nodeRectangle(room, entity, node)
    return utils.rectangle(node.x or 0, node.y or 0, entity.width or 8, entity.height or 8)
end

return linearIntroCrusher
local enums = require('consts.celeste_enums')
local drawableSpriteStruct = require('structs.drawable_sprite')

return {
	name = "progHelper/crumbleJumpThruOnJump",
	placements = {
		name = "default",
		data = {
			delay = 0.2,
			texture = "wood",
			permanent = false,
			width = 8,
			surfaceIndex = -1
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"delay",
		"texture",
		"surfaceIndex",
		"permanent"
	},
	fieldInformation = {
        delay = {fieldType = "number", minimumValue = 0 },
        texture = {
			fieldType = "string",
			options = {
				"wood",
				"dream",
				"temple",
				"templeB",
				"cliffside",
				"reflection",
				"core",
				"moon"
			}
		},
        surfaceIndex = {
            options = enums.tileset_sound_ids,
            fieldType = "integer"
        }
    },
    sprite = function(room, entity)
        local textureRaw = entity.texture and entity.texture ~= "default" and entity.texture or "wood"
        local texture = "objects/jumpthru/" .. textureRaw
    
        local x, y = entity.x or 0, entity.y or 0
        local width = entity.width or 8
    
        local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
        local stopX = startX + math.floor(width / 8) - 1
        local len = stopX - startX
    
        local sprites = {}
    
        for i = 0, len do
            local quadX = 8
            local quadY = 8
    
            if i == 0 then
                quadX = 0
                quadY = room.tilesFg.matrix:get(startX - 1, startY, "0") ~= "0" and 0 or 8
    
            elseif i == len then
                quadY = room.tilesFg.matrix:get(stopX + 1, startY, "0") ~= "0" and 0 or 8
                quadX = 16
            end
    
            local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    
            sprite:setJustification(0, 0)
            sprite:addPosition(i * 8, 0)
            sprite:useRelativeQuad(quadX, quadY, 8, 8)
    
            table.insert(sprites, sprite)
        end
    
        return sprites
    end,
	depth = -60,
    selection = function(room, entity)
        return utils.rectangle(entity.x, entity.y, entity.width, 8)
    end
}
local springDepth = -8501
local springTexture = "objects/spring/00"

local springUp = { }

springUp.name = "progHelper/strictSpring"
springUp.depth = springDepth
springUp.justification = { 0.5, 1.0 }
springUp.texture = springTexture
springUp.placements = {
    name = "default",
    data = { playerCanUse = true }
}

function springUp.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "progHelper/strictWallSpringLeft"
    else
        entity._name = "progHelper/strictWallSpringRight"
    end

    return true
end

local springRight = { }

springRight.name = "progHelper/strictWallSpringLeft"
springRight.depth = springDepth
springRight.justification = { 0.5, 1.0 }
springRight.texture = springTexture
springRight.rotation = math.pi / 2
springRight.placements = {
    name = "default",
    data = { playerCanUse = true }
}

function springRight.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity._name = "progHelper/strictWallSpringRight"
    end

    return horizontal
end

function springRight.rotate(room, entity, direction)
    if direction < 0 then
        entity._name = "progHelper/strictSpring"
    end

    return direction < 0
end

local springLeft = { }

springLeft.name = "progHelper/strictWallSpringRight"
springLeft.depth = springDepth
springLeft.justification = { 0.5, 1.0 }
springLeft.texture = springTexture
springLeft.rotation = -math.pi / 2
springLeft.placements = {
    name = "default",
    data = { playerCanUse = true }
}

function springLeft.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity._name = "progHelper/strictWallSpringLeft"
    end

    return horizontal
end

function springLeft.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "progHelper/strictSpring"
    end

    return direction > 0
end

return {
    springUp,
    springRight,
    springLeft
}
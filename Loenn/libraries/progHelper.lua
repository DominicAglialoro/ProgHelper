local loadedState = require("loaded_state")
local entities = require("entities")

local progHelper = {}

function progHelper.getAllSIDs()
    local sids = {}
    for sid, _ in pairs(entities.registeredEntities) do
        table.insert(sids, sid)
    end
    table.sort(sids)

    return sids
end

function progHelper.getMapSIDs()
    if not loadedState.map then return progHelper.getAllSIDs() end

    local sidsInMap = {}
    for _, room in pairs(loadedState.map.rooms) do
        for _, entity in pairs(room.entities) do
            sidsInMap[entity._name] = true
        end
    end

    local sids = {}
    for sid, _ in pairs(sidsInMap) do
        table.insert(sids, sid)
    end
    table.sort(sids)

    return sids
end

return progHelper
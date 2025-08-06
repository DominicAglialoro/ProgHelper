local spikeHelper = require("helpers.spikes")
local spikeOptions = {
    directionNames = {
        up = "progHelper/strictSpikesUp",
        down = "progHelper/strictSpikesDown",
        left = "progHelper/strictSpikesLeft",
        right = "progHelper/strictSpikesRight"
    },
	variants = { "default" }
}

return spikeHelper.createEntityHandlers(spikeOptions)
local modeOptions = {
	fieldType = "string",
	options = {
		{ "Always", "Always" },
		{ "Never", "Never" },
		{ "In Range", "InRange" },
		{ "Both in Range", "BothInRange" }
	},
	editable = false
}

return {
	name = "progHelper/setPlayerSpeedTrigger",
	placements = {
		name = "default",
		data = {
			speedX = 0,
			speedY = 0,
			modeX = "Always",
			modeY = "Always",
			rangeXMin = 0,
			rangeXMax = 0,
			rangeYMin = 0,
			rangeYMax = 0
		}
	},
	fieldOrder = {
		"x",
		"y",
		"width",
		"height",
		"speedX",
		"speedY",
		"modeX",
		"modeY",
		"rangeXMin",
		"rangeXMax",
		"rangeYMin",
		"rangeYMax"
	},
	fieldInformation = {
		modeX = modeOptions,
		modeY = modeOptions
	}
}
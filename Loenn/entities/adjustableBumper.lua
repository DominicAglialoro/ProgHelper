return {
	name = "progHelper/adjustableBumper",
	placements = {
		{
			name = "default",
			data = {
				respawnTime = 0.6,
				moveTime = 1.818,
				sprite = "bumper",
				bumperBoost = "Default",
				dashRestores = "Default",
				fireMode = "CoreMode",
				cardinal = false,
				snapUp = false,
				snapDown = false,
				wobble = true,
				boostHoldables = false,
				ignoreHoldableWhenHot = false
			}
		},
		{
			name = "oneUse",
			data = {
				respawnTime = 0.6,
				moveTime = 1.818,
				sprite = "bumper",
				bumperBoost = "Default",
				dashRestores = "Default",
				fireMode = "AfterHit",
				cardinal = false,
				snapUp = false,
				snapDown = false,
				wobble = true,
				boostHoldables = false,
				ignoreHoldableWhenHot = false
			}
		},
		{
			name = "noDashRestore",
			data = {
				respawnTime = 0.6,
				moveTime = 1.818,
				sprite = "bumper",
				bumperBoost = "Default",
				dashRestores = "None",
				fireMode = "Never",
				cardinal = false,
				snapUp = false,
				snapDown = false,
				wobble = true,
				boostHoldables = false,
				ignoreHoldableWhenHot = false
			}
		},
		{
			name = "doubleDashRestore",
			data = {
				respawnTime = 0.6,
				moveTime = 1.818,
				sprite = "bumper",
				bumperBoost = "Default",
				dashRestores = "Two",
				fireMode = "Never",
				cardinal = false,
				snapUp = false,
				snapDown = false,
				wobble = true,
				boostHoldables = false,
				ignoreHoldableWhenHot = false
			}
		}
	},
	fieldOrder = {
		"x",
		"y",
		"respawnTime",
		"moveTime",
		"sprite",
		"bumperBoost",
		"dashRestores",
		"fireMode",
		"cardinal",
		"snapUp",
		"snapDown",
		"wobble",
		"boostHoldables"
	},
	fieldInformation = {
		bumperBoost = {
			fieldType = "string",
			options = {
				["Disable"] = "Disable",
				["Default"] = "Default",
				["Force"] = "Force"
			},
			editable = false
		},
		dashRestores = {
			fieldType = "string",
			options = {
				["None"] = "None",
				["Default"] = "Default",
				["Two"] = "Two"
			},
			editable = false
		},
		fireMode = {
			fieldType = "string",
			options = {
				["Never"] = "Never",
				["Core Mode"] = "CoreMode",
				["After Hit"] = "AfterHit",
				["Always"] = "Always"
			},
			editable = false
		}
	},
	nodeLimits = { 0, 1 },
	texture = "objects/Bumper/Idle22"
}
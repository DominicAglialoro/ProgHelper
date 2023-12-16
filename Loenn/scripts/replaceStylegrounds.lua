local state = require("loaded_state")

local script = {
	name = "replaceStylegrounds",
	parameters = {
		oldName = "",
		newName = ""
	},
	displayName = "Replace Stylegrounds",
	tooltip = "",
	tooltips = {
		oldName = "",
		newName = ""
	}
}

function script.prerun(args)
	for _, style in ipairs(state.map.stylesFg) do
		if style._name == args.oldName then
			style._name = args.newName
		end
	end
	
	for _, style in ipairs(state.map.stylesBg) do
		if style._name == args.oldName then
			style._name = args.newName
		end
	end
end

return script
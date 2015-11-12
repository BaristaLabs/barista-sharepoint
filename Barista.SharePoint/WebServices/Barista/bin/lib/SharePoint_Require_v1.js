var vm = require("vm");
var edge = require("edge");

module.exports = function (sandbox, baristaContext, path) {

    var tryGetSPFileAsString = edge.func({
        source: function () {/*

		using Barista.SharePoint;
		using System.Threading.Tasks;

		public class Startup
		{
			public async Task<object> Invoke(string input)
			{
				string filePath, fileContents;
				bool isHiveFile;
				if (SPHelper.TryGetSPFileAsString(input, out filePath, out fileContents, out isHiveFile))
					return new {
						fileContents = fileContents,
						filePath = filePath,
						isHiveFile = isHiveFile
					};
				else if (SPHelper.TryGetSPFileAsString(input.TrimEnd('/') + "/index.js", out filePath, out fileContents, out isHiveFile))
					return new {
						fileContents = fileContents,
						filePath = filePath,
						isHiveFile = isHiveFile
					};
				
				throw new System.Exception("Cannot find module " + input);
			}
		}
	*/},
        references: [
			baristaContext.environment.sharePointAssembly,
			baristaContext.environment.baristaAssembly,
			baristaContext.environment.baristaSharePointAssembly
        ]
    });

    var result = tryGetSPFileAsString(path, true);
    if (result) {
        //Define module and exports on the sandbox;
        sandbox.exports = null;
        sandbox.module = {
            id: result.filePath,
            isHiveFile: result.isHiveFile,
            exports: sandbox.exports
        }

        vm.runInNewContext(result.fileContents, vm.createContext(sandbox), result.filePath);
        return sandbox.module.exports;
    }
    return undefined;
}
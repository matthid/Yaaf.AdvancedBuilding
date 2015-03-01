# Yaaf.AdvancedBuilding build and development documentation 

> NOTE: This document can be referenced from any project using Yaaf.AdvancedBuilding.

## Building

Open the ``.sln`` file or run ``./build``.

> NOTE: You can only build the `.sln` file AFTER doing an initial `./build` (because dependencies have to be restored first).

## Visual Studio / Monodevelop

As mentioned above you need to `build` at least once before you can open the 
solution file (`src/${ProjectName}.sln`) with Visual Studio / Monodevelop.

Just open the solution file and select the build you want to edit in the Configuration-Manager (ie. your Platform).


## The Project structure:

> Note: Projects using Yaaf.AdvancedBuilding can leave out some components or rename them! The following structure represents the default configuration.

- /.paket/

    Files for paket: https://github.com/fsprojects/Paket
	
- /build/

	The project assemblies will be build into this folder. This folder can safely be deleted without affecting the build.

- /doc/

	Project documentation files. This folder contains both development and user documentation.

- /gh-pages/

	Folder for the documentation branch (can be deleted safely).
	
- /lib/

	library dependencies (most of the time not used). Most dependencies are automatically managed by nuget and not in this folder. 
	Only some internal dependencies and packages not in downloaded automatically are available here. 
	The git repository should always be "complete" and ready to be build.

- /packages/

	Dependencies will be downloaded into this folder. 
	This folder can safely be deleted without affecting the build.
	
- /release/

    Place for the build to put the produced results (dll files, docs, nuget packages, ...)

- /src/

	The Solution directory for all projects

	- /src/source/

		The root for all projects (not including unit test projects).
		Each project should have has a corresponding project with the name `Test.${ProjectName}` in the test folder.
		This test project provides unit tests for the project `${ProjectName}`.

	- /src/test/

		The root for all unit test projects.

- /tmp/

	This folder is ignored by git.

- /build.cmd, /build.sh, /build.fsx

	Files to directly start a build including unit tests via console (windows & linux).

	
- /paket.dependencies, /paket.lock

	When those files exist they are used to restore dependencies with paket: https://github.com/fsprojects/Paket 

- /packages.config
	
	When this file exists a `nuget.exe` is downloaded by the build files (if not already restored through paket) and is used to restore BUILD dependencies.
	The build dependencies are restored into /packages and without version in the folder-name (so they look like paket folders and you can switch between both methods).
	
## Advanced Building

The build is done in different steps and you can execute the build until a given step or a single step:

First `build.sh` and `build.cmd` restore a `nuget.exe`, `paket.exe` or both (depending on the project) to restore build dependencies, then build.fsx is invoked:

 - `Clean`: cleans the directories (previous builds)
 - `RestorePackages`: restores nuget packages
 - `SetVersions`: sets the current version
 - `Build_net40`: build/test for net40, availability depends on the concrete project
 - `Build_net45`: build/test for net45, availability depends on the concrete project
 - `Build_profile111`: build/test for portable profile 111, availability depends on the concrete project
 - `Build_ ...`: more platforms / builds may be available depending on the project
 - `CopyToRelease`: copy the generated .dlls to release/lib
 - `LocalDoc`: create the local documentation you can view that locally
 - `All`: this does nothing itself but is used as a marker (executed by default when no parameter is given to ./build)
 - `VersionBump`: commits all current changes (when you change the version before you start the build you will have some files changed)
 - `NuGet`: generates the nuget packages
 - `GithubDoc`: generates the documentation for github
 - `ReleaseGithubDoc`: pushes the documentation to github
 - `Release`: a marker like "All"

You can execute all steps until a given point with `./build #Step#` (replace #Step# with `Build_net40` to execute `Clean`, `RestorePackages`, `SetVersions`, `Build_40`)

You can execute a single step with `build #Step#_single`: For example to build the nuget packages you can just invoke `./build NuGet_single` 

> Of course you need to have the appropriate dlls in place (otherwise the Nuget package creation will fail); ie have build the project before.


There is another (hidden) step `CleanAll` which will clean everything up (even build dependencies and the downloaded Nuget.exe), 
this step is ONLY needed when BUILD dependencies change AND you use `nuget.exe` instead of `paket.exe` to restore them! 
`git clean -d -x -f` is also a good way to do that.


## General overview specific to Yaaf.AdvancedBuilding:

### Issues / Features / TODOs 

New features are accepted via github pull requests (so just fork away right now!):  https://github.com/matthid/Yaaf.AdvancedBuilding

Issues and TODOs are tracked on github, see: https://github.com/matthid/Yaaf.AdvancedBuilding/issues

### Versioning: 

http://semver.org/

### High level documentation ordered by project.

- `Yaaf.AdvancedBuilding`: For now a project to test the building infrastructure, in the future a helper to generate/convert fsproj and csproj files.

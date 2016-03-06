### 0.13.0

 * Propper support for tagging and releasing multiple nuget packages (and uploading ony changed versions).
 * Support for multiple tags (if a repository manages multiple packages and only a single one changed its version).

### 0.12.3

 * Update Dependencies.

### 0.12.2

 * Update FAKE.

### 0.12.1

 * Paket update.

### 0.12.0

 * NUnit3 Support
 * Enable FSF Logging be default
 * Update to FAKE supporting F# warnings in the build script.

### 0.11.8

 * Only push branches when the merging succeeds.
 * Improve error message when merging fails with concrete steps to try locally.

### 0.11.7

 * Update dependencies.
 * Add small helpers to start processes while redirecting their output (live).

### 0.11.6

 * Update dependencies
 * Add diagnostics for documentation failure.

### 0.11.5

 * Add undocumented way to set arguments for FAKE in the documentation generation (for example `BuildInclude.documentationFAKEArgs <- "-nc"` to disable the cache).

### 0.11.4

 * Allow to disable NUnit and MSTest with 'DisableMSTest' and 'DisableNUnit'

### 0.11.3

 * Paket update and lock NUnit.Runner and NUnit

### 0.11.2

 * Run 'paket update' before starting the build when PAKET_UPDATE=y

### 0.11.1

 * Build reference before regular documentation, fixes https://github.com/matthid/Yaaf.AdvancedBuilding/issues/5

### 0.11.0

 * Add support for building on F# 4.0 only installations (VS2015 only) and build with F#4 by default.

### 0.10.0

 * Update to latest FSF, FAKE and FCS.

### 0.9.3

 * Update to latest 2.11.0-alpha2 FSF.
 * Update to alpha FSF.

### 0.9.2

 * Update dependencies.

### 0.9.1

 * Delegate PAKET_VERSION environment variable as argument to paket.bootstrapper.exe.

### 0.9

 * Update to latest FSharp.Core, FSharp.Compiler.Service and FAKE.
 * Use FAKE.exe instead of fsi.exe for documentation generation (fixes build when using FAKE 4.x)
 * Improve for building with TeamFoundation (on-premise and Visual Studio Online)

### 0.8.0

 * Support Net Client profiles.
 * Support more default project layouts out of the box.
 * Support for MSTest.
 * Ignore when Visual Studio locks the build directory.

### 0.7.2

 * added .fake/* (FAKE cache) and more files to gitignore
   use "cp packages/Yaaf.AdvancedBuilding/scaffold/build/.gitignore ./" in the root project directory to apply the change.

### 0.7.1

 * added *.svclog and *.mdb to the bundled gitignore.
   use "cp packages/Yaaf.AdvancedBuilding/scaffold/build/.gitignore ./" in the root project directory to apply the change.

### 0.7.0

 * Update all dependencies.

### 0.6.5

 * Support for building with F# on mono 4.

### 0.6.4

 * Add "AfterBuild" target to fix an inter-target dependency problem when running "AllDocs".

### 0.6.3

 * Paket update
 * Add "WatchDocs" target (currently beta, see https://github.com/tpetricek/FSharp.Formatting/pull/309)

### 0.6.2

 * Update all deps.
 * Enable `FsiEvaluator` to support http://tpetricek.github.io/FSharp.Formatting/evaluation.html

### 0.6.1

 * Combine the documentations steps 
   (slightly increases the time for a "normal" build, but improves the performance for a "release" build.)

### 0.6.0

 * Snippet evaluation support via F# Formatting (disabled until FSF updates).
 * Remove project file creation.

### 0.5.5

 * Update to latest FSharp.Formatting / RazorEngine

### 0.5.4

 * Add the possibility to source the build script and execute the "do_build" function afterwards
   This helps when you run into problems with paket and a locked 'build.sh' file.

### 0.5.3

 * Fix some broken defaults in documentation generation (which leads to broken links).

### 0.5.2

 * You can now disable Github integrations with `EnableGithub = false` (documentation generation).

### 0.5.1

 * Be more verbose in CopyToRelease.
 * You can now specify the used Razor references for the documentation generation via `DocRazorReferences`

### 0.5.0

 * Update to net45 and latest FSharp.Formatting.
 * Documentation output is now live.
 * Latex generation is disabled (fails on latest FSharp.Formatting)

### 0.4.0

 * Rename NuGet target to NuGetPack and introduce NuGetPush. NuGetPush is executed as last step.

### 0.3.6

 * make sure we are on develop branch when the build is on a build server and yaaf_merge_master is true.

### 0.3.5

 * Disable questions when running on a build server.
 * Add feature to create tags and merge to master.

### 0.3.4

 * Blacklist FSharp.Core and mscorlib in FSharp.Formatting
 * Lock dependencies to stabilize build and `paket update` on users.

### 0.3.3

 * fix incorrectly packaged bash scripts.
 
### 0.3.2

 * reference all libraries explicitly (prevents that outdated libraries are loaded from GAC instead of the libdir; EntityFramework).

### 0.3.1

 * Handle .dll files without corresponding .xml file in documentation generation.

### 0.3.0

* Renamed CustomBuildName to PlatformName and set the Platform variable for it.
* We now expect (the simpler) multi platform approach via variable instead of generating files as default.
* Support for building a solution file (FindSolutionFiles) and using the default build folders (UseProjectOutDir).
* Change default TestDir to /build/test instead of /test
* Use FSharp.Core nuget package.
* Added build.targets to make creation of multi platform msbuild files easier.

### 0.2.1

 * Fixed some bundled templates
 * remove an Include to Yaaf.FSharp.Scriping (removes a warning)

### 0.2.0

 * BREAKING: Redesigned how `buildConfig.fsx` has to be written!
   Now you need to implement a BuildConfiguration record type, this helps (in the future)
   that builds don't break when new features are introduced.
   This also allows us to set a lot of defaults for you.
   - you need to set `buildConfig` to a BuildConfiguration instance (all other variables are now ignored).
   - You can see the docs or look into `buildConfigDef.fsx` for a definition.
   - You need to update your `build.fsx` and `generateDocs.fsx` and add
     `#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"`
     on the top (or use the latest from the package (see Quick-start tutorial)).
 * Implemented initial project and solution generation.
   - Can be enabled with `EnableProjectFileCreation`

### 0.1.4

 * `releaseDir` Configuration is no longer required.
 * BREAKING: You can/must specify `outNugetDir` after updating!
 * fix some broken links in scaffold files.
 * add missing generateDocs.fsx scaffold file.
 * fix reference templates ending up in the wrong folder.
 * add chmod +x to default build.sh (because we can't add that to the nuget package)

 ### 0.1.3

 * add build.fsx as well.
 * fixed some problems with use_nuget=false.
 * added some files to the nuget package starting with a dot (nuget removed them previously).

### 0.1.2

 * a CONTRIBUTION.md file is now assumed in project dir (instead of doc/Contributing.md)
 * we assume a LICENSE.md now
 * added some scaffolding files to the nuget package
 * NuGet is now build on ./build All (as test on CI and to allow users to use them internally, its pretty fast anyway)

### 0.1.1

 * Fix invalid backward slash on linux.
 * Don't ask for task and branch push (use "git flow" and push manually!)


### 0.1.0

 * Initial release

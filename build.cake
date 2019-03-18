///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var artifactsDirectory = MakeAbsolute(Directory("./artifacts"));
var solution = "./WebExporter.sln";

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        Information("Clean up artifacts directory");
        CleanDirectory(artifactsDirectory);

        Information("Clean any previous build");
        DotNetCoreClean(solution, new DotNetCoreCleanSettings
        {
            Configuration = configuration
        });
    });

Task("Build")
    .Does(() =>
    {
        Information("Build and publish WebExporter");
        DotNetCorePublish(solution, new DotNetCorePublishSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDirectory,
            Runtime = "linux-x64",
        });
    });

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

RunTarget(target);

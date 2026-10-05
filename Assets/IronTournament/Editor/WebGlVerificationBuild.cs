using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace IronTournament.Editor
{
    public static class WebGlVerificationBuild
    {
        public static void Build()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("At least one enabled scene is required.");
            }

            var output = Path.Combine(
                Path.GetTempPath(),
                $"IronTournamentVerificationWebGL-{Guid.NewGuid():N}");
            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = output,
                    target = BuildTarget.WebGL
                });
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException($"WebGL build failed: {report.summary.result}.");
                }
            }
            finally
            {
                if (Directory.Exists(output))
                {
                    Directory.Delete(output, true);
                }
            }
        }
    }
}

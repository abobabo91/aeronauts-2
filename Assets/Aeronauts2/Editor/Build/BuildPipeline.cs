using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Aeronauts2.Editor.Build
{
    public static class BuildPipeline
    {
        private const string WebGlOutputPath = "Build/WebGL";

        [MenuItem("Aeronauts 2/Build/WebGL")]
        public static void BuildWebGL()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("No enabled scenes are available for the WebGL build.");
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.WebGL,
                    BuildTarget.WebGL))
            {
                throw new InvalidOperationException("Unity could not switch to the WebGL build target.");
            }

            string absoluteOutputPath = Path.GetFullPath(WebGlOutputPath);
            Directory.CreateDirectory(absoluteOutputPath);
            BuildPlayerOptions options = new()
            {
                scenes = scenes,
                locationPathName = absoluteOutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"WebGL build failed: {report.summary.result} with "
                    + $"{report.summary.totalErrors} errors and {report.summary.totalWarnings} warnings.");
            }

            Debug.Log(
                $"Aeronauts 2 WebGL build succeeded at {absoluteOutputPath} "
                + $"({report.summary.totalSize / (1024f * 1024f):0.0} MB).");
        }
    }
}

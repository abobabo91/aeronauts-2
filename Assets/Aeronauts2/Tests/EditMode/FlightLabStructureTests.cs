using System.Linq;
using Aeronauts2.CameraSystem;
using Aeronauts2.Combat;
using Aeronauts2.Flight;
using Aeronauts2.Input;
using Aeronauts2.Presentation;
using Aeronauts2.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Aeronauts2.Tests
{
    public sealed class FlightLabStructureTests
    {
        private const string ScenePath = "Assets/Aeronauts2/Scenes/FlightLab/FlightLab.unity";
        private const string InputPath = "Assets/Aeronauts2/Input/FlightLab.inputactions";

        [Test]
        public void FlightLabContainsRequiredVerticalSliceWithoutMissingScripts()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Assert.That(scene.IsValid(), Is.True);
            Assert.That(Object.FindFirstObjectByType<PlayerInputReader>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<FlightMotor>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<AimCameraController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<FlightDebugHud>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<PlayerWeaponController>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<CombatDebugHud>(), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<BrowserTelemetryReporter>(), Is.Not.Null);
            Assert.That(Object.FindObjectsByType<TargetDummy>(FindObjectsSortMode.None).Length, Is.EqualTo(3));
            Assert.That(Object.FindObjectsByType<TargetHitRegion>(FindObjectsSortMode.None)
                .Any(region => region.RegionType == HitRegionType.Critical), Is.True);
            Assert.That(UnityEngine.Camera.main, Is.Not.Null);

            PlayerWeaponController weapon = Object.FindFirstObjectByType<PlayerWeaponController>();
            Assert.That(weapon.CombatOrigin, Is.Not.Null);
            Assert.That(weapon.CombatOrigin.name, Is.EqualTo("HeadCockpitCombatOrigin"));
            Transform cockpitMarker = GameObject.Find("CockpitMarker").transform;
            Assert.That(weapon.CombatOrigin.parent, Is.EqualTo(cockpitMarker.parent));
            Assert.That(weapon.CombatOrigin.localPosition, Is.EqualTo(cockpitMarker.localPosition));
            Assert.That(GameObject.Find("HeadOriginBlockedMarker"), Is.Not.Null);

            int missingScriptCount = scene.GetRootGameObjects()
                .Sum(CountMissingScriptsRecursively);
            Assert.That(missingScriptCount, Is.EqualTo(0));

            EditorBuildSettingsScene buildScene = EditorBuildSettings.scenes.Single();
            Assert.That(buildScene.enabled, Is.True);
            Assert.That(buildScene.path, Is.EqualTo(ScenePath));
        }

        [Test]
        public void FlightInputMapContainsKeyboardMouseAndGamepadBindings()
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            Assert.That(actions, Is.Not.Null);

            InputActionMap flightMap = actions.FindActionMap("Flight", true);
            Assert.That(flightMap.FindAction("BodySteer", true).bindings.Any(binding => binding.path == "<Keyboard>/w"), Is.True);
            Assert.That(flightMap.FindAction("BodySteer", true).bindings.Any(binding => binding.path == "<Gamepad>/leftStick"), Is.True);
            Assert.That(flightMap.FindAction("Aim", true).bindings.Any(binding => binding.path == "<Pointer>/delta"), Is.True);
            Assert.That(flightMap.FindAction("Aim", true).bindings.Any(binding => binding.path == "<Gamepad>/rightStick"), Is.True);
            Assert.That(flightMap.FindAction("RollLeft", true).bindings.Any(binding => binding.path == "<Keyboard>/q"), Is.True);
            Assert.That(flightMap.FindAction("RollLeft", true).bindings.Any(binding => binding.path == "<Gamepad>/leftShoulder"), Is.True);
            Assert.That(flightMap.FindAction("RollRight", true).bindings.Any(binding => binding.path == "<Keyboard>/e"), Is.True);
            Assert.That(flightMap.FindAction("RollRight", true).bindings.Any(binding => binding.path == "<Gamepad>/rightShoulder"), Is.True);
            Assert.That(flightMap.FindAction("PrimaryFire", true).bindings.Any(binding => binding.path == "<Mouse>/leftButton"), Is.True);
            Assert.That(flightMap.FindAction("PrimaryFire", true).bindings.Any(binding => binding.path == "<Gamepad>/rightTrigger"), Is.True);
            Assert.That(flightMap.FindAction("SecondaryFire", true).bindings.Any(binding => binding.path == "<Mouse>/rightButton"), Is.True);
            Assert.That(flightMap.FindAction("SecondaryFire", true).bindings.Any(binding => binding.path == "<Gamepad>/leftTrigger"), Is.True);
            Assert.That(flightMap.FindAction("ToggleAimLimits", true).bindings.Any(binding => binding.path == "<Keyboard>/v"), Is.True);
            Assert.That(flightMap.FindAction("ToggleAimLimits", true).bindings.Any(binding => binding.path == "<Gamepad>/select"), Is.True);
            Assert.That(flightMap.FindAction("ToggleView", true).bindings.Any(binding => binding.path == "<Keyboard>/c"), Is.True);
            Assert.That(flightMap.FindAction("ToggleView", true).bindings.Any(binding => binding.path == "<Gamepad>/rightStickPress"), Is.True);
        }

        private static int CountMissingScriptsRecursively(GameObject gameObject)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            foreach (Transform child in gameObject.transform)
            {
                count += CountMissingScriptsRecursively(child.gameObject);
            }

            return count;
        }
    }
}

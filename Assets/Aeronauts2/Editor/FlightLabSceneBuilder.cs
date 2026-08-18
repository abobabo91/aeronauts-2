using Aeronauts2.CameraSystem;
using Aeronauts2.Combat;
using Aeronauts2.Flight;
using Aeronauts2.Input;
using Aeronauts2.Presentation;
using Aeronauts2.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Aeronauts2.Editor
{
    public static class FlightLabSceneBuilder
    {
        private const string ScenePath = "Assets/Aeronauts2/Scenes/FlightLab/FlightLab.unity";
        private const string InputActionsPath = "Assets/Aeronauts2/Input/FlightLab.inputactions";
        private const string InputTuningPath = "Assets/Aeronauts2/Settings/InputTuning.asset";
        private const string FlightTuningPath = "Assets/Aeronauts2/Settings/FlightTuning.asset";
        private const string AimTuningPath = "Assets/Aeronauts2/Settings/AimTuning.asset";
        private const string WeaponTuningPath = "Assets/Aeronauts2/Settings/WeaponTuning.asset";
        private const string TargetTuningPath = "Assets/Aeronauts2/Settings/TargetTuning.asset";
        private const string BotTuningPath = "Assets/Aeronauts2/Settings/BotTuning.asset";
        private const string MaterialRoot = "Assets/Aeronauts2/Art/Materials";

        [MenuItem("Aeronauts 2/Rebuild Flight Lab")]
        public static void Build()
        {
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new System.InvalidOperationException($"Input actions not found at {InputActionsPath}.");
            }

            FlightTuning flightTuning = LoadOrCreateAsset<FlightTuning>(FlightTuningPath);
            AimTuning aimTuning = LoadOrCreateAsset<AimTuning>(AimTuningPath);
            InputTuning inputTuning = LoadOrCreateAsset<InputTuning>(InputTuningPath);
            WeaponTuning weaponTuning = LoadOrCreateAsset<WeaponTuning>(WeaponTuningPath);
            TargetTuning targetTuning = LoadOrCreateAsset<TargetTuning>(TargetTuningPath);
            BotTuning botTuning = LoadOrCreateAsset<BotTuning>(BotTuningPath);
            Material darkMaterial = CreateOrUpdateMaterial(
                $"{MaterialRoot}/FlightLab_Dark.mat",
                new Color(0.035f, 0.055f, 0.08f),
                false);
            Material gridMaterial = CreateOrUpdateMaterial(
                $"{MaterialRoot}/FlightLab_Grid.mat",
                new Color(0.08f, 0.22f, 0.28f),
                true);
            Material cyanMaterial = CreateOrUpdateMaterial(
                $"{MaterialRoot}/FlightLab_Cyan.mat",
                new Color(0.05f, 0.85f, 1f),
                true);
            Material orangeMaterial = CreateOrUpdateMaterial(
                $"{MaterialRoot}/FlightLab_Orange.mat",
                new Color(1f, 0.34f, 0.06f),
                true);
            Material playerMaterial = CreateOrUpdateMaterial(
                $"{MaterialRoot}/FlightLab_Player.mat",
                new Color(0.72f, 0.82f, 0.9f),
                false);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FlightLab";

            ConfigureEnvironment(darkMaterial, gridMaterial, cyanMaterial, orangeMaterial);
            TargetDummy[] combatTargets = CreateCombatTargets(
                targetTuning,
                playerMaterial,
                orangeMaterial,
                cyanMaterial);

            GameObject systems = new("Systems");
            PlayerInputReader input = systems.AddComponent<PlayerInputReader>();
            input.Configure(inputActions, inputTuning);

            FlightMotor motor = CreatePlayer(
                input,
                flightTuning,
                targetTuning,
                playerMaterial,
                cyanMaterial,
                out CombatantHealth playerHealth);
            AimCameraController aimCamera = CreateCamera(motor, input, aimTuning);
            PlayerWeaponController weapon = motor.gameObject.AddComponent<PlayerWeaponController>();
            Transform combatOrigin = motor.transform.Find("SharedGameplayFrame/HeadCockpitCombatOrigin");
            weapon.Configure(
                input,
                aimCamera,
                combatOrigin,
                weaponTuning,
                cyanMaterial,
                orangeMaterial);
            PlayerCombatantController playerLifecycle = motor.gameObject
                .AddComponent<PlayerCombatantController>();
            playerLifecycle.Configure(playerHealth, motor, weapon);

            BotFlightController botFlight = CreateBot(
                motor,
                playerHealth,
                flightTuning,
                weaponTuning,
                targetTuning,
                botTuning,
                orangeMaterial,
                cyanMaterial,
                out CombatantHealth botHealth,
                out BotWeaponController botWeapon);
            DuelMatchController match = systems.AddComponent<DuelMatchController>();
            match.Configure(motor, weapon, botWeapon, playerHealth, botHealth, 5);
            CombatDrillController combatDrill = systems.AddComponent<CombatDrillController>();
            combatDrill.Configure(
                motor,
                weapon,
                new CombatantHealth[]
                {
                    playerHealth,
                    botHealth,
                    combatTargets[0],
                    combatTargets[1],
                    combatTargets[2],
                });
            BrowserTelemetryReporter browserTelemetry = systems.AddComponent<BrowserTelemetryReporter>();
            browserTelemetry.Configure(
                motor,
                aimCamera,
                weapon,
                playerHealth,
                botHealth,
                botFlight,
                botWeapon,
                match);
            CreateHud(
                motor,
                aimCamera,
                weapon,
                playerHealth,
                botHealth,
                botFlight,
                botWeapon,
                match);

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new System.InvalidOperationException($"Failed to save {ScenePath}.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
            };
            AssetDatabase.SaveAssets();
            Debug.Log($"Aeronauts 2 Flight Lab rebuilt at {ScenePath}.");
        }

        private static FlightMotor CreatePlayer(
            PlayerInputReader input,
            FlightTuning tuning,
            TargetTuning healthTuning,
            Material bodyMaterial,
            Material accentMaterial,
            out CombatantHealth health)
        {
            GameObject player = new("PlayerFlightFrame");
            player.transform.SetPositionAndRotation(new Vector3(0f, 22f, -65f), Quaternion.identity);
            FlightMotor motor = player.AddComponent<FlightMotor>();
            motor.Configure(input, tuning);
            health = player.AddComponent<CombatantHealth>();
            health.Configure(healthTuning);
            CreateSharedCombatantFrame(player.transform, health, bodyMaterial, accentMaterial);
            return motor;
        }

        private static BotFlightController CreateBot(
            FlightMotor playerMotor,
            CombatantHealth playerHealth,
            FlightTuning flightTuning,
            WeaponTuning weaponTuning,
            TargetTuning healthTuning,
            BotTuning botTuning,
            Material bodyMaterial,
            Material accentMaterial,
            out CombatantHealth botHealth,
            out BotWeaponController botWeapon)
        {
            GameObject bot = new("DuelBotFlightFrame");
            Vector3 spawnPosition = new(55f, 30f, 35f);
            Quaternion spawnRotation = Quaternion.LookRotation(
                (playerMotor.transform.position - spawnPosition).normalized,
                Vector3.up);
            bot.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
            botHealth = bot.AddComponent<CombatantHealth>();
            botHealth.Configure(healthTuning);
            Transform model = CreateSharedCombatantFrame(
                bot.transform,
                botHealth,
                bodyMaterial,
                accentMaterial);

            BotFlightController botFlight = bot.AddComponent<BotFlightController>();
            botFlight.Configure(
                flightTuning,
                botTuning,
                botHealth,
                playerMotor.transform,
                playerHealth,
                new Vector3(0f, 28f, 55f));
            botWeapon = bot.AddComponent<BotWeaponController>();
            botWeapon.Configure(
                botHealth,
                playerHealth,
                playerMotor.transform.Find("SharedGameplayFrame/CockpitMarker"),
                playerMotor,
                model.Find("HeadCockpitCombatOrigin"),
                weaponTuning,
                botTuning,
                accentMaterial);
            return botFlight;
        }

        private static Transform CreateSharedCombatantFrame(
            Transform parent,
            CombatantHealth health,
            Material bodyMaterial,
            Material accentMaterial)
        {
            GameObject model = new("SharedGameplayFrame");
            model.transform.SetParent(parent, false);
            CreateCombatantPrimitive(
                PrimitiveType.Capsule,
                "CentralBody",
                model.transform,
                Vector3.zero,
                new Vector3(0.75f, 1.5f, 0.75f),
                Quaternion.Euler(90f, 0f, 0f),
                bodyMaterial,
                health,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Cube,
                "LeftWing",
                model.transform,
                new Vector3(-2.45f, 0f, 0.15f),
                new Vector3(4.2f, 0.12f, 1.15f),
                Quaternion.Euler(0f, -5f, -4f),
                bodyMaterial,
                health,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Cube,
                "RightWing",
                model.transform,
                new Vector3(2.45f, 0f, 0.15f),
                new Vector3(4.2f, 0.12f, 1.15f),
                Quaternion.Euler(0f, 5f, 4f),
                bodyMaterial,
                health,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Cube,
                "TailFin",
                model.transform,
                new Vector3(0f, 0.75f, -1.2f),
                new Vector3(0.14f, 1.4f, 1.1f),
                Quaternion.identity,
                accentMaterial,
                health,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Sphere,
                "CockpitMarker",
                model.transform,
                new Vector3(0f, 0.25f, 1.15f),
                new Vector3(0.65f, 0.45f, 0.75f),
                Quaternion.identity,
                accentMaterial,
                health,
                HitRegionType.Critical);

            GameObject combatOrigin = new("HeadCockpitCombatOrigin");
            combatOrigin.transform.SetParent(model.transform, false);
            combatOrigin.transform.localPosition = new Vector3(0f, 0.25f, 1.15f);

            AddTrail(model.transform, "LeftTrail", new Vector3(-4.3f, 0f, -0.4f), accentMaterial);
            AddTrail(model.transform, "RightTrail", new Vector3(4.3f, 0f, -0.4f), accentMaterial);
            return model.transform;
        }

        private static AimCameraController CreateCamera(
            FlightMotor motor,
            PlayerInputReader input,
            AimTuning tuning)
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = motor.transform.position + new Vector3(1.4f, 2.2f, -10f);
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.fieldOfView = 75f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 1200f;
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.AddComponent<AudioListener>();
            AimCameraController controller = cameraObject.AddComponent<AimCameraController>();
            controller.Configure(camera, motor, input, tuning);
            return controller;
        }

        private static void ConfigureEnvironment(
            Material darkMaterial,
            Material gridMaterial,
            Material cyanMaterial,
            Material orangeMaterial)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.22f, 0.32f);
            RenderSettings.ambientEquatorColor = new Color(0.05f, 0.08f, 0.13f);
            RenderSettings.ambientGroundColor = new Color(0.015f, 0.02f, 0.03f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.025f, 0.05f, 0.08f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0022f;

            GameObject lightObject = new("Sun");
            lightObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(0.78f, 0.88f, 1f);
            light.shadows = LightShadows.Soft;

            GameObject environment = new("FlightLabEnvironment");
            CreatePrimitive(
                PrimitiveType.Cube,
                "Ground",
                environment.transform,
                new Vector3(0f, -1f, 80f),
                new Vector3(280f, 2f, 360f),
                Quaternion.identity,
                darkMaterial);

            GameObject grid = new("NavigationGrid");
            grid.transform.SetParent(environment.transform, false);
            for (int coordinate = -120; coordinate <= 120; coordinate += 20)
            {
                CreatePrimitive(
                    PrimitiveType.Cube,
                    $"GridX_{coordinate}",
                    grid.transform,
                    new Vector3(coordinate, 0.04f, 60f),
                    new Vector3(0.08f, 0.04f, 320f),
                    Quaternion.identity,
                    gridMaterial);
                CreatePrimitive(
                    PrimitiveType.Cube,
                    $"GridZ_{coordinate}",
                    grid.transform,
                    new Vector3(0f, 0.04f, coordinate + 60f),
                    new Vector3(240f, 0.04f, 0.08f),
                    Quaternion.identity,
                    gridMaterial);
            }

            CreateAltitudePillar(environment.transform, new Vector3(-70f, 30f, 30f), 60f, cyanMaterial);
            CreateAltitudePillar(environment.transform, new Vector3(70f, 45f, 85f), 90f, orangeMaterial);
            CreateAltitudePillar(environment.transform, new Vector3(-65f, 60f, 155f), 120f, cyanMaterial);
            CreateAltitudePillar(environment.transform, new Vector3(65f, 35f, 220f), 70f, orangeMaterial);

            CreateGate(environment.transform, new Vector3(0f, 22f, -5f), Quaternion.identity, 12f, cyanMaterial);
            CreateGate(environment.transform, new Vector3(18f, 32f, 45f), Quaternion.Euler(0f, 18f, 18f), 11f, orangeMaterial);
            CreateGate(environment.transform, new Vector3(-24f, 17f, 95f), Quaternion.Euler(0f, -25f, -12f), 10f, cyanMaterial);
            CreateGate(environment.transform, new Vector3(4f, 45f, 150f), Quaternion.Euler(15f, 12f, 32f), 12f, orangeMaterial);
            CreateGate(environment.transform, new Vector3(0f, 25f, 215f), Quaternion.Euler(-10f, 0f, -25f), 14f, cyanMaterial);
        }

        private static TargetDummy[] CreateCombatTargets(
            TargetTuning tuning,
            Material bodyMaterial,
            Material shieldMaterial,
            Material criticalMaterial)
        {
            GameObject root = new("CombatTargets");
            TargetDummy stationaryTarget = CreateTarget(
                "StationaryTarget",
                root.transform,
                new Vector3(0f, 23.6f, 35f),
                tuning,
                bodyMaterial,
                shieldMaterial,
                criticalMaterial);

            TargetDummy strafingTarget = CreateTarget(
                "StrafingTarget",
                root.transform,
                new Vector3(-25f, 35f, 100f),
                tuning,
                bodyMaterial,
                shieldMaterial,
                criticalMaterial);
            TargetMover strafingMover = strafingTarget.gameObject.AddComponent<TargetMover>();
            strafingMover.Configure(
                new Vector3(-25f, 35f, 100f),
                new Vector3(25f, 35f, 100f),
                13f);

            TargetDummy verticalTarget = CreateTarget(
                "VerticalTarget",
                root.transform,
                new Vector3(36f, 15f, 165f),
                tuning,
                bodyMaterial,
                shieldMaterial,
                criticalMaterial);
            TargetMover verticalMover = verticalTarget.gameObject.AddComponent<TargetMover>();
            verticalMover.Configure(
                new Vector3(36f, 15f, 165f),
                new Vector3(36f, 58f, 165f),
                9f);

            return new[]
            {
                stationaryTarget,
                strafingTarget,
                verticalTarget,
            };
        }

        private static TargetDummy CreateTarget(
            string name,
            Transform parent,
            Vector3 position,
            TargetTuning tuning,
            Material bodyMaterial,
            Material shieldMaterial,
            Material criticalMaterial)
        {
            GameObject root = new(name);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 180f, 0f));
            TargetDummy target = root.AddComponent<TargetDummy>();
            target.Configure(tuning);

            CreateCombatantPrimitive(
                PrimitiveType.Cube,
                "BodyRegion",
                root.transform,
                Vector3.zero,
                new Vector3(6f, 2.8f, 2.6f),
                Quaternion.identity,
                bodyMaterial,
                target,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Cube,
                "LeftWingRegion",
                root.transform,
                new Vector3(-4.1f, 0f, 0.2f),
                new Vector3(2.3f, 0.35f, 1.8f),
                Quaternion.Euler(0f, 0f, -6f),
                shieldMaterial,
                target,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Cube,
                "RightWingRegion",
                root.transform,
                new Vector3(4.1f, 0f, 0.2f),
                new Vector3(2.3f, 0.35f, 1.8f),
                Quaternion.Euler(0f, 0f, 6f),
                shieldMaterial,
                target,
                HitRegionType.Body);
            CreateCombatantPrimitive(
                PrimitiveType.Sphere,
                "CockpitCriticalRegion",
                root.transform,
                new Vector3(0f, 0.6f, 1.55f),
                new Vector3(1.35f, 1.15f, 0.9f),
                Quaternion.identity,
                criticalMaterial,
                target,
                HitRegionType.Critical);
            return target;
        }

        private static void CreateHud(
            FlightMotor motor,
            AimCameraController aimCamera,
            PlayerWeaponController weapon,
            CombatantHealth playerHealth,
            CombatantHealth botHealth,
            BotFlightController botFlight,
            BotWeaponController botWeapon,
            DuelMatchController match)
        {
            GameObject canvasObject = new("FlightLabHUD");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Text status = CreateText(
                canvasObject.transform,
                "FlightStatus",
                font,
                25,
                TextAnchor.UpperLeft,
                new Vector2(24f, -24f),
                new Vector2(650f, 250f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f));
            status.color = new Color(0.7f, 0.95f, 1f);

            Text combatStatus = CreateText(
                canvasObject.transform,
                "CombatStatus",
                font,
                25,
                TextAnchor.UpperRight,
                new Vector2(-24f, -24f),
                new Vector2(720f, 430f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f));
            combatStatus.color = new Color(1f, 0.78f, 0.42f);

            Text instructions = CreateText(
                canvasObject.transform,
                "Instructions",
                font,
                21,
                TextAnchor.LowerLeft,
                new Vector2(24f, 24f),
                new Vector2(1000f, 150f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f));
            instructions.text =
                "W/S pitch   A/D yaw   HOLD Q/E roll   DOUBLE-TAP Q/E 360 roll   SPACE accelerate   CTRL airbrake   SHIFT boost   R reset\n"
                + "C / R3 aim-forward view   V / Select test aim limits   LMB / RT projectile primary   RMB / LT instant railgun\n"
                + "Bank + pitch to turn; airbrake then boost for slingshot";
            instructions.color = new Color(0.78f, 0.86f, 0.95f);

            CreateCrosshair(canvasObject.transform);
            Text hitMarker = CreateText(
                canvasObject.transform,
                "HitMarker",
                font,
                27,
                TextAnchor.MiddleCenter,
                Vector2.zero,
                new Vector2(70f, 70f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));
            hitMarker.enabled = false;
            Text blockedMarker = CreateText(
                canvasObject.transform,
                "HeadOriginBlockedMarker",
                font,
                18,
                TextAnchor.MiddleCenter,
                new Vector2(0f, -48f),
                new Vector2(240f, 40f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f));
            blockedMarker.text = "HEAD ORIGIN BLOCKED";
            blockedMarker.color = new Color(1f, 0.4f, 0.08f);
            blockedMarker.enabled = false;
            FlightDebugHud hud = canvasObject.AddComponent<FlightDebugHud>();
            hud.Configure(motor, aimCamera, status);
            CombatDebugHud combatHud = canvasObject.AddComponent<CombatDebugHud>();
            combatHud.Configure(
                weapon,
                playerHealth,
                botHealth,
                botFlight,
                botWeapon,
                match,
                combatStatus,
                hitMarker,
                blockedMarker);
        }

        private static Text CreateText(
            Transform parent,
            string name,
            Font font,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 anchorMin,
            Vector2 anchorMax)
        {
            GameObject textObject = new(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void CreateCrosshair(Transform parent)
        {
            GameObject root = new("CenteredReticle", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = new Vector2(54f, 54f);

            CreateReticleBar(root.transform, "Top", new Vector2(0f, 18f), new Vector2(2f, 14f));
            CreateReticleBar(root.transform, "Bottom", new Vector2(0f, -18f), new Vector2(2f, 14f));
            CreateReticleBar(root.transform, "Left", new Vector2(-18f, 0f), new Vector2(14f, 2f));
            CreateReticleBar(root.transform, "Right", new Vector2(18f, 0f), new Vector2(14f, 2f));
            CreateReticleBar(root.transform, "Center", Vector2.zero, new Vector2(3f, 3f));
        }

        private static void CreateReticleBar(Transform parent, string name, Vector2 position, Vector2 size)
        {
            GameObject bar = new(name, typeof(RectTransform));
            bar.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)bar.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = bar.AddComponent<Image>();
            image.color = new Color(0.8f, 0.97f, 1f, 0.95f);
            image.raycastTarget = false;
        }

        private static void CreateGate(
            Transform parent,
            Vector3 position,
            Quaternion rotation,
            float openingSize,
            Material material)
        {
            GameObject gate = new($"Gate_{position.z:000}");
            gate.transform.SetParent(parent, false);
            gate.transform.SetPositionAndRotation(position, rotation);
            float barThickness = 0.45f;
            float barLength = openingSize + barThickness * 2f;
            float halfOpening = openingSize * 0.5f + barThickness * 0.5f;
            CreatePrimitive(PrimitiveType.Cube, "Top", gate.transform, new Vector3(0f, halfOpening, 0f), new Vector3(barLength, barThickness, barThickness), Quaternion.identity, material);
            CreatePrimitive(PrimitiveType.Cube, "Bottom", gate.transform, new Vector3(0f, -halfOpening, 0f), new Vector3(barLength, barThickness, barThickness), Quaternion.identity, material);
            CreatePrimitive(PrimitiveType.Cube, "Left", gate.transform, new Vector3(-halfOpening, 0f, 0f), new Vector3(barThickness, barLength, barThickness), Quaternion.identity, material);
            CreatePrimitive(PrimitiveType.Cube, "Right", gate.transform, new Vector3(halfOpening, 0f, 0f), new Vector3(barThickness, barLength, barThickness), Quaternion.identity, material);
        }

        private static void CreateAltitudePillar(Transform parent, Vector3 position, float height, Material material)
        {
            CreatePrimitive(
                PrimitiveType.Cube,
                $"AltitudePillar_{height:000}",
                parent,
                position,
                new Vector3(2.5f, height, 2.5f),
                Quaternion.identity,
                material);
        }

        private static GameObject CreatePrimitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Material material)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localRotation = rotation;
            primitive.transform.localScale = scale;
            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            Renderer renderer = primitive.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            return primitive;
        }

        private static GameObject CreateCombatantPrimitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Material material,
            CombatantHealth target,
            HitRegionType regionType)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = position;
            primitive.transform.localRotation = rotation;
            primitive.transform.localScale = scale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            TargetHitRegion hitRegion = primitive.AddComponent<TargetHitRegion>();
            hitRegion.Configure(target, regionType);
            return primitive;
        }

        private static void AddTrail(Transform parent, string name, Vector3 localPosition, Material material)
        {
            GameObject trailObject = new(name);
            trailObject.transform.SetParent(parent, false);
            trailObject.transform.localPosition = localPosition;
            TrailRenderer trail = trailObject.AddComponent<TrailRenderer>();
            trail.time = 0.65f;
            trail.startWidth = 0.16f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.2f;
            trail.sharedMaterial = material;
            trail.emitting = true;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material CreateOrUpdateMaterial(string path, Color color, bool emission)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else
            {
                material.color = color;
            }

            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.5f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }
    }
}

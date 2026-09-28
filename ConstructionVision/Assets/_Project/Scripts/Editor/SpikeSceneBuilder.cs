using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ConstructionVision.EditorTools
{
    [InitializeOnLoad]
    public static class SpikeSceneBuilder
    {
        const float F = 0.3048f;

        static SpikeSceneBuilder()
        {
            EditorApplication.delayCall += TryAutoBuild;
        }

        static void TryAutoBuild()
        {
            var flag = System.IO.Path.Combine(Application.dataPath, "_Project/rebuild-house.txt");
            if (!System.IO.File.Exists(flag))
                return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryAutoBuild;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += TryAutoBuild;
                return;
            }

            System.IO.File.Delete(flag);
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.isDirty && !string.IsNullOrEmpty(scene.path))
                EditorSceneManager.SaveScene(scene);
            Build();
        }
        const float FloorHeight = 8.5f;

        [MenuItem("Construction Vision/Build Spike House")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("House");

            var wall = Mat("Wall", new Color(0.93f, 0.91f, 0.87f), 0.14f);
            var floor = Mat("Floor", new Color(0.94f, 0.92f, 0.89f), 0.58f);
            var upper = Mat("UpperFloor", new Color(0.93f, 0.91f, 0.88f), 0.52f);
            var outside = Mat("Plot", new Color(0.82f, 0.88f, 0.74f), 0.08f);
            var doorMat = Mat("Door", new Color(0.78f, 0.66f, 0.50f), 0.38f);
            var trim = Mat("Trim", new Color(0.95f, 0.93f, 0.90f), 0.18f);
            var stairMat = Mat("Stairs", new Color(0.80f, 0.68f, 0.52f), 0.36f);
            var railMat = Mat("Rail", new Color(0.86f, 0.90f, 0.92f), 0.82f);
            var parkingMat = Mat("Parking", new Color(0.70f, 0.68f, 0.64f), 0.28f);
            var glass = Mat("Glass", new Color(0.86f, 0.91f, 0.93f, 0.2f), 0.88f);
            var roofMat = Mat("Roof", new Color(0.45f, 0.44f, 0.42f), 0.16f);

            Cube("Plot", 15f, -0.3f, 16f, 80f, 0.4f, 100f, outside, root.transform);
            Cube("Ground floor", 15f, -0.12f, 25f, 30f, 0.24f, 50f, floor, root.transform);
            Cube("Front path", 15f, -0.02f, -6f, 6.2f, 0.08f, 12f, parkingMat, root.transform);
            Cube("Parking", 6f, -0.02f, -12f, 11f, 0.06f, 16f, parkingMat, root.transform);

            FrontWall(wall, glass, root.transform);
            WallX("Back left", 0f, 2f, 49.75f, 0f, FloorHeight, wall, root.transform);
            WallX("Back under bedroom window", 2f, 7f, 49.75f, 0f, 3.2f, wall, root.transform);
            WallX("Back over bedroom window", 2f, 7f, 49.75f, 6.3f, FloorHeight, wall, root.transform);
            WallX("Back mid", 7f, 20f, 49.75f, 0f, FloorHeight, wall, root.transform);
            WallX("Back under stair window", 20f, 26f, 49.75f, 0f, 3.2f, wall, root.transform);
            WallX("Back over stair window", 20f, 26f, 49.75f, 6.3f, FloorHeight, wall, root.transform);
            WallX("Back right", 26f, 30f, 49.75f, 0f, FloorHeight, wall, root.transform);
            Pane("Back bedroom window", 4.5f, 4.75f, 49.75f, 4.8f, 3f, glass, root.transform);
            Pane("Back stair window", 23f, 4.75f, 49.75f, 5.8f, 3f, glass, root.transform);
            GrillX("Back bedroom grill", 2.2f, 6.8f, 3.3f, 6.2f, 50.15f, root.transform);
            GrillX("Back stair grill", 20.2f, 25.8f, 3.3f, 6.2f, 50.15f, root.transform);
            SideWall("Left", 0.25f, wall, glass, root.transform);
            SideWall("Right", 29.75f, wall, glass, root.transform);

            WallX("Living divider left", 0f, 1.2f, 20f, 0f, FloorHeight, wall, root.transform);
            WallX("Living divider mid", 5.2f, 18f, 20f, 0f, FloorHeight, wall, root.transform);
            WallX("Living divider right", 24f, 30f, 20f, 0f, FloorHeight, wall, root.transform);
            WallX("Bedroom divider left", 0f, 1.2f, 34f, 0f, FloorHeight, wall, root.transform);
            WallX("Bedroom divider mid", 5.2f, 30f, 34f, 0f, FloorHeight, wall, root.transform);
            WallZ("Room split", 14f, 20f, 34f, 0f, FloorHeight, wall, root.transform);
            WallZ("Stair wall back", 14f, 44f, 50f, 0f, FloorHeight, wall, root.transform);
            WallZ("Stair wall front", 14f, 34f, 40f, 0f, FloorHeight, wall, root.transform);

            BuildDoor(root.transform, doorMat, trim);
            BuildStairs(root.transform, stairMat);
            BuildUpperFloor(root.transform, upper, wall, railMat, roofMat);

            var plaque = Cube("Bedroom plaque", 0.62f, 4.8f, 41.2f, 0.08f, 0.7f, 1.4f, doorMat, root.transform);
            var info = plaque.AddComponent<RoomInfo>();
            info.title = "Bedroom";
            info.detail = "Approx. 12 × 14 ft";

            Furnish(root.transform);

            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.85f;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.7f;
            sunGo.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            sunGo.AddComponent<UniversalAdditionalLightData>();

            PointLight("Porch light", 15f, 6.8f, -1.2f, root.transform, 5.5f, 1.4f);
            PointLight("Living light", 24f, 7.15f, 12f, root.transform, 7f, 1.7f);
            PointLight("Kitchen light", 8f, 7.15f, 27f, root.transform, 6f, 1.35f);
            PointLight("Dining light", 22f, 7.15f, 27f, root.transform, 6.5f, 1.45f);
            PointLight("Bedroom light", 5f, 7.1f, 44f, root.transform, 6f, 1.25f);
            PointLight("Upper light", 22f, FloorHeight + 6.4f, 10f, root.transform, 7f, 1.45f);
            PointLight("Upper bedroom light", 5f, FloorHeight + 6.4f, 40f, root.transform, 6f, 1.15f);
            ApplySky();
            Look();

            var player = BuildPlayer();
            var gate = player.GetComponent<HouseSpecGate>();
            gate.movement = player.GetComponent<FirstPersonController>();
            gate.interactor = player.GetComponent<Interactor>();

            Physics.SyncTransforms();
            OpenEntranceDoor();
            ProveRoute();
            CloseEntranceDoor();

            DirectoryReady("Assets/_Project/Scenes");
            var scenePath = "Assets/_Project/Scenes/House.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            EditorSceneManager.playModeStartScene = sceneAsset;
            PlayerSettings.productName = "Construction Vision";
            PlayerSettings.companyName = "ConstructionVision";
            AssetDatabase.SaveAssets();
            Debug.Log("Spike house built: " + scenePath);
        }

        [MenuItem("Construction Vision/Build Desktop Players")]
        public static void BuildDesktop()
        {
            var scene = "Assets/_Project/Scenes/House.unity";
            var macDir = "Builds/macOS";
            var winDir = "Builds/Windows";
            System.IO.Directory.CreateDirectory(macDir);
            System.IO.Directory.CreateDirectory(winDir);

            var mac = BuildPipeline.BuildPlayer(new[] { scene }, macDir + "/ConstructionVision.app", BuildTarget.StandaloneOSX, BuildOptions.None);
            if (mac.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.InvalidOperationException("macOS build failed: " + mac.summary.result);

            var win = BuildPipeline.BuildPlayer(new[] { scene }, winDir + "/ConstructionVision.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            if (win.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.InvalidOperationException("Windows build failed: " + win.summary.result);

            Debug.Log("Desktop builds written to Builds/macOS and Builds/Windows");
        }

        static void FrontWall(Material wall, Material glass, Transform parent)
        {
            WallX("Front far left", 0f, 3f, 0.25f, 0f, FloorHeight, wall, parent);
            WallX("Front left of door", 7f, 13.4f, 0.25f, 0f, FloorHeight, wall, parent);
            WallX("Front under left window", 3f, 7f, 0.25f, 0f, 3f, wall, parent);
            WallX("Front over left window", 3f, 7f, 0.25f, 6f, FloorHeight, wall, parent);
            Pane("Left front window", 5f, 4.5f, 0.25f, 4f, 3f, glass, parent);
            GrillX("Left front grill", 3.15f, 6.85f, 3.1f, 5.9f, -0.12f, parent);
            Cube("Left chajja", 5f, 6.35f, -0.55f, 4.8f, 0.16f, 1.5f, wall, parent);

            WallX("Front header", 13.4f, 16.6f, 0.25f, 7f, FloorHeight, wall, parent);
            WallX("Front right of door", 16.6f, 21f, 0.25f, 0f, FloorHeight, wall, parent);
            WallX("Front far right", 26f, 30f, 0.25f, 0f, FloorHeight, wall, parent);
            WallX("Front under right window", 21f, 26f, 0.25f, 0f, 3f, wall, parent);
            WallX("Front over right window", 21f, 26f, 0.25f, 6f, FloorHeight, wall, parent);
            Pane("Right front window", 23.5f, 4.5f, 0.25f, 5f, 3f, glass, parent);
            GrillX("Right front grill", 21.15f, 25.85f, 3.1f, 5.9f, -0.12f, parent);
            Cube("Right chajja", 23.5f, 6.35f, -0.55f, 5.6f, 0.16f, 1.5f, wall, parent);
            Cube("Door chajja", 15f, 7.45f, -0.85f, 6.4f, 0.18f, 2f, wall, parent);
        }

        static void SideWall(string name, float x, Material wall, Material glass, Transform parent)
        {
            WallZ(name + " front", x, 0f, 22f, 0f, FloorHeight, wall, parent);
            WallZ(name + " under window", x, 22f, 30f, 0f, 3f, wall, parent);
            WallZ(name + " over window", x, 22f, 30f, 6f, FloorHeight, wall, parent);
            WallZ(name + " after kitchen window", x, 30f, 36f, 0f, FloorHeight, wall, parent);
            WallZ(name + " under bedroom window", x, 36f, 43f, 0f, 3.2f, wall, parent);
            WallZ(name + " over bedroom window", x, 36f, 43f, 6.3f, FloorHeight, wall, parent);
            WallZ(name + " back", x, 43f, 50f, 0f, FloorHeight, wall, parent);
            var sign = x < 15f ? -1f : 1f;
            Cube(name + " window", x, 4.5f, 26f, 0.12f, 3f, 7.6f, glass, parent);
            GrillZ(name + " grill", x + sign * 0.42f, 22.3f, 29.7f, 3.1f, 5.9f, parent);
            Cube(name + " chajja", x + sign * 0.7f, 6.35f, 26f, 1.15f, 0.16f, 8.2f, wall, parent);
            Cube(name + " bedroom window", x, 4.75f, 39.5f, 0.12f, 3f, 6.6f, glass, parent);
            GrillZ(name + " bedroom grill", x + sign * 0.42f, 36.3f, 42.7f, 3.3f, 6.2f, parent);
            Cube(name + " bedroom chajja", x + sign * 0.7f, 6.5f, 39.5f, 1.15f, 0.16f, 7.4f, wall, parent);
        }

        static void Pane(string name, float x, float y, float z, float sx, float sy, Material glass, Transform parent)
        {
            Cube(name, x, y, z, sx, sy, 0.12f, glass, parent);
        }

        static void BuildDoor(Transform parent, Material mat, Material trim)
        {
            Cube("Door frame left", 13.1f, 3.6f, -0.15f, 0.4f, 7.4f, 0.35f, trim, parent);
            Cube("Door frame right", 16.95f, 3.6f, -0.15f, 0.4f, 7.4f, 0.35f, trim, parent);
            Cube("Door frame top", 15f, 7.15f, -0.15f, 4.25f, 0.45f, 0.35f, trim, parent);

            var hinge = new GameObject("EntranceDoor");
            hinge.transform.SetParent(parent, false);
            hinge.transform.position = new Vector3(13.4f, 0f, 0.05f) * F;
            hinge.AddComponent<Door>();

            var panel = Cube("Entrance door panel", 0f, 0f, 0f, 3.15f, 6.85f, 0.28f, mat, hinge.transform);
            panel.transform.localPosition = new Vector3(1.58f, 3.42f, -0.12f) * F;
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = new Vector3(3.15f, 6.85f, 0.28f) * F;
            Soft(Cube("Door top panel", 14.98f, 5.05f, -0.26f, 2.35f, 2.15f, 0.06f, trim, panel.transform));
            Soft(Cube("Door bottom panel", 14.98f, 2.15f, -0.26f, 2.35f, 2.7f, 0.06f, trim, panel.transform));
            Soft(Cube("Door handle", 16.15f, 3.3f, -0.32f, 0.35f, 0.12f, 0.12f, Mat("Brass", new Color(0.72f, 0.58f, 0.22f), 0.7f), panel.transform));
        }

        static void CloseEntranceDoor()
        {
            var door = Object.FindFirstObjectByType<Door>();
            door.open = false;
            door.transform.localRotation = Quaternion.identity;
        }

        static void BuildStairs(Transform parent, Material mat)
        {
            const int count = 14;
            const float rise = FloorHeight / count;
            const float run = 0.72f;
            const float x0 = 16.3f;
            const float z = 42f;
            for (var i = 0; i < count; i++)
            {
                var top = rise * (i + 1);
                Cube("Step " + (i + 1), x0 + run * (i + 0.5f), top - 0.07f, z, run * 0.98f, 0.14f, 3.15f, mat, parent);
            }
        }

        static void BuildUpperFloor(Transform parent, Material floorMat, Material wall, Material rail, Material roof)
        {
            Slab("Upper front", 0f, 30f, 0f, 39.5f, floorMat, parent);
            Slab("Upper bedroom side", 0f, 15f, 39.5f, 50f, floorMat, parent);
            Slab("Upper back", 15f, 30f, 44.5f, 50f, floorMat, parent);
            Slab("Upper landing", 26.2f, 30f, 39.5f, 44.5f, floorMat, parent);
            Cube("Roof", 15f, 17.55f, 22f, 34f, 0.4f, 58f, roof, parent);

            WallX("Upper front left", 0f, 10f, 0.25f, FloorHeight, 17f, wall, parent);
            WallX("Upper front right a", 18f, 22f, 0.25f, FloorHeight, 17f, wall, parent);
            WallX("Upper front under window", 22f, 27.5f, 0.25f, FloorHeight, FloorHeight + 2.8f, wall, parent);
            WallX("Upper front over window", 22f, 27.5f, 0.25f, FloorHeight + 6.2f, 17f, wall, parent);
            WallX("Upper front right b", 27.5f, 30f, 0.25f, FloorHeight, 17f, wall, parent);
            WallX("Upper front header", 10f, 18f, 0.25f, 15.5f, 17f, wall, parent);
            Pane("Upper front window", 24.75f, FloorHeight + 4.5f, 0.25f, 5.2f, 3.2f, Mat("Glass", new Color(0.70f, 0.84f, 0.90f, 0.38f), 0.7f), parent);
            GrillX("Upper front grill", 22.2f, 27.3f, FloorHeight + 2.95f, FloorHeight + 6.05f, -0.12f, parent);
            Cube("Upper front chajja", 24.75f, FloorHeight + 6.45f, -0.55f, 6.2f, 0.16f, 1.4f, wall, parent);

            WindowedBack(wall, parent);
            WindowedSide("Upper left", 0.25f, -1f, wall, parent);
            WindowedSide("Upper right", 29.75f, 1f, wall, parent);

            Slab("Balcony", 8f, 22f, -6f, 0.4f, floorMat, parent);
            Rail("Balcony front rail", 8f, 22f, -5.85f, -5.85f, rail, parent);
            Rail("Balcony left rail", 8.15f, 8.15f, -6f, 0f, rail, parent);
            Rail("Balcony right rail", 21.85f, 21.85f, -6f, 0f, rail, parent);

            Rail("Stair rail front", 15f, 26.5f, 39.6f, 39.6f, rail, parent);
            Rail("Stair rail back", 15f, 26.5f, 44.4f, 44.4f, rail, parent);
            Rail("Stair rail left", 15.1f, 15.1f, 39.5f, 44.5f, rail, parent);
        }

        static void Slab(string name, float x0, float x1, float z0, float z1, Material mat, Transform parent)
        {
            Cube(name, (x0 + x1) * 0.5f, FloorHeight - 0.18f, (z0 + z1) * 0.5f, Mathf.Abs(x1 - x0), 0.36f, Mathf.Abs(z1 - z0), mat, parent);
        }

        static void Rail(string name, float x0, float x1, float z0, float z1, Material mat, Transform parent)
        {
            var sx = Mathf.Abs(x1 - x0);
            var sz = Mathf.Abs(z1 - z0);
            if (sx < 0.4f)
                sx = 0.06f;
            if (sz < 0.4f)
                sz = 0.06f;
            var glass = Mat("Glass", new Color(0.88f, 0.92f, 0.94f, 0.22f), 0.9f);
            var metal = Mat("Metal", new Color(0.28f, 0.28f, 0.29f), 0.62f);
            var x = (x0 + x1) * 0.5f;
            var z = (z0 + z1) * 0.5f;
            Cube(name, x, FloorHeight + 1.35f, z, Mathf.Max(sx, 0.06f), 2.45f, Mathf.Max(sz, 0.06f), glass, parent);
            Cube(name + " cap", x, FloorHeight + 2.62f, z, Mathf.Max(sx, 0.1f), 0.07f, Mathf.Max(sz, 0.1f), metal, parent);
        }

        static GameObject BuildPlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(15f, 0.05f, -4f) * F;
            var body = player.AddComponent<CharacterController>();
            body.height = 1.75f;
            body.radius = 0.28f;
            body.center = new Vector3(0f, 0.9f, 0f);
            body.stepOffset = 0.35f;
            body.slopeLimit = 55f;
            body.skinWidth = 0.05f;
            player.AddComponent<FirstPersonController>();

            var cameraGo = new GameObject("View");
            cameraGo.transform.SetParent(player.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var camera = cameraGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.08f;
            camera.farClipPlane = 120f;
            camera.fieldOfView = 68f;
            camera.clearFlags = RenderSettings.skybox != null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.64f, 0.74f, 0.82f);
            cameraGo.AddComponent<AudioListener>();
            var cameraData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
            player.AddComponent<Interactor>();
            player.AddComponent<HouseSpecGate>();
            return player;
        }

        static void OpenEntranceDoor()
        {
            var door = Object.FindFirstObjectByType<Door>();
            door.open = true;
            door.transform.localRotation = Quaternion.Euler(0f, door.openAngle, 0f);
            Physics.SyncTransforms();
        }

        static void ProveRoute()
        {
            var y = 0.05f;
            var up = FloorHeight + 0.05f;
            Check("entrance", 15f, y, -3f, 15f, y, 8f);
            Check("to kitchen door", 15f, y, 8f, 3.2f, y, 16f);
            Check("into kitchen", 3.2f, y, 16f, 3.2f, y, 24f);
            Check("through kitchen", 3.2f, y, 24f, 7f, y, 28f);
            Check("to bedroom door", 7f, y, 28f, 3.2f, y, 32f);
            Check("into bedroom", 3.2f, y, 32f, 3.2f, y, 38f);
            Check("through bedroom", 3.2f, y, 38f, 7f, y, 42f);
            Check("to stairs", 7f, y, 42f, 12f, y, 42f);
            Check("into stairs", 12f, y, 42f, 14.7f, y, 42f);
            Check("upper hall", 28.2f, up, 36f, 16f, up, 12f);
            Check("onto balcony", 16f, up, 12f, 15f, up, -3f);

            if (GameObject.Find("Step 1") == null || GameObject.Find("Step 14") == null)
                throw new System.InvalidOperationException("Stairs are missing.");
        }

        static void Check(string name, float x0, float y0, float z0, float x1, float y1, float z1)
        {
            var from = new Vector3(x0, y0, z0) * F;
            var to = new Vector3(x1, y1, z1) * F;
            var delta = to - from;
            var distance = delta.magnitude;
            var direction = delta / distance;
            var bottom = from + Vector3.up * 0.35f;
            var top = from + Vector3.up * 1.45f;
            if (Physics.CapsuleCast(bottom, top, 0.22f, direction, out var hit, distance, ~0, QueryTriggerInteraction.Ignore))
                throw new System.InvalidOperationException("Route blocked at " + name + " by " + hit.collider.name);
        }

        static void PointLight(string name, float x, float y, float z, Transform parent, float range = 6.5f, float intensity = 1.5f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, y, z) * F;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.color = new Color(1f, 0.91f, 0.78f);
            go.AddComponent<UniversalAdditionalLightData>();
        }

        static void ApplySky()
        {
            var hdri = LoadPbr("sky.hdr", false, false);
            var skyShader = Shader.Find("Skybox/Panoramic");
            if (hdri != null && skyShader != null)
            {
                var sky = new Material(skyShader);
                sky.SetTexture("_MainTex", hdri);
                sky.SetFloat("_Exposure", 1.05f);
                sky.SetFloat("_Rotation", 18f);
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1f;
                return;
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.62f);
        }

        static void Look()
        {
            var path = "Assets/_Project/Settings/LookProfile.asset";
            DirectoryReady("Assets/_Project/Settings");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            var bloom = Ensure<Bloom>(profile);
            bloom.threshold.Override(1.1f);
            bloom.intensity.Override(0.18f);
            bloom.scatter.Override(0.62f);
            var tone = Ensure<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.ACES);
            var color = Ensure<ColorAdjustments>(profile);
            color.postExposure.Override(0.12f);
            color.contrast.Override(10f);
            color.saturation.Override(-8f);
            color.colorFilter.Override(new Color(1f, 0.97f, 0.93f));
            var vignette = Ensure<Vignette>(profile);
            vignette.intensity.Override(0.14f);
            vignette.smoothness.Override(0.42f);

            var go = new GameObject("Look");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
            EditorUtility.SetDirty(profile);
        }

        static T Ensure<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component))
                component = profile.Add<T>();
            return component;
        }

        static void Label(string text, float x, float y, float z, float yaw)
        {
            var go = new GameObject(text + " label");
            go.transform.position = new Vector3(x, y, z) * F;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 48;
            mesh.characterSize = 0.08f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(0.16f, 0.18f, 0.16f);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
                mesh.font = font;
        }

        static void WallX(string name, float x0, float x1, float z, float y0, float y1, Material mat, Transform parent)
        {
            Cube(name, (x0 + x1) * 0.5f, (y0 + y1) * 0.5f, z, Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0), 0.5f, mat, parent);
        }

        static void WallZ(string name, float x, float z0, float z1, float y0, float y1, Material mat, Transform parent)
        {
            Cube(name, x, (y0 + y1) * 0.5f, (z0 + z1) * 0.5f, 0.5f, Mathf.Abs(y1 - y0), Mathf.Abs(z1 - z0), mat, parent);
        }

        static GameObject Cube(string name, float x, float y, float z, float sx, float sy, float sz, Material mat, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = new Vector3(x, y, z) * F;
            go.transform.localScale = new Vector3(sx, sy, sz) * F;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static void WindowedSide(string name, float x, float outward, Material wall, Transform parent)
        {
            var y0 = FloorHeight;
            var y1 = 17f;
            WallZ(name + " a", x, 0f, 5f, y0, y1, wall, parent);
            WallZ(name + " under 1", x, 5f, 11f, y0, y0 + 2.8f, wall, parent);
            WallZ(name + " over 1", x, 5f, 11f, y0 + 6.2f, y1, wall, parent);
            WallZ(name + " b", x, 11f, 36f, y0, y1, wall, parent);
            WallZ(name + " under 2", x, 36f, 42.5f, y0, y0 + 2.8f, wall, parent);
            WallZ(name + " over 2", x, 36f, 42.5f, y0 + 6.2f, y1, wall, parent);
            WallZ(name + " c", x, 42.5f, 50f, y0, y1, wall, parent);
            var glass = Mat("Glass", new Color(0.70f, 0.84f, 0.90f, 0.38f), 0.7f);
            Cube(name + " pane 1", x, y0 + 4.5f, 8f, 0.12f, 3.2f, 5.6f, glass, parent);
            Cube(name + " pane 2", x, y0 + 4.5f, 39.25f, 0.12f, 3.2f, 6.1f, glass, parent);
            GrillZ(name + " grill 1", x + outward * 0.42f, 5.2f, 10.8f, y0 + 2.95f, y0 + 6.05f, parent);
            GrillZ(name + " grill 2", x + outward * 0.42f, 36.3f, 42.2f, y0 + 2.95f, y0 + 6.05f, parent);
            Cube(name + " chajja 1", x + outward * 0.7f, y0 + 6.45f, 8f, 1.15f, 0.16f, 6.4f, wall, parent);
            Cube(name + " chajja 2", x + outward * 0.7f, y0 + 6.45f, 39.25f, 1.15f, 0.16f, 6.6f, wall, parent);
        }

        static void WindowedBack(Material wall, Transform parent)
        {
            var y0 = FloorHeight;
            var y1 = 17f;
            WallX("Upper back a", 0f, 2.2f, 49.75f, y0, y1, wall, parent);
            WallX("Upper back under 1", 2.2f, 8f, 49.75f, y0, y0 + 2.8f, wall, parent);
            WallX("Upper back over 1", 2.2f, 8f, 49.75f, y0 + 6.2f, y1, wall, parent);
            WallX("Upper back b", 8f, 18f, 49.75f, y0, y1, wall, parent);
            WallX("Upper back under 2", 18f, 25f, 49.75f, y0, y0 + 2.8f, wall, parent);
            WallX("Upper back over 2", 18f, 25f, 49.75f, y0 + 6.2f, y1, wall, parent);
            WallX("Upper back c", 25f, 30f, 49.75f, y0, y1, wall, parent);
            var glass = Mat("Glass", new Color(0.70f, 0.84f, 0.90f, 0.38f), 0.7f);
            Pane("Upper back pane 1", 5.1f, y0 + 4.5f, 49.75f, 5.4f, 3.2f, glass, parent);
            Pane("Upper back pane 2", 21.5f, y0 + 4.5f, 49.75f, 6.6f, 3.2f, glass, parent);
            GrillX("Upper back grill 1", 2.4f, 7.8f, y0 + 2.95f, y0 + 6.05f, 50.15f, parent);
            GrillX("Upper back grill 2", 18.2f, 24.8f, y0 + 2.95f, y0 + 6.05f, 50.15f, parent);
        }

        static void GrillX(string name, float x0, float x1, float y0, float y1, float z, Transform parent)
        {
            var metal = Mat("Metal", new Color(0.14f, 0.14f, 0.15f), 0.55f);
            var span = Mathf.Abs(x1 - x0);
            var bars = Mathf.Max(2, Mathf.RoundToInt(span / 1.7f));
            var y = (y0 + y1) * 0.5f;
            var h = Mathf.Abs(y1 - y0);
            for (var i = 0; i <= bars; i++)
                Cube(name + " v" + i, Mathf.Lerp(x0, x1, i / (float)bars), y, z, 0.07f, h, 0.07f, metal, parent);
            for (var i = 1; i <= 2; i++)
                Cube(name + " h" + i, (x0 + x1) * 0.5f, Mathf.Lerp(y0, y1, i / 3f), z, span, 0.07f, 0.07f, metal, parent);
        }

        static void GrillZ(string name, float x, float z0, float z1, float y0, float y1, Transform parent)
        {
            var metal = Mat("Metal", new Color(0.14f, 0.14f, 0.15f), 0.55f);
            var span = Mathf.Abs(z1 - z0);
            var bars = Mathf.Max(2, Mathf.RoundToInt(span / 1.7f));
            var y = (y0 + y1) * 0.5f;
            var h = Mathf.Abs(y1 - y0);
            for (var i = 0; i <= bars; i++)
                Cube(name + " v" + i, x, y, Mathf.Lerp(z0, z1, i / (float)bars), 0.07f, h, 0.07f, metal, parent);
            for (var i = 1; i <= 2; i++)
                Cube(name + " h" + i, x, Mathf.Lerp(y0, y1, i / 3f), (z0 + z1) * 0.5f, 0.07f, 0.07f, span, metal, parent);
        }

        static void Furnish(Transform parent)
        {
            var wood = Mat("Wood", new Color(0.86f, 0.76f, 0.62f), 0.34f);
            var sofa = Mat("Sofa", new Color(0.62f, 0.56f, 0.50f), 0.18f);
            var cushion = Mat("Cushion", new Color(0.90f, 0.86f, 0.80f), 0.14f);
            var sheet = Mat("Sheet", new Color(0.93f, 0.91f, 0.87f), 0.12f);
            var teal = Mat("SheetTeal", new Color(0.78f, 0.74f, 0.68f), 0.12f);
            var mustard = Mat("SheetMustard", new Color(0.72f, 0.68f, 0.62f), 0.12f);
            var granite = Mat("Granite", new Color(0.62f, 0.60f, 0.58f), 0.46f);
            var cabinet = Mat("Cabinet", new Color(0.94f, 0.93f, 0.91f), 0.32f);
            var fridgeMat = Mat("Fridge", new Color(0.78f, 0.79f, 0.80f), 0.55f);
            var curtain = Mat("Curtain", new Color(0.89f, 0.85f, 0.78f), 0.1f);
            var car = Mat("Car", new Color(0.20f, 0.21f, 0.22f), 0.62f);
            var tyre = Mat("Tyre", new Color(0.08f, 0.08f, 0.08f), 0.2f);
            var tank = Mat("Tank", new Color(0.28f, 0.29f, 0.30f), 0.22f);
            var dado = Mat("Dado", new Color(0.82f, 0.78f, 0.72f), 0.22f);
            var compound = Mat("Compound", new Color(0.90f, 0.88f, 0.84f), 0.12f);
            var tileK = Mat("KitchenFloor", new Color(0.84f, 0.86f, 0.84f), 0.4f);
            var woodFloor = Mat("BedroomFloor", new Color(0.58f, 0.40f, 0.24f), 0.3f);
            var ceiling = Mat("Ceiling", new Color(0.97f, 0.96f, 0.94f), 0.04f);
            var screen = Mat("Screen", new Color(0.03f, 0.05f, 0.08f), 0.75f);
            var pillow = Mat("Pillow", new Color(0.97f, 0.95f, 0.90f), 0.08f);
            var ceramic = Mat("Ceramic", new Color(0.96f, 0.96f, 0.95f), 0.5f);
            var bath = Mat("BathTile", new Color(0.78f, 0.86f, 0.88f), 0.35f);
            var cloth = Mat("PoojaCloth", new Color(0.45f, 0.24f, 0.22f), 0.16f);
            var brass = Mat("Brass", new Color(0.72f, 0.58f, 0.22f), 0.7f);
            var glass = Mat("Glass", new Color(0.70f, 0.84f, 0.90f, 0.38f), 0.7f);
            var wall = Mat("Wall", new Color(0.96f, 0.93f, 0.86f), 0.08f);
            var metal = Mat("Metal", new Color(0.14f, 0.14f, 0.15f), 0.55f);

            Cube("Kitchen tiles", 7f, 0.045f, 27f, 13.2f, 0.05f, 13.2f, tileK, parent);
            Cube("Bedroom wood", 7f, 0.045f, 42f, 13.2f, 0.05f, 15.2f, woodFloor, parent);
            Cube("FF front wood", 6.4f, FloorHeight + 0.04f, 8.1f, 12.2f, 0.05f, 15.2f, woodFloor, parent);
            Cube("FF rear wood", 6.4f, FloorHeight + 0.04f, 37.2f, 12.2f, 0.05f, 24.6f, woodFloor, parent);
            Cube("Living ceiling", 15f, 7.95f, 10f, 29.2f, 0.08f, 19.2f, ceiling, parent);
            Cube("Kitchen ceiling", 7f, 7.95f, 27f, 13.2f, 0.08f, 13.2f, ceiling, parent);
            Cube("Dining ceiling", 22f, 7.95f, 27f, 15.2f, 0.08f, 13.2f, ceiling, parent);
            Cube("Bedroom ceiling", 7f, 7.95f, 42f, 13.2f, 0.08f, 15.2f, ceiling, parent);
            Cube("Upper ceiling", 15f, 16.72f, 25f, 29.2f, 0.08f, 49f, ceiling, parent);

            Soft(Cube("Living rug", 25.6f, 0.025f, 13.4f, 7.4f, 0.02f, 6.2f, cushion, parent));
            Sofa(parent, 25.6f, 11.2f, 0f, sofa, cushion);
            Cube("TV wall", 26.8f, 4.15f, 19.32f, 5f, 6.6f, 0.1f, wood, parent);
            Cube("TV stand", 26.4f, 0.7f, 18.35f, 4.6f, 1.25f, 1.05f, wood, parent);
            Cube("TV screen", 26.4f, 2.7f, 18.85f, 3.6f, 2.05f, 0.08f, screen, parent);
            Cube("Coffee table", 25.6f, 0.95f, 14.3f, 3.4f, 0.1f, 1.55f, granite, parent);
            Cube("Foyer console", 19.5f, 1.15f, 2.05f, 3.2f, 2.15f, 0.85f, wood, parent);
            Soft(Cube("Foyer art", 19.5f, 4.5f, 0.62f, 2.1f, 1.35f, 0.05f, sofa, parent));
            Cube("Pooja", 1.45f, 1.35f, 1.45f, 1.6f, 2.7f, 1.15f, wood, parent);
            Cube("Pooja cloth", 1.45f, 2.78f, 1.45f, 1.35f, 0.08f, 0.9f, cloth, parent);
            Soft(Sphere("Diya", 1.45f, 2.95f, 1.45f, 0.28f, 0.16f, 0.28f, brass, parent));
            Cube("Shoe rack", 18.5f, 0.7f, 1.15f, 2.2f, 1.4f, 0.75f, wood, parent);
            Curtain(parent, 3.35f, 0.7f, curtain);
            Curtain(parent, 6.55f, 0.7f, curtain);
            Curtain(parent, 21.4f, 0.7f, curtain);
            Curtain(parent, 25.5f, 0.7f, curtain);
            Fan("Living fan", 24f, 7.25f, 12f, parent);
            Soft(Cube("Living tube", 24f, 7.7f, 12f, 3.2f, 0.08f, 0.35f, Mat("Tube", new Color(1f, 0.97f, 0.9f), 0.2f), parent));

            Cube("Dining table", 22f, 2.45f, 27f, 5.2f, 0.16f, 3f, wood, parent);
            Cube("Dining leg", 20.2f, 1.2f, 26f, 0.18f, 2.3f, 0.18f, wood, parent);
            Cube("Dining leg b", 23.8f, 1.2f, 26f, 0.18f, 2.3f, 0.18f, wood, parent);
            Cube("Dining leg c", 20.2f, 1.2f, 28f, 0.18f, 2.3f, 0.18f, wood, parent);
            Cube("Dining leg d", 23.8f, 1.2f, 28f, 0.18f, 2.3f, 0.18f, wood, parent);
            Chair("Chair south", 22f, 24.9f, 0f, 0f, -0.55f, wood, cushion, parent);
            Chair("Chair north", 22f, 29.1f, 0f, 0f, 0.55f, wood, cushion, parent);
            Chair("Chair west", 19f, 27f, 0f, -0.55f, 0f, wood, cushion, parent);
            Chair("Chair east", 25f, 27f, 0f, 0.55f, 0f, wood, cushion, parent);
            Cube("Sideboard", 28.4f, 1.3f, 27f, 1.05f, 2.5f, 5.2f, wood, parent);
            Fan("Dining fan", 22f, 7.25f, 27f, parent);

            Cube("Kitchen counter", 12.15f, 1.5f, 28f, 2.3f, 3f, 9.2f, cabinet, parent);
            Cube("Kitchen top", 12.15f, 3.05f, 28f, 2.45f, 0.1f, 9.3f, granite, parent);
            Cube("Kitchen back counter", 10.2f, 1.5f, 32.55f, 5.6f, 3f, 1.5f, cabinet, parent);
            Cube("Kitchen back top", 10.2f, 3.05f, 32.55f, 5.7f, 0.1f, 1.6f, granite, parent);
            Cube("Backsplash", 13.72f, 4.35f, 28f, 0.06f, 1.7f, 8.6f, tileK, parent);
            Cube("Stove", 12.2f, 3.18f, 29.2f, 1.5f, 0.08f, 1.5f, metal, parent);
            Cube("Sink", 12.15f, 3.12f, 25.4f, 1.3f, 0.12f, 1.05f, metal, parent);
            Cube("Chimney", 12.55f, 5.15f, 29.2f, 1.35f, 2.2f, 1.15f, metal, parent);
            Cube("Fridge", 12.25f, 2.7f, 21.7f, 2.15f, 5.3f, 2.15f, fridgeMat, parent);
            Cube("Loft", 12.15f, 6.85f, 28f, 1.5f, 1.35f, 8.4f, cabinet, parent);
            Soft(Cube("Under cabinet light", 11.7f, 4.55f, 28f, 0.12f, 0.05f, 8.2f, Mat("Tube", new Color(1f, 0.95f, 0.86f), 0.2f), parent));
            Fan("Kitchen fan", 6.5f, 7.25f, 26f, parent);

            Bed("GF bed", 3.3f, 46.3f, 0f, 4.6f, 5.2f, 1f, wood, sheet, pillow, parent);
            Cube("Master headboard", 3.3f, 2.15f, 48.85f, 5f, 2.8f, 0.16f, sofa, parent);
            Soft(Cube("Master rug", 3.6f, 0.03f, 44.6f, 5.2f, 0.02f, 2.4f, cushion, parent));
            Wardrobe("GF wardrobe", 0.95f, 34.8f, 0f, 1.25f, 3.2f, wood, parent);
            Cube("Side table", 6.55f, 0.85f, 47.6f, 1.05f, 1.15f, 1.05f, wood, parent);
            Soft(Sphere("Bedside lamp", 6.55f, 1.6f, 47.6f, 0.28f, 0.35f, 0.28f, Mat("Tube", new Color(1f, 0.93f, 0.8f), 0.3f), parent));
            Soft(Cube("AC", 3.3f, 6.7f, 48.9f, 2.3f, 0.7f, 0.55f, fridgeMat, parent));
            Fan("Bedroom fan", 5f, 7.25f, 43f, parent);
            WallZ("Bath side", 8.95f, 45.15f, 49.7f, 0f, 7f, bath, parent);
            WallX("Bath front a", 8.95f, 10.35f, 45.15f, 0f, 7f, bath, parent);
            WallX("Bath front b", 12.55f, 13.85f, 45.15f, 0f, 7f, bath, parent);
            WallX("Bath header", 10.35f, 12.55f, 45.15f, 6.5f, 7f, bath, parent);
            Cube("Bath floor", 11.4f, 0.08f, 47.4f, 4.7f, 0.04f, 4.3f, bath, parent);
            Cube("Vanity", 9.7f, 1.15f, 48.55f, 1.7f, 2.2f, 1.15f, wood, parent);
            Cube("Basin", 9.7f, 2.35f, 48.45f, 1.4f, 0.22f, 1.05f, ceramic, parent);
            Soft(Cube("Bath mirror", 9.7f, 4.55f, 49.42f, 1.45f, 2f, 0.04f, glass, parent));
            Cube("Shower glass", 11.35f, 3.3f, 47.6f, 0.06f, 6.2f, 2.4f, glass, parent);
            Cube("WC", 12.55f, 0.7f, 48.35f, 1.15f, 1.25f, 1.7f, ceramic, parent);

            WallZ("FF front wall a", 13f, 0.4f, 7f, FloorHeight, 17f, wall, parent);
            WallZ("FF front wall b", 13f, 11f, 16.2f, FloorHeight, 17f, wall, parent);
            WallX("FF front end", 0.4f, 13f, 16.2f, FloorHeight, 17f, wall, parent);
            WallX("FF rear front", 0.4f, 13f, 24f, FloorHeight, 17f, wall, parent);
            WallZ("FF rear wall a", 13f, 24f, 30f, FloorHeight, 17f, wall, parent);
            WallZ("FF rear wall b", 13f, 34f, 49.6f, FloorHeight, 17f, wall, parent);
            Bed("FF front bed", 4f, 5.6f, FloorHeight, 4.5f, 5.2f, -1f, wood, teal, pillow, parent);
            Wardrobe("FF front wardrobe", 4.2f, 14.9f, FloorHeight, 5.2f, 1.15f, wood, parent);
            Bed("FF rear bed", 4.2f, 41.5f, FloorHeight, 4.5f, 6f, 1f, wood, mustard, pillow, parent);
            Wardrobe("FF rear wardrobe", 1f, 27.2f, FloorHeight, 1.25f, 3.4f, wood, parent);
            Fan("FF front fan", 5f, FloorHeight + 6.4f, 8f, parent);
            Fan("FF rear fan", 5f, FloorHeight + 6.4f, 40f, parent);
            Fan("Hall fan", 22f, FloorHeight + 6.4f, 10f, parent);

            Cube("Hall sofa", 26.2f, FloorHeight + 0.55f, 7.2f, 2.3f, 1.05f, 5.2f, sofa, parent);
            Cube("Hall sofa back", 27.15f, FloorHeight + 1.25f, 7.2f, 0.28f, 1.35f, 5.2f, sofa, parent);
            Soft(Cube("Hall rug", 24f, FloorHeight + 0.02f, 8f, 4.2f, 0.02f, 6f, cushion, parent));
            Cube("Hall TV stand", 28.55f, FloorHeight + 0.7f, 7.2f, 0.7f, 1.3f, 3.2f, wood, parent);
            Cube("Hall TV", 28.85f, FloorHeight + 2.3f, 7.2f, 0.1f, 1.7f, 2.8f, screen, parent);
            Plant("Balcony plant", 9.3f, -4.4f, FloorHeight, parent);
            Plant("Balcony plant b", 20.5f, -4.4f, FloorHeight, parent);
            Chair("Balcony chair", 19.4f, -2.4f, FloorHeight, 0f, 0.4f, wood, cushion, parent);
            Chair("Balcony chair b", 11.2f, -2.4f, FloorHeight, 0f, -0.4f, wood, cushion, parent);
            WallZ("FF bath side", 8.15f, 45.7f, 49.6f, FloorHeight, FloorHeight + 7f, bath, parent);
            WallX("FF bath front a", 8.15f, 9.3f, 45.7f, FloorHeight, FloorHeight + 7f, bath, parent);
            WallX("FF bath front b", 11.5f, 12.7f, 45.7f, FloorHeight, FloorHeight + 7f, bath, parent);
            WallX("FF bath header", 9.3f, 11.5f, 45.7f, FloorHeight + 6.5f, FloorHeight + 7f, bath, parent);
            Cube("FF vanity", 9.2f, FloorHeight + 1.15f, 48.4f, 1.5f, 2.2f, 1.05f, wood, parent);
            Cube("FF basin", 9.2f, FloorHeight + 2.35f, 48.3f, 1.25f, 0.2f, 0.9f, ceramic, parent);
            Soft(Cube("FF mirror", 9.2f, FloorHeight + 4.4f, 49.4f, 1.2f, 1.8f, 0.04f, glass, parent));

            Cube("Dado front left", 6.5f, 1.2f, -0.22f, 13f, 2.4f, 0.16f, dado, parent);
            Cube("Dado front right", 23.2f, 1.2f, -0.22f, 12.8f, 2.4f, 0.16f, dado, parent);
            Cube("Dado left", -0.22f, 1.2f, 25f, 0.16f, 2.4f, 50f, dado, parent);
            Cube("Dado right", 30.22f, 1.2f, 25f, 0.16f, 2.4f, 50f, dado, parent);
            Cube("Dado back", 15f, 1.2f, 50.2f, 30f, 2.4f, 0.16f, dado, parent);
            Cube("Compound left", -8f, 2.5f, 19f, 0.55f, 5f, 74f, compound, parent);
            Cube("Compound right", 38f, 2.5f, 19f, 0.55f, 5f, 74f, compound, parent);
            Cube("Compound back", 15f, 2.5f, 56f, 46.5f, 5f, 0.55f, compound, parent);
            Cube("Compound front left", 1.4f, 2.5f, -18f, 18.8f, 5f, 0.55f, compound, parent);
            Cube("Compound front right", 28.6f, 2.5f, -18f, 18.8f, 5f, 0.55f, compound, parent);
            Cube("Gate pillar L", 10.6f, 3.3f, -18f, 1f, 6.6f, 1f, dado, parent);
            Cube("Gate pillar R", 19.4f, 3.3f, -18f, 1f, 6.6f, 1f, dado, parent);
            Cube("Gate leaf L", 8.6f, 2.3f, -18.5f, 0.12f, 4.2f, 2.8f, metal, parent);
            Cube("Gate leaf R", 21.4f, 2.3f, -18.5f, 0.12f, 4.2f, 2.8f, metal, parent);
            Cube("Gate path", 15f, -0.02f, -14.5f, 5.2f, 0.08f, 7f, Mat("Parking", new Color(0.70f, 0.68f, 0.64f), 0.28f), parent);
            Cube("Compound coping L", -8f, 5.08f, 19f, 0.7f, 0.12f, 74f, metal, parent);
            Cube("Compound coping R", 38f, 5.08f, 19f, 0.7f, 0.12f, 74f, metal, parent);
            ParkedCar(6.2f, -11.5f, car, glass, tyre, parent);
            Plant("Yard plant", 10.2f, -6f, 0f, parent);
            Plant("Yard plant b", 20f, -6f, 0f, parent);
            Plant("Corner plant", 28.4f, 2.2f, 0f, parent);

            Cylinder("Water tank", 26.2f, 19.7f, 45.5f, 4.4f, 3.8f, tank, parent);
            Cube("Parapet left", -1.8f, 19f, 22f, 0.4f, 2.3f, 57f, wall, parent);
            Cube("Parapet right", 31.8f, 19f, 22f, 0.4f, 2.3f, 57f, wall, parent);
            Cube("Parapet front", 15f, 19f, -6.8f, 34f, 2.3f, 0.4f, wall, parent);
            Cube("Parapet back", 15f, 19f, 50.8f, 34f, 2.3f, 0.4f, wall, parent);

            var rise = FloorHeight / 14f;
            for (var i = 0; i < 14; i++)
            {
                var x = 16.3f + 0.72f * (i + 0.5f);
                var tread = rise * (i + 1);
                Cube("Stair post " + i, x, tread + 1.35f, 40.32f, 0.1f, 2.5f, 0.1f, metal, parent);
            }

            var handrail = Cube("Stair handrail", 21.35f, 7.1f, 40.32f, 13.1f, 0.1f, 0.1f, metal, parent);
            handrail.transform.rotation = Quaternion.Euler(0f, 0f, 40f);
        }

        static void Sofa(Transform parent, float x, float z, float y, Material sofa, Material cushion)
        {
            Cube("Sofa seat", x, y + 0.85f, z, 6.2f, 1.05f, 2.45f, sofa, parent);
            Cube("Sofa back", x, y + 1.85f, z - 1.1f, 6.2f, 1.65f, 0.38f, sofa, parent);
            Cube("Sofa arm L", x - 2.95f, y + 1.35f, z, 0.38f, 1.25f, 2.45f, sofa, parent);
            Cube("Sofa arm R", x + 2.95f, y + 1.35f, z, 0.38f, 1.25f, 2.45f, sofa, parent);
            Soft(Sphere("Cushion a", x - 1.5f, y + 1.5f, z + 0.05f, 1.45f, 0.55f, 1.2f, cushion, parent));
            Soft(Sphere("Cushion b", x + 1.5f, y + 1.5f, z + 0.05f, 1.45f, 0.55f, 1.2f, cushion, parent));
        }

        static void Chair(string name, float x, float z, float y, float bx, float bz, Material wood, Material seat, Transform parent)
        {
            Cube(name + " seat", x, y + 1.45f, z, 1.15f, 0.12f, 1.15f, seat, parent);
            var backAlongX = Mathf.Abs(bx) > Mathf.Abs(bz);
            Cube(name + " back", x + bx, y + 2.25f, z + bz, backAlongX ? 0.12f : 1.15f, 1.35f, backAlongX ? 1.15f : 0.12f, wood, parent);
            Cube(name + " leg", x - 0.4f, y + 0.7f, z - 0.4f, 0.1f, 1.3f, 0.1f, wood, parent);
            Cube(name + " leg b", x + 0.4f, y + 0.7f, z - 0.4f, 0.1f, 1.3f, 0.1f, wood, parent);
            Cube(name + " leg c", x - 0.4f, y + 0.7f, z + 0.4f, 0.1f, 1.3f, 0.1f, wood, parent);
            Cube(name + " leg d", x + 0.4f, y + 0.7f, z + 0.4f, 0.1f, 1.3f, 0.1f, wood, parent);
        }

        static void Bed(string name, float x, float z, float y, float sx, float sz, float headSign, Material wood, Material sheet, Material pillow, Transform parent)
        {
            Cube(name + " frame", x, y + 0.45f, z, sx + 0.15f, 0.5f, sz + 0.1f, wood, parent);
            Cube(name + " mattress", x, y + 0.9f, z, sx, 0.36f, sz, sheet, parent);
            Cube(name + " head", x, y + 1.65f, z + headSign * (sz * 0.5f - 0.12f), sx + 0.2f, 1.55f, 0.26f, wood, parent);
            Cube(name + " pillow", x, y + 1.18f, z + headSign * (sz * 0.5f - 1f), sx * 0.6f, 0.2f, 0.85f, pillow, parent);
        }

        static void Wardrobe(string name, float x, float z, float y, float sx, float sz, Material wood, Transform parent)
        {
            Cube(name, x, y + 3.45f, z, sx, 6.7f, sz, wood, parent);
            var splitWide = sx >= sz;
            Cube(name + " seam", x, y + 3.5f, z, splitWide ? 0.06f : sx * 0.85f, 6.2f, splitWide ? sz * 0.85f : 0.06f, Mat("Trim", new Color(0.97f, 0.96f, 0.93f), 0.12f), parent);
        }

        static void Curtain(Transform parent, float x, float z, Material cloth)
        {
            Soft(Cube("Curtain " + x, x, 4.3f, z, 0.55f, 4.2f, 0.12f, cloth, parent));
        }

        static void Fan(string name, float x, float y, float z, Transform parent)
        {
            var body = Mat("Fan", new Color(0.82f, 0.78f, 0.70f), 0.35f);
            var bladeMat = Mat("FanBlade", new Color(0.48f, 0.34f, 0.2f), 0.25f);
            Soft(Cylinder(name + " rod", x, y + 0.28f, z, 0.1f, 0.5f, body, parent));
            Soft(Cylinder(name + " hub", x, y, z, 0.5f, 0.22f, body, parent));
            var center = new Vector3(x, y, z) * F;
            for (var i = 0; i < 3; i++)
            {
                var blade = Soft(Cube(name + " blade " + i, x + 1.15f, y, z, 2.1f, 0.05f, 0.38f, bladeMat, parent));
                blade.transform.RotateAround(center, Vector3.up, i * 120f);
            }
        }

        static void Plant(string name, float x, float z, float y, Transform parent)
        {
            var pot = Mat("Pot", new Color(0.62f, 0.30f, 0.16f), 0.15f);
            var leaf = Mat("Plant", new Color(0.18f, 0.46f, 0.22f), 0.08f);
            Cylinder(name + " pot", x, y + 0.4f, z, 0.85f, 0.8f, pot, parent);
            Soft(Sphere(name + " leaves", x, y + 1.35f, z, 1.35f, 1.15f, 1.35f, leaf, parent));
        }

        static void ParkedCar(float x, float z, Material body, Material glass, Material tyre, Transform parent)
        {
            Cube("Car body", x, 1.5f, z, 5.3f, 1.55f, 10.6f, body, parent);
            Cube("Car cabin", x, 2.75f, z + 0.2f, 4.7f, 1.25f, 5.4f, body, parent);
            Soft(Cube("Car glass", x, 2.8f, z + 0.15f, 4.35f, 0.95f, 4.6f, glass, parent));
            Wheel(x - 1.85f, 0.65f, z - 3.1f, tyre, parent);
            Wheel(x + 1.85f, 0.65f, z - 3.1f, tyre, parent);
            Wheel(x - 1.85f, 0.65f, z + 3.1f, tyre, parent);
            Wheel(x + 1.85f, 0.65f, z + 3.1f, tyre, parent);
        }

        static void Wheel(float x, float y, float z, Material tyre, Transform parent)
        {
            var wheel = Cylinder("Wheel", x, y, z, 1.45f, 0.45f, tyre, parent);
            wheel.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        static GameObject Soft(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Object.DestroyImmediate(collider);
            return go;
        }

        static GameObject Cylinder(string name, float x, float y, float z, float diameter, float height, Material mat, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = new Vector3(x, y, z) * F;
            go.transform.localScale = new Vector3(diameter, height * 0.5f, diameter) * F;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static GameObject Sphere(string name, float x, float y, float z, float sx, float sy, float sz, Material mat, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = new Vector3(x, y, z) * F;
            go.transform.localScale = new Vector3(sx, sy, sz) * F;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go;
        }

        static Material Mat(string name, Color color, float smooth = 0.15f)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            var path = "Assets/_Project/Materials/" + name + ".mat";
            DirectoryReady("Assets/_Project/Materials");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.color = color;
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smooth);
            if (material.HasProperty("_Metallic"))
            {
                var metal = 0f;
                if (name == "Metal" || name == "Brass")
                    metal = 0.72f;
                else if (name == "Fridge")
                    metal = 0.45f;
                else if (name == "Car")
                    metal = 0.4f;
                else if (name == "Tank")
                    metal = 0.06f;
                material.SetFloat("_Metallic", metal);
            }

            material.SetTexture("_BaseMap", null);
            if (name == "Glass")
            {
                if (color.a > 0.95f)
                    color.a = 0.4f;
                material.SetColor("_BaseColor", color);
                material.SetFloat("_Surface", 1f);
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                material.SetFloat("_Surface", 0f);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Geometry;
            }

            if (name == "Tube")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 3f);
            }
            else if (name == "Screen")
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(0.05f, 0.12f, 0.2f));
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                if (material.HasProperty("_EmissionColor"))
                    material.SetColor("_EmissionColor", Color.black);
            }

            ApplyPbr(material, name);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void ApplyPbr(Material material, string name)
        {
            string diff = null;
            string nor = null;
            var scale = new Vector2(2f, 2f);
            var bump = 0.4f;
            if (name == "Wall" || name == "Ceiling" || name == "Trim" || name == "Compound")
            {
                diff = "wall";
                nor = "wall";
                scale = new Vector2(2f, 2f);
                bump = 0.22f;
            }
            else if (name == "Floor" || name == "UpperFloor" || name == "KitchenFloor" || name == "BathTile")
            {
                diff = "floor";
                nor = "floor";
                scale = new Vector2(2.2f, 3.2f);
                bump = 0.3f;
            }
            else if (name == "BedroomFloor" || name == "Wood" || name == "Door" || name == "Stairs")
            {
                diff = "wood";
                nor = "wood";
                scale = new Vector2(1.6f, 1.6f);
                bump = 0.45f;
            }
            else if (name == "Granite" || name == "Dado" || name == "Parking")
            {
                diff = "stone";
                nor = "stone";
                scale = new Vector2(2.5f, 2.5f);
                bump = 0.35f;
            }
            else if (name == "Plot")
            {
                diff = "grass";
                nor = "grass";
                scale = new Vector2(7f, 9f);
                bump = 0.25f;
            }
            else if (name == "Sofa" || name == "Cushion" || name == "Curtain" || name == "Sheet" || name == "SheetTeal" || name == "SheetMustard" || name == "Pillow")
            {
                diff = "fabric";
                scale = new Vector2(1.5f, 1.5f);
            }

            material.SetTexture("_BumpMap", null);
            material.DisableKeyword("_NORMALMAP");
            if (diff == null)
            {
                material.SetTexture("_BaseMap", null);
                return;
            }

            var albedo = LoadPbr(diff + "_diff.jpg", false);
            if (albedo != null)
            {
                material.SetTexture("_BaseMap", albedo);
                material.SetTextureScale("_BaseMap", scale);
            }

            if (nor == null)
                return;
            var normal = LoadPbr(nor + "_nor.jpg", true);
            if (normal == null)
                return;
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", bump);
            material.EnableKeyword("_NORMALMAP");
        }

        static Texture2D LoadPbr(string file, bool normal, bool repeat = true)
        {
            var rel = "Assets/_Project/Materials/PBR/" + file;
            var importer = AssetImporter.GetAtPath(rel) as TextureImporter;
            if (importer != null)
            {
                var type = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                var wrap = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                var srgb = !normal && !file.EndsWith(".hdr");
                if (importer.textureType != type || importer.wrapMode != wrap || importer.sRGBTexture != srgb)
                {
                    importer.textureType = type;
                    importer.wrapMode = wrap;
                    importer.sRGBTexture = srgb;
                    importer.mipmapEnabled = !file.EndsWith(".hdr");
                    importer.SaveAndReimport();
                }
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(rel);
        }

        static Vector2 PatternScale(string name)
        {
            if (name == "Floor" || name == "UpperFloor")
                return new Vector2(8f, 12f);
            if (name == "Plot")
                return new Vector2(6f, 6f);
            if (name == "BedroomFloor")
                return new Vector2(3f, 3f);
            return new Vector2(4f, 4f);
        }

        static Texture2D PatternFor(string name)
        {
            if (name == "Floor" || name == "UpperFloor" || name == "KitchenFloor" || name == "BathTile")
                return Pattern(name, "tile");
            if (name == "BedroomFloor")
                return Pattern(name, "wood");
            if (name == "Plot" || name == "Parking" || name == "Granite" || name == "Dado")
                return Pattern(name, "noise");
            return null;
        }

        static Texture2D Pattern(string file, string kind)
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGB24, false);
            var rng = new System.Random(kind.Length * 17 + file.Length);
            for (var y = 0; y < n; y++)
            {
                for (var x = 0; x < n; x++)
                {
                    var pixel = Color.white;
                    if (kind == "tile")
                        pixel = (x % 32) < 2 || (y % 32) < 2 ? new Color(0.72f, 0.72f, 0.72f) : Color.white;
                    else if (kind == "wood")
                    {
                        var stripe = 0.9f + Mathf.Sin(y * 0.55f) * 0.06f;
                        pixel = new Color(stripe, stripe * 0.96f, stripe * 0.9f);
                        if ((x + y / 4) % 19 == 0)
                            pixel *= 0.82f;
                    }
                    else
                        pixel = Color.Lerp(Color.white, new Color(0.78f, 0.8f, 0.74f), (float)rng.NextDouble() * 0.35f);
                    tex.SetPixel(x, y, pixel);
                }
            }

            tex.Apply();
            var rel = "Assets/_Project/Materials/Textures/" + file + ".png";
            DirectoryReady("Assets/_Project/Materials/Textures");
            var abs = System.IO.Path.Combine(Application.dataPath, "_Project/Materials/Textures/" + file + ".png");
            System.IO.File.WriteAllBytes(abs, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(rel) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(rel);
        }

        static void DirectoryReady(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            var leaf = System.IO.Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                DirectoryReady(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}

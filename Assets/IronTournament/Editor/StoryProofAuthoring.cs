#if UNITY_EDITOR
using System;
using System.IO;
using IronTournament.Bootstrap;
using IronTournament.Content;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace IronTournament.Editor
{
    public static class StoryProofAuthoring
    {
        private const string Art = "Assets/IronTournament/Art/Backgrounds_new/Village/";
        private const string Data = "Assets/IronTournament/Content/Story/";
        public const string ScenePath = "Assets/Scenes/StoryProof.unity";

        [MenuItem("Iron Tournament/Prepare story proof")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Data);
            AssetDatabase.Refresh();
            var content = AssetDatabase.LoadAssetAtPath<StoryContent>(Data + "StoryProof.asset");
            if (content == null)
            {
                content = ScriptableObject.CreateInstance<StoryContent>();
                AssetDatabase.CreateAsset(content, Data + "StoryProof.asset");
            }
            content.warrior = Load<CombatantDefinition>("Assets/IronTournament/Content/Warrior.asset");
            content.mage = Load<CombatantDefinition>("Assets/IronTournament/Content/Mage.asset");
            content.goblin = Load<EncounterDefinition>("Assets/IronTournament/Content/GoblinEncounter.asset");
            content.opening = Import("opening_screen", false, "Assets/IronTournament/Art/Backgrounds_new/");
            content.village = Background("village_close");
            content.distantVillage = Background("village_distant_view");
            content.forestPath = Background("forest_floaded_path");
            content.swamp = Background("forest_clearing");
            content.grave = Character("forest_child_grave");
            var rooms = new[] { "house_caos_sword", "house_flame_cloak", "house_bloodstein_living_room", "house_cauldron_living_room", "house_blood_circle_living_room" };
            content.houses = Array.ConvertAll(rooms, Background);
            content.bedrooms = Array.ConvertAll(new[] { "house_bloodstein_bedroom", "house_cauldron_bedroom", "house_blood_circle_bedroom" }, Background);
            content.child = Character("child_idle");
            content.childSaved = Character("child_saved");
            content.childHurt = Character("child_hurt");
            content.childDead = Character("child_dead");
            content.goblinDead = Character("goblin_dead");
            content.warriorPoses = Poses("Warrior"); content.magePoses = Poses("Mage");
            content.goblinPoses = new[] { Existing("Goblin", "east"), Existing("Goblin", "west"), Existing("Goblin", "south-west") };
            content.script = Load<TextAsset>(Data + "story-script.json");
            EditorUtility.SetDirty(content); AssetDatabase.SaveAssets();

            // This scene is isolated from the existing campaign demonstration and its tests.
            if (!File.Exists(ScenePath))
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("StoryProof");
                var controller = root.AddComponent<StoryProofController>();
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            var proofScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var proof = UnityEngine.Object.FindFirstObjectByType<StoryProofController>();
            var serialized = new SerializedObject(proof);
            serialized.FindProperty("content").objectReferenceValue = Load<StoryContent>(Data + "StoryProof.asset");
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(proof);
            EditorSceneManager.MarkSceneDirty(proofScene);
            EditorSceneManager.SaveScene(proofScene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Battle.unity", true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("Story proof prepared: " + ScenePath);
        }

        private static Sprite[] Poses(string character) => new[] { Existing(character,"south-east"), Existing(character,"north"), Existing(character,"east"), Existing(character,"north-east") };
        private static Sprite Existing(string character, string direction) => Load<Sprite>($"Assets/IronTournament/Art/Characters/{character}/Static/Idle/{direction}.png");
        private static T Load<T>(string path) where T : UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException("Missing asset: " + path);

        [MenuItem("Iron Tournament/Play story proof")]
        public static void Preview()
        {
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.EnterPlaymode();
        }
        private static Sprite Background(string name) => Import(name, false);
        private static Sprite Character(string name) => Import(name, true);
        private static Sprite Import(string name, bool trim, string folder = Art)
        {
            var path = folder + name + ".png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null) throw new InvalidOperationException("Missing art: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048; importer.isReadable = trim; importer.alphaIsTransparency = trim;
            importer.SaveAndReimport();
            if (!trim) return Load<Sprite>(path);
            var assetPath = Data + name + "-trimmed.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            var texture = Load<Texture2D>(path);
            var pixels = texture.GetPixels32();
            var minX = texture.width; var minY = texture.height; var maxX = 0; var maxY = 0;
            for (var y = 0; y < texture.height; y++)
                for (var x = 0; x < texture.width; x++)
                    if (pixels[y * texture.width + x].a >= 128)
                    { minX = Math.Min(minX,x); minY = Math.Min(minY,y); maxX = Math.Max(maxX,x); maxY = Math.Max(maxY,y); }
            if (minX > maxX || minY > maxY) throw new InvalidOperationException("Empty character art: " + path);
            var sprite = Sprite.Create(texture, new Rect(minX,minY,maxX-minX+1,maxY-minY+1), new Vector2(.5f,0),100,0,SpriteMeshType.FullRect);
            sprite.name = name;
            if (existing != null)
            {
                EditorUtility.CopySerialized(sprite, existing);
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(sprite);
                return existing;
            }
            AssetDatabase.CreateAsset(sprite, assetPath);
            return sprite;
        }

        public static void BuildWeb()
        {
            Prepare();
            PlayerSettings.WebGL.template = "PROJECT:StoryProof";
            PlayerSettings.WebGL.decompressionFallback = true;
            var output = Environment.GetEnvironmentVariable("IRON_PROOF_BUILD") ?? "Builds/StoryProof-WebGL";
            var report = UnityEditor.BuildPipeline.BuildPlayer(new[] { ScenePath }, output, BuildTarget.WebGL, BuildOptions.None);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Story Web build failed: " + report.summary.result);
        }
    }
}
#endif

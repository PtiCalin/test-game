using System.IO;
using TestGame.AI;
using TestGame.Builders;
using TestGame.Camera;
using TestGame.Core;
using TestGame.Player;
using TestGame.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TestGame.Editor
{
    public static class ProjectScaffolder
    {
        private const string ScenesFolder = "Assets/Scenes";

        [MenuItem("Tools/Test Game/Scaffold Scenes")]
        public static void ScaffoldScenes()
        {
            EnsureFolder("Assets", "Scripts");
            EnsureFolder("Assets", "Scenes");

            ScaffoldMenuScene();
            ScaffoldCastleScene();
            ScaffoldEndScene(SceneNames.YouLost, "YOU LOST");
            ScaffoldEndScene(SceneNames.YouWin, "YOU WIN");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Scaffold complete. Open scenes from Assets/Scenes.");
        }

        private static void ScaffoldMenuScene()
        {
            string path = $"{ScenesFolder}/{SceneNames.Menu}.unity";
            Scene s = File.Exists(path)
                ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s.name = SceneNames.Menu;

            CreateOrFindGameManager();
            EnsureEventSystem();

            GameObject canvasGo = FindOrCreateRoot("Canva");
            EnsureCanvasComponents(canvasGo);

            GameObject screenGo = FindOrCreateChild(canvasGo.transform, "Screen");
            FindOrCreateChild(screenGo.transform, "Vidéo Menu");
            FindOrCreateChild(screenGo.transform, "Audio Intro");

            GameObject uiGo = FindOrCreateChild(canvasGo.transform, "UI Display");
            var menuController = uiGo.GetComponent<MenuController>() ?? uiGo.AddComponent<MenuController>();

            Button enter = FindOrCreateButton(uiGo.transform, "Enter", new Vector2(0.5f, 0.55f));
            Button quit = FindOrCreateButton(uiGo.transform, "Fuir", new Vector2(0.5f, 0.40f));
            SetPrivateField(menuController, "enterButton", enter);
            SetPrivateField(menuController, "quitButton", quit);

            EditorSceneManager.SaveScene(s, path);
        }

        private static void ScaffoldCastleScene()
        {
            string path = $"{ScenesFolder}/{SceneNames.Castle}.unity";
            Scene s = File.Exists(path)
                ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s.name = SceneNames.Castle;

            CreateOrFindGameManager();
            EnsureEventSystem();

            // Directional Light
            GameObject lightGo = FindOrCreateRoot("Directional Light");
            var l = lightGo.GetComponent<Light>() ?? lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Camera
            GameObject camGo = FindOrCreateRoot("Camera");
            var cam = camGo.GetComponent<UnityEngine.Camera>() ?? camGo.AddComponent<UnityEngine.Camera>();
            cam.tag = "MainCamera";
            if (camGo.GetComponent<AudioListener>() == null) camGo.AddComponent<AudioListener>();
            if (camGo.GetComponent<CameraRigController>() == null) camGo.AddComponent<CameraRigController>();
            FindOrCreateChild(camGo.transform, "Audio Castle");

            // Global Volume placeholder
            FindOrCreateRoot("Global Volume");

            // Structure
            GameObject structure = FindOrCreateRoot("Structure");
            GameObject corridor = FindOrCreateChild(structure.transform, "Corridor");
            var corridorBuilder = corridor.GetComponent<CorridorBuilder>() ?? corridor.AddComponent<CorridorBuilder>();

            GameObject maze = FindOrCreateChild(structure.transform, "Maze");
            if (maze.transform.localPosition == Vector3.zero)
                maze.transform.localPosition = new Vector3(0f, 0f, 14f);
            var mazeBuilder = maze.GetComponent<MazeBuilder>() ?? maze.AddComponent<MazeBuilder>();

            // Collectibles
            GameObject collectibles = FindOrCreateChild(maze.transform, "Collectibles");
            if (collectibles.GetComponent<TestGame.Collectibles.CollectibleSpawner>() == null)
                collectibles.AddComponent<TestGame.Collectibles.CollectibleSpawner>();

            // Character
            GameObject character = FindOrCreateRoot("Character");
            if (character.GetComponent<CapsuleCollider>() == null) character.AddComponent<CapsuleCollider>();
            if (character.GetComponent<Rigidbody>() == null) character.AddComponent<Rigidbody>();
            var cb = character.GetComponent<CharacterBehaviour>() ?? character.AddComponent<CharacterBehaviour>();
            SetPrivateField(cb, "corridor", corridorBuilder);

            // UI Display
            GameObject uiCanvas = FindOrCreateRoot("UI Display");
            EnsureCanvasComponents(uiCanvas);

            // AI Agents
            GameObject agents = FindOrCreateRoot("AI Agents");
            GameObject enemy = FindOrCreateChild(agents.transform, "Enemy");
            if (enemy.GetComponent<CharacterController>() == null) enemy.AddComponent<CharacterController>();
            var eb = enemy.GetComponent<EnemyBehaviour>() ?? enemy.AddComponent<EnemyBehaviour>();
            SetPrivateField(eb, "maze", mazeBuilder);

            EditorSceneManager.SaveScene(s, path);
        }

        private static void ScaffoldEndScene(string sceneName, string title)
        {
            string path = $"{ScenesFolder}/{sceneName}.unity";
            Scene s = File.Exists(path)
                ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            s.name = sceneName;
            CreateOrFindGameManager();
            EnsureEventSystem();

            GameObject canvasGo = FindOrCreateRoot("Canvas");
            EnsureCanvasComponents(canvasGo);

            GameObject uiGo = FindOrCreateChild(canvasGo.transform, "UI Display");
            var end = uiGo.GetComponent<EndScreenController>() ?? uiGo.AddComponent<EndScreenController>();

            CreateLabel(uiGo.transform, title, new Vector2(0.5f, 0.7f), 42);
            Button menu = FindOrCreateButton(uiGo.transform, "Menu", new Vector2(0.5f, 0.45f));
            Button quit = FindOrCreateButton(uiGo.transform, "Quit", new Vector2(0.5f, 0.30f));
            SetPrivateField(end, "menuButton", menu);
            SetPrivateField(end, "quitButton", quit);

            EditorSceneManager.SaveScene(s, path);
        }

        private static void CreateOrFindGameManager()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }

        private static void EnsureCanvasComponents(GameObject canvasGo)
        {
            var canvas = canvasGo.GetComponent<Canvas>() ?? canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (canvasGo.GetComponent<CanvasScaler>() == null) canvasGo.AddComponent<CanvasScaler>();
            if (canvasGo.GetComponent<GraphicRaycaster>() == null) canvasGo.AddComponent<GraphicRaycaster>();
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static GameObject FindOrCreateRoot(string name)
        {
            GameObject go = GameObject.Find(name);
            return go != null ? go : new GameObject(name);
        }

        private static GameObject FindOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null) return existing.gameObject;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Button FindOrCreateButton(Transform parent, string label, Vector2 anchor)
        {
            Transform existing = parent.Find(label);
            if (existing != null)
            {
                var existingBtn = existing.GetComponent<Button>();
                if (existingBtn != null) return existingBtn;
            }
            return CreateButton(parent, label, anchor);
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            instance.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(instance, value);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchor)
        {
            GameObject btnGo = new GameObject(label);
            btnGo.transform.SetParent(parent, false);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.85f);
            var btn = btnGo.AddComponent<Button>();

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(220f, 60f);
            rt.anchoredPosition = Vector2.zero;

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.black;
            RectTransform trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;

            return btn;
        }

        private static void CreateLabel(Transform parent, string textValue, Vector2 anchor, int fontSize)
        {
            GameObject labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent, false);
            var text = labelGo.AddComponent<Text>();
            text.text = textValue;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = fontSize;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.color = Color.white;

            RectTransform rt = labelGo.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = new Vector2(700f, 120f);
            rt.anchoredPosition = Vector2.zero;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = Path.Combine(parent, child).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}

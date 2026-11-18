using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public static class BuildGameScene
{
    private const string Root = "Assets/Game";
    private const string Materials = Root + "/Materials";
    private const string Prefabs = Root + "/Prefabs";
    private const string Audio = Root + "/Audio";
    private const string Scenes = Root + "/Scenes";

    [MenuItem("Game Project/Build Rubric Scene")]
    public static void Build()
    {
        EnsureFolders();
        EnsureTag("CollectibleNormal");
        EnsureTag("CollectibleSpecial");
        Material grass = MakeMaterial("M_Grass", new Color(0.16f, 0.48f, 0.28f), 0.05f);
        Material stone = MakeMaterial("M_Stone", new Color(0.24f, 0.31f, 0.42f), 0.25f);
        Material ramp = MakeMaterial("M_Ramp", new Color(0.82f, 0.48f, 0.16f), 0.15f);
        Material wall = MakeMaterial("M_Wall", new Color(0.02f, 0.72f, 0.78f), 0.15f);
        Material wallCap = MakeMaterial("M_WallCap", new Color(1f, 0.82f, 0.12f), 0.1f);
        Material playerMat = MakeMaterial("M_Player", new Color(0.12f, 0.68f, 0.95f), 0.45f);
        Material gold = MakeMaterial("M_Coin", new Color(1f, 0.72f, 0.08f), 0.75f);
        Material purple = MakeMaterial("M_Special", new Color(0.78f, 0.18f, 1f), 0.55f);
        Material white = MakeMaterial("M_White", Color.white, 0.1f);

        GameObject normalPrefab = MakeCollectiblePrefab("NormalCoin", Collectible.Kind.Normal, 1, 110f, gold, false);
        GameObject specialPrefab = MakeCollectiblePrefab("SpecialGem", Collectible.Kind.Especial, 3, 210f, purple, true);
        AudioClip pickupClip = MakePickupClip();
        AudioClip fallClip = MakeFallClip();
        string backgroundMusicPath = Audio + "/Musica De Circo Payaso Tihany.mp3";
        AudioClip backgroundMusic = null;
        if (File.Exists(backgroundMusicPath))
        {
            AssetDatabase.ImportAsset(backgroundMusicPath, ImportAssetOptions.ForceUpdate);
            backgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>(backgroundMusicPath);
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject environment = new GameObject("Environment");
        GameObject platforms = new GameObject("Platforms"); platforms.transform.SetParent(environment.transform);
        // Escenario mayormente plano: recorrido ancho con un pequeño desnivel y una rampa suave.
        Box(platforms.transform, "Plataforma_Inicial", new Vector3(-3f, -0.5f, 0f), new Vector3(10f, 1f, 14f), grass);
        // Plataformas separadas: el espacio entre ellas es un foso real por donde puede caer el jugador.
        Box(platforms.transform, "Plataforma_Superior", new Vector3(12f, -0.5f, 0f), new Vector3(10f, 1f, 14f), stone);
        GameObject pit = new GameObject("Foso_Abierto"); pit.transform.SetParent(platforms.transform);
        Box(pit.transform, "Fondo_Foso", new Vector3(4.5f, -5.5f, 0f), new Vector3(8f, 0.4f, 14f), wall);
        Box(pit.transform, "Pasarela_Sobre_Foso", new Vector3(4.5f, -0.1f, 0f), new Vector3(8f, 0.2f, 1.15f), ramp);
        Box(pit.transform, "Borde_Foso_Inicio", new Vector3(1.2f, 0.08f, 0f), new Vector3(0.16f, 0.14f, 14f), wallCap);
        Box(pit.transform, "Borde_Foso_Final", new Vector3(7.8f, 0.08f, 0f), new Vector3(0.16f, 0.14f, 14f), wallCap);

        // Muros sólidos con color muy contrastante y remate amarillo; los accesos quedan abiertos.
        GameObject boundaries = new GameObject("Muros_Perimetrales"); boundaries.transform.SetParent(environment.transform);
        Wall(boundaries.transform, "Muro_Trasero_Inicial", new Vector3(-3f, 0.7f, 7.15f), new Vector3(10f, 1.4f, 0.4f), wall, wallCap);
        Wall(boundaries.transform, "Muro_Lateral_Izquierdo", new Vector3(-8.15f, 0.7f, 0.2f), new Vector3(0.4f, 1.4f, 13.6f), wall, wallCap);
        Wall(boundaries.transform, "Muro_Frontal_Inicial", new Vector3(-3f, 0.7f, -7.15f), new Vector3(10f, 1.4f, 0.4f), wall, wallCap);
        Wall(boundaries.transform, "Muro_Trasero_Superior", new Vector3(12f, 0.7f, 7.15f), new Vector3(10f, 1.4f, 0.4f), wall, wallCap);
        Wall(boundaries.transform, "Muro_Lateral_Superior", new Vector3(17.15f, 0.7f, 0.2f), new Vector3(0.4f, 1.4f, 13.6f), wall, wallCap);
        Wall(boundaries.transform, "Muro_Frontal_Superior", new Vector3(12f, 0.7f, -7.15f), new Vector3(10f, 1.4f, 0.4f), wall, wallCap);

        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Jugador"; player.tag = "Player"; player.transform.position = new Vector3(-7f, 0.85f, 0f);
        player.GetComponent<Renderer>().sharedMaterial = playerMat;
        CapsuleCollider capsule = player.GetComponent<CapsuleCollider>();
        capsule.radius = 0.42f; capsule.height = 1.7f; capsule.center = Vector3.zero;
        Rigidbody rb = player.AddComponent<Rigidbody>(); rb.mass = 1f; rb.linearDamping = 0.25f; rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation; rb.interpolation = RigidbodyInterpolation.Interpolate; rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        PlayerController controller = player.AddComponent<PlayerController>();
        FallReset fallReset = player.AddComponent<FallReset>();
        fallReset.fallClip = fallClip;
        fallReset.fallAudioSource = player.AddComponent<AudioSource>();
        fallReset.fallAudioSource.playOnAwake = false;
        fallReset.fallAudioSource.spatialBlend = 0f;
        Transform ground = new GameObject("GroundCheck").transform; ground.SetParent(player.transform); ground.localPosition = new Vector3(0f, -0.82f, 0f);
        controller.groundCheck = ground;

        GameObject model = new GameObject("Modelo_Compuesto"); model.transform.SetParent(player.transform); model.transform.localPosition = Vector3.zero;
        Sphere(model.transform, "Visor", new Vector3(0f, 0.28f, 0.39f), new Vector3(0.55f, 0.22f, 0.12f), white);
        // Se quita la mochila/cubo para evitar que un collider hijo bloquee al personaje.

        GameObject cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>(); camera.fieldOfView = 60f;
        cameraObject.AddComponent<AudioListener>();
        CameraFollow follow = cameraObject.AddComponent<CameraFollow>(); follow.target = player.transform;
        follow.offset = new Vector3(0f, 9f, -13f); cameraObject.transform.position = player.transform.position + follow.offset;
        controller.cameraTransform = cameraObject.transform;

        GameObject lightObject = new GameObject("Luz_Direccional");
        Light light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.25f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        RenderSettings.ambientLight = new Color(0.36f, 0.4f, 0.48f);

        GameObject manager = new GameObject("GameManager");
        GameManager gameManager = manager.AddComponent<GameManager>();
        gameManager.targetScore = 16;
        gameManager.pickupClip = pickupClip;
        gameManager.pickupAudioSource = manager.AddComponent<AudioSource>();
        gameManager.pickupAudioSource.playOnAwake = false;
        manager.AddComponent<GameHUD>();
        if (backgroundMusic != null)
        {
            GameObject musicObject = new GameObject("Musica_Fondo");
            AudioSource musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.clip = backgroundMusic;
            musicSource.playOnAwake = true;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.125f;
        }
        else
        {
            Debug.LogError("No se pudo importar Musica De Circo Payaso Tihany.mp3 desde Assets/Game/Audio.");
        }
        GameObject items = new GameObject("Coleccionables");
        Vector3[] normalPositions = { new(-7f, 0.8f, -2f), new(-5f, 0.8f, 2f), new(-2f, 0.8f, -2f), new(0f, 0.8f, 3f), new(9f, 0.8f, -3f), new(12f, 0.8f, 2f), new(14f, 0.8f, -2f) };
        foreach (Vector3 p in normalPositions) ((GameObject)PrefabUtility.InstantiatePrefab(normalPrefab, scene)).transform.SetPositionAndRotation(p, Quaternion.identity);
        Vector3[] specialPositions = { new(-6f, 0.8f, -3.7f), new(10f, 0.8f, 3f), new(14f, 0.8f, 3.5f) };
        foreach (Vector3 p in specialPositions) ((GameObject)PrefabUtility.InstantiatePrefab(specialPrefab, scene)).transform.SetPositionAndRotation(p, Quaternion.identity);

        // Newly instantiated prefab roots are scene roots; parent them under the items container.
        foreach (Collectible item in Object.FindObjectsByType<Collectible>(FindObjectsSortMode.None)) item.transform.SetParent(items.transform, true);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Scenes + "/Plataforma3D.unity", true) };
        EditorSceneManager.SaveScene(scene, Scenes + "/Plataforma3D.unity");
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("Escena Plataforma3D creada con 7 coleccionables normales y 3 especiales. Pulsa Play para comenzar.");
    }

    private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent); go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; return go;
    }

    private static void Wall(Transform parent, string name, Vector3 position, Vector3 scale, Material wallMaterial, Material capMaterial)
    {
        GameObject body = Box(parent, name, position, scale, wallMaterial);
        body.transform.localScale = scale;
        bool horizontalWall = scale.x > scale.z;
        Vector3 capScale = horizontalWall
            ? new Vector3(scale.x, 0.16f, scale.z + 0.06f)
            : new Vector3(scale.x + 0.06f, 0.16f, scale.z);
        Box(parent, name + "_Remate_Visible", position + Vector3.up * (scale.y * 0.5f + 0.08f), capScale, capMaterial);
    }

    private static GameObject Sphere(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = name; go.transform.SetParent(parent); go.transform.localPosition = localPosition; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material; UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
    }

    private static AudioClip MakePickupClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.24f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float[] notes = { 880f, 1174.66f, 1567.98f };
        float[] starts = { 0f, 0.045f, 0.09f };
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float value = 0f;
            for (int note = 0; note < notes.Length; note++)
            {
                float localTime = time - starts[note];
                if (localTime < 0f) continue;
                float envelope = Mathf.Exp(-localTime * 18f) * Mathf.Min(1f, localTime * 180f);
                value += Mathf.Sin(2f * Mathf.PI * notes[note] * localTime) * envelope / notes.Length;
            }
            samples[i] = value * 0.65f;
        }

        string path = Audio + "/PickupChime.wav";
        using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
        {
            int dataBytes = sampleCount * sizeof(short);
            WriteAscii(writer, "RIFF"); writer.Write(36 + dataBytes); WriteAscii(writer, "WAVE");
            WriteAscii(writer, "fmt "); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(sampleRate); writer.Write(sampleRate * sizeof(short)); writer.Write((short)sizeof(short)); writer.Write((short)16);
            WriteAscii(writer, "data"); writer.Write(dataBytes);
            foreach (float sample in samples) writer.Write((short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static void WriteAscii(BinaryWriter writer, string value)
    {
        writer.Write(System.Text.Encoding.ASCII.GetBytes(value));
    }

    private static AudioClip MakeFallClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.62f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];
        float phase = 0f;
        uint noiseState = 0xA341316Cu;
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float progress = time / duration;
            float frequency = 118f - 62f * progress + 8f * Mathf.Sin(time * 19f);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            noiseState = 1664525u * noiseState + 1013904223u;
            float noise = ((noiseState >> 8) / 8388607.5f) - 1f;
            float envelope = Mathf.Sin(Mathf.PI * progress) * Mathf.Exp(-progress * 0.35f);
            float rasp = Mathf.Sin(phase) + 0.38f * Mathf.Sin(phase * 2.17f) + 0.16f * noise;
            samples[i] = Mathf.Clamp(rasp * envelope * 0.58f, -1f, 1f);
        }

        string path = Audio + "/FartFall.wav";
        using (BinaryWriter writer = new BinaryWriter(File.Create(path)))
        {
            int dataBytes = sampleCount * sizeof(short);
            WriteAscii(writer, "RIFF"); writer.Write(36 + dataBytes); WriteAscii(writer, "WAVE");
            WriteAscii(writer, "fmt "); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
            writer.Write(sampleRate); writer.Write(sampleRate * sizeof(short)); writer.Write((short)sizeof(short)); writer.Write((short)16);
            WriteAscii(writer, "data"); writer.Write(dataBytes);
            foreach (float sample in samples) writer.Write((short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static Material MakeMaterial(string name, Color color, float metallic)
    {
        string path = Materials + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = GraphicsSettings.currentRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Standard");
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }
        material.color = color;
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.45f);
        EditorUtility.SetDirty(material); return material;
    }

    private static GameObject MakeCollectiblePrefab(string name, Collectible.Kind kind, int points, float spin, Material material, bool gem)
    {
        string path = Prefabs + "/" + name + ".prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;
        GameObject root = new GameObject(name);
        root.tag = kind == Collectible.Kind.Normal ? "CollectibleNormal" : "CollectibleSpecial";
        GameObject visual = GameObject.CreatePrimitive(gem ? PrimitiveType.Sphere : PrimitiveType.Cylinder);
        visual.name = gem ? "Cristal_Especial" : "Moneda_Normal"; visual.transform.SetParent(root.transform); visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = gem ? new Vector3(0.72f, 1.05f, 0.72f) : new Vector3(0.8f, 0.18f, 0.8f);
        if (gem) visual.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
        visual.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = visual.GetComponent<Collider>(); UnityEngine.Object.DestroyImmediate(collider);
        SphereCollider trigger = root.AddComponent<SphereCollider>(); trigger.isTrigger = true; trigger.radius = 0.72f;
        Collectible collectible = root.AddComponent<Collectible>(); collectible.kind = kind; collectible.points = points; collectible.rotationSpeed = spin;
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path); UnityEngine.Object.DestroyImmediate(root); return prefab;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scripts")) AssetDatabase.CreateFolder("Assets", "Scripts");
        if (!AssetDatabase.IsValidFolder(Root)) AssetDatabase.CreateFolder("Assets", "Game");
        foreach (string folder in new[] { Materials, Prefabs, Audio, Scenes, "Assets/Editor" })
        {
            if (AssetDatabase.IsValidFolder(folder)) continue;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/'); string name = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolderPath(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = manager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++) if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        manager.ApplyModifiedProperties();
    }

    private static void EnsureFolderPath(string path)
    {
        string parent = Path.GetDirectoryName(path).Replace('\\', '/'); string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolderPath(parent);
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
    }
}

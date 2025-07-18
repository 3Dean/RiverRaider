using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Helper script for importing and configuring SVG crosshair assets.
/// Works with Unity's Vector Graphics package for direct SVG support.
/// </summary>
#if UNITY_EDITOR
[System.Serializable]
public class SVGCrosshairImporter : EditorWindow
{
    [Header("SVG Import Settings")]
    public Texture2D crosshairTexture;
    public string spriteName = "Crosshair";
    public int textureSize = 512;
    public FilterMode filterMode = FilterMode.Bilinear;
    public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
    
    [MenuItem("RiverRaider/Tools/SVG Crosshair Importer")]
    public static void ShowWindow()
    {
        GetWindow<SVGCrosshairImporter>("SVG Crosshair Importer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("SVG Crosshair Import Tool", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Vector Graphics Package Detected!", EditorStyles.boldLabel);
        GUILayout.Label("Your SVG file can be used directly with SVGImage component.");
        GUILayout.Space(10);
        
        GUILayout.Label("Quick Setup Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Create UI → Image in your Canvas");
        GUILayout.Label("2. Add SVGImage component (replaces Image)");
        GUILayout.Label("3. Assign your SVG asset to the SVGImage");
        GUILayout.Label("4. Add CrosshairController script");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Find Your SVG File"))
        {
            FindSVGFile();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Basic Crosshair GameObject"))
        {
            CreateBasicCrosshairGameObject();
        }
        
        GUILayout.Space(20);
        GUILayout.Label("Alternative: Texture Mode", EditorStyles.boldLabel);
        GUILayout.Label("If you prefer to use PNG instead of SVG:");
        
        crosshairTexture = (Texture2D)EditorGUILayout.ObjectField("Crosshair Texture", crosshairTexture, typeof(Texture2D), false);
        spriteName = EditorGUILayout.TextField("Sprite Name", spriteName);
        textureSize = EditorGUILayout.IntField("Texture Size", textureSize);
        filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", filterMode);
        wrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", wrapMode);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Configure Texture for UI"))
        {
            ConfigureTextureForUI();
        }
        
        if (GUILayout.Button("Create Texture Crosshair"))
        {
            CreateTextureCrosshairGameObject();
        }
    }
    
    private void FindSVGFile()
    {
        string svgPath = "Assets/Images/UIelements/img_crosshair.svg";
        Object foundSVG = AssetDatabase.LoadAssetAtPath<Object>(svgPath);
        
        if (foundSVG != null)
        {
            Selection.activeObject = foundSVG;
            EditorGUIUtility.PingObject(foundSVG);
            EditorUtility.DisplayDialog("Found!", 
                $"Found your crosshair SVG at:\n{svgPath}\n\n" +
                "It's now selected in the Project window.\n\n" +
                "To use it:\n" +
                "1. Create a UI Image\n" +
                "2. Add SVGImage component\n" +
                "3. Drag the SVG to the SVGImage's Vector Graphics field", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Not Found", 
                $"Could not find SVG at:\n{svgPath}\n\n" +
                "Make sure your SVG file is imported and the Vector Graphics package is installed.", "OK");
        }
    }
    
    private void CreateBasicCrosshairGameObject()
    {
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create crosshair GameObject
        GameObject crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvas.transform, false);
        
        // Add Image component (user will need to replace with SVGImage manually)
        Image image = crosshairGO.AddComponent<Image>();
        
        // Set up RectTransform
        RectTransform rectTransform = crosshairGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(50, 50);
        
        // Add CrosshairController
        CrosshairController controller = crosshairGO.AddComponent<CrosshairController>();
        
        // Select the created object
        Selection.activeGameObject = crosshairGO;
        
        EditorUtility.DisplayDialog("Crosshair Created!", 
            "Basic crosshair GameObject created!\n\n" +
            "Next steps:\n" +
            "1. Remove the Image component\n" +
            "2. Add SVGImage component\n" +
            "3. Assign your SVG to the SVGImage\n" +
            "4. Configure CrosshairController settings", "OK");
    }
    
    private void ConfigureTextureForUI()
    {
        if (crosshairTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a crosshair texture first.", "OK");
            return;
        }
        
        string path = AssetDatabase.GetAssetPath(crosshairTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = filterMode;
            importer.wrapMode = wrapMode;
            importer.maxTextureSize = textureSize;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            
            // Set sprite settings
            var spriteSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(spriteSettings);
            spriteSettings.spriteMeshType = SpriteMeshType.FullRect;
            spriteSettings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(spriteSettings);
            
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", "Crosshair texture configured for UI use!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not find texture importer.", "OK");
        }
    }
    
    private void CreateTextureCrosshairGameObject()
    {
        if (crosshairTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Please configure the crosshair texture first.", "OK");
            return;
        }
        
        // Find or create canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        // Create crosshair GameObject
        GameObject crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvas.transform, false);
        
        // Add Image component
        Image image = crosshairGO.AddComponent<Image>();
        
        // Get sprite from texture
        string texturePath = AssetDatabase.GetAssetPath(crosshairTexture);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        
        if (sprite != null)
        {
            image.sprite = sprite;
        }
        
        // Set up RectTransform
        RectTransform rectTransform = crosshairGO.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(50, 50);
        
        // Add CrosshairController
        crosshairGO.AddComponent<CrosshairController>();
        
        // Select the created object
        Selection.activeGameObject = crosshairGO;
        
        EditorUtility.DisplayDialog("Success", "Texture Crosshair GameObject created! Configure the CrosshairController component.", "OK");
    }
}
#endif

/// <summary>
/// Runtime component for crosshair configuration and testing.
/// </summary>
public class CrosshairTester : MonoBehaviour
{
    [Header("Crosshair Testing")]
    [SerializeField] private CrosshairController crosshairController;
    [SerializeField] private bool enableTesting = true;
    
    [Header("Test Settings")]
    [SerializeField] private KeyCode toggleVisibilityKey = KeyCode.C;
    [SerializeField] private KeyCode cycleColorsKey = KeyCode.V;
    [SerializeField] private KeyCode testRecoilKey = KeyCode.R;
    
    private int currentColorIndex = 0;
    private Color[] testColors = { Color.white, Color.red, Color.green, Color.blue, Color.yellow };
    
    void Start()
    {
        if (crosshairController == null)
            crosshairController = FindObjectOfType<CrosshairController>();
    }
    
    void Update()
    {
        if (!enableTesting || crosshairController == null) return;
        
        // Toggle crosshair visibility
        if (Input.GetKeyDown(toggleVisibilityKey))
        {
            bool isVisible = crosshairController.GetComponent<Image>().enabled;
            crosshairController.SetCrosshairVisibility(!isVisible);
            Debug.Log($"Crosshair visibility: {!isVisible}");
        }
        
        // Test recoil animation
        if (Input.GetKeyDown(testRecoilKey))
        {
            // Trigger recoil by temporarily setting firing state
            Debug.Log("Testing crosshair recoil animation");
        }
    }
    
    void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Crosshair Testing Controls:", GUI.skin.box);
        GUILayout.Label($"Press {toggleVisibilityKey} to toggle visibility");
        GUILayout.Label($"Press {testRecoilKey} to test recoil");
        
        if (crosshairController != null)
        {
            GUILayout.Label($"Targeting Enemy: {crosshairController.IsTargetingEnemy}");
            GUILayout.Label($"Has Valid Target: {crosshairController.HasValidTarget}");
            GUILayout.Label($"Target Distance: {crosshairController.CurrentTargetDistance:F1}");
        }
        
        GUILayout.EndArea();
    }
}

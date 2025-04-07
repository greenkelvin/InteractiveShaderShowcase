using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages shader selection and dynamically generates UI controls
/// for editing exposed shader properties such as floats and colors.
/// Also handles tooltip visibility and provides a singleton reference.
/// </summary>
public class ShaderPropertyManager : MonoBehaviour
{
    // Singleton instance for global access
    public static ShaderPropertyManager Instance { get; private set; }

    [Header("UI References")]
    public TMP_Dropdown shaderDropdown;         // Dropdown for selecting the shader
    public Renderer targetObject;               // The object whose material will be modified
    public GameObject propertiesPanel;          // Parent UI container for dynamically generated sliders
    public GameObject sliderPrefab;             // Prefab used to create new property sliders
    public GameObject colorPickerPrefab;        // Prefab for color picker UI
    public Toggle tooltipToggle;                // Toggle to enable/disable tooltips

    // Internal state
    private Dictionary<string, Slider> activeSliders = new Dictionary<string, Slider>(); // Active UI sliders
    private Dictionary<string, Material> shaderMaterials = new Dictionary<string, Material>(); // Material instances mapped by internal name
    private Dictionary<string, string> shaderNameMapping = new Dictionary<string, string>();   // Maps dropdown display name to internal material name
    private string currentShaderInternalName; // Internal name of the currently active shader

    /// <summary>
    /// Initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Initializes the dropdown, material references, and tooltip toggle listener.
    /// Automatically loads the first shader on scene start.
    /// </summary>
    void Start()
    {
        // Mapping display names (UI) to actual Resources folder material names
        shaderNameMapping["Procedural Marble"] = "MarbleMaterial";
        shaderNameMapping["Water"] = "WaterMaterial";
        shaderNameMapping["Refraction/Transparency"] = "RefractionMaterial";
        shaderNameMapping["Dynamic Lighting"] = "DynamicLightingMaterial";
        shaderNameMapping["Iridescent"] = "IridescentMaterial";

        // Load material assets from Resources/Materials/
        shaderMaterials["MarbleMaterial"] = Resources.Load<Material>("Materials/MarbleMaterial");
        shaderMaterials["WaterMaterial"] = Resources.Load<Material>("Materials/WaterMaterial");
        shaderMaterials["RefractionMaterial"] = Resources.Load<Material>("Materials/RefractionMaterial");
        shaderMaterials["DynamicLightingMaterial"] = Resources.Load<Material>("Materials/DynamicLightingMaterial");
        shaderMaterials["IridescentMaterial"] = Resources.Load<Material>("Materials/IridescentMaterial");

        // Connect the tooltip toggle to the tooltip system
        tooltipToggle.onValueChanged.AddListener(isOn =>
        {
            TooltipManager.Instance.ToggleTooltipVisibility(isOn);
        });

        // Load and display the first shader from the dropdown on start
        UpdateShaderUI(0);
    }

    /// <summary>
/// Updates the shader properties panel when a new shader is selected from the dropdown.
/// This method clears existing sliders, assigns the selected material to the target object,
/// displays appropriate color pickers, and dynamically generates sliders for float properties.
/// Also triggers the tooltip system with shader-specific descriptions.
/// </summary>
/// <param name="index">The index of the selected shader in the dropdown menu.</param>
    public void UpdateShaderUI(int index)
    {
        //Remove old UI elements (sliders, labels, etc.)
        foreach (Transform child in propertiesPanel.transform)
        {
            Destroy(child.gameObject);
        }
        activeSliders.Clear();

        // Get the display name of the selected shader from the dropdown
        string dropdownName = shaderDropdown.options[index].text;

        // Check if the name exists in the dropdown-to-material mapping
        if (!shaderNameMapping.ContainsKey(dropdownName))
        {
            Debug.LogError($"Shader dropdown name '{dropdownName}' does not match any mapped material names!");
            return;
        }

        // Look up the internal material name
        string materialName = shaderNameMapping[dropdownName];

        // Make sure the corresponding material is loaded and valid
        if (!shaderMaterials.ContainsKey(materialName) || shaderMaterials[materialName] == null)
        {
            Debug.LogError($"Shader material '{materialName}' not found! Check the Resources folder.");
            return;
        }

        // Apply the material to the target object
        targetObject.material = shaderMaterials[materialName];
        Material mat = targetObject.material;

        Debug.Log($"Switching shader to: {dropdownName} -> {materialName}");

        // Collect all color-type properties available in this shader
        List<string> colorProperties = new List<string>();
        if (mat.HasProperty("_BaseColor")) colorProperties.Add("_BaseColor");
        if (mat.HasProperty("_Color")) colorProperties.Add("_Color");
        if (mat.HasProperty("_VeinColor")) colorProperties.Add("_VeinColor");

        // Open the color picker if any color properties are present
        if (colorProperties.Count > 0)
        {
            FindFirstObjectByType<CustomColorPicker>().OpenColorPicker(mat, colorProperties, targetObject);
        }
        else
        {
            Debug.LogError($"No valid color properties found for {materialName}.");
        }

        // 📏 Create UI sliders for any float properties supported by the shader
        if (mat.HasProperty("_Scale")) CreateSlider("_Scale", 1f, 10f, mat);
        if (mat.HasProperty("_Intensity")) CreateSlider("_Intensity", 0f, 5f, mat);
        if (mat.HasProperty("_WaveSpeed")) CreateSlider("_WaveSpeed", 0.1f, 5f, mat);
        if (mat.HasProperty("_WaveStrength")) CreateSlider("_WaveStrength", 0.01f, 0.5f, mat);
        if (mat.HasProperty("_FresnelPower")) CreateSlider("_FresnelPower", 1f, 5f, mat);
        if (mat.HasProperty("_Transparency")) CreateSlider("_Transparency", 0f, 1f, mat);
        if (mat.HasProperty("_RefractionStrength")) CreateSlider("_RefractionStrength", 0f, 0.1f, mat);
        if (mat.HasProperty("_SpecularStrength")) CreateSlider("_SpecularStrength", 0f, 1f, mat);
        if (mat.HasProperty("_Shininess")) CreateSlider("_Shininess", 1f, 5f, mat);
        if (mat.HasProperty("_IridescenceStrength")) CreateSlider("_IridescenceStrength", 0f, 5f, mat);
        if (mat.HasProperty("_NoiseScale")) CreateSlider("_NoiseScale", 1f, 20f, mat);

        // 💡 Update tooltip with the shader description
        UpdateTooltipForShader(dropdownName);
    }

/// <summary>
/// Returns the internal name of the currently selected shader/material.
/// </summarypublic>
public string GetCurrentShaderInternalName()
{
    return currentShaderInternalName;
}

/// <summary>
/// Converts a shader property name (e.g., "_BaseColor") into a user-friendly label (e.g., "Base Color").
/// Removes leading underscores and adds spaces between camelCase segments.
/// </summary>
/// <param name="rawPropertyName">The raw property name from the shader.</param>
/// <returns>A cleaned-up, readable version of the property name.</returns>
private string FormatPropertyName(string rawPropertyName)
{
    if (string.IsNullOrEmpty(rawPropertyName)) return "";

    if (rawPropertyName.StartsWith("_"))
        rawPropertyName = rawPropertyName.Substring(1);

    return System.Text.RegularExpressions.Regex.Replace(
        rawPropertyName,
        "(\\B[A-Z])", " $1"
    );
}

    /// <summary>
/// Dynamically creates a labeled slider UI element for a float shader property and hooks it up to the corresponding material.
/// This enables real-time control over shader values via the UI.
/// </summary>
/// <param name="propertyName">The internal name of the shader property (e.g., "_Intensity").</param>
/// <param name="min">Minimum value of the slider.</param>
/// <param name="max">Maximum value of the slider.</param>
/// <param name="mat">The material instance containing the property to modify.</param>
void CreateSlider(string propertyName, float min, float max, Material mat)
{
    // Instantiate a new slider object from the prefab and attach it to the properties panel
    GameObject sliderObj = Instantiate(sliderPrefab, propertiesPanel.transform);
    sliderObj.name = propertyName;

    // Configure RectTransform to stretch and fit properly within the panel
    RectTransform sliderRT = sliderObj.GetComponent<RectTransform>();
    if (sliderRT != null)
    {
        sliderRT.anchorMin = new Vector2(0, 0);  // Align left
        sliderRT.anchorMax = new Vector2(1, 0);  // Align right
        sliderRT.pivot = new Vector2(0.5f, 1);   // Pivot at top center
        sliderRT.sizeDelta = new Vector2(500, 50); // Set default size
    }

    // Add a LayoutElement component to maintain consistent layout spacing
    LayoutElement layout = sliderObj.GetComponent<LayoutElement>();
    if (layout == null)
    {
        layout = sliderObj.AddComponent<LayoutElement>();
    }
    layout.preferredHeight = 50;
    layout.preferredWidth = 400;

    // Add and configure a label above the slider to describe the property
    TMP_Text label = sliderObj.GetComponentInChildren<TMP_Text>(true);
    if (label != null)
    {
        label.text = FormatPropertyName(propertyName); // Format "_Intensity" → "Intensity"
        RectTransform labelRT = label.GetComponent<RectTransform>();
        if (labelRT != null)
        {
            labelRT.anchorMin = new Vector2(0, 1);
            labelRT.anchorMax = new Vector2(1, 1);
            labelRT.pivot = new Vector2(0.5f, 1);
            labelRT.anchoredPosition = new Vector2(0, 20); // Move label above the slider
            labelRT.sizeDelta = new Vector2(400, 30);
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    // Configure the slider’s min/max range and initial value from the material
    Slider slider = sliderObj.GetComponent<Slider>();
    if (slider == null)
    {
        Debug.LogError($"Slider component missing inside {sliderObj.name}!");
        return;
    }

    slider.minValue = min;
    slider.maxValue = max;
    slider.value = mat.GetFloat(propertyName);

    // Hook up value change listener to update material in real-time
    slider.onValueChanged.AddListener(value =>
    {
        mat.SetFloat(propertyName, value);
        Debug.Log($"Updated {propertyName} to {value}");
    });

    // Track this slider in the active dictionary for later reference if needed
    activeSliders[propertyName] = slider;
}


    void CreateColorPicker(Material mat)
    {
        if (mat == null)
        {
            Debug.LogError("CreateColorPicker received a NULL material!");
            return;
        }

        // 🔥 Collect all available color properties
        List<string> colorProperties = new List<string>();

        if (mat.HasProperty("_BaseColor")) colorProperties.Add("_BaseColor");
        if (mat.HasProperty("_Color")) colorProperties.Add("_Color");
        if (mat.HasProperty("_VeinColor")) colorProperties.Add("_VeinColor");

        if (colorProperties.Count == 0)
        {
            Debug.LogError($"🚨 No valid color properties found for {mat.name}.");
            return;
        }

        Debug.Log($"Opening Color Picker automatically for properties: {string.Join(", ", colorProperties)} on {mat.name}");

        // 🔥 Pass the list of properties instead of a single property name
        FindFirstObjectByType<CustomColorPicker>().OpenColorPicker(mat, colorProperties, targetObject);
    }

    /// <summary>
/// Opens the color picker UI for a given material, allowing the user to modify any available color properties.
/// It scans the material for known color property names and displays them as selectable options.
/// </summary>
/// <param name="mat">The material whose color properties should be exposed to the color picker.</param>
void OpenColorPicker(Material mat)
{
    if (mat == null)
    {
        Debug.LogError("OpenColorPicker received a NULL material!");
        return;
    }

    // Collect all supported color properties from the material
    List<string> colorProperties = new List<string>();

    if (mat.HasProperty("_BaseColor")) colorProperties.Add("_BaseColor");
    if (mat.HasProperty("_Color")) colorProperties.Add("_Color");
    if (mat.HasProperty("_VeinColor")) colorProperties.Add("_VeinColor");

    if (colorProperties.Count == 0)
    {
        Debug.LogError($"No valid color properties found for {mat.name}.");
        return;
    }

    Debug.Log($"Opening Color Picker for properties: {string.Join(", ", colorProperties)} on material {mat.name}");

    // Launch the color picker UI, passing the material, available properties, and target object
    FindFirstObjectByType<CustomColorPicker>().OpenColorPicker(mat, colorProperties, targetObject);
}


    /// <summary>
/// Displays a descriptive tooltip based on the currently selected shader in the dropdown.
/// Each shader is matched to a detailed explanation and a tooltip context,
/// which allows the "Show Code" button to load the correct shader source file.
/// </summary>
/// <param name="shaderName">The display name of the selected shader from the dropdown.</param>
void UpdateTooltipForShader(string shaderName)
{
    // Select the appropriate tooltip message for the given shader name
    string message = shaderName switch
    {
        "Procedural Marble" => 
            "Procedural Marble Shader:\nGenerates marble-like textures using layered procedural noise. " +
            "You can modify the base color, vein color, scale, and intensity to simulate natural stone patterns. " +
            "The effect is completely textureless and purely mathematical.",

        "Water" => 
            "Water Shader:\nSimulates flowing water using animated sine waves and normal maps. " +
            "Wave speed and strength determine the surface distortion, while Fresnel effects simulate light bending near edges. " +
            "Useful for oceans, lakes, or stylized fluid surfaces.",

        "Refraction/Transparency" => 
            "Refraction Shader:\nCreates a transparent, glass-like surface that distorts background elements based on a normal map. " +
            "Transparency and refraction strength allow you to simulate water, glass, or ice. " +
            "Great for optical illusion effects.",

        "Dynamic Lighting" => 
            "Dynamic Lighting Shader:\nImplements Lambertian diffuse and Blinn-Phong specular highlights, reacting to real-time scene lights. " +
            "Adjust shininess and specular strength to simulate plastic, metal, or glossy surfaces. " +
            "Useful for learning how surface lighting works.",

        "Iridescent" => 
            "Iridescent Shader:\nSimulates color shifting effects like oil slicks or holograms by blending colors based on view angle and surface normals. " +
            "The result is a dynamic, rainbow-like reflection that changes as the camera moves. " +
            "Excellent for visualizing angular reflectance.",

        _ => 
            "Shader:\nThis shader demonstrates custom visual effects. Use the sliders to explore how the parameters affect the outcome."
    };

    // Assign the appropriate context for internal use (e.g. code preview selection)
    TooltipContext context = shaderName switch
    {
        "Procedural Marble" => TooltipContext.ProceduralMarble,
        "Water" => TooltipContext.Water,
        "Refraction/Transparency" => TooltipContext.Refraction,
        "Dynamic Lighting" => TooltipContext.DynamicLighting,
        "Iridescent" => TooltipContext.Iridescent,
        _ => TooltipContext.None
    };

    // Display the tooltip with the chosen message and context
    TooltipManager.Instance.ShowTooltip(message, context);
}
}
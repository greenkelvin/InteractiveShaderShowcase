using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages post-processing effects in the scene, including color tinting and edge detection.
/// Connects UI elements like toggles, sliders, and dropdowns to update material properties in real-time.
/// </summary>
public class PostProcessingManager : MonoBehaviour
{
    [Header("Tint Effect Controls")]
    public Toggle effectToggle;                    // Toggle to enable/disable the tint effect
    public TMP_Dropdown colorDropdown;             // Dropdown to select tint color
    public Slider intensitySlider;                 // Slider to control tint intensity

    [Header("Edge Detection Controls")]
    public Toggle edgeToggle;                      // Toggle to enable/disable edge detection
    public Slider edgeThresholdSlider;             // Slider to adjust edge detection threshold
    public TMP_Dropdown edgeColorDropdown;         // Dropdown to select edge highlight color

    [Header("Post Processing Material")]
    public Material postProcessingMaterial;        // Material using the PostProcessingCombined shader

    /// <summary>
    /// Subscribes to UI control events and initializes post-processing settings based on UI state.
    /// </summary>
    void Start()
    {
        // Hook up UI event listeners
        effectToggle.onValueChanged.AddListener(ToggleEffect);
        colorDropdown.onValueChanged.AddListener(ChangeTintColor);
        intensitySlider.onValueChanged.AddListener(UpdateTintIntensity);

        edgeToggle.onValueChanged.AddListener(ToggleEdgeDetection);
        edgeThresholdSlider.onValueChanged.AddListener(UpdateEdgeThreshold);
        edgeColorDropdown.onValueChanged.AddListener(ChangeEdgeColor);

        // Apply initial values from UI
        ToggleEffect(effectToggle.isOn);
        ToggleEdgeDetection(edgeToggle.isOn);
        UpdateTintIntensity(intensitySlider.value);
        UpdateEdgeThreshold(edgeThresholdSlider.value);
        ChangeEdgeColor(edgeColorDropdown.value);
    }

    /// <summary>
    /// Enables or disables the tint effect and updates tooltip.
    /// </summary>
    void ToggleEffect(bool isOn)
    {
        postProcessingMaterial.SetFloat("_TintIntensity", isOn ? intensitySlider.value : 0f);

        if (isOn)
        {
            TooltipManager.Instance.ShowTooltip(
                "Post-Processing Tint:\nApplies a full-screen color tint by blending the rendered image with a user-selected color. " +
                "Use the dropdown to select a color and the slider to adjust its intensity. " +
                "Useful for mood filters or visual emphasis.",
                TooltipContext.PostProcessing
            );
        }
    }

    /// <summary>
    /// Changes the tint color applied by the post-processing shader based on dropdown selection.
    /// </summary>
    void ChangeTintColor(int index)
    {
        Color selectedColor = Color.white;
        switch (index)
        {
            case 0: selectedColor = Color.red; break;
            case 1: selectedColor = Color.green; break;
            case 2: selectedColor = Color.blue; break;
            case 3: selectedColor = Color.yellow; break;
        }
        postProcessingMaterial.SetColor("_TintColor", selectedColor);
    }

    /// <summary>
    /// Updates the intensity of the tint effect if the effect is enabled.
    /// </summary>
    void UpdateTintIntensity(float value)
    {
        if (effectToggle.isOn)
        {
            postProcessingMaterial.SetFloat("_TintIntensity", value);
        }
    }

    /// <summary>
    /// Enables or disables the edge detection effect and updates tooltip.
    /// </summary>
    void ToggleEdgeDetection(bool isOn)
    {
        postProcessingMaterial.SetFloat("_EnableEdgeDetection", isOn ? 1f : 0f);

        if (isOn)
        {
            TooltipManager.Instance.ShowTooltip(
                "Post-Processing Edge Detection:\nHighlights edges in the scene based on differences in color values between neighboring pixels. " +
                "This creates an outline or sketch-like effect. " +
                "You can customize the edge color and threshold to exaggerate or soften edge sensitivity.",
                TooltipContext.PostProcessing
            );
        }
    }

    /// <summary>
    /// Updates the edge threshold used to determine how strong the edge detection is.
    /// </summary>
    void UpdateEdgeThreshold(float value)
    {
        postProcessingMaterial.SetFloat("_EdgeThreshold", value);
    }

    /// <summary>
    /// Changes the color of the edge highlights based on dropdown selection.
    /// </summary>
    void ChangeEdgeColor(int index)
    {
        Color selectedColor = Color.white;

        switch (index)
        {
            case 0: selectedColor = Color.white; break;
            case 1: selectedColor = Color.red; break;
            case 2: selectedColor = Color.green; break;
            case 3: selectedColor = Color.blue; break;
            case 4: selectedColor = Color.black; break;
        }

        postProcessingMaterial.SetColor("_EdgeColor", selectedColor);
    }
}

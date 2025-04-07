using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Provides a custom UI for selecting and applying color values to a shader's color properties at runtime.
/// </summary>
public class CustomColorPicker : MonoBehaviour
{
    // UI Elements for the color picker
    public Image colorPreview;
    public Slider redSlider, greenSlider, blueSlider, alphaSlider;
    public TMP_Dropdown propertyDropdown; // Dropdown to select which color property to modify
    public Button applyButton;

    private Material currentMaterial;
    private string propertyName; // Currently selected shader property
    private Renderer targetObject;
    private List<string> availableProperties = new List<string>();
    private Dictionary<string, string> formattedToActualProperty = new();

    void Start()
    {
        // Subscribe to slider change events to update preview
        redSlider.onValueChanged.AddListener(value => UpdateColorPreview());
        greenSlider.onValueChanged.AddListener(value => UpdateColorPreview());
        blueSlider.onValueChanged.AddListener(value => UpdateColorPreview());
        alphaSlider.onValueChanged.AddListener(value => UpdateColorPreview());

        // Subscribe to dropdown change and apply button events
        propertyDropdown.onValueChanged.AddListener(index => ChangeProperty(index));
        applyButton.onClick.AddListener(ApplyColorToMaterial);
    }

    /// <summary>
    /// Initializes the color picker with a list of color properties to edit for a given material and renderer.
    /// </summary>
    /// <param name="material">Material to be modified.</param>
    /// <param name="properties">List of color property names (e.g., _Color, _BaseColor).</param>
    /// <param name="obj">Renderer that holds the target material instance.</param>
    public void OpenColorPicker(Material material, List<string> properties, Renderer obj)
    {
        if (propertyDropdown == null)
        {
            Debug.LogError("Property Dropdown is not assigned in the inspector.");
            return;
        }

        if (properties == null || properties.Count == 0)
        {
            Debug.LogError("Color Picker received an empty property list.");
            return;
        }

        targetObject = obj;
        currentMaterial = material;
        availableProperties = properties;

        // Populate dropdown with formatted property names
        propertyDropdown.ClearOptions();
        formattedToActualProperty.Clear();

        List<string> formattedOptions = new List<string>();
        foreach (string prop in properties)
        {
            string formatted = FormatPropertyName(prop);
            formattedOptions.Add(formatted);
            formattedToActualProperty[formatted] = prop;
        }
        propertyDropdown.AddOptions(formattedOptions);

        ChangeProperty(0); // Load the first property initially
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called when a different property is selected in the dropdown. Updates sliders and preview.
    /// </summary>
    /// <param name="index">Dropdown index of the selected property.</param>
    private void ChangeProperty(int index)
    {
        string formattedName = propertyDropdown.options[index].text;
        propertyName = formattedToActualProperty.ContainsKey(formattedName) ? formattedToActualProperty[formattedName] : formattedName;

        Debug.Log($"Switching color property to: {propertyName}");

        if (currentMaterial.HasProperty(propertyName))
        {
            Color currentColor = currentMaterial.GetColor(propertyName);
            redSlider.value = currentColor.r;
            greenSlider.value = currentColor.g;
            blueSlider.value = currentColor.b;
            alphaSlider.value = currentColor.a;

            UpdateColorPreview();
        }
        else
        {
            Debug.LogError($"Material '{currentMaterial.name}' does not have property: '{propertyName}'");
        }
    }

    /// <summary>
    /// Updates the on-screen preview color to reflect current slider values.
    /// </summary>
    private void UpdateColorPreview()
    {
        if (colorPreview != null)
        {
            Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
            colorPreview.color = newColor;
        }
    }

    /// <summary>
    /// Applies the selected color to the material’s selected shader property.
    /// </summary>
    private void ApplyColorToMaterial()
    {
        if (targetObject == null)
        {
            Debug.LogError("Target object is missing. Ensure OpenColorPicker() is called correctly.");
            return;
        }

        // Ensure we're applying to the most current material reference
        if (currentMaterial != targetObject.material)
        {
            Debug.Log($"Material mismatch. Updating to match {targetObject.material.name}");
            currentMaterial = targetObject.material;
        }

        if (currentMaterial != null && currentMaterial.HasProperty(propertyName))
        {
            Color selectedColor = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);
            currentMaterial.SetColor(propertyName, selectedColor);
            Debug.Log($"Applied {selectedColor} to {propertyName} on {currentMaterial.name}");
        }
        else
        {
            Debug.LogError($"Material '{currentMaterial.name}' does not have property: '{propertyName}'");
        }
    }

    /// <summary>
    /// Converts shader property names like "_BaseColor" to a human-readable format like "Base Color".
    /// </summary>
    /// <param name="rawPropertyName">Original shader property name.</param>
    /// <returns>Formatted name for UI display.</returns>
    private string FormatPropertyName(string rawPropertyName)
    {
        if (string.IsNullOrEmpty(rawPropertyName)) return "";

        if (rawPropertyName.StartsWith("_"))
            rawPropertyName = rawPropertyName.Substring(1);

        return Regex.Replace(rawPropertyName, "(\\B[A-Z])", " $1");
    }
}

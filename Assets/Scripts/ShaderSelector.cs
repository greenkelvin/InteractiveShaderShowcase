using UnityEngine;
using TMPro;

/// <summary>
/// Populates the shader dropdown and handles switching shaders on the target object.
/// Also notifies the ShaderPropertyManager to update the UI when a new shader is selected.
/// </summary>
public class ShaderSelector : MonoBehaviour
{
    // Dropdown UI element for selecting shaders
    public TMP_Dropdown shaderDropdown;

    // The object whose material will be updated
    public Renderer targetObject;

    // Array of available shader materials to switch between
    public Material[] shaderMaterials;

    // Optional display names to show in the dropdown UI
    public string[] shaderDisplayNames;

    /// <summary>
    /// Initializes the dropdown with shader options and sets the initial shader.
    /// </summary>
    void Start()
    {
        // Clear any existing options
        shaderDropdown.ClearOptions();

        // Populate dropdown options with display names or material names
        for (int i = 0; i < shaderMaterials.Length; i++)
        {
            string displayName = (i < shaderDisplayNames.Length) ? shaderDisplayNames[i] : shaderMaterials[i].name;
            shaderDropdown.options.Add(new TMP_Dropdown.OptionData(displayName));
        }

        // Subscribe to dropdown selection change
        shaderDropdown.onValueChanged.AddListener(delegate { ChangeShader(shaderDropdown.value); });

        // Set the first shader as the default if any are available
        if (shaderMaterials.Length > 0)
        {
            ChangeShader(0);
        }
    }

    /// <summary>
    /// Called when a new shader is selected from the dropdown.
    /// Applies the selected material to the target object and updates the UI.
    /// </summary>
    /// <param name="index">Index of the selected shader in the dropdown/material array.</param>
    void ChangeShader(int index)
    {
        if (index >= 0 && index < shaderMaterials.Length)
        {
            // Assign the selected material to the target object's renderer
            targetObject.material = shaderMaterials[index];

            // Notify the ShaderPropertyManager to update the UI for the selected shader
            ShaderPropertyManager shaderManager = FindFirstObjectByType<ShaderPropertyManager>();
            if (shaderManager != null)
            {
                shaderManager.UpdateShaderUI(index);
            }
        }
    }
}

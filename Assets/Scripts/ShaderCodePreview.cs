using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the preview panel that displays the HLSL shader source code
/// based on the current shader selection or tooltip context.
/// </summary>
public class ShaderCodePreview : MonoBehaviour
{
    [Tooltip("Panel that displays the shader code.")]
    public GameObject codePanel;

    [Tooltip("Text field that displays the contents of the shader file.")]
    public TMP_Text codeText;

    [Tooltip("Dropdown for selecting the shader (used to determine which code to show).")]
    public TMP_Dropdown shaderDropdown;

    [Tooltip("Button to close the code panel.")]
    public Button closeButton;

    /// <summary>
    /// Dictionary mapping display names (from dropdown/tooltips) to .txt file names in Resources/ShaderCode/.
    /// </summary>
    private Dictionary<string, string> shaderToFileName = new()
    {
        { "Procedural Marble", "MarbleShader" },
        { "Water", "WavyWater" },
        { "Refraction/Transparency", "Transparency" },
        { "Dynamic Lighting", "DynamicLighting" },
        { "Iridescent", "IridescentShader" },
        { "Post Processing Effects", "PostProcessingCombined" }
    };

    /// <summary>
    /// Singleton reference to this component so other managers can access it easily.
    /// </summary>
    public static ShaderCodePreview Instance { get; private set; }

    private void Awake()
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

    private void Start()
    {
        if (codePanel != null)
            codePanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => { codePanel.SetActive(false); });
        }
    }

    /// <summary>
    /// Called when the "Show Code" button is pressed for dropdown-based shaders.
    /// Loads and displays the shader file based on the current dropdown selection.
    /// </summary>
    public void ShowCode()
    {
        string shaderName = shaderDropdown.options[shaderDropdown.value].text;

        if (shaderToFileName.TryGetValue(shaderName, out string fileName))
        {
            TextAsset shaderTextAsset = Resources.Load<TextAsset>("ShaderCode/" + fileName);
            if (shaderTextAsset != null)
            {
                codeText.text = shaderTextAsset.text;
                codePanel.SetActive(true);
            }
            else
            {
                codeText.text = "// Shader code not found for " + fileName;
                codePanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Shows the shader code preview based on a direct string name,
    /// typically from the tooltip context (not dropdown).
    /// </summary>
    /// <param name="shaderName">The display name of the shader as used in the mapping dictionary.</param>
    public void ShowCodeByShaderName(string shaderName)
    {
        if (shaderToFileName.TryGetValue(shaderName, out string fileName))
        {
            TextAsset shaderTextAsset = Resources.Load<TextAsset>("ShaderCode/" + fileName);
            if (shaderTextAsset != null)
            {
                codeText.text = shaderTextAsset.text;
                codePanel.SetActive(true);
            }
            else
            {
                codeText.text = "// Shader code not found for " + fileName;
                codePanel.SetActive(true);
            }
        }
        else
        {
            codeText.text = $"// Shader mapping not found for '{shaderName}'";
            codePanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the shader code panel unless the current context is post-processing,
    /// in which case the panel should stay open for clarity.
    /// </summary>
    public void HideCode()
    {
        if (TooltipManager.Instance != null &&
            TooltipManager.Instance.CurrentContext == TooltipContext.PostProcessing)
        {
            return; // Keep code panel open during post-processing previews
        }

        codePanel.SetActive(false);
    }
}

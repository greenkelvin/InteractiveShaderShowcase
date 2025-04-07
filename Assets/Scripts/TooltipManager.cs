using TMPro;
using UnityEngine;

/// <summary>
/// Enum representing the context of the currently selected shader or post-processing effect.
/// This context is used to display the appropriate tooltip and load the correct shader source code.
/// </summary>
public enum TooltipContext
{
    None,
    ProceduralMarble,
    Water,
    Refraction,
    DynamicLighting,
    PostProcessing,
    Iridescent
}

/// <summary>
/// TooltipManager is responsible for showing and hiding contextual tooltips
/// and linking the correct shader name to the code preview system.
/// </summary>
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance; // Singleton instance for global access

    public GameObject tooltipPanel; // The UI panel that holds the tooltip text
    public TMP_Text tooltipText; // The text component used to display the tooltip message
    public ShaderCodePreview codePreviewManager; // Reference to the code preview manager that displays shader source code

    public TooltipContext CurrentContext { get; private set; } = TooltipContext.None; // Tracks the current context for determining which code to show

    /// <summary>
    /// Ensures only one instance of TooltipManager exists.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Displays a tooltip message and sets the associated context.
    /// </summary>
    /// <param name="message">The message to display in the tooltip.</param>
    /// <param name="context">The context used to identify which shader or effect is being described.</param>
    public void ShowTooltip(string message, TooltipContext context)
    {
        CurrentContext = context;

        if (tooltipPanel.activeSelf)
        {
            tooltipText.text = message;
        }
    }

    /// <summary>
    /// Shows or hides the tooltip panel based on user input.
    /// </summary>
    /// <param name="isVisible">Whether the tooltip panel should be visible.</param>
    public void ToggleTooltipVisibility(bool isVisible)
    {
        tooltipPanel.SetActive(isVisible);
    }

    /// <summary>
    /// Hides the tooltip panel and resets the context.
    /// </summary>
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        CurrentContext = TooltipContext.None;
    }

    /// <summary>
    /// Triggered by the "Show Code" button in the tooltip.
    /// Maps the current tooltip context to the corresponding shader name
    /// and opens the code preview panel with the correct shader source.
    /// </summary>
    public void OnCodeButtonClicked()
    {
        if (codePreviewManager == null)
        {
            Debug.LogWarning("CodePreviewManager is not assigned in TooltipManager.");
            return;
        }

        string shaderKey = CurrentContext switch
        {
            TooltipContext.ProceduralMarble => "Procedural Marble",
            TooltipContext.Water => "Water",
            TooltipContext.Refraction => "Refraction/Transparency",
            TooltipContext.DynamicLighting => "Dynamic Lighting",
            TooltipContext.PostProcessing => "Post Processing Effects",
            TooltipContext.Iridescent => "Iridescent",
            _ => ""
        };

        if (!string.IsNullOrEmpty(shaderKey))
        {
            codePreviewManager.ShowCodeByShaderName(shaderKey);
        }
        else
        {
            Debug.LogWarning("Tooltip context does not match any known shader name.");
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the visibility of the side shader menu panel in the UI.
/// Provides functionality to toggle the panel on or off using a button.
/// </summary>
public class ShaderMenuManager : MonoBehaviour
{
    // Reference to the panel containing the shader menu (e.g., dropdown, sliders, etc.)
    public GameObject sideMenuPanel;

    // Reference to the button that toggles the visibility of the menu
    public Button toggleButton;

    // Tracks whether the menu is currently visible
    private bool menuVisible = true;

    /// <summary>
    /// Initializes the toggle button's onClick listener.
    /// </summary>
    void Start()
    {
        toggleButton.onClick.AddListener(ToggleMenu);
    }

    /// <summary>
    /// Toggles the visibility of the side menu panel.
    /// Called whenever the toggle button is clicked.
    /// </summary>
    public void ToggleMenu()
    {
        menuVisible = !menuVisible;
        sideMenuPanel.SetActive(menuVisible);
    }
}

# 🧪 Interactive Shader Showcase (Unity URP)

This project is a **real-time educational shader viewer** built with Unity and the Universal Render Pipeline (URP). It showcases multiple custom HLSL shaders with an interactive UI for modifying properties, exploring effects, and previewing shader source code—all in real time.

## ✨ Features

- 🖌️ **5+ Custom Shaders**:
  - **Procedural Marble** – noise-based marble patterns.
  - **Water Shader** – animated waves and fresnel reflection.
  - **Refraction/Transparency** – simulate glass and light bending.
  - **Dynamic Lighting** – Blinn-Phong and Lambert lighting.
  - **Iridescent Shader** – angle-based rainbow reflectance.
  - **Post-Processing Tint & Edge Detection** – combined full-screen effect.

- 🎛️ **Interactive UI**:
  - Real-time sliders and dropdowns for shader parameters.
  - Color picker with property selector.
  - Shader selection dropdown and post-processing toggles.
  - Tooltip system explains what each shader does.
  - Built-in code viewer to display shader source.

- 🌀 **Camera Rotation Toggle**:
  - Slowly orbits around the object when enabled.

- 🌲 **Low-Poly Nature Scene**:
  - Adds background context for shader reflection and interaction.

## 🎮 Controls & UI

- Use the **shader dropdown** to switch between shader materials.
- Adjust sliders to change numeric properties.
- Click **"Show Code"** to reveal the HLSL shader code.
- Use the **Color Picker** to edit shader color properties.
- Enable/disable **Post-Processing Effects** via toggles.
- Enable **Camera Rotation** to orbit the scene for a better view.

## 🧱 Project Structure
Assets/ 
├── Materials/ # Custom materials using each shader 
  ├── Resources/ │ 
      └── ShaderCode/ # .txt files for each shader code 
  ├── Scripts/ │ 
      ├── ShaderPropertyManager.cs │ 
      ├── PostProcessingManager.cs │ 
      ├── TooltipManager.cs │ 
      ├── ShaderCodePreview.cs 
      ├── CustomColorPicker.cs 
  ├── Shaders/ │ 
      ├── ProceduralMarble.shader │ 
      ├── RefractionShader.shader │ 
      ├── IridescentShader.shader │ 
      ├── DynamicLighting.shader │ 
      ├── SimpleWaterShader.shader 
      ├── PostProcessingCombined.shader

## 🛠️ Requirements

- **Unity 2022.3+**
- **URP (Universal Render Pipeline)** installed and configured
- Shader pipeline set up with:
  - Custom render feature (`PostProcessingFeature.cs`)
  - URP-compatible materials

## 🚀 Build Instructions

To build as a desktop app:

1. Go to `File > Build Settings`.
2. Select `PC, Mac & Linux Standalone`.
3. Add your scene to the "Scenes in Build".
4. Click **Build** and choose an output folder.
5. Share the `.exe` (Windows) or `.app` (Mac) from that folder.

To run in Unity:

- Clone/download the repo.
- Open with Unity.
- Ensure URP is enabled in **Graphics Settings**.
- Press Play.

## 🌐 Optional: WebGL Export

You can export this project to WebGL and host it on GitHub Pages or [itch.io](https://itch.io) for easy browser-based interaction.

> ⚠ Note: URP post-processing shaders may need extra adjustments for full WebGL compatibility.

---

## 📄 License

This project is for educational use and shader experimentation. Attribution appreciated but not required. All custom shaders and UI logic were developed for learning purposes.

---

## 👨‍💻 Author

Created by Kelvin Green as a visual, interactive tool to explore and explain real-time shader effects using Unity URP and HLSL.

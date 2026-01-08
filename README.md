# MAPITesting

**MAPITesting** is a testing, demonstration, and learning repository for [MAPI](https://github.com/ifBars/MAPI). It provides working examples of all major MAPI features, serves as an integration validation suite, and helps developers understand how to use MAPI effectively in their own Schedule 1 mods.

## What MAPITesting Provides

### Feature Demonstration

MAPITesting includes complete, working implementations of every major MAPI feature:

- **Procedural Mesh Generation**: Boxes, spheres, cylinders, capsules, and custom mesh combinations
- **Building Construction**: Complete structures with walls, floors, ceilings, windows, and roofs
- **GLTF Model Loading**: Embedded resource loading and custom import settings
- **Prefab Placement**: Networked and non-networked instantiation patterns
- **Material Management**: Presets for opaque, transparent, glass, metallic, and emissive materials

### Pattern Reference

MAPITesting establishes best practices for common use cases:

- Fluent builder patterns for mesh and building construction
- Material creation and application workflows
- Networked prefab placement strategies
- Embedded resource loading for GLTF models
- Building configuration management with palettes

## License and Usage Rights

MAPITesting is distributed under the **Preview Learning-Only License (PLOL)**. This license distinguishes between open-source MAPI code and the specific examples in this repository.

### What You May Do

**Copy Project Structure:**
- Use the `.csproj` and `.sln` files as templates for your own mods
- Adopt the build configuration and MelonLoader integration patterns
- Reference the project structure as a starting point

**Use MAPI Open-Source Code:**
- MAPI's `BuildingConfig` presets (`BuildingConfig.Large`, `BuildingConfig.Medium`, etc.) are MIT-licensed and free to use
- `BuildingPalette` configurations are open source
- All MAPI patterns and code snippets are freely usable

**Learn and Study:**
- View, study, and discuss the code for educational purposes
- Use general ideas, techniques, and patterns in your own projects
- Copy small code snippets that are not defining parts of examples

**Create Transformative Works:**
- Publish your own projects that use MAPI's open-source components
- Build original constructions using BuildingConfig presets
- Develop unique examples that are not substantially similar to protected examples

### What You May NOT Do

**Protected Content (Not Allowed):**
- Copy the dispensary building example (`GreenLabDispensary.cs`) in whole or substantial part
- Replicate specific decorations, signage, or interior layouts
- Publish reskinned versions of protected examples
- Redistribute the Work or substantial portions of it

**Substantially Similar Works:**
- If a reasonable person would recognize your work as originating from this repository, it is prohibited
- Cosmetic changes (materials, colors, prop swaps) do not make a work transformative
- Transformation requires substantial creative changes beyond surface modifications

**Misrepresentation:**
- Do not claim authorship of the Work or substantially similar versions
- Do not represent protected examples as your original creation

See the [LICENSE](LICENSE) file for the complete PLOL text.

## Installation

### For Developers (Building from Source)

Clone the repository and build for your target runtime:

```bash
git clone https://github.com/ifBars/MAPITesting.git
cd MAPITesting

# Development testing (Mono)
dotnet build MAPITesting.csproj -c Mono

# Production validation (IL2CPP)
dotnet build MAPITesting.csproj -c Il2cpp
```

Copy the built DLL from `bin/<Config>/MAPITesting.dll` to your game's `Mods` folder.

Note: Do not use CrossCompat/Universal build configs

## Relationship to Other Projects

**MAPI** - Core mesh and building library. Create procedural 3D content without game dependencies.

**MAPITesting** - This repository. Demonstrates MAPI features and validates functionality.

**S1API** - Game component wrappers. Access game entities, NPC behaviors, and quest systems. Use alongside MAPI for complete mod solutions.

## Support

- **Issues**: Report bugs and feature requests via [GitHub Issues](https://github.com/ifBars/MAPI/issues)
- **Discord**: Contact ifBars directly for license inquiries or other concerns
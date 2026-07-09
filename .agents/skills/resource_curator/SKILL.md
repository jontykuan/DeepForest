---
name: resource-curator
description: Use the resource curator tool to sync and update card/event/ending resources in the DeepForest codebase.
---
# DeepForest Resource Curator Skill

Use this skill when you need to inspect, modify, export, or import `.tres` resource files (cards and events) or generate the resource catalog for the DeepForest game.

## Commands

Always run these commands from the project root directory:

1. **Export all `.tres` resources to JSON**:
   `python tools/resource_curator.py export`
   Outputs to `ResourceCuration.json`.

2. **Import JSON changes back to `.tres` files**:
   `python tools/resource_curator.py import`
   Reads from `ResourceCuration.json` and updates respective `.tres` files.

3. **Generate Markdown catalog**:
   `python tools/resource_curator.py catalog`
   Updates the human-readable [ResourceCatalog.md](file:///d:/cykuan/Godot_v4.6.2-stable_mono_win64/godot%20project/DeepForest/ResourceCatalog.md).

## Usage Guidelines

- Do not manually edit the `.tres` files if bulk editing. Export them first, modify `ResourceCuration.json`, and then import.
- Running `export` will overwrite `ResourceCuration.json`. If the user has made manual changes to `ResourceCuration.json`, read it first or parse it before running `export`.

#!/usr/bin/env python

import os
import shutil
import subprocess
import argparse
import tempfile

# BepInEx https://github.com/BepInEx/BepInEx
# MonoMod https://github.com/MonoMod/MonoMod
# AssemblyPublicizer https://github.com/WeylandMod/AssemblyPublicizer
# Unity https://unity3d.com/unity/qa/lts-releases

parser = argparse.ArgumentParser(description="MonoMod MMHOOK generator")
parser.add_argument("-m", "--monomod", type=str, required=True,
                    help="Path to MonoMod directory")
parser.add_argument("-b", "--bepinex", type=str, required=True,
                    help="Path to BepInEx directory")
parser.add_argument("-p", "--publicizer", type=str, required=True,
                    help="AssemblyPublicizer executable")
parser.add_argument("-u", "--unity", type=str, required=True,
                    help="Path to Unity installation directory")
parser.add_argument("-v", "--valheim", type=str, required=True,
                    help="Path to Valheim Managed directory")
options = parser.parse_args()

valheim_files = [
    "assembly_valheim.dll",
    "assembly_utils.dll",
]

unity_files = [
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
]

bepinex_files = [
    "BepInEx.dll",
]

hookgen_files = [
    "MonoMod.RuntimeDetour.dll",
    "MonoMod.Utils.dll",
]

script_dir = os.path.abspath(os.path.dirname(__file__))
dependencies_dir = os.path.join(script_dir, "Dependencies")

# Copy Unity dependencies
unity_dependencies_path = os.path.join(dependencies_dir, "Unity")
unity_path = os.path.join(
    options.unity,
    "Editor", "Data", "Managed", "UnityEngine"
)

for filename in unity_files:
    shutil.copyfile(
        os.path.join(unity_path, filename),
        os.path.join(unity_dependencies_path, filename)
    )

# Copy BepInEx dependencies
bepinex_dependencies_path = os.path.join(dependencies_dir, "BepInEx")
bepinex_path = os.path.join(
    options.bepinex,
    "BepInEx", "core"
)

for filename in bepinex_files:
    shutil.copyfile(
        os.path.join(bepinex_path, filename),
        os.path.join(bepinex_dependencies_path, filename)
    )

# Generate Hooks
try:
    temp_dir = tempfile.TemporaryDirectory()

    os.chdir(temp_dir.name)

    for filename in os.listdir(options.valheim):
        shutil.copyfile(
            os.path.join(options.valheim, filename),
            os.path.join(temp_dir.name, filename)
        )

    for filename in hookgen_files:
        shutil.copyfile(
            os.path.join(options.monomod, filename),
            os.path.join(temp_dir.name, filename)
        )

    publicized_path = os.path.join(temp_dir.name, "publicized")
    valheim_dependencies_path = os.path.join(dependencies_dir, "Valheim")
    hooks_dependencies_path = os.path.join(dependencies_dir, "Hooks")

    for filename in valheim_files:
        subprocess.call([
            options.publicizer,
            "-e",
            "-i", os.path.join(temp_dir.name, filename),
            "-o", os.path.join(publicized_path, filename),
        ])

        shutil.copyfile(
            os.path.join(publicized_path, filename),
            os.path.join(valheim_dependencies_path, filename)
        )

        subprocess.call([
            os.path.join(options.monomod, "MonoMod.RuntimeDetour.HookGen.exe"),
            os.path.join(publicized_path, filename),
        ])

        shutil.copyfile(
            os.path.join(publicized_path, "MMHOOK_" + filename),
            os.path.join(hooks_dependencies_path, "MMHOOK_" + filename)
        )

finally:
    os.chdir(script_dir)
    temp_dir.cleanup()

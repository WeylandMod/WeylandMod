![](https://i.imgur.com/NlUKpYA.png)

---

WeylandMod is a [Valheim](https://www.valheimgame.com/) QoL mods collection, like shared map exploration, shared pins, item radar and extended storage.

WeylandMod uses [MonoMod](https://github.com/MonoMod/MonoMod) framwework, and relies on [BepInEx](https://github.com/BepInEx/BepInEx) for mod loading.

## Issues

Report any WeylandMod issues [on GitHub](https://github.com/WeylandMod/WeylandMod/issues).

## Sections

* [Features](#features)
* [Roadmap](#roadmap)
* [Installation](#installation)
* [Known Issues](#known-issues)
* [Building](#building)

## Features

All features are divided into different assemblies `WeylandMod.[FeatureName].dll` and configurable through corresponding config files `WeylandMod.[FeatureName].cfg` inside BepInEx `config` directory. Only `WeylandMod.Core` is mandatory, any other feature can be removed completely.

Different features demands installation on server, client or both in order to work properly, you can check requirements in the table below. The mod syncs client configuration with server configuration on connect, server side configuration has priority over client-server features, but client-only features are unaffected.

By default WeylandMod changes game version string to deny connections from clients without WeylandMod installed, you can disable this in `WeylandMod.Core.cfg` configuration file.

Feature | Description | Server | Client
:------ | :------ | :----: | :----:
[Shared Map](#shared-map) | Shared map exploration and custom pins between all players on server. | ✓ | ✓
[Extended Storage](#extended-storage) | Extends size of every game container. | ✓ | ✓
[Extended Server Password](#extended-server-password) | Launch server with no password and permitted players can join server without password. | ✓ |
[Favorite Servers](#favorite-servers) | Adds Favorite Servers tab to game. | | ✓
[Item Radar](#item-radar) | Display icons for specific items on minimap. | | ✓
[Extended Death Pins](#extended-death-pins) | All deaths are marked and player can remove death pins. | | ✓
[Precise Rotation](#precise-rotation) | Rotate objects by custom angle while building. | | ✓

### Shared Map

Shares map exploration among all players on server, new or returned players will be synced on connection. Stores explored map on server. You can use this mod retroactively, explored map will be updated once a player is connected to the server.

Implemented features:

* Server-side explored map shared bewtween all players.
* Server-side shared players pins.
* Forced players public positions.
* Keep explored map and shared pins on server between restarts.

### Extended Storage

Adds slots to every available container in game (cheats, boats, wagon).

### Extended Server Password

Implemented features:

* Let you start public server without password.
* Allow to log in to server without password if user is listed in permittedlist.txt.
* Allow to remove Steam password request on connection through `Steam > View > Servers > Favourite` (note if you enable this option your server will be listed as server without password on Community Servers tab in the game).

### Favorite Servers

Adds favorite servers support over Steam API as a separate tab in start game menu.

Implemented features:

* List favorite servers.
* Add and remove favorite servers.
* Add selected server from Join tab to favorites. 

### Item Radar

Allow you to enable some kind of "radar" to display icons for configured items on minimap in specific radius. You can look into [ObjectDB-Table](https://github.com/Valheim-Modding/Wiki/wiki/ObjectDB-Table) (column `Prefab Name`) for specific item names.

### Extended Death Pins

All you death positions (not only last one) are now presented on map and you can remove any death marker by right-clicking on it.

### Precise Rotation

Let you configure the arbitrary rotation angles for placeable objects in build mode. Precision rotation mode activated by holding down configurable key (LCtrl by default). Server can force clients to disable this feature or configure custom rotation angles.

## Roadmap

* Current features improvements, especially Shared Map.
* User-friendly installer for Windows platform.
* Private (hidden) servers.

## Installation

**You must install mod on server and all clients for proper work!** There is no guarantee that client without mod will be able to play on modded server and vice verse.

Tou can use [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/) mod manager to install [WeylandMod](https://valheim.thunderstore.io/package/WeylandMod/WeylandMod/) with all needed dependencies.

Alternatively you can download appropriate archive with pre-built binaries from [releases section](https://github.com/WeylandMod/WeylandMod/releases), unpack it into Valheim installation directory and you ready to go.

## Known Issues

Linux:

* You need to install libc6-dev for BepInEx to work.
* BepInEx versions 5.4.6.0-5.4.8.0 use UnityDoorstop.Unix 1.5.0.0 which fails to start due to a bug, you should export `DOORSTOP_CORLIB_OVERRIDE_PATH` environment variable pointing to `Managed` directory of Valheim in order to fix this.

## Building

Download and unpack/install these dependencies:

* [BepInEx](https://github.com/BepInEx/BepInEx)
* [MonoMod](https://github.com/MonoMod/MonoMod)
* [AssemblyPublicizer](https://github.com/WeylandMod/AssemblyPublicizer)
* [Unity 2019.4.20f1](https://unity3d.com/unity/qa/lts-releases)
* [Valheim](https://www.valheimgame.com/)

Alternative to Valheim paid copy you can use [Valheim Dedicated Server](https://steamdb.info/app/896660/) installed using [SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD) anonymous login. We'll not cover this process here.

Once you've done this use Python script `prepare_build.py` (see `--help`), it will prepare working copy for build process.

After this you can use either .NET CLI or your favorite IDE to build mod binaries.

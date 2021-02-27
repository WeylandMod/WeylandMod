![](https://i.imgur.com/NlUKpYA.png)

---

WeylandMod is a [Valheim](https://www.valheimgame.com/) mod named after legendary blacksmith [Weyland](https://en.wikipedia.org/wiki/Wayland_the_Smith) from Germanic mythology.

WeylandMod uses [MonoMod](https://github.com/MonoMod/MonoMod) framwework, and relies on [BiPinEx](https://github.com/BepInEx/BepInEx) for mod loading.

## Issues

Report any WeylandMod issues [on mod GitHub](https://github.com/WeylandMod/WeylandMod/issues).

## Sections

* [Features](#features)
* [Roadmap](#roadmap)
* [Installation](#installation)
* [Building](#building)

## Features

All features is configurable through config file `io.github.WeylandMod.cfg` inside BepInEx `config` directory.

* [Shared Map](#shared-map)
* [Extended Storage](#extended-storage)
* [No Server Password](#no-server-password)

### Shared Map

Shares map exploration among all players on server, new or returned players will be synced on connection. Stores explored map on server. You can use this mod retroactively, explored map will be updated once a player is connected to the server.

Implemented features:

* Server-side explored map shared bewtween all players.
* Forced players public positions.

Planned features:

* Share bosses positions and custom pins.
* Keep explored map on server between restarts.

### Extended Storage

Adds slots to every available container in game (cheats, boats, wagon).

### No Server Password

Implemented features:

* Let you start public server without password.
* Allow to log in to server without password if user listed in permittedlist.txt on server.
* Allow to remove Steam password request on connection through `Steam > View > Servers > Favourite`.
  * Note: if you enable this option your server will be listed as server without password (no key icon) on Community Servers tab in the game.

## Roadmap

* Current features improvements, especially Shared Map.
* Mod version check on connection.
* User-friendly installer for Windows platform.
* Keep favorite server list and resolve domain names on connect.
* Private (hidden) servers.

## Installation

**You must install mod on server and all clients for proper work!** There is no guarantee that client without mod will be able to play on modded server and vice verse.

Tou can use [r2modman](https://valheim.thunderstore.io/package/ebkr/r2modman/) mod manager to install [WeylandMod](https://valheim.thunderstore.io/package/WeylandMod/WeylandMod/) with all needed dependencies.

Alternatively you can download appropriate archive with pre-built binaries from [releases section](https://github.com/WeylandMod/WeylandMod/releases), unpack it into Valheim installation directory and you ready to go.

## Building

Download and unpack/install these dependencies:

* [BepInEx](https://github.com/BepInEx/BepInEx)
* [MonoMod](https://github.com/MonoMod/MonoMod)
* [AssemblyPublicizer](https://github.com/WeylandMod/AssemblyPublicizer)
* [Unity 2019.4.20f1](https://unity3d.com/unity/qa/lts-releases)
* [Valheim](https://www.valheimgame.com/)

Alternative to Valheim paid copy you can use [Valheim Dedicated Server](https://steamdb.info/app/896660/) installed using [SteamCMD](https://developer.valvesoftware.com/wiki/SteamCMD) anonymous login. We'll not cover this process here.

Once you've done this use Python script `prepare_build.py` (see `--help`), it will prepare working copy for build process.

After this you can use either .NET CLI or Microsoft Visual Studio 2019 to build mod binaries.

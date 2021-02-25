# WeylandMod

WeylandMod is a Valheim mod named after legendary blacksmith Weyland from Germanic mythology.

WeylandMod uses [MonoMod](https://github.com/MonoMod/MonoMod) framwework, and relies on [BiPinEx](https://github.com/BepInEx/BepInEx) with [BepInEx.MonoMod.Loader](https://github.com/BepInEx/BepInEx.MonoMod.Loader) for mod loading.

## Sections

* [Features](#features)
* [Roadmap](#roadmap)
* [Installation](#installation)
* [Building](#building)

## Features

All features is configurable through config file `WeylandMod.cfg` inside BepInEx `config` directory.

* [Shared Map](#sharedmap)
* [Extended Storage](#extendedstorage)
* [No Server Password](#noserverpassword)

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

Let you start public server without password. Can be configured to skip password check only for whitelisted players.

## Roadmap

* Current features improvements, especially Shared Map.
* Mod version check on connection.
* User-friendly installer for Windows platform.
* [thunderstore.io](https://thunderstore.io/) integration.
* Keep favorite server list and resolve domain names on connect.

## Installation

**You must install mod on server and all clients for proper work!** There is no guarantee that client without mod will be able to play on modded server and vice verse.

Download appropriate archive with pre-built binaries from [releases section](https://github.com/WeylandMod/WeylandMod/releases), unpack it into Valheim installation directory and you ready to go.

## Building

The build process is pretty straightforward, you need to place appropriate assembly files into `Dependencies` directory (check `REAMDE.md` inside). After this you can use either `dotned` CLI or Microsoft Visual Studio 2019 to build mod binaries.

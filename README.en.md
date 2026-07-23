[English](README.en.md) | [日本語](README.md)

# ce-keymap-for-remotedesk

This app runs on Windows 11 and adds keyboard shortcut functionality.
It can substitute for shortcut features that don't work when connecting remotely from Android via Parsec, for example.
It contains no unnecessary processing, so it runs extremely lightweight.

## Added shortcut features

Keys can be changed. Each shortcut can also be individually turned ON/OFF.

- App window switch (`Right Alt + Right Shift + A`)
- Zoom change - Desktop (`Right Alt + Right Shift + D`)
- Zoom change - Mobile (`Right Alt + Right Shift + M`)
- Press Win key (`Right Alt + Right Shift + W`)

## Launching & usage

Click CeKeymap.exe to launch it.
It stays resident in the system tray, so you can right-click to change shortcut keys and toggle
each feature ON/OFF individually.

## Settings list

The following changes can be made from the tray menu. These settings are saved as a
settings.json file in the same folder as the app (exe). You can share settings across other PCs
by copying this file.

- Start on Windows login ... registers/unregisters this app in the registry Startup entry
- Feature settings
  - App window switch ... feature ON/OFF and keybinding change are available
  - Zoom change - Desktop ... feature ON/OFF, keybinding change, and zoom percent are available
  - Zoom change - Mobile ... feature ON/OFF, keybinding change, and zoom percent are available
  - Press Win key ... feature ON/OFF and keybinding change are available
- Developer
  - Open app log ... opens log.txt (in the same folder as the app/exe) with the system default app

## About the Windows SmartScreen warning

This app is not code-signed, so the first time you run the downloaded CeKeymap.exe, Windows
SmartScreen may show a "Windows protected your PC" warning. This is a generic warning shown for
any executable from an unverified publisher, not an issue specific to this app.

To run it anyway:

1. Click "More info" in the warning dialog
2. Click the "Run anyway" button that appears

Once you've done this, the same warning won't appear again for this file.

## Internal architecture spec

For developer-facing internal implementation details, see the [architecture spec](docs/spec_arch.md) (Japanese only).

# DayTrack Privacy

DayTrack is designed as a local-first Windows activity tracker.

## Data location

The primary database is stored locally at:

```text
%LOCALAPPDATA%\DayTrack\data.db
```

DayTrack does not require an account and does not use a DayTrack cloud backend.

## Keyboard input

DayTrack uses a Windows low-level keyboard hook only to increment an aggregate counter.

The current implementation does **not** extract or store:

- key codes;
- characters;
- typed text;
- passwords.

## Mouse input

DayTrack uses a Windows low-level mouse hook only to count click events.

It does **not** store mouse coordinates.

## Clipboard and screen

DayTrack does not record clipboard contents and does not take screenshots.

## Application activity

DayTrack stores application names and aggregate active time.

The current release does not persist window titles in the activity database.

## Network activity

DayTrack reads aggregate byte counters from active Windows network interfaces.

It does **not** inspect:

- packet contents;
- URLs;
- messages;
- browser history;
- per-application network traffic.

## Exports

TXT, CSV, and JSON exports are created locally.

Daily reports can be written to a location selected by the user.

## Uninstall

Removing the DayTrack application does not automatically delete the user's local statistics. This prevents accidental data loss.

Local DayTrack data can be removed manually by deleting:

```text
%LOCALAPPDATA%\DayTrack
```

and any user-selected DayTrack report folder.

## Source code

The source code is available in this repository so the tracking behavior can be inspected directly.

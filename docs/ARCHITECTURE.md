# DayTrack Architecture

High-level data flow:

```text
Windows APIs
    ↓
CollectorService
    ↓
SQLite
    ↓
Widget / Dashboard / TXT / CSV / JSON
```

## Single-process design

DayTrack runs as one Windows process with a notification-area icon.

Closing the desktop widget hides the window while the collector can continue running in the background. This keeps the application simple and avoids requiring a separate background-service executable.

## Collection intervals

The current implementation approximately uses:

- foreground application / AFK state: 1 second;
- network counters: 5 seconds;
- application launch sampling: 2 seconds;
- SQLite flush: 10 seconds;
- daily TXT refresh: 60 seconds.

## Storage

The primary source of truth is:

```text
%LOCALAPPDATA%\DayTrack\data.db
```

Readable TXT files are generated as daily exports.

## Privacy model

Global keyboard and mouse hooks increment aggregate counters only.

The input-counter implementation does not extract keyboard key codes, typed characters, clipboard content, or mouse coordinates.

Network tracking uses Windows interface byte counters and does not inspect packet contents.

# Changelog

All notable public changes to DayTrack are documented here.

## [1.0.0] - 2026-08-18

### Added

- Standalone Windows activity tracking without an ActivityWatch dependency
- Active / AFK tracking
- Per-application active time
- PC uptime statistics
- Lock/unlock and sleep/resume handling
- Aggregate keyboard press counting
- Aggregate mouse click counting
- Aggregate network traffic totals
- Basic application launch sampling
- SQLite local storage
- Daily TXT reports
- CSV and JSON exports
- Desktop widget
- Dashboard with multiple time ranges
- Tray controls and pause/resume
- Windows autostart
- Dark / Light / System themes
- English, Russian, Ukrainian, Japanese, and Simplified Chinese localization
- User-selectable daily report directory
- Start Menu shortcut support
- Optional desktop shortcut
- Self-contained Windows x64 Portable build
- Inno Setup Windows installer
- Standard Windows uninstall support
- Local settings/database safeguards for first-run and shutdown flows

### Notes

- Application launch counts use periodic process sampling.
- Network counters are aggregate network-interface totals.
- Public binaries are currently unsigned.

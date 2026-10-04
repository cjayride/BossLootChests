# Changelog

## 1.0.2

- Fix: boss chests vanishing instantly on spawn (watcher no longer runs empty-check before loot is added).

## 1.0.1

- Fix: boss chests empty after logout/login now smoke-delete again (`BossChestWatcher` re-hooks inventory on load).

## 1.0.0

- Boss loot goes straight into world chests at the death spot. Nothing is spawned as a ground drop, so auto-pickup cannot grab it.
- Extra chests spawn if one chest is not enough.
- Chests are indestructible (hammer and damage do nothing).
- A chest deletes itself when the last item is taken, with a smoke puff (`RemoveSmoke`, on by default).
- EpicLoot magic-item rolls are stored in the same chests when EpicLoot is installed.
- Install on the server and all clients.

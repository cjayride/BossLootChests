# Changelog

## 1.0.3

- Fix remove smoke VFX for all players (uses chest destroy effect from configured prefab).

## 1.0.2

- Fix: chests appear again on boss kill (1.0.1 could delete the chest before loot was added).

## 1.0.1

- Fix: chests still smoke-delete when emptied after you log out and back in.

## 1.0.0

- Boss loot goes straight into world chests at the death spot. Nothing is spawned as a ground drop, so auto-pickup cannot grab it.
- Extra chests spawn if one chest is not enough.
- Chests are indestructible (hammer and damage do nothing).
- A chest deletes itself when the last item is taken, with a smoke puff (`RemoveSmoke`, on by default).
- EpicLoot magic-item rolls are stored in the same chests when EpicLoot is installed.
- Install on the server and all clients.

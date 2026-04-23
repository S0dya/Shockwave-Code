# Shockwave Code (Gameplay Extract)

Gameplay-focused C# code from my Unity project.  
This repository intentionally contains **only the gameplay domain** (no full Unity template, no complete project setup, no plugin packages).

## Quick Navigation

- [What It Does](#what-it-does)
- [Main Highlights](#main-highlights)
- [Code Architecture at a Glance](#code-architecture-at-a-glance)
- [Tech Stack and Patterns](#tech-stack-and-patterns)
- [How to Read This Repo Fast](#how-to-read-this-repo-fast)

## What It Does

At a high level, the game loop is:

1. Player performs an action on a board cell.
2. The action triggers a shockwave push/merge phase.
3. Merges chain into additional effects and turn progression.
4. Systems like skills, mega-merge, undo, and scoring react through signals.
5. Next turn starts, or game-over flow is triggered if no valid moves remain.

## Main Highlights

- **Shockwave merge engine**  
  Directional push/merge logic with queued follow-up merges and async turn processing.
- **Multiple action states**  
  Tap / Swap / Destroy actions handled via dedicated state classes (`ActionState` pattern).
- **Mega Merge mechanic**  
  Charge-based special move that can trigger directional board-wide merge waves.
- **Undo system with economy hooks**  
  Turn undo integrated with consumable counts and ad reward fallback.
- **Signal-driven orchestration**  
  Systems communicate through Zenject's `SignalBus`, reducing hard coupling between gameplay modules.

## Code Architecture at a Glance

```text
Scripts/
  Gameplay/
    GameplayController.cs                  -> Top-level gameplay flow
    Shockwave2048/
      Board/                               -> Grid state, turns, merges, actions
      MegaMerge/                           -> Special charged merge mechanic
      Skills/                              -> Skill activation/buy/use flow
      Undo/                                -> Undo model + UI/controller
      Elements/, Grid/, Slot/              -> Core board entities and data
      Installers/                          -> Zenject bindings/composition root
    Analytics/                             -> Gameplay analytics listeners/types
  PT/
    Backend/                               -> Interfaces + service adapters (Firebase/Yandex/Fake)
```

## Tech Stack and Patterns

- **Language/Engine:** C# / Unity
- **DI:** Zenject
- **Reactive/Event flow:** SignalBus, UniRx
- **Async flow:** UniTask
- **Architecture style:** modular gameplay services + state-driven controllers

## How to Read This Repo Fast

If you have only a few minutes, start here:

1. `Scripts/Gameplay/GameplayController.cs` (global turn/game-over orchestration)
2. `Scripts/Gameplay/Shockwave2048/Board/BoardActionController.cs` (player action state machine)
3. `Scripts/Gameplay/Shockwave2048/Board/BoardShockwaveController.cs` (core push/merge algorithm)
4. `Scripts/Gameplay/Shockwave2048/MegaMerge/MegaMergeController.cs` (special mechanic)
5. `Scripts/Gameplay/Shockwave2048/Skills/SkillsManager.cs` (economy + skill interactions)

---
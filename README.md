# AutoHackGame
**A collection of modding tools for Legion TD 2** \
![AutoHackGame](https://raw.githubusercontent.com/LegionTD2-Modding/AutoHackGame/main/icon.png)

### Description
This library is a collection of tools that enable non-destructive and easy modding experience for Legion TD 2. Included:
- A patching tool allowing for easy UI modifications
- A bridge to the game HudApi, enabling the sending of updates from the game engine to the UI
- (WIP) A bridge to the game engine, enabling for the UI to send updates to the game engine

### Quick tutorial
#### UI patching
- Create a zip file containing .patch files (`mkdir -p mods && git diff file.html > mods/file.html.patch` then `(cd mods && zip -r ../Patches.zip ./*)`)
- Each patch file will be applied to the corresponding file inside the game folder `Legion TD 2/Legion TD 2_Data/uiresources/AeonGT`. For ex:
    - If you put `gateway.html.patch` at the root of the patch zip file, then the patch will be applied to `Legion TD 2/Legion TD 2_Data/uiresources/AeonGT/gateway.html`
    - If you put `hud/js/bindings.js.patch` inside the patch zip file, then the patch will be applied to `Legion TD 2/Legion TD 2_Data/uiresources/AeonGT/hud/js/bindings.js`
- A special auto-patch is always applied to `gateway.html` to make sure all the libraries patched are called at their '__' name: so if you patched`hud/js/global-state.js`, then the patched `__gateway.html` will have all its occurences of `hud/js/global-state.js` replaced by `hud/js/__global-state.js`. Even if no patch happened, `__gateway.html` will always be created at game start, because the engine automatically forces the game to load `__gateway.html` no matter what
- The patches are automatically undone when the game is closed

#### Game -> UI communication
- From the game, use `HudApi.TriggerHudEvent(string eventName, [...])` to send a message to the UI. You can add parameters, but only those combinations (for example, `event_name` and `1`, `2`):
```csharp
void TriggerHudEvent(string eventName)
void TriggerHudEvent(string eventName, string arg)
void TriggerHudEvent(string eventName, string arg1, string arg2)
void TriggerHudEvent(string eventName, bool arg)
void TriggerHudEvent(string eventName, int arg1)
void TriggerHudEvent(string eventName, float arg1)
void TriggerHudEvent(string eventName, float arg1, float arg2)
void TriggerHudEvent(string eventName, int arg1, int arg2)
```
- From the UI, patch `hud/js/global-state.js` and add the values you want to recover inside the `var globalState` (for example `event_state`). Then patch `hud/js/bindings.js` and add your listener in the `var bindings` (for example `event_listener`). Then add your listener at the end of the file:
```csharp
// 'event_name' is the `string eventName`
// args are the arguments passed with it
engine.on('event_name', function (arg1, arg2) {
    // Update 'globalState' if you want
    globalState.event_state = arg1

    // Callbacks in 'bindings' if you want
    if (bindings.event_listener != null) {
        bindings.event_listener(arg2)
    }
});
```

#### UI -> Game communication
WIP
# RoundEndSound

[![Github All Releases](https://img.shields.io/github/downloads/webpashtet/RoundEndSound/total.svg)]()

**Support the author: [DonationAlerts](https://www.donationalerts.com/r/gleb_khlebov)**

**CounterStrikeSharp plugin to play cool musics when a round ends.**

## Features
- Turn on/off music playback
- Enable/disable notifications at the end of the round
- Two sounds selection modes: **in order** and **random** (_"random_selection_mode"_ in config to _true_ for **random** or _false_ for **in order**)
- Listen to the last track
- Multi language (default English and Russian but you can add new)

## Commands
- `!res / css_res` | Main plugin menu

## How add music to server
In few:
1. Create addon with your songs and post him in workshop
   **[Example SoundEvents addon file](https://github.com/webpashtet/RoundEndSound/blob/main/workshop/soundevents_addon.vsndevts)**
2. Add the addon to the server using [**MultiAddonManager**](https://github.com/Source2ZE/MultiAddonManager)
3. Configure the **Round End Sound** config
```json
"music_list": [
  {
    "name": "Test track 1",
    "path": "sounds/music/res_1.vsnd_c"
  },
  {
    "name": "Test track 2",
    "path": "sounds/music/res_2.vsnd_c"
  }
]
```

## English video guide (updated)
[![VIDEO GUIDE_ENG](https://img.youtube.com/vi/7JBq2zmlWU0/0.jpg)](https://www.youtube.com/watch?v=7JBq2zmlWU0)

## To do...
- [x] Rework random sound selection logic
- [x] Add "in order" sound selection mode
- [ ] Refactoring repository and rework SQL queries constants
- [ ] Add menu point to view all songs
- [ ] Аdd a scheduler and command to clear old players
- [ ] Create instructions for assembling your workshop addon with music




## Sound files verification checklist
Use this quick process when HUD/chat says a song is playing but no audio is heard:

1. **Copy the exact path from server logs**
   - Example from logs: `play sounds/my_sound_pack/gdzie.vsnd_c`

2. **Check that this exact path exists inside your workshop addon/VPK**
   - Path must match exactly (folder names and letter case).
   - Linux servers are case-sensitive.

3. **Verify config paths**
   - Every `music_list[].path` must point to a `.vsnd_c` file that exists in the mounted addon.

4. **Verify addon is mounted on the server**
   - Ensure MultiAddonManager mounted your workshop addon and search path points to your addon VPK.

5. **Verify client can receive/use the resource**
   - Join with a real player (not only bots) and test after one full round end.

6. **Cross-check plugin diagnostics**
   - If you see `PlaySound: executing 'play ...'` but still no sound, issue is usually missing/wrong resource path in addon.


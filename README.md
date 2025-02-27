# VS-AvoidBlock Mod
<a name="my-custom-anchor-point"></a>
A Vintage Story Mod to guide NPCs away from certain blocks


### Overview
VS-AvoidBlock is a mod for Vintage Story that allows modders and server administrators to define blocks that NPCs should avoid.

### Features
🚧 Block Avoidance: Prevent NPCs from walking into specific block.
🧠 Smart Pathfinding: Entities dynamically adjust their paths to avoid obstacles.
🔄 Performance Optimized: Lightweight and optimized to avoid unnecessary path recalculations.

### How to use

example I was using trader and be sure that priority is highest

[trader-buildmaterials.json](https://github.com/user-attachments/files/18512996/trader-buildmaterials.json)

```json
{
	code: "taskai",
	aitasks: [
		{
			"code": "avoidblock",
			"priority": 3.0
		}
	]
},
```

Alternatively, use the configuration file which should be generated `VintagestoryData\Config` [EntityList.json](https://github.com/userattachments/files/18526469/EntityList.json)

I used creativebarrier block to stop entities passing through block it self.
[creativebarrier.json](https://github.com/user-attachments/files/18526468/creativebarrier.json)

### Compatibility
Vintage Story Version: 1.20+
Compatible with both singleplayer and multiplayer servers.
Works with most AI-related mods without conflicts.
Known Issues

### Future Plans

### Support & Feedback
Have issues or suggestions? Contact us via:

### License
This mod is licensed under the MIT License, meaning you are free to use, modify, and distribute it with attribution.





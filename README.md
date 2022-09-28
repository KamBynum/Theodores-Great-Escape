# VGDDojoDynasty

## File Contributions
### Kam Bynum
### Scripts/Animations
* Assets/Scripts/SceneTransition/*
* Assets/Scripts/UI/* (minus health and stuffing bar special effects in HoneyBar.cs)
* Assets/Scripts/Utility/LevelData.cs
* Assets/Scripts/Utility/GameManager.cs
* Assets/Scripts/Utility/PlayerData.cs
* Assets/Scripts/Utility/ResourceManager.cs
* Assets/Scripts/Utility/SaveManager.cs
* Assets/Scripts/Utility/TimeManager.cs
* Assets/Scripts/Utility/TutorialManager.cs
* Assets/Scripts/Utility/Events/AllHoneyOnLevelEvent
* Assets/Scripts/Utility/Events/ClickEvent
* Assets/Scripts/Utility/Events/GameOverEvent
* Assets/Scripts/Utility/Events/MusicStartEvent
* Assets/Scripts/Utility/Events/VictoryEvent
* Assets/Animations/SceneTransition/*
* Assets/Pickups/Honey/Events/*
* Assets/Pickups/Honey/HoneyPickup.cs (partial - struct handling)
* Assets/Players/PlayerV3.2/Animator Controller/RootMotionAnimator3_2.controller (partial - layer separation to masked animations)
* Assets/Enemies/Spider/Scripts/*
* Assets/Enemies/Spider/Web Shot/Scripts/*
* Assets/Misc/Scripts/Step/

----------------------
### Prefabs
* Assets/LevelBuilding/Import/Door Pack/Prefab/DoorV1.prefab
* Assets/Prefabs/Button With Image.prefab
* Assets/Prefabs/Button.prefab
* Assets/Prefabs/Game Manager.prefab
* Assets/Prefabs/HUD.prefab
* Assets/Prefabs/Level Stats.prefab
* Assets/Prefabs/LevelData.prefab
* Assets/Prefabs/LevelLoader.prefab
* Assets/Prefabs/MainMenu.prefab
* Assets/Misc/StuffingBall/DustCloudEffect.prefab
* Assets/Misc/Scripts/Step/StepDustEffect.prefab

----------------------
### Imports
* Assets/LevelBuilding/Import/Cobwebs
* Assets/LevelBuilding/Import/Door Pack
* Assets/LevelBuilding/Import/SuperCyan Free Forest Sample
* Assets/Sprites/*
* Assets/Sounds/GameState/*
* Assets/Sounds/Music/*
* Assets/Pickups/Honey/Sounds/*
* Assets/Misc/Fonts/*
* Assets/Misc/StuffingBall/Source/*
* Assets/Misc/StuffingBall/Sprites/*
* Assets/Enemies/Spider/Sounds/*

----------------------
### Scenes
* Assets/Scenes/MainMenu
* Assets/Scenes/LevelSelect (Alpha)
* Assets/Scenes/Canyon/Canyon_v0 (Puzzle 7 Spider cave only)
* Assets/LevelBuilding/Buildings/SpiderCave

----------------------
### Rayshawn Eatmon
Main area of effort was Flying AI Enemies - Bees and Birds

#### Assets\Enemies\FlyingEnemy\Animations
* Eagle_Fly_Feet.anim (adjustments/blend)
* Eagle_Swoop_Attack.anim (blend)

#### Assets\Enemies\FlyingEnemy\Scripts
* BeeAI.cs
* BeeAttackControl.cs 		  
* BeeWaypoints.cs
* BirdAI.cs
* PickupPlayer.cs 		 

#### Assets\Enemies\FlyingEnemy\Prefabs
* GameBee.prefab         
* BeeWaypoints.prefab
* Bird.prefab	  

#### Assets\Enemies\FlyingEnemy\Sounds
* Bee-noise.wav          
* Bird-flap.wav 

#### Assets\Enemies\FlyingEnemy\Util
* AnimationState.cs
* BeeAnimationActions.cs
* BeeAnimationTransition.cs
* BirdAnimationTransition
* FlyingEnemyState

#### Assets\Scenes\Testing\FlyingEnemy\\*
* NavMesh, etc		      

#### Imports
* Assets\EgyptMonsters\*
* Assets\amusedART\*      

----------------------
### John-Michael H. Smith
### Scripts/Animations
* Assets/LevelBuilding/Obstacles/BreakableWalls/Scripts/BreakableScript.cs
* Assets/Pickups/Cotton/CottonPickup.cs
* Assets/Pickups/Honey/HoneyPickup.cs
* Assets/Players/PlayerV2/Scripts/CollectsCotton.cs
* Assets/Players/PlayerV3.1/Scripts/TakesFallDamage.cs
* Assets/Scripts/Utility/Events/PlayerFallDamageEvent.cs
* Assets/Scripts/Utility/Events/PlayerGruntsEvent.cs
* Assets/Scripts/Utility/Events/PlayerSqueaksEvent.cs
* Assets/Scripts/Utility/AudioEventManager.cs

----------------------
### Prefabs
* Assets\LevelBuilding\Obstacles\BreakableWalls\Prefabs\Wall_Large_100_Fractures.prefab
* Assets\LevelBuilding\Obstacles\BreakableWalls\Prefabs\Wall_Small_64_Cubes.prefab
* Assets\LevelBuilding\Obstacles\BreakableWalls\Prefabs\Wall_Small_100_Fractures.prefab
* Assets\LevelBuilding\Obstacles\BreakableWalls\Prefabs\Wall_Wide_300_Fractures.prefab
* Assets\LevelBuilding\Obstacles\BreakableWalls\Prefabs\Wall_XLarge_500_Fractures.prefab
* Assets\Pickups\Cotton\Cotton.prefab
* Assets\Pickups\Honey\Honey.prefab
* Assets\Pickups\Honey\Animations\HoneyAnimated.prefab

----------------------
### Imports
* Assets\Pickups\Cotton\Materials\*
* Assets\Pickups\Cotton\wholeplant_cotton.fbx
* Assets\Pickups\Import\cottonplant.prefab
* Assets\Pickups\Import\honeycomb.prefab
* Assets\Players\PlayerV3.2\Sounds\Grunts
* Assets\Players\PlayerV3.2\Sounds\Squeaks

----------------------
### Scnenes
* Assets\Scenes\Testing\Breakable\TEST_BreakableWall.unity

----------------------
### Francisco Ochoa
### Scripts/Animations
* Assets/Players/PlayerV1/*
* Assets/Players/PlayerV3/*
* Assets/Players/PlayerV3.2/*
* Assets/Animator Controller/*

----------------------
### Prefabs
* Assets/LevelBuilding/Obstacles/MovingPlatforms*

----------------------
### Imports
* Assets/MixamoImports/*

----------------------
### Mike Davis
----------------------
Created: Pillbug (original mesh is 3rd party), Alpha Scene, Farm Scene, Canyon Scene, MovingPlatformV2 [moves player around], CollapsingPlatforms, Spikes, Rotator, BearTrap, Cactus trap scripts (model is 3rd party), Character movement-jump-falling-groundcollision control, stuffing ball control, dizzy birds effects, level building measurement tools to decrease dev time, BeeHive, Lots of polish, and more

#### Mike Davis' File Contributions
* Assets/Enemies/GroundEnemy/Materials/*
* Assets/Enemies/GroundEnemy/PhysicsMaterials/*
* Assets/Enemies/GroundEnemy/Prefabs/*
* Assets/Enemies/GroundEnemy/Scripts/*
* Assets/Enemies/PillBug/Events/*
* Assets/Enemies/PillBug/FBX/ (I modified the 3rd party 3D model in blender to decimate mesh, provided my own rigging, and created my own animations--none were provided)
* Assets/Enemies/PillBug/PhysicsMaterials/*
* Assets/Enemies/PillBug/Prefabs/*
* Assets/Enemies/PillBug/Scripts/*
* Assets/Enemies/PillBug/Sounds/* (I recorded these myself)
* Assets/LevelBuilding/Buildings/Barn/* (This is the orange-box asset variant I created)
* Assets/LevelBuilding/Buildings/BigBeeHive/* (Created beehive with enemy-passable breakable door variant to look like honeycomb. Original breakable door was created by John-Michael)
* Assets/LevelBuilding/Buildings/House/* (This is the orange-box asset variant I created)
* Assets/LevelBuilding/Buildings/LevelExit/* (Door prefab used in asset was created by Kam)
* Assets/LevelBuilding/Buildings/Silo/* (This is the orange-box asset variant I created)
* Assets/LevelBuilding/Fortress/Doors/* (This is a variant of John-Michael's breakable door I added with orange-box textures)
* Assets/LevelBuilding/Fortress/Walls/* 
* Assets/LevelBuilding/Graveyard/CollapsingGrave/* (Incorporated script from John-Michael's breakable door)
* Assets/LevelBuilding/Hazards/BearTrap/* (I recorded the sounds for this asset as well)
* Assets/LevelBuilding/Hazards/DamageField/*
* Assets/LevelBuilding/Hazards/DeathField/*
* Assets/LevelBuilding/Hazards/Spikes/* (I created this asset in Blender, this folder contains 6 prefabs for level building.)
* Assets/LevelBuilding/Import/Cloud/Scripts/*
* Assets/LevelBuilding/Import/Fence/*.prefab (I created all prefabs here, cosmetic parts are 3rd party)
* Assets/LevelBuilding/Import/Fence/Scripts/* (Added polish with player interactions)
* Assets/LevelBuilding/Import/Hay Bales/Scripts/*
* Assets/LevelBuilding/Import/Hay Bales/*.prefab (I created all prefabs here, cosmetic parts are 3rd party)
* Assets/LevelBuilding/Measuring/* (I created everything in here -- lots of level design utilities to check character capabilities vs level design)
* Assets/LevelBuilding/Measuring/Jumps/*
* Assets/LevelBuilding/Measuring/Materials/*
* Assets/LevelBuilding/Measuring/PlayerSize/*
* Assets/LevelBuilding/Measuring/SizeCubes/*
* Assets/LevelBuilding/Measuring/Textures/* (I created these orange-box textures in Gimp)
* Assets/LevelBuilding/Misc/FrameRateLimiter/*
* Assets/LevelBuilding/Misc/InvisibleWall/*
* Assets/LevelBuilding/Misc/Rotator/*
* Assets/LevelBuilding/Obstacles/BreakableWalls/Prefabs/Wall_Desert.prefab (Some code is derived from John-Michael's breakable wall)
* Assets/LevelBuilding/Obstacles/Cloud/* (This is the orange-box asset I created)
* Assets/LevelBuilding/Obstacles/CollapsingPlatform/* 
* Assets/LevelBuilding/Obstacles/Fence/* (This is the orange-box asset I created)
* Assets/LevelBuilding/Obstacles/HayBales/* (This is the orange-box asset I created)
* Assets/LevelBuilding/Obstacles/MeasuredPrefabs/* 
* Assets/LevelBuilding/Obstacles/MovingPlatformV2/* (This is not based on the non-V2 version.)
* Assets/LevelBuilding/Obstacles/Tractor/* (This is the orange-box asset I created)
* Assets/LevelBuilding/PhysicsMaterials/*
* Assets/LevelBuilding/SoundEmitters/*
* Assets/Materials/HoneySplat (Created texture in Gimp)
* Assets/Misc/DizzyBirds/Animation/*
* Assets/Misc/DizzyBirds/Events/*
* Assets/Misc/DizzyBirds/Materials/*
* Assets/Misc/DizzyBirds/Scripts/*
* Assets/Misc/DizzyBirds/Sounds/*
* Assets/Misc/DizzyBirds/*.prefab
* Assets/Misc/Events/*
* Assets/Misc/Scripts/CactusHit/*
* Assets/Misc/Scripts/JumpHit/*
* Assets/Misc/Scripts/Punch/*
* Assets/Misc/Scripts/Ram/*
* Assets/Misc/Scripts/SpikeHit/*
* Assets/Misc/Scripts/StuffingHit/*
* Assets/Misc/Scripts/CollisionOrganizer.cs
* Assets/Misc/Scripts/Constants.cs
* Assets/Misc/Scripts/FSM.cs
* Assets/Misc/Scripts/IDestroySelf.cs
* Assets/Misc/Scripts/RandomizeTransformOffsets.cs
* Assets/Misc/StuffingBall/* (everything except for the DustCloud effects, Source/*, and TEST_stuffing updates for particles)
* Assets/Pickups/Cotton/ (Generated a decimated icosphere mesh in blender, updated animations, and integrated with Cotton asset to get ~13x reduction in rendered triangle count to make game run smoother)
* Assets/Players/FreeCamera/Input/*
* Assets/Players/FreeCamera/Materials/*
* Assets/Players/FreeCamera/Prefabs/*
* Assets/Players/FreeCamera/Scripts/*
* Assets/Players/PlayerV2/Input/*
* Assets/Players/PlayerV2/Materials/*
* Assets/Players/PlayerV2/PhysicsMaterials/*
* Assets/Players/PlayerV2/Prefabs/*
* Assets/Players/PlayerV2/Scripts/*
* Assets/Players/PlayerV3.2/Scripts/PlayerControlV3_3.cs (Implemented character motion control code, jump and falling control logic with spherecasts, grounded state detection and control, player knockback effects and most of damage effects, everything related to scaling the player)
* Assets/Scenes/AlphaScene/* (all puzzles 1-7 (of 8) in scene, all terrain, lighting, etc.)
* Assets/Scenes/Farm/* (everything except for adding the two Eagle enemy instances to the scene)
* Assets/Scenes/Canyon/* (everything except for: floating sky puzzle over spider cave, and the spider cave itself)
* Assets/Scenes/Testing/GroundEnemy/*
* Assets/Scenes/Testing/PlayerV2/*
* Assets/Scenes/LevelSelect/ (Added new skybox, invisible walls to keep player in, ambient sounds, fixed asset collider issues)
* Assets/Scripts/UI/ (added honey bar "special effects" in HoneyBar.cs and super punch highlight to stuffing bar)
* Assets/Scripts/Utility/AudioManager.cs (integrated sounds for pillbug, beartrap, others, balanced all sound levels relative to eachother)
* Assets/Scripts/Utility/StuffingScaler.cs
* Assets/Sounds/click/* (recorded playing with tape measure)
* Assets/Sounds/metallicClink/* (recorded bumping pipes together)
* Assets/Sounds/pop/* (recorded making mouth sounds)
* Assets/Sprites/Bar_lightGreen_highlight.png (Created in Gimp)
* Assets/Sprites/Bar_yellow_highlight.png (Created in Gimp)

#### Mike Davis' 3rd-Party Imports
**NOTE: Any assets that I modified have been altered within the terms of their respective EULA's. Unity store asset meshes have not been directly altered.**
* Assets/Enemies/PillBug/FBX/<original is not directly included> (I got the original 3D object from a 3rd part, but heavily modified mesh, added my own rigging, animations, sounds)
* Assets/LevelBuilding/Import/Barn (I modified the mesh in blender to flatten roof for better platforming)
* Assets/LevelBuilding/Import/Cloud (I modified this to create prefab with flattened clouds, add colliders, add physics materials)
* Assets/LevelBuilding/Import/Desert Rock Materials
* Assets/LevelBuilding/Import/Fantasy Skybox FREE
* Assets/LevelBuilding/Import/Fence (created multiple prefabs with single-colliders to reduce physics overhead)
* Assets/LevelBuilding/Import/Grain Silo
* Assets/LevelBuilding/Import/Hay Bales (Note that I created all prefabs here)
* Assets/LevelBuilding/Import/Old Tractor (Note that I created the prefab with compound colliders)
* Assets/LevelBuilding/Import/PolyDesert Mobile
* Assets/LevelBuilding/Import/Polygon Desert Pack (I updated prefabs to include my cactus hit control scripts and colliders)
* Assets/LevelBuilding/Import/Rocks Pack Lite
* Assets/Sounds/Import/mixkit-cow-moo-1744.wav
* Assets/Sounds/Import/mixkit-double-little-bird-chirp-21.wav
* Assets/Sounds/Import/mixkit-dusty-debris-short-fall-404.wav
* Assets/Sounds/Import/mixkit-falling-on-undergrowth-390.wav
* Assets/Sounds/Import/mixkit-fly-buzz-trapped-in-a-window-2695.wav
* Assets/Sounds/Import/mixkit-goat-baa-stutter-1771.wav
* Assets/Sounds/Import/mixkit-gravel-stones-small-avalanche-1273.wav
* Assets/Sounds/Import/mixkit-hawk-call-squawk-1277.wav
* Assets/Sounds/Import/mixkit-little-bird-calling-chirp-23.wav
* Assets/Sounds/Import/mixkit-losing-bleeps-2026.wav
* Assets/Sounds/Import/mixkit-newspaper-falling-to-the-floor-386.wav
* Assets/Sounds/Import/mixkit-page-back-chime-1108.wav
* Assets/Sounds/Import/mixkit-page-forward-single-chime-1107.wav
* Assets/Sounds/Import/mixkit-rooster-crowing-in-the-morning-2462.wav
* Assets/Sounds/Import/mixkit-short-rooster-crowing-2470.wav
* Assets/Sounds/Import/mixkit-single-cricket-screech-1780.wav
* Assets/Sounds/Import/mixkit-small-stone-avalanche-1272.wav
* Assets/Sounds/Import/mixkit-small-win-2020.wav
* Assets/Sounds/Import/mixkit-werewolf-roar-1730.mp3
* Assets/Sounds/Import/jaw_harp9.mp3
* Assets/TOZ/Triplanar Shaders (I needed triplanar rendering shaders to get terrain texture UV mapping to work on vertical cliff faces of canyon without stretching)


----------------------
## External Packages
----------------------

### Door Free Pack Aferar
* https://assetstore.unity.com/packages/3d/props/interior/door-free-pack-aferar-148411#description

### Environment Pack: Free Forest Sample
* https://assetstore.unity.com/packages/3d/vegetation/environment-pack-free-forest-sample-168396#description

### Fantasy Skybox FREE
* https://assetstore.unity.com/packages/2d/textures-materials/sky/fantasy-skybox-free-18353

## Bee Model
* https://assetstore.unity.com/packages/3d/characters/animals/fantasy-bee-135487

## Eagle/Bird Model
* https://assetstore.unity.com/packages/3d/characters/animals/birds/egypt-pack-eagle-140079

### PILLBUG Model
"PILLBUG (Rollie Pollie) Armadillidiidae" (https://skfb.ly/ouAKX) by Luciano Filicetti is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
***Model modified by Mike Davis to include rigging, animations, and mesh decimation. These changes are not endorsed by the original content creator.***

## Spider Model
* https://assetstore.unity.com/packages/3d/characters/animals/insects/animated-spider-22986

### Clouds
* https://www.cgtrader.com/free-3d-models/various/various-models/clouds-low-poly
***Version of asset used in game is highly distorted to create platforms. This modification is not endorsed by the original content creator.***

### Desert Rock Materials
* https://assetstore.unity.com/packages/2d/textures-materials/stone/desert-rock-materials-151868 

### Fence
* https://www.cgtrader.com/free-3d-models/exterior/landscape/lowpoly-fence-cc4a87aa-65e7-4454-b211-69e0eabee78f

### Hay Bales
* https://www.cgtrader.com/free-3d-models/plant/other/hay-bale-3d-model

### Grain Silo
* "Grain Silo - Low Poly Game Ready" (https://skfb.ly/opFzV) by Gobby is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).

### Barn
* "Barn_lowpoly" (https://skfb.ly/6XQSV) by Raphael Escamilla is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
***Mesh modified to have flatter roof in blender. These changes are not endorsed by the original content created.***

### POLYDesert
* https://assetstore.unity.com/packages/3d/environments/landscapes/polydesert-107196

### TOZ Triplanar Shaders
* https://assetstore.unity.com/packages/vfx/shaders/toz-triplanar-shaders-32519

### Tractor
* https://www.cgtrader.com/free-3d-models/vehicle/industrial-vehicle/cartoon-tractor

### Stuffing Model
* https://sketchfab.com/3d-models/stylized-clouds-e326c36890364526910cba03c1393ebc

### Cobweb Model
* https://sketchfab.com/3d-models/cobwebs-941c92fcf27b44caafbf6d690ba9593f

### WebShot Model
* https://sketchfab.com/3d-models/spider-web-a3ab615729e945e48676774f54cfc370

### Teddy Bear Model
* https://rigmodels.com/model.php?view=Teddy_Bear-3d-model__PQVDQEJDZ6RDDY2INDXRCJR3H&searchkeyword=teddy%20bear&manualsearch=1

### Vert Vaults - Rocks pack LITE
* https://assetstore.unity.com/packages/3d/environments/landscapes/vert-vaults-rocks-pack-lite-178085

### AudioManger file boilerplate from CS4455_M1 Project
* https://github.gatech.edu/IMTC/CS4455_M1_Support/blob/master/Assets/Scripts/AppEvents/AudioEventManager.cs
### EventManager file boilerplate from CS4455_M1 Project
* https://github.gatech.edu/IMTC/CS4455_M1_Support/blob/master/Assets/Scripts/EventSystem/EventManager.cs
### EventSound3D file boilerplate from CS4455_M1 Project
* https://github.gatech.edu/IMTC/CS4455_M1_Support/blob/master/Assets/Scripts/AppEvents/EventSound3D.cs
### PlayerLandsEvent file boilerplate from CS4455_M1 Project
* https://github.gatech.edu/IMTC/CS4455_M1_Support/blob/master/Assets/Scripts/AppEvents/PlayerLandsEvent.cs
### JumpEvent file boilerplate from CS4455_M1 Project
* https://github.gatech.edu/IMTC/CS4455_M1_Support/blob/master/Assets/Scripts/AppEvents/JumpEvent.cs
### DeathEvent file boilerplate from CS4455_M1 Project
* https://github.gatech.edu/IMTC/CS4455_M1_Support/blob/master/Assets/Scripts/AppEvents/DeathEvent.cs

### Trombone Losing Sound
* https://assets.mixkit.co/sfx/download/mixkit-sad-game-over-trombone-471.wav

### Bee Sound
* https://www.freesoundslibrary.com/bee-noise/

### Bird/Eagle Sound
* https://pixabay.com/sound-effects/birds-flapmp3-14504/

### All Honey Collecetd Sound
* https://assets.mixkit.co/sfx/download/mixkit-achievement-bell-600.wav

### Honey Pickup Sound
* https://assets.mixkit.co/sfx/download/mixkit-chewing-something-crunchy-2244.wav

### Spider Leap Sound
* https://assets.mixkit.co/sfx/download/mixkit-boing-hit-sound-2894.wav

### Spider Punch Sound
* https://assets.mixkit.co/sfx/download/mixkit-cartoon-insect-scape-buzz-38.wav

### Spider Death Sound
* https://assets.mixkit.co/sfx/download/mixkit-juicy-insect-fast-squish-324.wav

### Spider Step Sound
* https://assets.mixkit.co/sfx/download/mixkit-cartoon-insect-running-fast-37.wav

### Spider Webshot Sound
* https://assets.mixkit.co/sfx/download/mixkit-martial-arts-punch-2052.wav


### Cloud Bounce Sound
* Assets/Sounds/Import/jaw_harp9.mp3 from (https://freesound.org/people/3bagbrew/sounds/95613/)

### MixKit Sounds in (Assets/Sounds/Import)
* All from: https://mixkit.co/free-sound-effects/ released under the "Mixkit Sound Effects Free License"
* Assets/Sounds/Import/mixkit-cow-moo-1744.wav
* Assets/Sounds/Import/mixkit-double-little-bird-chirp-21.wav
* Assets/Sounds/Import/mixkit-dusty-debris-short-fall-404.wav
* Assets/Sounds/Import/mixkit-falling-on-undergrowth-390.wav
* Assets/Sounds/Import/mixkit-fly-buzz-trapped-in-a-window-2695.wav
* Assets/Sounds/Import/mixkit-goat-baa-stutter-1771.wav
* Assets/Sounds/Import/mixkit-gravel-stones-small-avalanche-1273.wav
* Assets/Sounds/Import/mixkit-hawk-call-squawk-1277.wav
* Assets/Sounds/Import/mixkit-little-bird-calling-chirp-23.wav
* Assets/Sounds/Import/mixkit-losing-bleeps-2026.wav
* Assets/Sounds/Import/mixkit-newspaper-falling-to-the-floor-386.wav
* Assets/Sounds/Import/mixkit-page-back-chime-1108.wav
* Assets/Sounds/Import/mixkit-page-forward-single-chime-1107.wav
* Assets/Sounds/Import/mixkit-rooster-crowing-in-the-morning-2462.wav
* Assets/Sounds/Import/mixkit-short-rooster-crowing-2470.wav
* Assets/Sounds/Import/mixkit-single-cricket-screech-1780.wav
* Assets/Sounds/Import/mixkit-small-stone-avalanche-1272.wav
* Assets/Sounds/Import/mixkit-small-win-2020.wav
* Assets/Sounds/Import/mixkit-werewolf-roar-1730.mp3

### Music 1
* https://assets.mixkit.co/music/download/mixkit-goin-back-to-alabama-830.mp3

### Music 2
* https://assets.mixkit.co/music/download/mixkit-kidding-around-9.mp3

### Honey Material
* https://sketchfab.com/3d-models/honeycomb-material-7585e6e13cb04527a8ab56bd2ff56673

### Cotton Plant 
* https://sketchfab.com/3d-models/cottonplant-0918412ae8ce463192b117e5b901c81c

### Grunt Audio clips
* https://quicksounds.com/sound/15319/male-land-grunt-3 (3, 4, 5, 6)

## External Images

### Honey Pot Image
* http://pixelartmaker.com/art/cfc8d122e043270

### Health Bar and Heart Images
* https://github.com/Brackeys/Health-Bar/tree/master/Health%20Bar/Assets/Sprites

### Small Bear Image
* https://media.istockphoto.com/vectors/cute-brown-bear-vector-illustration-vector-id1266295994?k=20&m=1266295994&s=612x612&w=0&h=HE-9vcTX09VY4L_3AxsB7MzaE1XQkfQorRY1ZKfl17Q=

### Medium Bear Image
* https://media.istockphoto.com/vectors/vector-flat-teddy-bear-baby-toy-vector-id893429478?k=20&m=893429478&s=612x612&w=0&h=T2OmOZOsa7nKghGgNha3uQawcY9sUEKc5x-1atRrRRQ=

### Large Bear Image
* https://ih1.redbubble.net/image.639619259.3364/st,small,845x845-pad,1000x1000,f8f8f8.u2.jpg

### Stiched Bear Image
* https://ih1.redbubble.net/image.1436210739.8498/st,small,845x845-pad,1000x1000,f8f8f8.jpg

### Cotton Bar Fill Image
* https://media.istockphoto.com/vectors/clouds-volumetric-illustration-vector-id1145892173?k=20&m=1145892173&s=612x612&w=0&h=KCvy1qjyvmPIYPTf9qmySc9YHqHhGvaRce3vB-vggic=e/master/Health%20Bar/Assets/Sprites

### Pause Menu Images
* https://pngtree.com/freepng/game-button-for-with-play-pause-replay-and-exit_5981889.html
* https://pngtree.com/freepng/cartoon-game-button-ui-interface_5238863.html

### Dotted Arrow
* https://www.template.net/editable/73384/dotted-curved-arrow-vector

### Red X
* https://similarpng.com/red-cross-mark-icon-on-transparent-background-png/

### Gold Star
* https://www.iconspng.com/image/81212/cartoon-gold-star

### Lock
* https://findicons.com/icon/84760/lock

### Gear Icon
* https://en.m.wikipedia.org/wiki/File:Windows_Settings_app_icon.png

### Power Icon
* https://www.pngwing.com/en/free-png-nnmod/download

### Spider Web Icon
* https://pnghut.com/png/idfehmPuai/spider-web-cartoon-clip-art-monochrome-halloween-cobwebs-transparent-png#_=_

## External Fonts

### Garden Collection C Caps
* https://en.bestfonts.pro/font/garden-collection

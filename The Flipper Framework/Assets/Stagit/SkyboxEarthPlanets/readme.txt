Welcome to this package.

Version 1.1 Date: 12 September 2017
- Fixed issues with blurred stars. Instead of transparancy i added a black background to each png as easy fix as i still don't know what the main issue is with the newer versions of Unity3D.
- Added demo scene of my new paid version of Earth Skyboxes Pro available on the asset store https://www.assetstore.unity3d.com/en/#!/content/96430
- If your having issues or need the old version of this package contact me contact@stagit.mobi

Notes
- If your having issues or need the old version of this package contact me contact@stagit.mobi
- A new paid package is on the way including over 90+ different (our solar system planets) skyboxes. This time it will also include Saturn, Uranus and other celestials not included in the free package all in 4K.
- I have also released a 64K Earth 3D model (including city/country geo locations) which all these Earth renders are based on https://www.assetstore.unity3d.com/en/#!/content/53113 (However also paid)
- Once again a very detailed Earth Skyboxes Pro version is available at https://www.assetstore.unity3d.com/en/#!/content/96430 (or if you just want to support me)


This package contains various directories.

To get straight to the point the ones you are interested in are

- Skyboxes (This directory contains all the skyboxes you can use and you can drag and drop them in to your unity menu selecting [Window] -> [Lightning] -> skybox area) 
- Scenes (This contains the example scene showing all the available skyboxes available, it will also show you how to use the sunflare and also contains the sun coordinates)


Other more unimportant directories

- Textures (Contains all the textures for the skyboxes)
- Script (Just some scripts used for the demo scene)
- Flares (contains the HD flare which you can select doing -> select your light in scene -> select the flare option) and it should popup

If you are a beginner always check out the Demo scene in the Scenes directory this will show you how to set up the Sun and the Skyboxes.

SUN LOCATIONS
Ofcourse i forgot to write the locations down with each render i did, in the demo scene i tried to recreate the positions but you have to do some tweaking.

TEXTURES
The textures are rendered in PNG files, this is done so it is still possible to edit them (example removing the stars, adding nebulas).
However this can also cause some trouble with Unity. Sometimes you will see a message that the texture is nog compatible with HDR and when you click the fix now everything will be blurry. This is because of the alpha. 
You can either solve this by changing the texture settings again, but also if you don't need the alpha channel the best option is to remove the alpha channel by for example saving all textures in to a 100% jpeg file.
The order of entering the skybox textures is Front: 1 Back: 3 Left: 2 Right: 4 Up: 6 Down: 5 (this is the same for all textures)


WARNINGS
All the textures are delivered in 2048px (and some even 4096px) if your developing a mobile game or web based game make sure that all the textures match the those based settings (so you need to make them lower -> selecting all textures and select overide [selecting the platfrom you want to ovveride]) for mobile games 1024px is mostly suitable.
The Lens Flare delivered with this package is also 2048px so take care of this one also and resize those according to the platform you are developing for.

Now everything is done for you to setup all things its time for the rest of the boring story ;).

BUGS
Yes textures can contain alot of bugs, mostly when we are talking about stitching enormous amounts of data which NASA does making those images available. Somehow NASA is kinda lazy stitching 16K sized images together which i'm stiching myself again since the things they deliver are just parts. All combined together and edited multiple times trough photoshop i'm doing my best to get all the stitching errors out of there. In some of the rendered images they may be still vissible and i'm still trying to find those and coreccting them.
Let me know if you see some of those (make a screen marking the area) and i willl solve it.

OTHER
I had some issues resizing the textures from 4096 back to 2048 for this version. So you will find some 4096 textures in this package especially the closeup Mars texture which is worth it. 

FINAL NOTES
I have been spending enourmous amounts of time creating those skyboxes and we are talking about weeks and alot of CPU power. But nothing is better to get some amazing things for free, which we all need as indie developers so its my time to share this work for free also. 

# YakuzaGMDTexCopy
Simple tool for copying textures from a Yakuza .gmd model file for use with mods  

The program will ask for the following:  
- Texture Source Folder -> This is the folder where the model's game origin textures would be located  
- Destination Folder -> This is where the textures will be copied to (i.e. your dds_hires/00/ folder in your mod)  
- Common Textures Folder -> You will be asked if you wish to pick one, if yes, this is the folder where the textures for the game you are porting the model to are located (i.e. if you are attempting to port a model to Yakuza 8, pick the Yakuza 8 chara par dump output folder here, this is so that textures that already exist are separated into an easily deletable "Common" folder and avoid unnecessarily bloating the mod size)  

Note: When picking source folder for textures, you do NOT need to specifically pick the folder where the textures are in the chara.par output folder, you can simply select the chara.par output folder and the program will search every directory and subdirectory for texture files, this can avoid weird scenarios where some textures are split such as Nishitani's model from Gaiden where some textures are in the 00 folder and some are in the 01, in this case if you simply pick the chara par output folder and not the dds 00 folder specifically, the program will successfully copy everything.

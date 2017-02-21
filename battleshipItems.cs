datablock ItemData(battleship_5Item) {
	category = "Tools";  // Mission editor category
	className = "Weapon"; // For inventory system

	colorShiftColor = "1 1 1";
	shapeFile = "./items/battleship/5.dts";

	image = "hammerImage";
	doColorShift = 1;

	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	canDrop = 1;
	cannotPickup = 1;
	uiname = "Battleship - Carrier";
};

datablock ItemData(battleship_4Item : battleship_5Item) {
	shapeFile = "./items/battleship/4.dts";
	image = "hammerImage";
	cannotPickup = 1;

	uiname = "Battleship - Battleship";
};

datablock ItemData(battleship_3_1Item : battleship_5Item) {
	shapeFile = "./items/battleship/3.dts";
	image = "hammerImage";
	cannotPickup = 1;

	uiname = "Battleship - Submarine";
};

datablock ItemData(battleship_3_2Item : battleship_5Item) {
	shapeFile = "./items/battleship/3.dts";
	image = "hammerImage";
	cannotPickup = 1;

	uiname = "Battleship - Crusier";
};

datablock ItemData(battleship_2Item : battleship_5Item) {
	shapeFile = "./items/battleship/2.dts";
	image = "hammerImage";
	cannotPickup = 1;

	uiname = "Battleship - Destroyer";
};

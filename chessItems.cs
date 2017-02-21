datablock ItemData(white_kingItem) {
	category = "Tools";  // Mission editor category
	className = "Weapon"; // For inventory system

	colorShiftColor = "1 1 1";
	shapeFile = "./items/king.dts";

	image = "kingImage";
	doColorShift = 1;

	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	canDrop = 1;
	cannotPickup = 1;
	uiname = "White King";
};

datablock ItemData(white_queenItem : white_kingItem) {
	shapeFile = "./items/queen.dts";
	image = "queenImage";
	cannotPickup = 1;

	uiname = "White Queen";
};

datablock ItemData(white_bishopItem : white_kingItem) {
	shapeFile = "./items/bishop.dts";
	image = "bishopImage";
	cannotPickup = 1;
	
	uiname = "White Bishop";
};

datablock ItemData(white_knightItem : white_kingItem) {
	shapeFile = "./items/knight.dts";
	image = "knightImage";
	cannotPickup = 1;
	
	uiname = "White Knight";
};

datablock ItemData(white_rookItem : white_kingItem) {
	shapeFile = "./items/rook.dts";
	image = "rookImage";
	cannotPickup = 1;
	
	uiname = "White Rook/Castle";
};

datablock ItemData(white_pawnItem : white_kingItem) {
	shapeFile = "./items/pawn.dts";
	image = "pawnImage";
	cannotPickup = 1;
	
	uiname = "White Pawn";
};

datablock ItemData(black_kingItem : white_kingItem) {
	colorShiftColor = "0.2 0.2 0.2";	
	uiname = "Black King";
	cannotPickup = 1;
};

datablock ItemData(black_queenItem : white_queenItem) {
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Queen";
};

datablock ItemData(black_bishopItem : white_bishopItem) {
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Bishop";
};

datablock ItemData(black_knightItem : white_knightItem) {
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Knight";
};

datablock ItemData(black_rookItem : white_rookItem) {
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Rook/Castle";
};

datablock ItemData(black_pawnItem : white_pawnItem) {
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Pawn";
};

datablock ShapeBaseImageData(kingImage)
{
	shapeFile = "./items/king.dts";
	emap = true;

	colorShiftColor = "1 1 0 1";
	doColorShift = 1;
	item = white_kingItem;
	className = "WeaponImage";

	mountPoint = 6;
};

datablock ShapeBaseImageData(queenImage)
{
	shapeFile = "./items/queen.dts";
	emap = true;

	colorShiftColor = "1 1 0 1";
	doColorShift = 1;
	item = white_queenItem;
	className = "WeaponImage";

	mountPoint = 2;
};

datablock ShapeBaseImageData(bishopImage)
{
	shapeFile = "./items/bishop.dts";
	emap = true;

	colorShiftColor = "1 1 0 1";
	doColorShift = 1;
	item = white_bishopItem;
	className = "WeaponImage";

	mountPoint = 2;
};

datablock ShapeBaseImageData(knightImage)
{
	shapeFile = "./items/knight.dts";
	emap = true;

	colorShiftColor = "1 1 0 1";
	doColorShift = 1;
	item = white_knightItem;
	className = "WeaponImage";

	mountPoint = 2;
};

datablock ShapeBaseImageData(rookImage)
{
	shapeFile = "./items/rook.dts";
	emap = true;

	colorShiftColor = "1 1 0 1";
	doColorShift = 1;
	item = white_rookItem;
	className = "WeaponImage";

	mountPoint = 2;
};

datablock ShapeBaseImageData(pawnImage)
{
	shapeFile = "./items/pawn.dts";
	emap = true;

	colorShiftColor = "1 1 0 1";
	doColorShift = 1;
	item = white_pawnItem;
	className = "WeaponImage";

	mountPoint = 2;
};


//////////////////////////////
//////////////////////////////
/////////BATTLE CHESS/////////
//////////////////////////////
//////////////////////////////

datablock ItemData(white_king_battleItem) {
	category = "Tools";  // Mission editor category
	className = "Weapon"; // For inventory system

	colorShiftColor = "1 1 1";
	shapeFile = "./items/battle/white/king.dts";

	image = "kingImage";
	doColorShift = 1;

	rotate = false;
	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	emap = true;

	canDrop = 1;
	cannotPickup = 1;
	uiname = "White Battle King";
};

datablock ItemData(white_queen_battleItem : white_king_battleItem) {
	shapeFile = "./items/battle/white/queen.dts";
	image = "queenImage";
	cannotPickup = 1;

	uiname = "White Battle Queen";
};

datablock ItemData(white_bishop_battleItem : white_king_battleItem) {
	shapeFile = "./items/battle/white/bishop.dts";
	image = "bishopImage";
	cannotPickup = 1;
	
	uiname = "White Battle Bishop";
};

datablock ItemData(white_knight_battleItem : white_king_battleItem) {
	shapeFile = "./items/battle/white/knight.dts";
	image = "knightImage";
	cannotPickup = 1;
	
	uiname = "White Battle Knight";
};

datablock ItemData(white_rook_battleItem : white_king_battleItem) {
	shapeFile = "./items/battle/white/castle.dts";
	image = "rookImage";
	cannotPickup = 1;
	
	uiname = "White Battle Rook/Castle";
};

datablock ItemData(white_pawn_battleItem : white_king_battleItem) {
	shapeFile = "./items/battle/white/pawn.dts";
	image = "pawnImage";
	cannotPickup = 1;
	
	uiname = "White Battle Pawn";
};

datablock ItemData(black_king_battleItem : white_king_battleItem) {
	shapeFile = "./items/battle/black/king.dts";

	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;

	uiname = "Black Battle King";
};

datablock ItemData(black_queen_battleItem : white_queen_battleItem) {
	shapeFile = "./items/battle/black/queen.dts";

	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Battle Queen";
};

datablock ItemData(black_bishop_battleItem : white_bishop_battleItem) {
	shapeFile = "./items/battle/black/bishop.dts";

	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Battle Bishop";
};

datablock ItemData(black_knight_battleItem : white_knight_battleItem) {
	shapeFile = "./items/battle/black/knight.dts";
	
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Battle Knight";
};

datablock ItemData(black_rook_battleItem : white_rook_battleItem) {
	shapeFile = "./items/battle/black/castle.dts";
	
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Battle Rook/Castle";
};

datablock ItemData(black_pawn_battleItem : white_pawn_battleItem) {
	shapeFile = "./items/battle/black/pawn.dts";
	
	colorShiftColor = "0.2 0.2 0.2";
	cannotPickup = 1;
	
	uiname = "Black Battle Pawn";
};
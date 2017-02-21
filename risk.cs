$xItemOffset = 1;
$yItemOffset = 2;
$lineMax = 3;

$onesDB = white_pawn_battleItem;
$fivesDB = white_knight_battleItem;
$tensDB = white_rook_battleItem;

function getItemPosArray(%itemCnt, %center) {
	%y = mCeil(%itemCnt/$lineMax);
	%y = getMax(%y, 1);
	%t = %itemCnt;
	for (%i = 0; %i < %y; %i++) {
		%x[%i] = getMin(%t, $lineMax);
		%t -= $lineMax;
	}

	//rows offset
	%yOffVal = $yItemOffset;
	%start = (%y-1)*%yOffVal/2;
	if (%start < 0) {
		echo("error");
	}
	%yOff = "";
	for (%i = 0; %i < %y; %i++) {
		%yOff = %yOff SPC %start;
		%start -= %yOffVal;
	}
	%yOff = trim(%yOff);

	//row item offset
	%xOffVal = $xItemOffset;
	for (%i = 0; %i < %y; %i++) {
		%xOff[%i] = "";
		%start = (%x[%i]-1)*%xOffVal/2;
		for (%j = 0; %j < %x[%i]; %j++) {
			%xOff[%i] = %xOff[%i] SPC %start;
			%start -= %xOffVal;
		}
		%xOff[%i] = trim(%xOff[%i]);
	}

	//combine
	for (%i = 0; %i < %y; %i++) {
		%yCurr = getWord(%yOff, %i);
		for (%j = 0; %j < getWordCount(%xOff[%i]); %j++) {
			%next = getWord(%xOff[%i], %j) SPC %yCurr SPC "0";
			%preRet = %next TAB %preRet;
		}
	}
	trim(%preRet);

	for (%i = 0; %i < getFieldCount(%preRet); %i++) {
		%ret = %ret TAB vectorAdd(getField(%preRet, %i), %center);
	}

	return trim(%ret);
}

function createItem(%db, %rot, %pos, %color) {
	%item = new Item() {
		datablock = %db;
		static = true;
		position = %pos;
		rotation = %rot;
	};
	MissionCleanup.add(%item);
	%item.setNodeColor("ALL", %color);
	return %item;
}

function createRiskItems(%brick, %armyCnt, %color) {
	for (%i = 0; %i < %brick.itemCnt; %i++) {
		%brick.item[%i].delete();
	}
	%tens = mFloor(%armyCnt/10);
	%fives = mFloor((%armyCnt%10)/5);
	%ones = %armyCnt%5;

	%total = %tens + %fives + %ones;
	%brick.itemCnt = %total;
	%itemPos = getItemPosArray(%total, vectorAdd(%brick.getPosition(), "0 0 " @ (%brick.dataBlock.brickSizeZ * 0.1)));
	for (%i = 0; %i < %total; %i++) {
		if (%tens > 0) {
			%brick.item[%i] = createItem($tensDB, %brick.rotation, getField(%itemPos, %i), %color);
			%tens--;
		} else if (%fives > 0) {
			%brick.item[%i] = createItem($fivesDB, %brick.rotation, getField(%itemPos, %i), %color);
			%fives--;
		} else if (%ones > 0) {
			%brick.item[%i] = createItem($onesDB, %brick.rotation, getField(%itemPos, %i), %color);
			%ones--;
		}
	}

	return %brick SPC %total;
}

function Risk::updateNationArmies (%this, %n) {
	%team = %this.n[%n @ "owner"];
	%teamColor = $t[%team @ "::color"];
	%armyCnt = %this.n[%n @ "armies"];
	%brick = %this.n[%n @ "center"];

	createRiskItems(%brick, %armyCnt, %teamColor);
	%brick.setShapeName(%armyCnt);
	%brick.setShapeNameColor (%teamColor);
}

function getSortedDiceRolls(%numDice) {
	while (%numDice > 0) {
		%preRet = "";
		%nextRoll = getRandom(1, 6);
		for (%i = 0; %i < getWordCount(%ret); %i++) {
			if (%nextRoll > getWord(%ret, %i)) {
				break;
			}
			%preRet = %preRet SPC getWord(%ret, %i);
		}
		%ret = trim(trim(%preRet) SPC %nextRoll SPC trim(getWords(%ret, %i, getWordCount(%ret))));
		%numDice--;
	}
	return trim(%ret);
}
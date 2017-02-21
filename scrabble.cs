//////////////// SCRABBLE.CS ////////////////
if(!isObject(MasterScrabbleGroup))
	new SimObject(MasterScrabbleGroup);
$BG::Scrabble::Alpha="ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
$BG::Scrabble::Value="1 3 3 2 1 4 2 4 1 8 5 1 3 1 1 3 10 1 1 1 1 4 4 8 4 10 0";
$BG::Scrabble::Amount="9 2 2 4 12 2 3 2 9 1 1 4 2 6 8 2 1 6 4 6 4 2 2 1 2 1 2";
function BG_Scrabble_DefineValues() {
	for(%i=0;%i<strLen(%alpha=$BG::Scrabble::Alpha);%i++) {
		%char = getSubStr(%alpha, %i, 1);
		$BG::Scrabble::Value[%char] = getWord($BG::Scrabble::Value, %i);
		$BG::Scrabble::Amount[%char] = getWord($BG::Scrabble::Amount, %i);
	}
}
function ScrabbleBag::setup(%this) {
	%this.grabBag = "";
	for(%i=0;%i<27;%i++) {
		%char = getSubStr($BG::Scrabble::Alpha, %i, 1);
		for(%n=0;%n<$BG::Scrabble::Amount[%char];%n++)
			%this.grabBag=%this.grabBag@%char;
	}
	%this.setup = true;
	echo("Called setup");
}
function ScrabbleBag::grabLetter(%this) {
	if(!%this.setup) {
		%this.setup();
	}
	%length = strLen(%this.grabBag);
	if(%length == 0)
		return 0;
	%n = getRandom(0, %length-1);
	%char = getSubStr(%this.grabBag, %n, 1);
	%this.grabBag = getSubStr(%this.grabBag,0,%n)@getSubStr(%this.grabBag,%n+1,%length);
	return %char;
}
// ScriptObject ScrabbleGame
//
//   GameConnection client[2-4]
//   int            numClients
//   int            turn
//   String         letters[2-4]
//   int            index[2-4]
//   int            score[2-4]
//   int            rank[2-4]
//   int            rankScore[2-4]
//   String         history[4]
//   char           board[15][15]
//   ScrabbleBag    bag
//   bool           isFirstMove
function ScrabbleSession::getStringValue(%this, %string) {
	%n = 0;
	for(%i=0;%i<strLen(%string);%i++) {
		%n += $BG::Scrabble::Value[getSubStr(%string, %i, 1)];
	}
	return %n;
}
function ScrabbleSession::logicTick(%this) {
	cancel(%this.logicTick);
	%centerPrint = "<just:left>\c6Score:<just:right>\c6Words:\n<just:left>" @
					"\c61. " @ %this.client[%this.rank[0]].getPlayerName() @ " ( \c4 " @ %this.score[%this.rank[0]] @
					 "\c6 )<just:right>\c6" @ %this.history[0] @ " ( \c4" @ %this.getStringValue(%this.history[0]) @ "\c6 )<just:left>\n" @
					"\c62. " @ (isObject(%this.client[%this.rank[1]]) ? %this.client[%this.rank[1]].getPlayerName():"N/A") @ " ( \c4 " @ %this.score[%this.rank[1]] @
					 "\c6 )<just:right>\c6" @ %this.history[1] @ " ( \c4" @ %this.getStringValue(%this.history[1]) @ "\c6 )<just:left>\n" @
					"\c63. " @ (isObject(%this.client[%this.rank[2]]) ? %this.client[%this.rank[2]].getPlayerName():"N/A") @ " ( \c4 " @ %this.score[%this.rank[2]] @
					 "\c6 )<just:right>\c6" @ %this.history[2] @ " ( \c4" @ %this.getStringValue(%this.history[2]) @ "\c6 )<just:left>\n" @
					"\c64. " @ (isObject(%this.client[%this.rank[3]]) ? %this.client[%this.rank[3]].getPlayerName():"N/A") @ " ( \c4 " @ %this.score[%this.rank[3]] @
					 "\c6 )<just:right>\c6" @ %this.history[3] @ " ( \c4" @ %this.getStringValue(%this.history[3]) @ "\c6 )";
	for(%i = 0; %i < %this.numClients; %i++) {
		%index = %this.index[%i];
		%client = %this.client[%i];
		%letters = %this.letters[%i];
		centerPrint(%client, %centerPrint, 2);
		%char = getSubStr(%letters, %index, 1);
		if(%this.messageTime[%i]+3 <= $Sim::Time)
			%this.message[%i] = "";
		bottomPrint(%client, "\c3" @ %this.message[%i] @ "\n\c6" @ getSubStr(%letters, 0, %index) @ "\c0" @ %char @ "\c6" @ getSubStr(%letters, %index+1, strLen(%letters)) @
		 "\r\n\c6Letter worth: " @ $BG::Scrabble::Value[%char], 2);
	}
	%this.logicTick = %this.schedule(100, logicTick);
}
function ScrabbleSession::dumpBoard(%this) {
	for(%x = 0;%x < 15; %x++) {
		%str = "";
		for(%y = 0; %y < 15; %y++) {
			%str = %str@(%this.board[%x,%y] $= "" ? "_":%this.board[%x,%y]);
		}
		talk(%str);
	}
}
function ScrabbleSession::setup(%this, %brickGroup) {
	if(!isObject(%this.ScrabbleBag))
		%this.ScrabbleBag = new ScriptObject(ScrabbleBag);
	%this.ScrabbleBag.setup();
	%this.numClients = 0;
	%this.brickGroup = %brickGroup;
	%this.isFirstMove = true;
}
//%t = new ScriptObject(ScrabbleSession);%t.setup(%cl.brickGroup);%t.addPlayer(%cl);
function ScrabbleSession::updateRanking(%this) {
	for(%i = 0; %i < %this.numClients; %i++) {
		%this.rankScore[%i] = %this.score[%i];
		%this.rank[%i] = %i;
	}
	%count = 0;
	%s = true;
	while(%s && %count < 100) {
		%s = false;
		%count++;
		for(%i=1;%i<4;%i++) {
			if(%this.rankScore[%i-1] < %this.rankScore[%i]) {
				%ida = %this.rank[%i-1];
				%this.rank[%i-1] = %this.rank[%i];
				%this.rankScore[%i-1] = %this.rankScore[%i];
				%this.rank[%i] = %ida;
				%this.rankScore[%i] = %this.score[%ida];
				%s = true;
			}
		}
	}
}
function ScrabbleSession::addPlayer(%this, %client) {
	if(isObject(%client.ScrabbleSession)) {
		talk("Returning at line 163");
		return;
	}
	%client.ScrabbleSession = %this;
	%this.client[%this.slot[%client] = atoi(%this.numClients)] = %client;
	%this.pushLetters(7, atoi(%this.numClients));
	%this.numClients++;
	%this.updateRanking();
}
function ScrabbleSession::pushLetters(%this, %amount, %id) {
	for(%i = 0; %i < %amount; %i++) {
		%char = %this.ScrabbleBag.grabLetter();
		talk("Char: " @ %char);
		if(%char $= 0)
			break;
		%this.letters[%id] = %this.letters[%id] @ %char;
	}
}
function ScrabbleSession::pushLetter(%this, %letter, %id) {
	%this.letters[%id] = %this.letters[%id] @ %letter;
}
function ScrabbleSession::getLetter(%this, %id) {
	return getSubStr(%this.letters[%id], %this.index[%id], 1);
}
function ScrabbleSession::popLetter(%this, %id) {
	%i = %this.index[%id];
	%char = getSubStr(%this.letters[%id], %this.index[%id], 1);
	%this.letters[%id] = getSubStr(%this.letters[%id], 0, %this.index[%id])@getSubStr(%this.letters[%id],%this.index[%id]+1,strLen(%this.letters[%id]));
	return %char;
}
package scrabblePackage {
	function serverCmdPrevSeat(%this) {
		if(isObject(%session = %this.ScrabbleSession)) {
			%session.index[%session.slot[%this]]--;
			if(%session.index[%session.slot[%this]] <= 0) {
				%session.index[%session.slot[%this]] = 0;
			}
		}
		Parent::serverCmdPrevSeat(%this);
	}
	function serverCmdNextSeat(%this) {
		if(isObject(%session = %this.ScrabbleSession)) {
			%session.index[%session.slot[%this]]++;
			if(%session.index[%session.slot[%this]] >= (%len=strLen(%session.letters[%session.slot[%this]]))) {
				%session.index[%session.slot[%this]] = (%len-1);
			}
		}
		Parent::serverCmdNextSeat(%this);
	}
	function serverCmdShiftBrick(%this, %x, %y, %z) {
		if(isObject(%session = %this.ScrabbleSession) && %y != 0) {
			%session.index[%session.slot[%this]] -= %y > 0 ? 1:-1;
			if(%session.index[%session.slot[%this]] >= (%len=strLen(%session.letters[%session.slot[%this]]))) {
				%session.index[%session.slot[%this]] = (%len-1);
			} else if(%session.index[%session.slot[%this]] <= 0) {
				%session.index[%session.slot[%this]] = 0;
			}
		}
		Parent::servercmdShiftBrick(%this, %x, %y, %z);
	}
	function serverCmdSuperShiftBrick(%this, %x, %y, %z) {
		if(isObject(%session = %this.ScrabbleSession) && %y != 0) {
			%session.index[%session.slot[%this]] -= %y > 0 ? 1:-1;
			if(%session.index[%session.slot[%this]] >= (%len=strLen(%session.letters[%session.slot[%this]]))) {
				%session.index[%session.slot[%this]] = (%len-1);
			} else if(%session.index[%session.slot[%this]] <= 0) {
				%session.index[%session.slot[%this]] = 0;
			}
		}
		Parent::servercmdSuperShiftBrick(%this, %x, %y, %z);
	}
	function Player::activateStuff(%this) {
		Parent::activateStuff(%this);
		if(isObject(%session = %this.client.ScrabbleSession)) {
			%start = %this.getEyePoint();
			%end = vectorAdd(vectorScale(%this.getEyeVector(), 1000), %start);
			%ray = containerRaycast(%start, %end, $TypeMasks::FxBrickObjectType);
			%hit = firstWord(%ray);
			%slot = %session.slot[%this.client];

			if(!isObject(%hit))
				return;
			if(%hit.getGroup() != %session.brickGroup)
				return;
			if(%session.turn != %slot) {
				%session.message[%slot] = "Please wait your turn!";
				%session.messageTime[%slot] = $Sim::Time;
				return;
			}
			// Placement logic. (Temp)
			if(!isObject(%hit.item))
				%hit.setItem(WrenchItem);
			%name = %hit.getName();
			if(%name $= "")
				return;
			%name = getSubStr(%name, 1, strLen(%name));
			%posY = nextToken(%name, "posX", "_");
			%state = %session.board["STATE", %posX, %posY];
			if(%state == 2)
				return;
			else if(%state == 1) {
				%session.board["STATE", %posX, %posY] = 0;
				%char = %session.board["CHAR", %posX, %posY];
				%session.board["CHAR", %posX, %posY] = "";
				%hit.item.setShapeName(%posX @ "_" @ %posY @ "[ _ ]");
				%session.pushLetter(%char, %slot);
			} else {
				%session.board["STATE", %posX, %posY] = 1;
				%char = %session.board["CHAR", %posX, %posY] = %session.popLetter(%slot);
				%hit.item.setShapeName(%posX @ "_" @ %posY @ "[ " @ %char @ " ]");
				if(%session.index[%slot] >= strLen(%session.letters[%slot]))
					%session.index[%slot] = strLen(%session.letters[%slot])-1;
			}
		}
	}
	function serverCmdPlantBrick(%this) {
		Parent::serverCmdPlantBrick(%this);
		if(isObject(%session = %this.ScrabbleSession)) {
			%slot = %session.slot[%this.client];
			if(%session.turn != %slot)
				return;
		}
	}
};
activatePackage("scrabblePackage");

$exampleUTTT = new ScriptObject(UTTT) {
	name = "";
	player0 = ""; //red
	player1 = ""; //black
	turn = 0;
	bigGrid2 = "9 9 9";
	bigGrid1 = "9 9 9";
	bigGrid0 = "9 9 9";

	smallGrid00_2 = "9 9 9";
	smallGrid00_1 = "9 9 9";
	smallGrid00_0 = "9 9 9";

	smallGrid01_2 = "9 9 9";
	smallGrid01_1 = "9 9 9";
	smallGrid01_0 = "9 9 9";

	smallGrid02_2 = "9 9 9";
	smallGrid02_1 = "9 9 9";
	smallGrid02_0 = "9 9 9";

	smallGrid10_2 = "9 9 9";
	smallGrid10_1 = "9 9 9";
	smallGrid10_0 = "9 9 9";

	smallGrid11_2 = "9 9 9";
	smallGrid11_1 = "9 9 9";
	smallGrid11_0 = "9 9 9";

	smallGrid12_2 = "9 9 9";
	smallGrid12_1 = "9 9 9";
	smallGrid12_0 = "9 9 9";

	smallGrid20_2 = "9 9 9";
	smallGrid20_1 = "9 9 9";
	smallGrid20_0 = "9 9 9";

	smallGrid21_2 = "9 9 9";
	smallGrid21_1 = "9 9 9";
	smallGrid21_0 = "9 9 9";

	smallGrid22_2 = "9 9 9";
	smallGrid22_1 = "9 9 9";
	smallGrid22_0 = "9 9 9";

	//		0123456
	nextGrid = "00"; //## or "any"
	lastMovedBrick = "";
    turnTimer = "";

	brickGroup = Brickgroup_4928;
};

$UTTT::selectedColorFX = 4;
$UTTT::player0Color = 9;
$UTTT::player1Color = 14;
$UTTT::playerTieColor = 7;
$UTTT::timePerTurn = 70;
$UTTT::lastOffset = "-100 -40 0";
$UTTT::offsetAmount = "-30 0 0";

function startUltimateTicTacToeGame(%client0, %client1) {
	if (isObject(%client0.boardGame) || isObject(%client1.boardGame)) {
		talk("Cannot start board game: A client is in a boardgame already!");
		return;
	}

	%newGame = new ScriptObject(UTTT);
	$GameList.add(%newGame);

	%newGame.startGame(%client0, %client1);
}

function UTTT::doTurnTimer(%this, %client) {
	if (!isObject(%client)) {
		return;
	}
    %timeleft = $UTTT::timePerTurn - %this.secondsPassed;
    %min = mFloor(%timeleft/60);
    %client.bottomPrint("<font:Arial Bold:48><just:center>\c5 " @ getTimeString(%timeleft) @ " ", 1, 1);
    %this.secondsPassed++;

    if (%this.secondsPassed >= $UTTT::timePerTurn+1) {
        %client.bottomPrint("", 1, 1);
        %this.endGame((%this.player0 == %client ? 1 : 0), "");
        chatMessageAll('', "\c7" @ %client.name @ " took too long to make their turn!");
    } else if (!isEventPending(%this.turnTimer)) {
        %this.turnTimer = %this.schedule(1000, doTurnTimer, %client);
    }
    if ($UTTT::timePerTurn - %this.secondsPassed < 10) {
        %client.play2D(Synth_00_Sound);
    }
}

function UTTT::getSmallTile(%this, %bigTile, %smallTile) {
	if (strLen(%bigTile) != 2 || strLen(%smallTile) != 2) {
		echo("UTTT Error: Bad tile input: b" @ %bigTile @ " s" @ %smallTile);
		return;
	}
	return getWord(%this.smallGrid[%bigTile @ "_" @ getSubStr(%smallTile, 0, 1)], getSubStr(%smallTile, 1, 1));
}

function UTTT::getBigTile(%this, %bigTile) {
	if (strLen(%bigTile) != 2) {
		echo("UTTT Error: Bad tile input: b" @ %bigTile);
		return;
	}
	return getWord(%this.bigGrid[getSubStr(%bigTile, 0, 1)], getSubStr(%bigTile, 1, 1));
}

function UTTT::setTileAt(%this, %tileName, %player) { //tilename = "00_00"
	%bigTile = getSubStr(%tileName, 0, 2);
	%smallTile = getSubStr(%tileName, 3, 2);

	%suff = %bigTile @ "_" @ getSubStr(%smallTile, 0, 1);
	%orig = %this.smallGrid[%suff];
	%this.smallGrid[%suff] = setWord(%this.smallGrid[%suff], getSubStr(%smallTile, 1, 1), %player);

	%this.setTileColor(%bigTile, %smallTile, %player);
}

function UTTT::setTileColor(%this, %bigTile, %smallTile, %player) {
	%brick = %this.brickGroup.NTObject_[%bigTile @ "_" @ %smallTile @ "_0"];
	%brick.setEmitter(pongExplosionEmitter);
	%brick.playSound(brickPlantSound);

	if (isObject(%this.lastMovedBrick)) {
		%this.lastMovedBrick.setEmitter("");
	}
	%this.lastMovedBrick = %brick;

	if (%player == 0) {
		%brick.setColor($UTTT::player0Color);
		return 0;
	} else if (%player == 1) {
		%brick.setColor($UTTT::player1Color);
		return 1;
	}
}

function UTTT::setBigTile(%this, %bigTile, %player) {
	%orig = %this.bigGrid[getSubStr(%bigTile, 0, 1)];
	%this.bigGrid[getSubStr(%bigTile, 0, 1)] = setWord(%orig, getSubStr(%bigTile, 1, 1), %player);
	return %player;
}

function UTTT::setBigTileColorFX(%this, %which, %fx) {
	if (%which $= "all") {
		for (%i = 0; %i < 3; %i++) {
			for (%j = 0; %j < 3; %j++) {
				if (%this.getBigTile(%i @ %j) == 9) {
					%this.setBigTileColorFX(%i @ %j, %fx);
				}
			}
		}
	} else {
		for (%i = 0; %i < %this.brickGroup.NTObjectCount_[%which @ "_border"]; %i++) {
			%this.brickGroup.NTObject_[%which @ "_border_" @ %i].setColorFX(%fx);
		}
	}
}

function UTTT::setBigTileColor(%this, %which, %id) {
	if (%which $= "all") {
		for (%i = 0; %i < 3; %i++) {
			for (%j = 0; %j < 3; %j++) {
				if (%this.getBigTile(%i @ %j) == 9) {
					%this.setBigTileColor(%i @ %j, %id);
				}
			}
		}
	} else {
		for (%i = 0; %i < %this.brickGroup.NTObjectCount_[%which @ "_border"]; %i++) {
			%this.brickGroup.NTObject_[%which @ "_border_" @ %i].setColor(%id);
		}
	}
}

function UTTT::checkSmallBoardWin(%this, %bigTile) {
	//horiz, vert, diag
	for (%i = 0; %i < 3; %i++) {
		%player0 = 0;
		%player1 = 0;
		for (%j = 0; %j < 3; %j++) {
			%player[%this.getSmallTile(%bigTile, %i @ %j)]++;
		}
		if (%player0 == 3 || %player1 == 3) {
			%this.setBigTile(%bigTile, %player1 > %player0);
			%this.setBigTileColor(%bigTile, $UTTT::player[(%player1 > %player0) @ "Color"]);
			return %player1 > %player0;
		}
	}

	for (%i = 0; %i < 3; %i++) {
		%player0 = 0;
		%player1 = 0;
		for (%j = 0; %j < 3; %j++) {
			%player[%this.getSmallTile(%bigTile, %j @ %i)]++;
		}
		if (%player0 == 3 || %player1 == 3) {
			%this.setBigTile(%bigTile, %player1 > %player0);
			%this.setBigTileColor(%bigTile, $UTTT::player[(%player1 > %player0) @ "Color"]);
			return %player1 > %player0;
		}
	}

	%player0 = 0;
	%player1 = 0;
	for (%i = 0; %i < 3; %i++) {
		%player[%this.getSmallTile(%bigTile, %i @ (2 - %i))]++;		
	}
	if (%player0 == 3 || %player1 == 3) {
		%this.setBigTile(%bigTile, %player1 > %player0);
		%this.setBigTileColor(%bigTile, $UTTT::player[(%player1 > %player0) @ "Color"]);
		return %player1 > %player0;
	}

	%player0 = 0;
	%player1 = 0;
	for (%i = 0; %i < 3; %i++) {
		%player[%this.getSmallTile(%bigTile, %i @ %i)]++;		
	}
	if (%player0 == 3 || %player1 == 3) {
		%this.setBigTile(%bigTile, %player1 > %player0);
		%this.setBigTileColor(%bigTile, $UTTT::player[(%player1 > %player0) @ "Color"]);
		return %player1 > %player0;
	}

	if (%player9 == 0){ //tie
		%this.setBigTile(%bigTile, 2);
		%this.setBigTileColor(%bigTile, $UTTT::playerTieColor);
		return 3;
	} else {
		return -1;
	}
}

function UTTT::checkBigBoardWin(%this) {
	for (%i = 0; %i < 3; %i++) {
		%player0 = 0;
		%player1 = 0;
		for (%j = 0; %j < 3; %j++) {
			%player[%this.getBigTile(%i @ %j)]++;
		}
		if (%player0 == 3 || %player1 == 3) {
			return %player1 > %player0;
		}
	}

	for (%i = 0; %i < 3; %i++) {
		%player0 = 0;
		%player1 = 0;
		for (%j = 0; %j < 3; %j++) {
			%player[%this.getBigTile(%j @ %i)]++;
		}
		if (%player0 == 3 || %player1 == 3) {
			return %player1 > %player0;
		}
	}

	%player0 = 0;
	%player1 = 0;
	for (%i = 0; %i < 3; %i++) {
		%player[%this.getBigTile(%i @ (2 - %i))]++;		
	}
	if (%player0 == 3 || %player1 == 3) {
		return %player1 > %player0;
	}

	%player0 = 0;
	%player1 = 0;
	for (%i = 0; %i < 3; %i++) {
		%player[%this.getBigTile(%i @ %i)]++;		
	}
	if (%player0 == 3 || %player1 == 3) {
		return %player1 > %player0;
	}

	if (%player9 == 0){ //tie
		return 3;
	} else {
		return -1;
	}
}

function UTTT::endGame(%this, %winCondition) {
	if (%winCondition == 3) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 and \c3" @ %this.player1.name @ "\c5 have tied at \c3Ultimate Tic Tac Toe\c5!");
		%this.player0.UTTTTies++;
		%this.player1.UTTTTies++;
		$Pref::Server::BoardGames::UTTTTies += 2;
	} else if (%winCondition == 1) {
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 has beaten \c3" @ %this.player0.name @ "\c5 at \c3Ultimate Tic Tac Toe\c5!");
		%this.player0.UTTTLosses++;
		%this.player1.UTTTWins++;
		$Pref::Server::BoardGames::UTTTWins++;
		$Pref::Server::BoardGames::UTTTLosses++;
	} else if (%winCondition == 0) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 has beaten \c3" @ %this.player1.name @ "\c5 at \c3Ultimate Tic Tac Toe\c5!");
		%this.player0.UTTTWins++;
		%this.player1.UTTTLosses++;
		$Pref::Server::BoardGames::UTTTWins++;
		$Pref::Server::BoardGames::UTTTLosses++;
	} else if (%winCondition < 0){
		chatMessageAll('', "\c3" @ %this.player1.name @ " and " @ %this.player0.name @ " have quit their \c3Ultimate Tic Tac Toe game.");
	}

	%this.displayWin(%winCondition);

	%this.hasEnded = 1;

	cancel(%this.turnTimer);

	%this.player0.inBoardGame = false;
	%this.player1.inBoardGame = false;

	%this.player0.boardGameSpawn = "";
	%this.player1.boardGameSpawn = "";

	%this.schedule(13000, postEndGame);
}

function UTTT::postEndGame(%this) {
    %this.player0.spawnPlayer();
    %this.player1.spawnPlayer();
	%this.player0.boardGame = "";
	%this.player1.boardGame = "";
	%this.player0 = "";
	%this.player1 = "";
	chainKillBrickGroup(%this.brickGroup, %this.brickGroup.getCount());
	%this.delete();
}

function UTTT::displayWin(%this, %winCondition) {
	for (%i = 0; %i < 3; %i++) {
		for (%j = 0 ; %j < 3; %j++) {
			if (%this.getBigTile(%i @ %j) == %winCondition) {
				%this.setBigTileColorFX(%i @ %j, $UTTT::selectedColorFX);
			} else {
				%this.setBigTileColorFX(%i @ %j, 0);
			}
		}
	}
}

function UTTT::getOffset(%this) {
	$UTTT::lastOffset = vectorAdd($UTTT::lastOffset, $UTTT::offsetAmount);
	return vectorSub($UTTT::lastOffset, $UTTT::offsetAmount);
}

function UTTT::startGame(%this, %client0, %client1) {
	if (getRandom(0, 1)) {
		%tempclient = %client0;
		%client0 = %client1;
		%client1 = %tempClient;
	}

	%this.name = %client0.name @ " vs " @ %client1.name @ " UTTT Game";
	%this.player0 = %client0; //red
	%this.player1 = %client1; //black
	%this.turn = 0;
	%this.bigGrid2 = "9 9 9";
	%this.bigGrid1 = "9 9 9";
	%this.bigGrid0 = "9 9 9";

	%this.smallGrid00_2 = "9 9 9";
	%this.smallGrid00_1 = "9 9 9";
	%this.smallGrid00_0 = "9 9 9";

	%this.smallGrid01_2 = "9 9 9";
	%this.smallGrid01_1 = "9 9 9";
	%this.smallGrid01_0 = "9 9 9";

	%this.smallGrid02_2 = "9 9 9";
	%this.smallGrid02_1 = "9 9 9";
	%this.smallGrid02_0 = "9 9 9";

	%this.smallGrid10_2 = "9 9 9";
	%this.smallGrid10_1 = "9 9 9";
	%this.smallGrid10_0 = "9 9 9";

	%this.smallGrid11_2 = "9 9 9";
	%this.smallGrid11_1 = "9 9 9";
	%this.smallGrid11_0 = "9 9 9";

	%this.smallGrid12_2 = "9 9 9";
	%this.smallGrid12_1 = "9 9 9";
	%this.smallGrid12_0 = "9 9 9";

	%this.smallGrid20_2 = "9 9 9";
	%this.smallGrid20_1 = "9 9 9";
	%this.smallGrid20_0 = "9 9 9";

	%this.smallGrid21_2 = "9 9 9";
	%this.smallGrid21_1 = "9 9 9";
	%this.smallGrid21_0 = "9 9 9";

	%this.smallGrid22_2 = "9 9 9";
	%this.smallGrid22_1 = "9 9 9";
	%this.smallGrid22_0 = "9 9 9";

	//		0123456
	%this.nextGrid = "all"; //## or "any"
	%this.lastMovedBrick = "";
    %this.turnTimer = "";

    %this.secondsPassed = 0;

	%id = getNextGameBrickgroupID();

	while (!%hasLoaded) {
		if (isObject(%this.brickGroup)) {
			%this.brickGroup.chainDeleteAll();
			%this.brickGroup.delete();
		}
		%this.brickGroup = new SimGroup("Brickgroup_" @ %id) {
			name = %this.name;
			bl_id = %id;
		};
		mainBrickGroup.add(%this.brickGroup);
		%hasLoaded = loadBuild(%this.getOffset(), "UTTT", %this.brickGroup);
	}
	echo("Loaded build: brickcount " @ %this.brickGroup.getCount());
	echo("    Spawn0: " @ %this.brickGroup.NTObject_spawn0_0);
	echo("    Spawn1: " @ %this.brickGroup.NTObject_spawn1_0);

	%client0.boardGame = %this;
	%client1.boardGame = %this;

	%this.schedule(1000, postStartGame, %client0, %client1);
}

function UTTT::postStartGame(%this, %client0, %client1) {
    %spawn0 = %this.brickGroup.NTObject_spawn0_0;
    %spawn1 = %this.brickGroup.NTObject_spawn1_0;

    %client0.boardGameSpawn = %spawn0.getSpawnPoint();
    %client1.boardGameSpawn = %spawn1.getSpawnPoint();
    %client0.inBoardGame = true;
    %client1.inBoardGame = true;
    %client0.spawnPlayer();
    %client1.spawnPlayer();
    %client0.player.setWhiteout(0.9);
    %client1.player.setWhiteout(0.9);

    %client0.centerprint("\c4You get the first turn!", 8);  
    %client1.centerprint("\c4" @ %client0.name @ " gets the first turn!", 8);

    %this.doTurnTimer(%client0);
    %this.setBigTileColorFX("all", $UTTT::selectedColorFX);
}

function UTTT::takeTurn(%this, %col, %client) {
	%tileName = getSubStr(%col.getName(), 1, strlen(%col.getName()));
	if (%client == %this.player0) {
		%player = 0;
	} else if (%client == %this.player1) {
		%player = 1;
	} else {
		return;
	}
	if (stripChars(%tileName, "012") !$= "_" || strLen(%tileName) != 5) {
		return;
	}

	%bigTile = getSubStr(%tileName, 0, 2);
	%smallTile = getSubStr(%tileName, 3, 2);

	if (%this.turn != %player) {
		%client.centerprint("\c3It's not your turn! Please wait for your opponent.", 3);
		return -1;
	} else if (%this.getBigTile(%bigTile) != 9) {
		%client.centerprint("\c3This board is already taken!", 6);
		return;
	} else if (%this.nextGrid !$= %bigTile && %this.nextGrid !$= "all") {
		%client.centerprint("\c3You can't play in this board!", 6);
		return;
	} else if (%this.getSmallTile(%bigTile, %smallTile) != 9) {
		%client.centerprint("\c3This tile is already taken!", 6);
		return;
	} 

    if (isEventPending(%this.turnTimer)) {
        cancel(%this.turnTimer);  
    }

	%this.setTileAt(%tileName, %player);

	if (%this.checkSmallBoardWin(%bigTile) >= 0) {
		serverPlay3D(rewardSound, %this.brickGroup.NTObject_[%bigTile @ "_" @ %smallTile @ "_0"].getPosition());
		%winType = %this.checkBigBoardWin();
		if (%winType >= 0) {
			%this.endGame(%winType);
			return;
		}
	}

	//show big tiles that can be played
	%this.setBigTileColorFX("all", 0);
	%this.setBigTileColorFX(%bigTile, 0);

	if (%this.getBigTile(%smallTile) == 9) {
		%this.setBigTileColorFX(%smallTile, $UTTT::selectedColorFX);
		%this.nextGrid = %smallTile;
	} else {
		%this.setBigTileColorFX("all", $UTTT::selectedColorFX);
		%this.nextGrid = "all";
	}

    %client.bottomPrint("", 1, 1);
    %client.centerPrint("", 1, 1);

	%this.turn = (%this.turn + 1) % 2;
	%this.player[%this.turn].centerprint("<font:Palatino Linotype:40>\c3It's your turn!", 20);
	messageClient(%this.player[%this.turn], 'MsgUploadEnd', '');
    %this.secondsPassed = 0;
    %this.turnTimer = %this.doTurnTimer(%this.player[%this.turn]);
}
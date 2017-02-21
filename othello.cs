$exampleOthello = new ScriptObject(Othello) {
	name = "";
	player0 = ""; //red
	player1 = ""; //black
	turn = 0;
	row7 = "99999999";
	row6 = "99999999";
	row5 = "99999999";
	row4 = "99910999";
	row3 = "99901999";
	row2 = "99999999";
	row1 = "99999999";
	row0 = "99999999";
	//		01234567
	lastMove = "";
	lastColoredChip = "";

	brickGroup = "";
};

$Othello::highlightColor = 1;
$Othello::highlightFX = 4;
$Othello::defaultColor = 2;
$Othello::player1brick = brickOthelloWhite6x6Data;
$Othello::player0brick = brickOthelloBlack6x6Data;
$Othello::timePerTurn = 50;
$Othello::lastoffset = "-100 -80 0";
$Othello::offsetAmount = "-40 0 0";


function Othello::getChipAt(%this, %col, %row) {
	if (%col < 0 || %col > 7 || %row < 0 || %row > 7) {
		return -1;
	}
	return getSubStr(%this.row[%row], %col, 1);
}

function Othello::setChipAt(%this, %col, %row, %val) {
	if (%col < 0 || %col > 7 || %row < 0 || %row > 7) {
		return -1;
	} else if (%val != 0 && %val != 1 && %val != 9) {
		return -1;
	}
	if (%val $= "") {
		talk("CRITICAL ERROR");
	}
	%orig = %this.row[%row];
	%ret = getSubStr(%orig, 0, %col) @ %val @ getSubStr(%orig, %col+1, 8);
	%this.row[%row] = %ret;
	if (strLen(%orig) != strLen(%ret)) {
		talk("CRITICAL ERROR");
		talk(%orig);
		talk(%ret);
	}

	%this.setColorAt(%col, %row, %val);

	return %val;
}

function Othello::setColorAt(%this, %col, %row, %val) {
	//ntobject_row00_0
	%brick = %this.brickGroup.NTObject_[%row @ %col @ "_0"];
	if (!isObject(%Brick)) {
		echo("Othello Bad Setcolor: " @ %row SPC %col);
	}

	if (%val == 0) {
		%brick.setDatablock($Othello::player0brick);
		return 0;
	} else if (%val == 1) {
		%brick.setDatablock($Othello::player1brick);
		return 1;
	} else {
		%brick.setDatablock(brickOthelloEmpty6x6Data);
		return 9;
	}
}

function concatenateMoves(%move1, %move2) {
	%move2 = trim(strReplace(%move2, "  ", " "));
	%wordCount = getWordCount(%move2);
	for (%i = 0; %i < %wordCount; %i++) {
		if (strPos(%move1, getWord(%move2, %i)) < 0) {
			%move1 = %move1 SPC getWord(%move2, %i);
		}
	}
	return %move1;
}

function Othello::getPossibleMoves(%this, %col, %row) {
	%player = %this.getChipAt(%col, %row);
	//iterate until hit board edge or empty space or friendly piece
	//horiz
	%moves = "";

	%enemyCount = 0;
	for(%i = %col + 1; %i < 8; %i++) {
		%chip = %this.getChipAt(%i, %row);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC %row @ %i;
			//echo("1. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}
	echo("1. " @ %moves @ " -- " @ %i SPC %enemyCount);

	%enemyCount = 0;
	for(%i = %col - 1; %i >= 0; %i--) {
		%chip = %this.getChipAt(%i, %row);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC %row @ %i;
			//echo("2. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}
	echo("1. " @ %moves @ " -- " @ %i SPC %enemyCount);

	//vert
	%enemyCount = 0;
	for(%i = %row + 1; %i < 8; %i++) {
		%chip = %this.getChipAt(%col, %i);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC %i @ %col;
			//echo("3. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}

	%enemyCount = 0;
	for(%i = %row - 1; %i >= 0; %i--) {
		%chip = %this.getChipAt(%col, %i);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC %i @ %col;
			//echo("4. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}

	//diag
	%enemyCount = 0;
	for(%i = 1; %i < 8; %i++) {
		%chip = %this.getChipAt(%col + %i, %row + %i);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC (%row + %i) @ (%col + %i);
			//echo("5. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}
	%enemyCount = 0;
	for(%i = 1; %i < 8; %i++) {
		%chip = %this.getChipAt(%col - %i, %row - %i);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC (%row - %i) @ (%col - %i) ;
			//echo("6. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}

	%enemyCount = 0;
	for(%i = 1; %i < 8; %i++) {
		%chip = %this.getChipAt(%col + %i, %row - %i);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC (%row - %i) @ (%col + %i);
			//echo("7. got move, breaking..." @ %moves);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}
	%enemyCount = 0;
	for(%i = 1; %i < 8; %i++) {
		%chip = %this.getChipAt(%col - %i, %row + %i);
		if (%chip == 9) {
			if (%enemyCount == 0) {
				break;
			}
			%moves = %moves SPC (%row + %i) @ (%col - %i);
			break;
		} else if (%chip < 0 || %chip == %player) {
			break;
		}
		%enemyCount++;
	}
	echo(%row @ %col @ " : " @ %moves);

	return %moves;
}

function Othello::getAllPossibleMoves(%this) {
	for (%i = 0; %i < 8; %i++) {
		for (%j = 0; %j < 8; %j++) {
			if (%this.turn == %this.getChipAt(%i, %j)) {
				%moves = concatenateMoves(%moves, %this.getPossibleMoves(%i, %j));
			}
		}
	}
	//messageClient(fcn(conan), '', %moves);
	return %moves;
}

function Othello::setMoveDisplay(%this, %moves) {
	if (%moves !$= "clear") {
		for (%i = 0; %i < getWordCount(%moves); %i++) {
			%brick = %this.brickGroup.NTObject_[getWord(%moves, %i) @ "_0"];
			if (!isObject(%brick) || %brick $= "") {
				echo("Othello Bad Move: " @ getWord(%moves, %i));
			}
			%brick.setColor($Othello::highlightColor);
			%brick.setColorFX($Othello::highlightFX);
		}
	} else {
		for (%i = 0; %i < 8; %i++) {
			for (%j = 0; %j < 8; %j++) {
				%brick = %this.brickGroup.NTObject_[%i @ %j @ "_0"];
				%brick.setColor($Othello::defaultColor);
				%brick.setColorFX(0);		
			}
		}
	}

	return %moves;
}

function Othello::takeTurn(%this, %col, %client) {
	%brickName = getSubStr(%col.getName(), 1, strlen(%col.getName()));
	if (%client == %this.player0) {
		%player = 0;
	} else if (%client == %this.player1) {
		%player = 1;
	} else {
		return -1;
	}

	%moves = %this.nextPossibleMoves;
	%row = getSubStr(%brickName, 0, 1);
	%col = getSubStr(%brickName, 1, 1);
	if (stripChars(%brickName, "01234567") !$= "" || strLen(%brickName) != 2) {
		return -1;
	} else if (%this.turn != %player) {
		%client.centerprint("\c3It's not your turn! Please wait for your opponent.", 3);
		return -1;
	} else if (%this.getChipAt(%col, %row) != 9) {
		%client.centerprint("\c3This space is already occupied!", 3);
		return -1;
	} else if (strPos(%moves, %brickname) < 0) {
		%client.centerprint("\c3This is not a valid place to move!", 3);
		return -1;
	}

    if (isEventPending(%this.turnTimer)) {
        cancel(%this.turnTimer);  
    }

    %this.setChipAt(%col, %row, %player);
    %this.doFlips(%col, %row, %player);
    %this.setMoveDisplay("clear");

    %client.bottomPrint("", 1, 1);
    %client.centerPrint("", 1, 1);

    %prePossibleMoves = trim(%this.getAllPossibleMoves());

    %this.turn = (%this.turn + 1) % 2;
    %this.nextPossibleMoves = trim(%this.getAllPossibleMoves());
    //echo(%this.nextPossibleMoves);

    if (%this.nextPossibleMoves $= "" && %prePossibleMoves $= "") {
    	%this.endGame(%this.getWinner());
    	return;
    } else if (%this.nextPossibleMoves $= "") {
		%this.player[%this.turn].centerprint("<font:Palatino Linotype:40>\c3You have no valid moves - your opponent gets an extra turn!", 20);
		%client.centerprint("<font:Palatino Linotype:40>\c3Your opponent has no valid moves - you get an extra turn!", 20);
		messageClient(%this.player[%this.turn], 'MsgUploadEnd', '');
		messageClient(%client, 'MsgUploadEnd', '');

    	%this.setMoveDisplay(%prePossibleMoves);
    	%this.nextPossibleMoves = trim(%prePossibleMoves);
		
		%this.turn = (%this.turn + 1) % 2;
    	%this.turnTimer = %this.doTurnTimer(%client);

	    %this.secondsPassed = 0;
		return;
    }
    %this.setMoveDisplay(%this.nextPossibleMoves);

	%this.player[%this.turn].centerprint("<font:Palatino Linotype:40>\c3It's your turn!", 20);
	messageClient(%this.player[%this.turn], 'MsgUploadEnd', '');
    %this.turnTimer = %this.doTurnTimer(%this.player[%this.turn]);

    %this.secondsPassed = 0;
}

function Othello::getWinner(%this) {
	%player0 = 0;
	%player1 = 0;
	for (%i = 0; %i < 8; %i++) {
		for (%j = 0; %j < 8; %j++) {
			%player[%this.getChipAt(%j, %i)]++;
		}
	}

	%this.player0Count = %player0;
	%this.player1Count = %player1;

	if (%player0 == %player1) {
		return 3;
	} else {
		return %player1 > %player0;
	}
}

function Othello::doFlips(%this, %col, %row) {
	%brick = %this.brickGroup.NTObject_[%row @ %col @ "_0"];
	%brick.setEmitter(pongExplosionEmitter);
	%brick.playSound(brickPlantSound);

	if (isObject(%this.lastColoredChip)) {
		%this.lastColoredChip.setEmitter("");
	}
	%this.lastColoredChip = %brick;
	%flipTargets = trim(%this.getFlips(%col, %row));

	for (%i = 0; %i < getWordCount(%flipTargets); %i++) {
		%word = getWord(%flipTargets, %i);
		%r = getSubStr(%word, 0, 1);
		%c = getSubStr(%word, 1, 1);
		if (%this.getChipAt(%c, %r) == (%this.turn + 1) % 2) {
			%this.setChipAt(%c, %r, %this.turn);
		}
	}
}

function Othello::getFlips(%this, %col, %row) {
	%player = %this.getChipAt(%col, %row);
	//iterate until hit board edge or empty space or friendly piece
	//horiz
	%enemyDiscs = "";

	%enemyCount = "";
	for(%i = %col + 1; %i < 9; %i++) {
		%chip = %this.getChipAt(%i, %row);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC %row @ %i;
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	%enemyCount = "";
	for(%i = %col - 1; %i >= -1; %i--) {
		%chip = %this.getChipAt(%i, %row);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC %row @ %i;
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	//vert
	%enemyCount = "";
	for(%i = %row + 1; %i < 9; %i++) {
		%chip = %this.getChipAt(%col, %i);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC %i @ %col;
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	%enemyCount = "";
	for(%i = %row - 1; %i >= -1; %i--) {
		%chip = %this.getChipAt(%col, %i);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC %i @ %col;
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	//diag
	%enemyCount = "";
	for(%i = 1; %i < 9; %i++) {
		%chip = %this.getChipAt(%col + %i, %row + %i);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC (%row + %i) @ (%col + %i);
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	%enemyCount = "";
	for(%i = 1; %i < 9; %i++) {
		%chip = %this.getChipAt(%col - %i, %row - %i);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC (%row - %i) @ (%col - %i);
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	%enemyCount = "";
	for(%i = 1; %i < 9; %i++) {
		%chip = %this.getChipAt(%col + %i, %row - %i);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC (%row - %i) @ (%col + %i);
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	%enemyCount = "";
	for(%i = 1; %i < 9; %i++) {
		%chip = %this.getChipAt(%col - %i, %row + %i);
		if (%chip == (%player + 1) % 2) {
			%enemyCount = %enemyCount SPC (%row + %i) @ (%col - %i);
		} else if (%chip < 0 || %chip == 9) {
			%enemyCount = "";
			break;
		} else if (%chip == %player) {
			break;
		}
	}
	%enemyDiscs = %enemyDiscs @ %enemyCount;

	return %enemyDiscs;
}

function Othello::endGame(%this, %winCondition) {
	if (%winCondition == 3) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 and \c3" @ %this.player1.name @ "\c5 have tied at \c3Othello\c5! (" @ %this.player0Count @ " to " @ %this.player1Count @ ")");
		%this.player0.OthelloTies++;
		%this.player1.OthelloTies++;
		$Pref::Server::BoardGames::OthelloTies += 2;
	} else if (%winCondition == 1) {
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 has beaten \c3" @ %this.player0.name @ "\c5 at \c3Othello\c5! (" @ %this.player1Count @ " to " @ %this.player0Count @ ")");
		%this.player0.OthelloLosses++;
		%this.player1.OthelloWins++;
		$Pref::Server::BoardGames::OthelloWins++;
		$Pref::Server::BoardGames::OthelloLosses++;
	} else if (%winCondition == 0) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 has beaten \c3" @ %this.player1.name @ "\c5 at \c3Othello\c5! (" @ %this.player0Count @ " to " @ %this.player1Count @ ")");
		%this.player0.OthelloWins++;
		%this.player1.OthelloLosses++;
		$Pref::Server::BoardGames::OthelloWins++;
		$Pref::Server::BoardGames::OthelloLosses++;
	} else if (%winCondition < 0){
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 and \c3" @ %this.player0.name @ "\c5 have quit their \c3Othello\c5 game.");
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

function Othello::doTurnTimer(%this, %client) {
	if (!isObject(%client)) {
		return;
	}
    %timeleft = $Othello::timePerTurn - %this.secondsPassed;
    %min = mFloor(%timeleft/60);
    %client.bottomPrint("<font:Arial Bold:48><just:center>\c5 " @ getTimeString(%timeleft) @ " ", 1, 1);
    %this.secondsPassed++;

    if (%this.secondsPassed >= $Othello::timePerTurn+1) {
        %client.bottomPrint("", 1, 1);
        %this.endGame((%this.player0 == %client ? 1 : 0), "");
        chatMessageAll('', "\c7" @ %client.name @ " took too long to make their turn!");
    } else if (!isEventPending(%this.turnTimer)) {
        %this.turnTimer = %this.schedule(1000, doTurnTimer, %client);
    }
    if ($Othello::timePerTurn - %this.secondsPassed < 10) {
        %client.play2D(Synth_00_Sound);
    }
}

function Othello::postEndGame(%this) {
    %this.player0.spawnPlayer();
    %this.player1.spawnPlayer();
	%this.player0.boardGame = "";
	%this.player1.boardGame = "";
	%this.player0 = "";
	%this.player1 = "";
	chainKillBrickGroup(%this.brickGroup, %this.brickGroup.getCount());
	%this.delete();
}

function Othello::startGame(%this, %client0, %client1) {
	if (getRandom(0, 1)) {
		%tempclient = %client0;
		%client0 = %client1;
		%client1 = %tempClient;
	}

	%this.name = %client0.name @ " vs " @ %client1.name @ " Othello Game";
	%this.player0 = %client0; //black
	%this.player1 = %client1; //white
	%this.turn = 0;
	%this.row7 = "99999999";
	%this.row6 = "99999999";
	%this.row5 = "99999999";
	%this.row4 = "99910999";
	%this.row3 = "99901999";
	%this.row2 = "99999999";
	%this.row1 = "99999999";
	%this.row0 = "99999999";
	//		01234567
	%this.lastMove = "";
	%this.lastColoredChip = "";

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
		%hasLoaded = loadBuild(%this.getOffset(), "Othello", %this.brickGroup);
	}
	echo("Loaded build: brickcount " @ %this.brickGroup.getCount());
	echo("    Spawn0: " @ %this.brickGroup.NTObject_spawn0_0);
	echo("    Spawn1: " @ %this.brickGroup.NTObject_spawn1_0);

	%client0.boardGame = %this;
	%client1.boardGame = %this;

	%this.schedule(1000, postStartGame, %client0, %client1);
}

function Othello::postStartGame(%this, %client0, %client1) {
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
    %this.nextPossibleMoves = %this.getAllPossibleMoves();
    %this.setMoveDisplay(%this.nextPossibleMoves);
    echo(%this.nextPossibleMoves);
}

function startOthelloGame(%client0, %client1) {
	if (isObject(%client0.boardGame) || isObject(%client1.boardGame)) {
		echo("Cannot start board game: A client is in a boardgame already!");
		return;
	}

	%newGame = new ScriptObject(Othello);
	$GameList.add(%newGame);

	%newGame.startGame(%client0, %client1);
}

function Othello::getOffset(%this) {
	$Othello::lastOffset = vectorAdd($Othello::lastOffset, $Othello::offsetAmount);
	return vectorSub($Othello::lastOffset, $Othello::offsetAmount);
}
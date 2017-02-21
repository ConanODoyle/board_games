$exampleConnect4 = new ScriptObject(Connect4) {
	name = "";
	player0 = ""; //red
	player1 = ""; //black
	turn = 0;
	row5 = "9999999";
	row4 = "9999999";
	row3 = "9999999";
	row2 = "9999999";
	row1 = "9999999";
	row0 = "9999999";
	//		0123456
	lastMove = 5;
	lastColoredChip = "";
    turnTimer = "";

	brickGroup = "";
};

$Connect4::redColorID = 0;
$Connect4::blackColorID = 16;
$Connect4::transColorID = 17;
$Connect4::lastOffset = "-100 0 0";
$Connect4::offsetAmount = "-40 0 0";
$Connect4::timePerTurn = 40;

function startConnect4Game(%client0, %client1) {
	if (isObject(%client0.boardGame) || isObject(%client1.boardGame)) {
		talk("Cannot start board game: A client is in a boardgame already!");
		return;
	}

	%newGame = new ScriptObject(Connect4);
	$GameList.add(%newGame);

	%newGame.startGame(%client0, %client1);
}

function Connect4::getChipAt(%this, %col, %row) {
	if (%col < 0 || %col > 6 || %row < 0 || %row > 5) {
		return -1;
	}
	return getSubStr(%this.row[%row], %col, 1);
}

function Connect4::setChipAt(%this, %col, %row, %val) {
	echo("Setting Chip at " @ %col SPC %row @ " to " @ %val);
	if (%col < 0 || %col > 6 || %row < 0 || %row > 5) {
		return -1;
	} else if (%val != 0 && %val != 1 && %val != 9) {
		return -1;
	}
	%orig = %this.row[%row];
	%ret = getSubStr(%orig, 0, %col) @ %val @ getSubStr(%orig, %col+1, 6);
	%this.row[%row] = %ret;
	echo("Changed " @ %orig);
	echo("     to " @ %ret);
	echo("for board " @ %this.name);

	%this.setColorAt(%col, %row, %val);

	return %val;
}

function Connect4::setColorAt(%this, %col, %row, %val) {
	//ntobject_row00_0
	%brick = %this.brickGroup.NTObject_row[%row @ %col @ "_0"];
	%brick.setEmitter(pongExplosionEmitter);
	%brick.playSound(brickPlantSound);

	if (isObject(%this.lastColoredChip)){
		%this.lastColoredChip.setEmitter("");
	}
	%this.lastColoredChip = %brick;
	if (%val == 0) {
		%brick.setColor($Connect4::redColorID);
		return 0;
	} else if (%val == 1) {
		%brick.setColor($Connect4::blackColorID);
		return 1;
	} else {
		%brick.setColor($Connect4::transColorID);
		return 9;
	}
}

function Connect4::getOffset(%this) {
	$Connect4::lastOffset = vectorAdd($Connect4::lastOffset, $Connect4::offsetAmount);
	return vectorSub($Connect4::lastOffset, $Connect4::offsetAmount);
}

function Connect4::startGame(%this, %client0, %client1) {
	if (getRandom(0, 1)) {
		%tempclient = %client0;
		%client0 = %client1;
		%client1 = %tempClient;
	}

	%this.name = %client0.name @ " vs " @ %client1.name @ " Connect4 Game";
	
	%this.player0 = %client0;
	%this.player1 = %client1;
	%this.turn = 0;
	%this.row5 = "9999999";
	%this.row4 = "9999999";
	%this.row3 = "9999999";
	%this.row2 = "9999999";
	%this.row1 = "9999999";
	%this.row0 = "9999999";
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
		%hasLoaded = loadBuild(%this.getOffset(), "Connect4", %this.brickGroup);
	}
	echo("Loaded build: brickcount " @ %this.brickGroup.getCount());
	echo("    Spawn0: " @ %this.brickGroup.NTObject_spawn0_0);
	echo("    Spawn1: " @ %this.brickGroup.NTObject_spawn1_0);

	%client0.boardGame = %this;
	%client1.boardGame = %this;

	%this.schedule(1000, postStartGame, %client0, %client1);
}

function Connect4::postStartGame(%this, %client0, %client1) {
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
}

function Connect4::doTurnTimer(%this, %client) {
	if (!isObject(%client)) {
		return;
	}
    %timeleft = $Connect4::timePerTurn - %this.secondsPassed;
    %min = mFloor(%timeleft/60);
    %client.bottomPrint("<font:Arial Bold:48><just:center>\c5 " @ getTimeString(%timeleft) @ " ", 1, 1);
    %this.secondsPassed++;

    if (%this.secondsPassed >= $Connect4::timePerTurn+1) {
        %client.bottomPrint("", 1, 1);
        %this.endGame((%this.player0 == %client ? 1 : 0), "");
        chatMessageAll('', "\c7" @ %client.name @ " took too long to make their turn!");
    } else if (!isEventPending(%this.turnTimer)) {
        %this.turnTimer = %this.schedule(1000, doTurnTimer, %client);
    }
    if ($Connect4::timePerTurn - %this.secondsPassed < 10) {
        %client.play2D(Synth_00_Sound);
    }
}

function Connect4::endGame(%this, %winCondition, %winParams) {
	if (%winCondition == 3) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 and \c3" @ %this.player1.name @ "\c5 have tied at \c3Connect4\c5!");
		%this.player0.Connect4Ties++;
		%this.player1.Connect4Ties++;
		$Pref::Server::BoardGames::Connect4Ties += 2;
	} else if (%winCondition == 1) {
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 has beaten \c3" @ %this.player0.name @ "\c5 at \c3Connect4\c5!");
		%this.player0.Connect4Losses++;
		%this.player1.Connect4Wins++;
		$Pref::Server::BoardGames::Connect4Wins++;
		$Pref::Server::BoardGames::Connect4Losses++;
	} else if (%winCondition == 0) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 has beaten \c3" @ %this.player1.name @ "\c5 at \c3Connect4\c5!");
		%this.player0.Connect4Wins++;
		%this.player1.Connect4Losses++;
		$Pref::Server::BoardGames::Connect4Wins++;
		$Pref::Server::BoardGames::Connect4Losses++;
	} else if (%winCondition < 0){
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 and \c3" @ %this.player0.name @ "\c5 have quit their \c3Connect4 \c5game.");
	}
	if (%winParams !$= "") {
		%this.displayWin(%winParams);
	}

	%this.hasEnded = 1;

	cancel(%this.turnTimer);

	%this.player0.inBoardGame = false;
	%this.player1.inBoardGame = false;

	%this.player0.boardGameSpawn = "";
	%this.player1.boardGameSpawn = "";

	%this.schedule(13000, postEndGame);
}

function Connect4::postEndGame(%this) {
    %this.player0.spawnPlayer();
    %this.player1.spawnPlayer();
	%this.player0.boardGame = "";
	%this.player1.boardGame = "";
	%this.player0 = "";
	%this.player1 = "";
	chainKillBrickGroup(%this.brickGroup, %this.brickGroup.getCount());
	%this.delete();
}

function Connect4::takeTurn(%this, %col, %client) {
	%col = getSubStr(%col.getName(), 1, strlen(%col.getName()));
	if (%client == %this.player0) {
		%player = 0;
	} else if (%client == %this.player1) {
		%player = 1;
	} else {
		return;
	}
    %col = getSubStr(%col, 4, 1);

	if (%this.turn != %player) {
		%client.centerprint("\c3It's not your turn! Please wait for your opponent.", 3);
		return -1;
	}

    if (isEventPending(%this.turnTimer)) {
        cancel(%this.turnTimer);  
    }

	//check if column is full
	if (%this.getChipAt(%col, 5) != 9) {
		%client.centerprint("\c3This column is already full!", 3);
		return -1;
	} else {
		%client.centerprint("");
	}

	%down = 0;
	for (%i = 4; %i >= -1; %i--) {
		if (%this.getChipAt(%col, %i) != 9) {
			%down = %i+1;
			break;
		}
	}
	echo("Placing " @ %player @ " chip in col " @ %col @ ", row " @ %down);

	%this.setChipAt(%col, %down, %player);
	%this.lastMove = %col;

	if ((%winCondition = %this.checkForWin()) > -1 && %winCondition !$= "") {
		messageClient(%this.player0, 'MsgAdminForce', '');
		messageClient(%this.player1, 'MsgAdminForce', '');
        %this.player0.bottomPrint("", 1, 1);
        %this.player1.bottomPrint("", 1, 1);
		%this.endGame(getWord(%winCondition, 0), getWords(%winCondition, 1, 4));
		return;
	}

    %this.player[%this.turn].bottomPrint("", 1, 1);

	%this.turn = (%this.turn + 1) % 2;
	%this.player[%this.turn].centerprint("<font:Palatino Linotype:40>\c3It's your turn!", 20);
	messageClient(%this.player[%this.turn], 'MsgUploadEnd', '');
    %this.secondsPassed = 0;
    %this.turnTimer = %this.doTurnTimer(%this.player[%this.turn]);
}

function Connect4::checkForWin(%this) {
	if (%this.lastMove $= "") {
		return -1;
	}

	//get starting chip
	%row = -1;
	%col = %this.lastMove;
	%player = %this.turn;
	for (%i = 5; %i >= 0; %i--) {
		if (%this.getChipAt(%col, %i) != 9) {
			%row = %i;
			break;
		}
	}
	if (%row == -1) {
		echo("Connect4 Error: Last moved column has no chip");
		return;
	} else if (%this.getChipAt(%col, %row) != %player) {
		echo("Connect4 Error: Top chip in column is not correct");
		return;
	}

	//check horizontals
	%right = 0;
	%left = 0;
	for (%i = %col + 1; %i < 7; %i++) {
		if (%this.getChipAt(%i, %row) != %player) {
			break;
		}
		%right++;
	}
	for (%i = %col - 1; %i >= 0; %i--) {
		if (%this.getChipAt(%i, %row) != %player) {
			break;
		}
		%left++;
	}

	//check vertical
	%down = 0;
	for (%i = %row - 1; %i >= 0; %i--) {
		if (%this.getChipAt(%col, %i) != %player) {
			break;
		}
		%down++;
	}

	//check diagonals
	%upright = 0;
	%upleft = 0;
	%downright = 0;
	%downleft = 0;
	for (%i = 1; %i < 7; %i++) {
		if (%this.getChipAt(%col + %i, %row + %i) != %player) {
			break;
		}
		%upright++;
	}
	for (%i = 1; %i < 7; %i++) {
		if (%this.getChipAt(%col - %i, %row + %i) != %player) {
			break;
		}
		%upleft++;
	}
	for (%i = 1; %i < 7; %i++) {
		if (%this.getChipAt(%col + %i, %row - %i) != %player) {
			break;
		}
		%downright++;
	}
	for (%i = 1; %i < 7; %i++) {
		if (%this.getChipAt(%col - %i, %row - %i) != %player) {
			break;
		}
		%downleft++;
	}

	//is win? or tie?
	%win1 = 0;
	%win2 = 0;
	%win3 = 0;
	%win4 = 0;
	if ((%win1 = %left + %right) >= 3 || (%win2 = %down) >= 3 || (%win3 = %upright + %downleft) >= 3 || (%win4 = %upleft + %downright) >= 3) {
		return %player SPC %win1 SPC %win2 SPC %win3 SPC %win4;
	} else if (strlen(stripChars(%this.row5, "9")) == strlen(%this.row5)) { 
		return 3;
	} else {
		return -1;
	}
}

function Connect4::displayWin(%this, %winParams) {
	%row = -1;
	%col = %this.lastMove;
	%player = %this.turn;
	for (%i = 5; %i >= 0; %i--) {
		if (%this.getChipAt(%col, %i) != 9) {
			%row = %i;
			break;
		}
	}
	%this.brickGroup.NTObject_row[%row @ %col @ "_0"].setEmitter(pongExplosionEmitter);

	if (%row == -1) {
		talk("Connect4 Win Error: Last moved column has no chip");
		return;
	} else if (%this.getChipAt(%col, %row) != %player) {
		talk("Connect4 Win Error: Top chip in column is not correct");
		return;
	}

	//check horizontals
	if (getWord(%winParams, 0) >= 3) {
		for (%i = %col + 1; %i < 7; %i++) {
			if (%this.getChipAt(%i, %row) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[%row @ %i @ "_0"].setEmitter(pongExplosionEmitter);
		}
		for (%i = %col - 1; %i >= 0; %i--) {
			if (%this.getChipAt(%i, %row) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[%row @ %i @ "_0"].setEmitter(pongExplosionEmitter);
		}
	}

	//check vertical
	if (getWord(%winParams, 1) >= 3) {
		%down = 0;
		for (%i = %row - 1; %i >= 0; %i--) {
			if (%this.getChipAt(%col, %i) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[%i @ %col @ "_0"].setEmitter(pongExplosionEmitter);
		}
	}

	//check diagonals
	%upright = 0;
	%upleft = 0;
	%downright = 0;
	%downleft = 0;
	if (getWord(%winParams, 2) >= 3) {
		for (%i = 1; %i < 7; %i++) {
			if (%this.getChipAt(%col + %i, %row + %i) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[(%row + %i) @ (%col + %i) @ "_0"].setEmitter(pongExplosionEmitter);
		}
		for (%i = 1; %i < 7; %i++) {
			if (%this.getChipAt(%col - %i, %row - %i) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[(%row - %i) @ (%col - %i) @ "_0"].setEmitter(pongExplosionEmitter);
		}
	}


	if (getWord(%winParams, 3) >= 3) {
		for (%i = 1; %i < 7; %i++) {
			if (%this.getChipAt(%col - %i, %row + %i) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[(%row + %i) @ (%col - %i) @ "_0"].setEmitter(pongExplosionEmitter);
		}
		for (%i = 1; %i < 7; %i++) {
			if (%this.getChipAt(%col + %i, %row - %i) != %player) {
				break;
			}
			%this.brickGroup.NTObject_row[(%row - %i) @ (%col + %i) @ "_0"].setEmitter(pongExplosionEmitter);
		}
	}
}

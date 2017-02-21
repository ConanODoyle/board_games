if (!isObject($GameList)) {
	$GameList = new SimSet() {
		gameTypes = 1;
		type0 = "Connect4";
	};
}

if (!isObject(NewBrickToolTrailShape)) {
	datablock StaticShapeData(NewBrickToolTrailShape) {
	    shapeFile = "./cylinder_glow.dts";
	};
}

function StaticShape::newBrickToolFade(%this, %a, %b, %color, %alpha) {
    if (%alpha <= 0) {
        %this.delete();
        return;
    }

    %size = 0.05 + %alpha * 0.075;
    %vector = vectorNormalize(vectorSub(%b, %a));

    %xyz = vectorNormalize(vectorCross("1 0 0", %vector));
    %u = mACos(vectorDot("1 0 0", %vector)) * -1;

    %this.setTransform(vectorScale(vectorAdd(%a, %b), 0.5) SPC %xyz SPC %u);
    %this.setScale(vectorDist(%a, %b) SPC %size SPC %size);
    %this.setNodeColor("ALL", %color SPC %alpha);

    if (%this.repeat) {
        %this.a = %a;
        %this.b = %b;
        %this.color = %color;
        return;
    }

    %this.schedule(25, "newBrickToolFade", %a, %b, %color, %alpha - 0.1);
}

exec("./buildload.cs");
exec("./connect4.cs");
exec("./chess.cs");
exec("./uttt.cs");
exec("./othello.cs");
exec("./risk.cs");
exec("./battleship.cs");

exec("./chessItems.cs");
exec("./battleshipItems.cs");

$uttt = "Add-ons/Server_Board_Games/uttt.cs";
$connect4 = "Add-ons/Server_Board_Games/connect4.cs";
$othello = "Add-ons/Server_Board_Games/othello.cs";
$chess = "Add-ons/Server_Board_Games/chess.cs";
$risk = "Add-ons/Server_Board_Games/risk.cs";
$battleship = "Add-ons/Server_Board_Games/battleship.cs";
$boardgame = "Add-ons/Server_Board_Games/";
$boardgames = "Add-ons/Server_Board_Games/server.cs";


datablock fxDTSBrickData(brickConnect4Data){
	brickFile = "./bricks/connect4.blb";
	uiName = "Connect4 Brick";

	category = "Special";
	subCategory = "Board Games";
	iconName = "";
};

datablock fxDTSBrickData(brickChessboard6x6Data){
	brickFile = "./bricks/Chessboard6x6.blb";
	uiName = "Chessboard 6x6";

	category = "Special";
	subCategory = "Board Games";
	iconName = "";	
};

datablock fxDTSBrickData(brickOthelloWhite6x6Data : brickChessboard6x6Data) {
	brickFile = "./bricks/othelloWhite.blb";
	uiName = "Othello - White";
};

datablock fxDTSBrickData(brickOthelloBlack6x6Data : brickChessboard6x6Data) {
	brickFile = "./bricks/othelloBlack.blb";
	uiName = "Othello - Black";
};

datablock fxDTSBrickData(brickOthelloEmpty6x6Data : brickChessboard6x6Data) {
	brickFile = "./bricks/othelloEmpty.blb";
	uiName = "Othello - Empty";
};

datablock fxDTSBrickData(brickBattleshipScreenEmptyPiece : brickChessboard6x6Data) {
	brickFile = "./bricks/screenEmpty.blb";
	uiName = "Battleship Screen - Empty";
};

datablock fxDTSBrickData(brickBattleshipScreenSelectedPiece : brickChessboard6x6Data) {
	brickFile = "./bricks/screenTarget.blb";
	uiName = "Battleship Screen - Selected";
};

datablock fxDTSBrickData(brickBattleshipScreenHitPiece : brickChessboard6x6Data) {
	brickFile = "./bricks/screenHit.blb";
	uiName = "Battleship Screen - Hit";
};

datablock fxDTSBrickData(brickBattleshipScreenMissPiece : brickChessboard6x6Data) {
	brickFile = "./bricks/screenMiss.blb";
	uiName = "Battleship Screen - Miss";
};


package BoardGame {
	function GameConnection::createPlayer(%this, %pos) {
		if (%this.inBoardGame) {
			%pos = %this.boardGameSpawn;
			if (%pos $= "") {
				talk("Error: Player in board game does not have spawn assigned");
			}
		}
		return parent::createPlayer(%this, %pos);
	}

	function Armor::onCollision(%this, %obj, %col, %a, %b, %c, %d, %e, %f)
	{
		//talk(%obj SPC %col SPC %a SPC %b SPC %c SPC %d SPC %e SPC %f);
		//lets you pick up multiple proptools if disappear on use is enabled
		if(%col.getDatablock().cannotPickup)
		{
			return;
		}
		return parent::onCollision(%this, %obj, %col, %a, %b, %c, %d, %e, %f);
	}

	function Player::activateStuff(%player) {
		%client = %player.client;
		if (%client.inBoardGame) {
	        %start = getWords(%player.getEyeTransform(), 0, 2);
	        %end = vectorAdd(vectorScale(%player.getEyeVector(), 55), %start);
	        %masks = $TypeMasks::FxBrickObjectType | $TypeMasks::TerrainObjectType;
	        %ray = containerRaycast(%start, %end, %masks, %player);
	        %hit = getWord(%ray, 0);
	        %a = %end;
        	%b = %player.getMuzzlePoint(0);
	        if (isObject(%hit)) {
	        	%a = getWords(%ray, 1, 3);
	        	%color = "1 1 1 1";
		        if (%hit.getClassName() $= "FxDTSBrick" && strLen(stripChars(%hit.getName(), "_")) > 0) {
		        	if (%hit.getGroup() != %client.boardGame.brickGroup) {
		        		return parent::activateStuff(%player);
		        	}
		        	%client.boardGame.takeTurn(%hit, %client);
		        }

		    }
		    if (!isObject(%cl.staticShape)) {
		    	%cl.staticShape = 0;
		    }
	    }
	    return parent::activateStuff(%player);
	}

	function GameConnection::onDrop(%client, %param) {
		if (isObject(%client.boardGame) && !%client.boardGame.hasEnded) {
			%enemyTeam = (%client.boardGame.player0 == %client ? 1 : 0);
			%client.boardGame.endGame(%enemyTeam);
			messageAll('', "\c7" @ %client.name @ " pussied out of their game!");
		}
		return parent::onDrop(%client, %param);
	}
};
activatePackage(BoardGame);

$nextBrickgroup = 800813;

function getNextGameBrickgroupID() {
	if ($nextBrickgroup == 888888) {
		$nextBrickgroup = 800813;
	}
	$nextBrickgroup++;
	return $nextBrickgroup - 1;
}

function chainKillBrickGroup(%brickGroup, %i) {
	if (%i < 0) {
        %brickGroup.schedule(1000, delete);
		return;
	}
	%brickGroup.getObject(%i).killBrick();
	schedule(1, 0, chainKillBrickGroup, %brickGroup, %i--);
}


//////////////
//servercmds//
//////////////

$listOfGames = "Connect4 UTTT Othello Chess";
$BoardGame::numGames = 15;

$BoardGame::Game0 = "Connect4 Connect4";
$BoardGame::Game1 = "UTTT UltimateTicTacToe";
$BoardGame::Game2 = "Othello Othello";
$BoardGame::Game11 = "Chess Chess";

$BoardGame::Game3 = "C4 Connect4";
$BoardGame::Game4 = "UT UltimateTicTacToe";
$BoardGame::Game5 = "UTT UltimateTicTacToe";
$BoardGame::Game6 = "Ot Othello";
$BoardGame::Game7 = "Oth Othello";
$BoardGame::Game8 = "Othe Othello";
$BoardGame::Game9 = "Othel Othello";
$BoardGame::Game10 = "Othell Othello";
$BoardGame::Game12 = "Ch Chess";
$BoardGame::Game13 = "Che Chess";
$BoardGame::Game14 = "Ches Chess";

function serverCmdChallenge(%cl, %name, %boardGame, %theme) {
	%challenged = fcn(%name);

	%boardGame = strlwr(%boardGame);
	for(%i = 0; %i < $BoardGame::numGames; %i++) {
		if (%boardGame $= strlwr(getWord($BoardGame::Game[%i], 0))) {
			%boardGame = getWord($BoardGame::Game[%i], 1);
			%gameFound = 1;
			break;
		}
	}
	if (%boardGame $= "Chess") {
		%cl.gameTheme = %theme;
	}

	if (!%gameFound || %boardGame $= "") {
		messageClient(%cl, '', "<font:Arial Bold:18>\c5Usage\c6: /challenge [name] [" @ strReplace($listOfGames, " ", "/") @ "] ");
		return;
	} else if (isObject(%challenged.currChallengeTo) || isObject(%challenged.currChallengeBy) || isObject(%challenged.boardgame)) {
		messageClient(%cl, '', "<font:Arial Bold:18>\c3" @ %challenged.name @ "\c5 is busy right now.");
		return;
	} else if (isObject(%cl.currChallengeTo)) {
		messageClient(%cl, '', "<font:Arial Bold:18>\c5You can only challenge one player at a time!");
		return;
	} else if (isObject(%cl.currChallengeBy)) {
		messageClient(%cl, '', "<font:Arial Bold:18>\c5You are currently being challenged - /deny or /accept them before challenging others!");
		return;
	} else if (isObject(%cl.boardgame)) {
		messageClient(%cl, '', "<font:Arial Bold:18>\c5You can't challenge someone while in a board game!");
		return;
	} else if (%cl == %challenged) {
		messageClient(%cl, '', "<font:Arial Bold:18>\c5You can't challenge yourself!");
		return;
	} else if(%challenged.ignoreChallenges) {
		messageClient(%cl, '', "<font:Arial Bold:18>\c3" @ %challenged.name @ "\c5 isn't receiving game invites right now.");
		return;
	}

    challengeToGame(%challenged, %cl, %boardGame);
    messageClient(%cl, '', "<font:Arial Bold:18>\c5----------------");
    messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c5You have challenged \c3" @ %challenged.name @ "\c5 to a game of \c3" @ %boardGame);
    messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c5Use \c3/cancel\c5 to cancel your challenge");
    messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c7This challenge will time out in 20 seconds");
    messageClient(%cl, '', "<font:Arial Bold:18>\c5----------------");
    %cl.currChallengeTo = %challenged;
    %cl.currGameChallenge = %boardGame;
}

function serverCmdToggleChallenge(%this) {
	%this.ignoreChallenges = !%this.ignoreChallenges;
	messageClient(%this, '', "\c5You are currently " @ (%this.ignoreChallenges ? "\c0not ":"\c2") @ "receiving\c5 challenges.");
}

function serverCmdIgnoreChallenges(%this) {
	serverCmdToggleChallenge(%this);	
}

function serverCmdIgnoreChallenge(%this) {
	serverCmdToggleChallenge(%this);	
}

function challengeToGame(%cl, %challenger, %boardGame) {
	messageClient(%cl, '', "<font:Arial Bold:18>\c5----------------");
    messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c5You have been challenged by \c3" @ %challenger.name @ "\c5 to a game of \c3" @ %boardGame);
    messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c5You can \c3/accept\c5 or \c3/decline\c5 this challenge.");
    messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c7This challenge will time out in 20 seconds");
    messageClient(%cl, '', "<font:Arial Bold:18>\c5----------------");
    %cl.currChallengeBy = %challenger;
    %cl.currGameChallenge = %boardGame;
    %cl.currChallengedSchedule = schedule(20000, 0, challengeExpired, %challenger, %cl, %boardGame);
}

function serverCmdAccept(%cl) {
	if (!isObject(%cl.currChallengeBy)) {
		return;
	}
	if (%cl.currGameChallenge !$= "Chess") {
		eval("start" @ %cl.currGameChallenge @ "Game(" @ %cl @ ", " @ %cl.currChallengeBy @ ");");
	} else {
		eval("start" @ %cl.currGameChallenge @ "Game(" @ %cl @ ", " @ %cl.currChallengeBy @ ", 0" @ $Chess::Themes[%cl.currChallengeBy.gameTheme] @ ");");
	}
	%challenger = %cl.currChallengeBy;
	%challenged = %cl;
	messageAll('', "<font:Arial Bold:18>\c3" @ %challenger.name @ "\c5 and \c3" @ %challenged.name @ "\c5 have started a game of \c3" @ %challenger.currGameChallenge @ "\c5!");

	%challenger.currChallengeTo = "";
	%challenged.currChallengeBy = "";
	%challenger.currGameChallenge = "";
	%challenged.currGameChallenge = "";
	cancel(%cl.currChallengedSchedule);	
}

function serverCmdDecline(%cl) {
	if (!isObject(%cl.currChallengeBy)) {
		return;
	}
	messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c5You have declined \c3" @ %cl.currChallengeBy.name @ "\c5's challenge to a game of \c3" @ %cl.currGameChallenge @ "\c5.");
	messageClient(%cl.currChallengeBy, 'MsgUploadEnd', "<font:Arial Bold:18>\c3" @ %cl.name @ "\c5 has declined the challenge.");
	challengeExpired(%cl.currChallengeBy, %cl, %cl.currGameChallenge);
}

function serverCmdCancel(%cl) {
	if (!isObject(%cl.currChallengeTo)) {
		return;
	}
	messageClient(%cl, 'MsgUploadEnd', "<font:Arial Bold:18>\c5You have canceled your challenge to \c3" @ %cl.currChallengeTo.name @ "\c5.");
	messageClient(%cl.currChallengeTo, 'MsgUploadEnd', "<font:Arial Bold:18>\c3" @ %cl.name @ "\c5 has canceled their challenge.");
	challengeExpired(%cl, %cl.currChallengeTo, %cl.currGameChallenge);
}

function challengeExpired(%challenger, %challenged, %game) {
	messageClient(%challenger, 'MsgUploadEnd', "<font:Arial Bold:18>\c5The " @ %game @ " game has expired.");
	messageClient(%challenged, 'MsgUploadEnd', "<font:Arial Bold:18>\c5The " @ %game @ " game has expired.");

	%challenger.currChallengeTo = "";
	%challenged.currChallengeBy = "";
	%challenger.currGameChallenge = "";
	%challenged.currGameChallenge = "";
	cancel(%challenged.currChallengedSchedule);	
}

function serverCmdForfeit(%client) {
	if (isObject(%client.boardGame) && !%client.boardGame.hasEnded) {
		%enemyTeam = (%client.boardGame.player0 == %client ? 1 : 0);
		messageAll('', "\c3" @ %client.name @ "\c5 surrendered to \c3" @ %client.boardGame.player[%enemyTeam].name @ "\c5!");
		%client.boardGame.endGame(%enemyTeam);
	} else if (%client.boardgame.hasEnded) {
		messageClient(%client, '', "\c7Your opponent has already surrendered!");
	}else {
		messageClient(%client, '', "\c7You need to be in a game to surrender!");
	}
}

function serverCmdSurrender(%client) {
	serverCmdForfeit(%client);
}

function serverCmdQuit(%client) {
	serverCmdForfeit(%client);
}


function fullShutdown() {
	shutdown();
	export("$pre*", "config/server/prefs.cs");
	schedule(1000, 0, quit);
}

function exportPrefs() {
	export("$pre*", "config/server/prefs.cs");
}
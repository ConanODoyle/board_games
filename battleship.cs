$Battleship::letters = "abcdefghij";

$Battleship::timePerTurn = 500000;
$Battleship::lastoffset = "-140 -140 0";
$Battleship::offsetAmount = "-40 0 0";
$Battleship::whitePegDb = brickBattleshipWhitePegData;
$Battleship::redPegDb = brickBattleshipRedPegData;
$Battleship::noPegDb = brickBattleshipNoPegData;
$Battleship::numShips = 5;

$Battleship::datablockSet = "battleship_5Item battleship_4Item battleship_3_1Item battleship_3_2Item battleship_2Item";

$Battleship::CARRIER = 0;
$Battleship::BATTLESHIP = 1;
$Battleship::CRUISER = 2;
$Battleship::SUBMARINE = 3;
$Battleship::DESTROYER = 4;

$Battleship::PRIMARYGRID = 0;
$Battleship::TRACKINGGRID = 1;

$Battleship::Stage::SETUP = 0;
$Battleship::Stage::PLAY = 1;

$Battleship::FORWARD = 0;
$Battleship::RIGHT = 1;
$Battleship::BACKWARD = 2;
$Battleship::LEFT = 3;

$Battleship::scale = "2 2 2";

// it allll starts here
function startBattleshipGame(%client1, %client2)
{
	// one of the clients doesnt exist
	if(!isObject(%client1) || !isObject(%client2))
		return;

	//create the game and load the board
	%battleship = battleship_createGame(%client1, %client2);

	// find position to load battleship board
	%boardPos = %battleship.getOffset();

	%battleship.createBoard(%boardPos);
	%battleship.initClients();
	%battleship.schedule(2200, initSetupStage);
}

function battleship_createGame(%client1, %client2)
{
	%battleship = new ScriptObject(Battleship)
	{
		player0 = %client1;
		player1 = %client2;
		stage = $Battleship::Stage::SETUP;
	};

	%id = getNextGameBrickgroupId();
	%battleship.brickGroup = new SimGroup("BrickGroup_" @ %id);
	%battleship.brickGroup.bl_id = %id;
	%battleship.brickGroup.ispublicdomain = 0;
	%battleship.brickGroup.name = "Battleship Game: " @ %client1.name SPC "VS" SPC %client2.name;
	mainBrickGroup.add(%battleship.brickGroup);

	return %battleship;
}

function Battleship::getOffset(%this) 
{
	$Battleship::lastOffset = vectorAdd($Battleship::lastOffset, $Battleship::offsetAmount);
	return vectorSub($Battleship::lastOffset, $Battleship::offsetAmount);
}

function Battleship::createBoard(%this, %pos)
{
	%buildName = "battleshiptest";

	// load it in with the games brickgroup used
	while(!loadBuild(%pos, %buildName, %this.brickGroup))
	{
		%pos = %this.getOffset();
		%this.brickGroup.deleteAll();
	}

	// record the original color and colorfx of each brick
	%count = %this.brickGroup.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%obj = %this.brickGroup.getObject(%i);
		%obj.originalColor = %obj.getColorId();
		%obj.originalColorFx = %obj.getColorFxId();
	}
}

function Battleship::initClients(%this)
{
	%client1 = %this.player0;
	%client2 = %this.player1;

	// move players there somehow
	%spawn0Transform = %this.getBrick("spawn0").getSpawnPoint();
	%spawn1Transform = %this.getBrick("spawn1").getSpawnPoint();

	%client1.boardGameSpawn = %spawn0Transform;
	%client1.inBoardGame = true;
	%client1.boardGame = %this;
	%client1.battleshipId = 0;

	%client2.boardGameSpawn = %spawn1Transform;
	%client2.inBoardGame = true;
	%client2.boardGame = %this;
	%client2.battleshipId = 1;

	%client1.schedule(2000, spawnPlayer);
	%client2.schedule(2000, spawnPlayer);
}

// location here is just name of the brick
function Battleship::getBrick(%this, %location)
{
	return %this.brickGroup.ntObject["", %location, 0];
}


function Battleship::initSetupStage(%this)
{
	%client1 = %this.player0;
	%client2 = %this.player1;

	// set available ships to place for each client
	for(%shipId = 0; %shipId < 5; %shipId++)
	{
		%this.addAvailableShip(%client1, %shipId);
		%this.addAvailableShip(%client2, %shipId);
	}

	%this.selectionIndex[%client1] = 0;
	%this.selectionIndex[%client2] = 0;

	// set their initial ships at a1
	%this.setSelectedShip(%client1, "a1", $Battleship::RIGHT);
	%this.setSelectedShip(%client2, "a1", $Battleship::RIGHT);

	// message the clients that they need to place their ships
	messageClient(%client1, '', "\c6Set up your ships to begin the game.");
	messageClient(%client2, '', "\c6Set up your ships to begin the game.");
}

function Battleship::addAvailableShip(%this, %client, %shipId)
{
	if(strStr(%this.availableShips[%client], %shipId) == -1)
		%this.availableShips[%client] = trim(%this.availableShips[%client] SPC %shipId);
}

function Battleship::setSelectedShip(%this, %client, %location, %direction)
{
	//talk("set to: " @ %direction);
	%brick = %this.getBrick(%client.battleshipId @ "p" @ %location);
	%brick.setItem(%this.getDB(getWord(%this.availableShips[%client], %this.selectionIndex[%client])));
	%brick.setItemDirection(%this.BDTID(%client, %direction));
	//talk("setting ship direction to: " @ %brick.itemDirection);
	%brick.item.startFade(0, 0, 1);
	%brick.item.setNodeColor("ALL", "1 1 1 0.3");
	%brick.item.setScale($Battleship::scale);

	%this.selectionBrick[%client] = %brick;
}

function Battleship::getDB(%this, %shipId)
{
	return getWord($Battleship::datablockSet, %shipId);
}

function Battleship::BDTID(%this, %client, %direction)
{
	switch(%direction)
	{
		case $Battleship::FORWARD:		return %client.battleshipId == 0 ? 3 : 5;
		case $Battleship::RIGHT:		return %client.battleshipId == 0 ? 4 : 2;
		case $Battleship::BACKWARD:		return %client.battleshipId == 0 ? 5 : 3;
		case $battleship::LEFT:			return %client.battleshipId == 0 ? 2 : 4;
	}
}

function Battleship::removeAvailableShip(%this, %client, %shipId)
{
	//talk("attempting to remove id: " @ %shipId);
	%shipIds = %this.availableShips[%client];
	for(%i = 0; %i < getWordCount(%shipIds); %i++)
	{
		%id = getWord(%shipIds, %i);
		if(%id == %shipId)
		{
			%this.availableShips[%client] = removeWord(%shipIds, %i);

			if(%this.selectionIndex[%client] == getWordCount(%this.availableShips[%client]))
				%this.selectionIndex[%client]--;
		}
	}
}

function Battleship::getGrid(%this, %brickName)
{
	return strStr(%brickName, "t") != -1 ? $Battleship::TRACKINGGRID : $Battleship::PRIMARYGRID;
}

function Battleship::takeTurn(%this, %obj, %client)
{
	if(%this.stage == $Battleship::Stage::SETUP)
	{
		if(%this.getGrid(%obj.getName()) != $Battleship::PRIMARYGRID || getSubStr(%obj.getName(), 1, 1) != %client.battleshipId)
			return;

		if(%this.selectionIndex[%client] == -1)
				return;
		
		%currentShipId = getWord(%this.availableShips[%client], %this.selectionIndex[%client]);
		%location = strReplace(%obj.getName(), "_" @ %client.battleshipId @ "p", "");
		%direction = %this.findValidDirection(%client, %location, getWord(%this.availableShips[%client], %this.selectionIndex[%client]));

		if(!%this.isInList(%this.unhitLocations[%client], %location) && %direction != -1)
		{
			%prevBrick = %this.selectionBrick[%client];
			%prevBrick.setItem("");
			%this.setSelectedShip(%client, %location, %direction);
		}
	}
	else
	{
		// is the player in a game and is it their turn
		if(%this.currentTurn != %client)
			return;

		// did the player hit a brick on the tracking board
		if(%this.getGrid(%obj.getName()) != $Battleship::TRACKINGGRID)
			return;

		%piece = %obj.item;
		%location = strReplace(%obj.getName(), "_" @ %client.battleshipId @ "t", "");
	//	talk(%location);

		if(%this.validMove(%client, %location))
		{
			if(!%this.applySelection(%client, %location))
				%this.switchTurns();

			%this.doLogic();
		}
	}
}

function Battleship::switchTurns(%this)
{
	%this.currentTurn = %this.currentTurn == %this.player0 ? %this.player1 : %this.player0;
	messageClient(%this.currentTurn, 'MsgUploadEnd', "\c6It is your turn.");

	if(%this.finished)
		%this.currentTurn = -1;

	//messageClient(%this.currentTurn, '', "It is your turn.");
}

function Battleship::findValidDirection(%this, %client, %location, %shipId)
{
	%length = %this.getShipLength(%shipId);

	for(%i = 0; %i < 4; %i++)
	{
		%dx = getWord("0 1 0 -1", %i);
		%dy = getWord("-1 0 1 0", %i);
		%goodDirection = true;

		for(%j = 0; %j < %length - 1; %j++)
		{
			%newLocation = %this.locationFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(!%this.isValidLocation(%newLocation) || %this.isInList(%this.unhitLocations[%client], %newLocation))
			{
				%goodDirection = false;
				break;
			}
		}

		if(%goodDirection)
			return %i;
	}

	return -1;
}

function Battleship::findValidLocation(%this, %client, %startFrom)
{
	if(%startFrom $= "")
		%startFrom = "a0";

	%startI = strStr($Battleship::letters, getSubStr(%startFrom, 0, 1));
	if(%startI == -1)
		return -1;

	%startJ = getSubStr(%startFrom, 1, 2);
	if(%startJ >= 10)
	{
		%startI++;
		%startJ = 0;
		//talk("setting startj to 0");
	}

	for(%i = %startI; %i < 10; %i++)
	{
		for(%j = %startJ; %j < 10; %j++)
		{
			%location = getSubStr($Battleship::letters, %i, 1) @ (%j + 1);
			if(%this.isValidLocation(%location) && !%this.isInList(%this.unhitLocations[%client], %location))
			{
				//talk("choosing " @ %location);
				return %location;
			}

			//talk(%location @ " wasnt chosen. (%startJ = " @ %startJ @ ")");
		}

		%startJ = 0;
	}

	return -1;
}

function Battleship::isInList(%this, %list, %location)
{
	for(%i = 0; %i < getWordCount(%list); %i++)
	{
		%loc = getWord(%list, %i);

		if(%loc $= %location)
			return true;
	}

	return false;
}

function Battleship::getShipLength(%this, %shipId)
{
	switch(%shipId)
	{
		case $Battleship::CARRIER:		return 5;
		case $Battleship::BATTLESHIP:	return 4;
		case $Battleship::CRUISER:		return 3;
		case $Battleship::SUBMARINE:	return 3;
		case $Battleship::DESTROYER:	return 2;
	}
}

function Battleship::locationFrom(%this, %pos, %offX, %offY)
{
	%y = strPos($Battleship::letters, getSubStr(%pos, 0, 1)) + %offY;

	if(%y < 0)
		return "";

	%x = getSubStr(%pos, 1, 2) + %offX;

	//talk(%x SPC %y);

	%potentialLocation = getSubStr($Battleship::letters, %y, 1) @ %x;

	if(%this.isValidLocation(%potentialLocation))
		return %potentialLocation;

	return "";
}

function Battleship::isValidLocation(%this, %location)
{
	%row = getSubStr(%location, 0, 1);
	%column = getSubSTr(%location, 1, 2);
	
	return strStr($Battleship::letters, %row) != -1 && %column > 0 && %column < 11 && (strLen(%location) == 2 || strLen(%location) == 3); 
}

function Battleship::getShipAt(%this, %client, %location)
{
	if(!isObject(%b = %this.getBrick(%client.battleshipId @ "p" @ %location)))
		return 0;

	if(!isObject(%b.item))
		return 0;

	return %b.item;
}

function Battleship::applySelection(%this, %client, %location)
{
	%opponent = %client == %this.player0 ? %this.player1 : %this.player0;

	//talk("test");
	%hit = %this.isInList(%this.unhitLocations[%opponent], %location);

	%pegColorId = 15;


	// if its a hit, update opponent ship status
	if(%hit)
	{
		%this.unhitLocations[%opponent] = %this.removeFromList(%this.unhitLocations[%opponent], %location);
		%pegColorId = 0;
	}

	// fire a missile down to spite puny ships
	%brick = %this.getBrick(%opponent.battleshipId @ "p" @ %location);
	%startPos = vectorAdd(%brick.getPosition(), "0 0 10");
	%p = new Projectile()
	{
		dataBlock = "rocketLauncherProjectile";
		initialPosition = %startPos;
		initialVelocity = "0 0 -10";
	};
	%p.setScale("0.5 0.5 0.5");

	// update players tracking grid and opponents primary grid
	%this.addPeg(%client, %location, %hit);
	//%this.addPeg(%opponent, $Battleship::PRIMARYGRID, %location, %pegColorId);

	%this.chosenMoves[%client] = trim(%this.chosenMoves[%client] SPC %location);
	//talk("hit: " @ %hit);
	return %hit;
}

function Battleship::removeFromList(%this, %list, %location)
{
	for(%i = 0; %i < getWordCount(%list); %i++)
	{
		%loc = getWord(%list, %i);

		if(%loc $= %location)
		{
			%list = removeWord(%list, %i);
			break;
		}
	}

	return %list;
}

function Battleship::addPeg(%this, %client, %location, %hit)
{
	%opponent = %client == %this.player0 ? %this.player1 : %this.player0;

	// get brick and change datablock
	
	%brick = %this.getBrick(%client.battleshipId @ "t" @ %location);
	%brick.setDatablock(%hit ? "brickbattleshipscreenhitpiece" : "brickbattleshipscreenmisspiece");

	%itemDB = %hit ? "hammerItem" : "wrenchItem";
	%brick.setItem(%itemDB);

	%brick2 = %this.getBrick(%opponent.battleshipId @ "p" @ %location);
	%brick.item.setTransform(vectorAdd(%brick2.getPosition(), "0 0 1"));
}

function Battleship::removePeg(%this, %client, %grid, %location)
{
	%brick = %this.getBrick(%client.battleshipId @ %grid == $Battleship::PRIMARYGRID ? "p" : "g" @ %location);
	%brick.setDatablock($Battleship::noPegDb);
}


function Battleship::validMove(%this, %client, %location)
{
	return %this.isValidLocation(%location) && !%this.isInList(%this.chosenMoves[%client], %location);
}

function Battleship::doLogic(%this)
{
	switch(%this.stage)
	{
		// 1st stage
		case $Battleship::Stage::SETUP:
			%this.doSetupLogic();


		// 2nd stage: gameplay
		case $Battleship::Stage::PLAY:
			%this.doPlayLogic();
	}
}

function Battleship::doSetupLogic(%this)
{
	%client = %this.player0;
	%client2 = %this.player1;

	if(%this.finishedSetup(%client) && !%this.finishedSetup(%client2) && !%this.notified)
	{
		messageClient(%client, '', "\c5Waiting on \c3" @ %client2.name @ "\c5 to finish setting up.");
		messageClient(%client2, '', "\c3" @ %client.name @ " \c5has finished setting up his ships.");
		%this.notified = true;
	}
	else if(%this.finishedSetup(%client2) && !%this.finishedSetup(%client) && !%this.notified)
	{
		messageClient(%client2, '', "\c5Waiting on \c3" @ %client.name @ "\c5 to finish setting up.");
		messageClient(%client, '', "\c3" @ %client2.name @ " \c5has finished setting up his ships.");
		%this.notified = true;
	}

	if(%this.finishedSetup(%client) && %this.finishedSetup(%client2))
	{
		// setup bs for gameplay
		%this.stage = $Battleship::Stage::PLAY;
		%this.currentTurn = %this.player0;

		messageClient(%client2, 'MsgUploadEnd', "\c6Both players finished setting up their ships. \c3" @ %this.player0.name @ "\c6 gets the first move.");
		messageClient(%client, 'MsgUploadEnd', "\c6Both players finished setting up their ships. You get the first move.");
	}
}

function Battleship::finishedSetup(%this, %client)
{
	return %this.selectionIndex[%client] == -1;
}

function Battleship::doPlayLogic(%this)
{
	%client = %this.currentTurn;
	%opponent = %this.currentTurn == %this.player0 ? %this.player1 : %this.player0;

	// are all of our ships destroyed
	%allDestroyed = !strLen(trim(%this.unhitLocations[%opponent]));
	//talK("all destroyed?: " @ %allDestroyed);

	if(%allDestroyed)
	{
		talk("game is over.");
		%this.endGame(%client.battleshipId);
		%this.finished = true;
	}
}

function Battleship::endGame(%this, %winType)
{
	if (%winType == 3) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 and \c3" @ %this.player1.name @ "\c5 have tied at \c3Battleship\c5!");
		%this.player0.battleshipTies++;
		%this.player1.battleshipTies++;
		$Pref::Server::BoardGames::BattleshipTies += 2;
	} else if (%winType == 1) {
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 has beaten \c3" @ %this.player0.name @ "\c5 at \c3Battleship\c5!");
		%this.player0.battleshipLosses++;
		%this.player1.battleshipWins++;
		$Pref::Server::BoardGames::BattleshipWins++;
		$Pref::Server::BoardGames::BattleshipLosses++;
	} else if (%winType == 0) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 has beaten \c3" @ %this.player1.name @ "\c5 at \c3Battleship\c5!");
		%this.player0.battleshipWins++;
		%this.player1.battleshipLosses++;
		$Pref::Server::BoardGames::BattleshipWins++;
		$Pref::Server::BoardGames::BattleshipLosses++;
	} else if (%winType < 0){
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 and \c3" @ %this.player0.name @ "\c5 have quit their \c3Battleship\c5 game.");
	}

	cancel(%this.turnTimer);
	%this.hasEnded = true;

	%client1 = %this.player0;
	%client2 = %this.player1;

	%client1.boardGameSpawn = "";
	%client1.inBoardGame = false;
	%client1.boardGame = 0;
	%client1.battleshipId = "";

	%client2.boardgmaeSpawn = "";
	%client2.inBoardGame = false;
	%client2.boardGame = 0;
	%client2.battleshipId = "";

	%this.finished = true;
	%this.currentTurn = -1;

	%this.schedule(10000, postEndGame);
}

function Battleship::postEndGame(%this) 
{
	%this.player0.spawnPlayer();
	%this.player1.spawnPlayer();
	chainKillBrickGroup(%this.brickGroup, %this.brickGroup.getCount());
	%this.delete();
}

function Battleship::getSelectedShip(%this, %client)
{
	return %this.selectedShip[%client];
}

function Battleship::isValidPlacement(%this, %client, %shipId, %location, %boardDirection)
{
	if(!%this.isValidLocation(%location))
		return;
	//talk("Direction: " @ %direction);
	%length = %this.getShipLength(%shipId);
	//talk(%length);
	%boardDirection = mClamp(%boardDirection, 0, 3);

	// location test
	if(%this.isInList(%this.unhitLocations[%client], %location))
		return false;

	%dx = getWord("0 1 0 -1", %boardDirection);
	%dy = getWord("-1 0 1 0", %boardDirection);

	for(%i = 0; %i < %length - 1; %i++)
	{
		%newLocation = %this.locationFrom(%location, %dx * (%i + 1), %dy * (%i + 1));
		if(!%this.isValidLocation(%newLocation) || %this.isInList(%this.unhitLocations[%client], %newLocation))
			return false;

		//talk("[direction]ship at location: " @ %newLocation @ "?: " @ %this.isInUnhitList(%newLocation));			
	}

	return true;
}

function Battleship::getNextDirection(%this, %boardDirection, %rot)
{
	%rot = %rot / mAbs(%rot);

	%dir = %boardDirection + %rot;

	if(%dir < 0)
		%dir = 3;

	return %dir % 4;
}

function Battleship::IDTBD(%this, %client, %itemDir)
{
	switch(%itemDir)
	{
		case 2:		return %client.battleshipId == 0 ? $Battleship::LEFT : $Battleship::RIGHT;
		case 3:		return %client.battleshipId == 0 ? $Battleship::FORWARD : $Battleship::BACKWARD;
		case 4:		return %client.battleshipId == 0 ? $Battleship::RIGHT : $Battleship::LEFT;
		case 5:		return %client.battleshipId == 0 ? $Battleship::BACKWARD : $Battleship::FORWARD;
	}
}

package BattleshipInput
{
	function serverCmdRotateBrick(%client, %rot)
	{
		%p = parent::serverCmdRotateBrick(%client, %rot);

		if(%client.battleshipId !$= "" && %client.boardGame.stage == $Battleship::Stage::SETUP)
		{
			%battleship = %client.boardGame;

			if(%battleship.selectionIndex[%client] == -1)
				return %p;

			%currentShipId = getWord(%battleship.availableShips[%client], %battleship.selectionIndex[%client]);
			%location = strReplace((%brick = %battleship.selectionBrick[%client]).getName(), "_" @ %client.battleshipId @ "p", "");
			%newDirection = %battleship.IDTBD(%client, %brick.itemDirection);

			// rotate until we find a valid rotation
			for(%i = 0; %i < 3; %i++)
			{
				%newDirection = %battleship.getNextDirection(%newDirection, %rot);
				//talk("new dir: " @ %newDirection);

				if(%battleship.isValidPlacement(%client, %currentShipId, %location, %newDirection))
				{
					%battleship.setSelectedShip(%client, %location, %newDirection);
					break;
				}
			}
		}

		return %p;
	}

	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		%p = parent::serverCmdShiftBrick(%client, %x, %y, %z);
		
		if(%client.battleshipId !$= "" && %client.boardGame.stage == $Battleship::Stage::SETUP)
		{
			%forward = %client.player.getForwardVector();
			%fVec = mFloatLength(getWord(%forward, 0), 0) SPC mFloatLength(getWord(%forward, 1), 0) SPC "0";
			%right = vectorCross(%fvec, %client.player.getUpVector());

			%xVec = %x SPC "0 0";
			%yVec = "0" SPC %y SPC "0";

			//talk("forward: " @ %fVec @ " right: " @ %right);
			//talk(%xVec @ " | " @ %yVec);


			if(getWord(%fvec, 0))
			{
				%dy = getWord(%right, 1) * %x * (%client.battleshipId == 0 ? 1 : -1);
				%dx = -getWord(%fvec, 0) * %y * (%client.battleshipId == 0 ? 1 : -1);
			}
			else
			{
				%dx = -getWord(%right, 0) * %x * (%client.battleshipId == 0 ? 1 : -1);
				%dy = getWord(%fvec, 1) * %y * (%client.battleshipId == 0 ? 1 : -1);
			}

			//talk(%dx SPC %dy);
			%battleship = %client.boardGame;

			if(%battleship.selectionIndex[%client] == -1)
				return %p;


			%currentShipId = getWord(%battleship.availableShips[%client], %battleship.selectionIndex[%client]);
			%location = strReplace((%brick = %battleship.selectionBrick[%client]).getName(), "_" @ %client.battleshipId @ "p", "");
			//talk("original Location: " @ %location);
			%direction = %battleship.IDTBD(%client, %brick.itemDirection);

			%newLocation = %battleship.locationFrom(%location, %dx, %dy);

			if(%battleship.isValidPlacement(%client, %currentShipId, %newLocation, %direction))
			{
				%prevBrick = %battleship.selectionBrick[%client];
				%prevBrick.setItem("");
				%battleship.setSelectedShip(%client, %newLocation, %direction);
			}
		}

		return %p;
	}

	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		%p = parent::serverCmdSuperShiftBrick(%client, %x, %y, %z);

		if(%client.battleshipId !$= "")
			serverCmdShiftBrick(%client, %x, %y, %z);

		return %p;
	}

	function serverCmdPlantBrick(%client)
	{
		%p = parent::serverCmdPlantBrick(%client, %dir);

		if(%client.battleshipId !$= "" && %client.boardGame.stage == $Battleship::Stage::SETUP)
		{
			%battleship = %client.boardGame;

			if(%battleship.selectionIndex[%client] == -1)
				return %p;

			%currentShipId = getWord(%battleship.availableShips[%client], %battleship.selectionIndex[%client]);
			%location = strReplace((%brick = %battleship.selectionBrick[%client]).getName(), "_" @ %client.battleshipId @ "p", "");

			if(%battleship.isValidPlacement(%client, %currentShipId, %location, %battleship.IDTBD(%client, %brick.itemDirection)))
			{
				%battleship.placeShip(%client, %currentShipId, %location, %battleship.IDTBD(%client, %brick.itemDirection));
				%battleship.doLogic();

				if(%battleship.selectionIndex[%client] != -1)
				{
					%currentShipId = getWord(%battleship.availableShips[%client], %battleship.selectionIndex);

					%location = %battleship.findValidLocation(%client);
					%direction = %battleship.findValidDirection(%client, %location, %currentShipId);

					//talk(%location SPC %direction);

					%battleship.setSelectedShip(%client, %location, %direction);
				}
			}
		}

		return %p;
	}
};
activatePackage(BattleshipInput);

function Battleship::placeShip(%this, %client, %shipId, %location, %boardDirection)
{
	// make the item not a dumb color
	%brick = %this.selectionBrick[%client];
	%item = %brick.item;
	%item.startFade(0, 0, 0);
	%item.setNodeColor("ALL", "1 1 1 1");


	// where tf is this ship on the grid??? code takeover for me
	%length = %this.getShipLength(%shipId);
	%dx = getWord("0 1 0 -1", %boardDirection);
	%dy = getWord("-1 0 1 0", %boardDirection);

	for(%i = 0; %i < %length; %i++)
	{
		%newLocation = %this.locationFrom(%location, %dx * %i, %dy * %i);
		%this.unhitLocations[%client] = trim(%this.unhitLocations[%client] SPC %newLocation);
		%this.unhitLocations[%client, %shipId] = trim(%this.unhitLocations[%client, %shipId] SPC %unhitLocations);
	}

	// remove this ship from the list of available ones
	%this.removeAvailableShip(%client, %shipId);
}


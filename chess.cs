/////////////////////////////// THEMES.cs ///////////////////////////////
// theme id and path to theme build
$Chess::Themes::DEFAULT = 0;
$Chess::Themes["default"] = $Chess::Themes::DEFAULT;
$Chess::buildName[$Chess::Themes::DEFAULT] = "chessboard";

// theme datablock set (must be in order: rook knight bishop queen king bishop knight rook pawn)
$Chess::datablockSet[$Chess::Themes::DEFAULT] = "rookItem knightItem bishopItem queenItem kingItem bishopItem knightItem rookItem pawnItem";
for(%pieceId = 0; %pieceId < 8; %pieceId++)
{
	$Chess::DB[$Chess::Themes::DEFAULT, %pieceId] 		= getWord($Chess::datablockSet[$Chess::Themes::DEFAULT], %pieceId);
	$Chess::DB[$Chess::Themes::DEFAULT, %pieceId + 8] 	= getWord($Chess::datablockSet[$Chess::Themes::DEFAULT], 8);
}

// theme tile colors and colorfx
$Chess::lightTileColorId[$Chess::Themes::DEFAULT] = 4;
$Chess::darkTileColorId[$Chess::Themes::DEFAULT] = 16;
$Chess::selectionColorId[$Chess::Themes::DEFAULT] = 29;
$Chess::selectionColorFxId[$Chess::Themes::DEFAULT] = 3;
$Chess::selectionEmitter[$Chess::Themes::DEFAULT] = "PlayerTeleportEmitterB";
$Chess::lastMoveEmitter[$Chess::Themes::DEFAULT] = "PlayerTeleportEmitterB";
$Chess::lightAvailableMoveColorId[$Chess::Themes::DEFAULT] = 34;
$Chess::darkAvailableMoveColorId[$Chess::Themes::DEFAULT] = 35;
$Chess::availableMoveColorFxId[$Chess::Themes::DEFAULT] = 3;
$Chess::checkColorId[$Chess::Themes::DEFAULT] = 11;






$Chess::Themes::FUTURISTIC = 1;
$Chess::Themes["futuristic"] = $Chess::Themes::FUTURISTIC;
$Chess::Themes["battle"] = $Chess::Themes::FUTURISTIC;
$Chess::buildName[$Chess::Themes::FUTURISTIC] = "futurechess3";

// theme datablock set (must be in order: rook knight bishop queen king bishop knight rook pawn)
$Chess::datablockSet[$Chess::Themes::FUTURISTIC] = "rook_battleItem knight_battleItem bishop_battleItem queen_battleItem king_battleItem bishop_battleItem knight_battleItem rook_battleItem pawn_battleItem";
for(%pieceId = 0; %pieceId < 8; %pieceId++)
{
	$Chess::DB[$Chess::Themes::FUTURISTIC, %pieceId] 		= getWord($Chess::datablockSet[$Chess::Themes::FUTURISTIC], %pieceId);
	$Chess::DB[$Chess::Themes::FUTURISTIC, %pieceId + 8] 	= getWord($Chess::datablockSet[$Chess::Themes::FUTURISTIC], 8);
}

// theme tile colors and colorfx
$Chess::lightTileColorId[$Chess::Themes::FUTURISTIC] = 4;
$Chess::darkTileColorId[$Chess::Themes::FUTURISTIC] = 16;
$Chess::selectionColorId[$Chess::Themes::FUTURISTIC] = 30;
$Chess::selectionColorFxId[$Chess::Themes::FUTURISTIC] = 3;
$Chess::selectionEmitter[$Chess::Themes::FUTURISTIC] = "PlayerTeleportEmitterB";
$Chess::lastMoveEmitter[$Chess::Themes::FUTURISTIC] = "PlayerTeleportEmitterB";
$Chess::lightAvailableMoveColorId[$Chess::Themes::FUTURISTIC] = 34;
$Chess::darkAvailableMoveColorId[$Chess::Themes::FUTURISTIC] = 35;
$Chess::availableMoveColorFxId[$Chess::Themes::FUTURISTIC] = 3;
$Chess::checkColorId[$Chess::Themes::FUTURISTIC] = 11;

/////////////////////////////////////////////////////////////////////////

$Chess::letters = "abcdefgh";
$Chess::timePerTurn = 90;
$Chess::lastoffset = "-100 -140 0";
$Chess::offsetAmount = "-40 0 0";
$Chess::rook0 = 0;
$Chess::knight0 = 1;
$Chess::bishop0 = 2;
$Chess::queen = 3;
$Chess::king = 4;
$Chess::bishop1 = 5;
$Chess::knight1 = 6;
$Chess::rook1 = 7;

for(%i = 0; %i < 8; %i++)
	$Chess::pawn[%i] = %i + 8;

$Chess::EAST = 3;
$Chess::WEST = 5;
$Chess::selectSound = brickRotateSound;
$Chess::moveSound = brickPlantSound;
$Chess::takePieceSound = wrenchHitSound;

function startChessGame(%client1, %client2, %theme)
{
	// one of the clients doesnt exist
	if(!isObject(%client1) || !isObject(%client2))
		return;

	if(%theme $= "")
	{
		echo("No theme given. Using default theme.");
		%theme = $Chess::THEMES::DEFAULT;
	}

	//create the game and load the board
	%chessGame = chess_createGame(%client1, %client2, %theme);

	// find position to load chess board (still gotta implement func)
	%boardPos = %chessGame.getOffset();
	echo("loading chess board at " @ %boardPos);

	%chessGame.createBoard(%boardPos);
	%chessGame.schedule(300, initialize);

	// move players there somehow
	%whiteTransform = %chessGame.getBrick("whitespawn").position SPC "0 0 1" SPC $pi/2;
	%blackTransform = %chessGame.getBrick("blackspawn").position SPC "0 0 1" SPC -$pi/2;

	%client1.boardGameSpawn = %whiteTransform;
	%client1.inBoardGame = true;
	%client1.boardGame = %chessGame;
	%client1.color = "white";

	%client2.boardGameSpawn = %blackTransform;
	%client2.inBoardGame = true;
	%client2.boardGame = %chessGame;
	%client2.color = "black";

	%client1.schedule(2000, spawnPlayer);
	%client2.schedule(2000, spawnPlayer);
}

function chess_createGame(%client1, %client2, %theme)
{
	%chessGame = new ScriptObject(ChessGame)
	{
		theme = %theme;
		light = %client1;
		dark = %client2;
		player0 = %client1;
		player1 = %client2;
		currentTurn = %client1;

		selectedPieceId = -1;
		selectedTile = "";
		potentialMoves = "";
	};

	%id = getNextGameBrickgroupId();
	%chessGame.brickGroup = new SimGroup("BrickGroup_" @ %id);
	%chessGame.brickGroup.bl_id = %id;
	%chessGame.brickGroup.ispublicdomain = 0;
	%chessGame.brickGroup.name = "Dope ass chess game: " @ %client1.name SPC "VS" SPC %client2.name;
	mainBrickGroup.add(%chessGame.brickGroup);

	return %chessGame;
}

function ChessGame::createBoard(%this, %pos)
{
	// get the name of the build
	if((%buildName = $Chess::buildName[%this.theme]) $= "")
	{
		talk("Unable to find the build name for theme: " @ %this.theme @ ". Using the default build");
		%buildName = $Chess::buildName[$Chess::Themes::DEFAULT];
	}

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

function ChessGame::endGame(%this, %type)
{
	if (%type == 3) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 and \c3" @ %this.player1.name @ "\c5 have tied at \c3Chess\c5!");
		%this.player0.chessTies++;
		%this.player1.chessTies++;
		$Pref::Server::BoardGames::ChessTies += 2;
	} else if (%type == 1) {
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 has beaten \c3" @ %this.player0.name @ "\c5 at \c3Chess\c5!");
		%this.player0.chessLosses++;
		%this.player1.chessWins++;
		$Pref::Server::BoardGames::chessWins++;
		$Pref::Server::BoardGames::chessLosses++;
	} else if (%type == 0) {
		chatMessageAll('', "\c3" @ %this.player0.name @ "\c5 has beaten \c3" @ %this.player1.name @ "\c5 at \c3Chess\c5!");
		%this.player0.chessWins++;
		%this.player1.chessLosses++;
		$Pref::Server::BoardGames::ChessWins++;
		$Pref::Server::BoardGames::ChessLosses++;
	} else if (%type < 0){
		chatMessageAll('', "\c3" @ %this.player1.name @ "\c5 and \c3" @ %this.player0.name @ "\c5 have quit their \c3Chess\c5 game.");
	}

	cancel(%this.turnTimer);
	%this.hasEnded = true;

	%client1 = %this.light;
	%client2 = %this.dark;

	%client1.boardGameSpawn = "";
	%client1.inBoardGame = false;
	%client1.boardGame = 0;
	%client1.color = ""; 

	%client2.boardgmaeSpawn = "";
	%client2.inBoardGame = false;
	%client2.boardGame = 0;
	%client2.color = "";

	%this.finished = true;
	%this.currentTurn = -1;

	%this.schedule(10000, postEndGame);
}

function ChessGame::postEndGame(%this) 
{
	%this.light.spawnPlayer();
	%this.dark.spawnPlayer();
	chainKillBrickGroup(%this.brickGroup, %this.brickGroup.getCount());
	%this.delete();
}

function ChessGame::initialize(%this)
{
	// for each piece id, set the intial values and get the datablocks
	for(%pieceId = 0; %pieceId < 16; %pieceId++)
	{
		%location1 = %this.tileFrom("a1", %pieceId % 8, mFloor(%pieceId / 8));
		%location2 = %this.tileFrom("a8", %pieceId % 8, -mFloor(%pieceId / 8));

		%this.addPiece(%this.light, %pieceId, %location1);
		%this.addPiece(%this.dark, %pieceId, %location2);
	}

	// In order to get the initial moves for each player, we have to call this
	%this.doLogic();
}

function ChessGame::getOffset(%this) 
{
	$Chess::lastOffset = vectorAdd($Chess::lastOffset, $Chess::offsetAmount);
	return vectorSub($Chess::lastOffset, $Chess::offsetAmount);
}

function ChessGame::resetBoard(%this)
{
	%this.clearBoard();
	%this.clearHighlightedTiles();
	%this.initialize();

	%this.currentTurn = %this.light;
}

function ChessGame::clearBoard(%this)
{
	%count = %this.brickGroup.getCount();

	for(%i = 0; %i < %count; %i++)
		%this.brickGroup.getObject(%i).setItem("");
}

function ChessGame::getMoveCount(%this, %client, %piece)
{
	return %this.moveCount[%client, %piece];
}

function ChessGame::getPieceLocation(%this, %client, %piece)
{
	return %this.pieceLocation[%client, %piece];
}

function ChessGame::isPieceAlive(%this, %client, %piece)
{
	return %this.pieceAlive[%client, %piece];
}

function ChessGame::getOwnerOfPiece(%this, %piece)
{
	return strStr(%piece.dataBlock.getName(), "white") != -1 ? %this.light : %this.dark;
}

function ChessGame::getDB(%this, %client, %pieceId)
{
	return %client.color @ "_" @ $Chess::DB[%this.theme, %pieceId];
}

function ChessGame::getColorDirection(%this, %color)
{
	return %color $= "white" ? $Chess::EAST : $Chess::WEST;
}

// location here is just name of the brick
function ChessGame::getBrick(%this, %location)
{
	return %this.brickGroup.ntObject["", %location, 0];
}

function ChessGame::getPieceAt(%this, %location)
{
	if(!isObject(%b = %this.getBrick(%location)))
		return 0;

	if(!isObject(%b.item))
		return 0;

	return %b.item;
}

function ChessGame::validMove(%this, %client, %pieceId, %desiredSpot)
{
	return strStr(%this.availableMoves[%client, %pieceId], %desiredSpot) != -1 && %desiredSpot !$= "";
}


function ChessGame::addPiece(%this, %client, %pieceId, %location)
{
	if(%this.isPieceAlive(%client, %pieceId))
	{
		%pieceLocation = %this.getPieceLocation(%client, %pieceId);
		%this.removePiece(%this, %pieceLocation);
	}

	%tile = %this.getBrick(%location);
	%tile.setItem(%this.getDB(%client, %pieceId));
	%tile.setItemDirection(%client.color $= "white" ? $Chess::EAST : $Chess::WEST);
	%tile.item.pieceId = %pieceId;

	%this.moveCount[%client, %pieceId] = 0;
	%this.pieceLocation[%client, %pieceId] = %location;
	%this.pieceAlive[%client, %pieceId] = true;
}

function ChessGame::movePiece(%this, %client, %pieceId, %location)
{
	%pieceLocation = %this.getPieceLocation(%client, %pieceId);

	// is there a piece at %location, if so remove and setalive = 0
	if(%piece = %this.getPieceAt(%location))
		%this.removePiece(%location);

	// remove piece from old location
	%brick = %this.getBrick(%pieceLocation);
	%brick.setItem("");

	// place piece at new location and set pieceId
	%brick = %this.getBrick(%location);

	if(%x = %this.truePieceId[%client, %pieceId])
		%brick.setItem(%this.getDB(%client, %x));
	else
		%brick.setItem(%this.getDB(%client, %pieceId));

	%brick.setItemDirection(%this.getColorDirection(%client.color));
	%brick.item.pieceId = %pieceId;

	// update move count and location
	%this.moveCount[%client, %pieceId]++;
	%this.pieceLocation[%client, %pieceId] = %location;

	// check if en passant becomes available
	// did they move a pawn and move 2 squares
	if(%this.getDB(%client, %pieceId) $= %this.getDB(%client, $Chess::PAWN0) && %this.tileFrom(%pieceLocation, 0, %client == %this.light ? 2 : -2) $= %location)
	{
		%opponent = %client == %this.light ? %this.dark : %this.light;

		// get the two pieces to the side of the pawn
		%piece1 = %this.getPieceAt(%pos1 = %this.tileFrom(%location, 1, 0));
		%piece2 = %this.getPieceAt(%pos2 = %this.tileFrom(%location, -1, 0));


		if(%piece1 && %this.getDB(%client, %piece1.pieceId) $= %this.getDB(%client, $Chess::PAWN0) && %this.playerOwnsPiece(%opponent, %piece1))
			%this.potentialEnPassant[%opponent] = %pos1 @ "|" @ %location;

		//talk(%otherClient.color);
		if(%piece2 && %this.getDB(%client, %piece2.pieceId) $= %this.getDB(%client, $Chess::PAWN0) && %this.playerOwnsPiece(%opponent, %piece2))
			%this.potentialEnPassant[%opponent] = %this.potentialEnPassant[%opponent] SPC %pos2 @ "|" @ %location;
	}


	// check if they moved a pawn to the other side of the board
	if(%this.getDB(%client, %pieceId) $= %this.getDB(%client, $Chess::PAWN0) && (%client == %this.light && getSubStr(%location, 1, 1) == 8 || %client == %this.dark && getSubStr(%location, 1, 1) == 1))
	{
		%this.removePiece(%pieceLocation);
		%brick.setItem(%this.getDB(%client, $Chess::QUEEN));
		%brick.setItemDirection(%this.getColorDirection(%client.color));
		%brick.item.pieceId = %pieceId;
		%this.truePieceId[%client, %pieceId] = $Chess::QUEEN;
	}


	// remove all potential en passant moves if there were any
	if(%this.potentialEnPassant[%client] !$= "")
		%this.potentialEnPassant[%client] = "";
}

function ChessGame::removePiece(%this, %location)
{
	if(!(%piece = %this.getPieceAt(%location)))
		return;

	%pieceId = %piece.pieceId;
	%client = %this.getOwnerOfPiece(%piece);

	%brick = %this.getBrick(%location);
	%brick.setItem("");

	%this.pieceLocation[%client, %pieceId] = "";
	%this.pieceAlive[%client, %pieceId] = false;
}

function ChessGame::highlightTile(%this, %tile, %color, %colorfx, %emitter)
{
	if(!isObject(%brick = %this.getBrick(%tile)))
		return;

	%brick.setColor(%color);
	%brick.setColorFx(%colorfx);
	%brick.setEmitter(%emitter);

	// add this tile to the list of highlighted tiles
	%this.highlightedTiles = trim(%this.highlightedTiles SPC %tile);
}

function ChessGame::highlightAvailableMoves(%this, %moves)
{
	// get the available color ids and fx
	if((%darkAvailableMoveColor = $Chess::darkAvailableMoveColorId[%this.theme]) $= "")
		%darkAvailableMoveColor = $Chess::darkAvailableMoveColorId[$Chess::Themes::DEFAULT];

	if((%lightAvailableMoveColor = $Chess::lightAvailableMoveColorId[%this.theme]) $= "")
		%lightAvailableMoveColor = $Chess::lightAvailableMoveColorId[$Chess::Themes::DEFAULT];

	if((%availableMoveColorFx = $Chess::availableMoveColorFxId[%this.theme]) $= "")
		%availableMoveColorFx = $Chess::availableMoveColorFxId[$Chess::Themes::DEFAULT];

	// highlight all the available moves
	%count = getWordCount(%moves);

	for(%i = 0; %i < %count; %i++)
	{
		%tile = getWord(%moves, %i);
		%brick = %this.getBrick(%tile);
		// if this tile is a dark tile, use the dark available move color id, otherwise use light available color id
		%this.highlightTile(%tile, %brick.getColorId() == $Chess::darkTileColorId[%this.theme] ? %darkAvailableMoveColor : %lightAvailableMoveColor, %availableMoveColorFx);
	}
}

function ChessGame::switchTurns(%this)
{
	%this.selectedPieceId = -1;
	%this.clearHighlightedTiles();

	%this.currentTurn = %this.currentTurn == %this.light ? %this.dark : %this.light;

	if(%this.finished)
		%this.currentTurn = -1;

	//messageClient(%this.currentTurn, '', "It is your turn.");
}

function ChessGame::setSelectedPiece(%this, %client, %pieceId)
{
	%this.selectedPieceId = %pieceId;

	// unhighlight the previously highlighted tiles
	%this.clearHighlightedTiles();

	// get the location of the piece and the available moves of this piece
	%location = %this.getPieceLocation(%client, %pieceId);
	%availableMoves = %this.availableMoves[%client, %pieceId];

	// get the color, colorfx, and emitter for the theme of the game
	if((%colorId = $Chess::selectionColorId[%this.theme]) $= "")
		%colorId = $Chess::selectionColorId[$Chess::Themes::DEFAULT];

	if((%colorFxId = $Chess::selectionColorFxId[%this.theme]) $= "")
		%colorFxId = $Chess::selectionColorFxId[$Chess::Themes::DEFAULT];

	if((%emitter = $Chess::selectionEmitter[%this.theme]) $= "")
		%emitter = $Chess::selectionEmitter[$Chess::Themes::DEFAULT];

	// highlight the location and the available moves
	%this.highlightTile(%location, %colorId, %colorFxId, %emitter);
	%this.highlightAvailableMoves(%availableMoves);
}

function ChessGame::clearHighlightedTiles(%this)
{
	// get number of highlighted tiles
	%count = getWordCount(%this.highlightedTiles);

	// remove all highlights
	for(%i = 0; %i < %count; %i++)
	{
		%tile = getWord(%this.highlightedTiles, %i);
		%brick = %this.getBrick(%tile);

		%brick.setColor(%brick.originalColor);
		%brick.setColorFx(%brick.originalColorFx);
		%brick.setEmitter(0);
	}

	%this.highlightedTiles = "";
}

function Chessgame::canCastle(%this, %client, %rookId)
{
	//talk("args: " @ %client.name SPC %rookId);
	// have they moved either piece yet?
	%opponent = %client.color $= "white" ? %this.dark : %this.light;
	if(!(%this.getMoveCount(%client, $Chess::KING) == 0 && %this.getMoveCount(%client, %rookId) == 0))
		return false;

	if(%this.isKingInCheck(%client))
		return false;

	// are the spaces between them empty and not under threat
	%column = getSubStr(%this.getPieceLocation(%client, %rookId), 0, 1);

	%row = %client.color $= "white" ? 1 : 8;
	if(%column $= "a")
	{
		%c = !%this.getPieceAt("b" @ %row) && !%this.getPieceAt("c" @ %row) && !%this.getPieceAt("d" @ %row);
		%d = strStr(%this.availableMoves[%opponent], "c" @ %row) == -1 && strStr(%this.availableMoves[%opponent], "d" @ %row) == -1;
	}
	else if(%column $= "h")
	{
		%c = !%this.getPieceAt("f" @ %row)  && !%this.getPieceAt("g" @ %row);
		%d = strStr(%this.availableMoves[%opponent], "f" @ %row) == -1 && strStr(%this.availableMoves[%opponent], "g" @ %row) == -1;
	}

	return %c && %d;
}

function ChessGame::performCastle(%this, %client, %rookId)
{
	// get the king and rook location
	%kingLocation = %this.getPieceLocation(%client, $Chess::KING);
	%rookLocation = %this.getPieceLocation(%client, %rookId);

	// if the rook is rook0, (on a8 / a1), then we need to move the king left
	if(%rookId == $Chess::ROOK0)
		%dx = -1;
	else
		%dx = 1;

	// move king 2 to the left / right and move the rook to the inside
	%newKingLocation = %this.tileFrom(%kingLocation, %dx * 2, 0);
	%newRookLocation = %this.tileFrom(%newKingLocation, -%dx, 0);

	%this.movePiece(%client, $Chess::KING, %newKingLocation);
	%this.movePiece(%client, %rookId, %newRookLocation);
}

function ChessGame::performEnPassant(%this, %client, %pawnId, %victimLoc, %desiredLocation)
{
	// remove the other players pawn
	%this.removePiece(%victimLoc);

	// move initators piece to new position
	%this.movePiece(%client, %pawnId, %desiredLocation);
}

function ChessGame::setKingInCheck(%this, %client, %threatPieceId, %pathToKing)
{
	// get the check color for this theme
	if((%checkColor = $Chess::checkColorId[%this.theme]) $= "")
		%checkColor = $Chess::checkColorId[$Chess::Themes::DEFAULT];

	if((%checkColorFx = $Chess::selectionColorFxId[%this.theme]) $= "")
		%checkColorFx = $Chess::selectionColorFxId[$Chess::Themes::DEFAULT];

	// highlight the checked king and threatening piece
	%opponent = %client == %this.light ? %this.dark : %this.light;
	%this.highlightTile(%this.getPieceLocation(%opponent, %threatPieceId), %checkColor, %checkColorFx);
	%this.highlightTile(%this.getPieceLocation(%client, $Chess::KING), %checkColor, %checkColorFx);

	// update the checks and path to king
	%this.activeCheck[%client] = trim(%this.activeCheck[%client] SPC %threatPieceId);
	%this.pathToKing[%client, %threatPieceId] = %pathToKing;
}

function ChessGame::doLogic(%this)
{
	//talk("it is " @ %this.currentTurn.name @ "'s turn.");
	%client = %this.currentTurn;
	%opponent = %this.currentTurn == %this.light ? %this.dark : %this.light;


	// reset all the previous moves from the last turn
	%this.activeCheck[%client] = "";
	%this.availableMoves[%client] = "";
	%this.unrestrictedMoves[%client] = "";


	%this.activeCheck[%opponent] = "";
	%this.availableMoves[%opponent] = "";
	%this.unrestrictedMoves[%opponent] = "";

	for(%i = 0; %i < 16; %i++)
	{
		%this.pathToKing[%client, %i] = "";
		%this.potentialCheck[%client, %i] = "";

		%this.pathToKing[%opponent, %i] = "";
		%this.potentialCheck[%opponent, %i] = "";
	}

	// generate unrestricted moves for opponent. the unrestricted moves contain all the moves
	// that could be under threat by %opponent
	%this.generateUnrestrictedMoveSet(%opponent);

	// generate available moves for each player
	%this.generateAvailableMoves(%opponent);
	%this.generateAvailableMoves(%client);

	
	if(%this.availableMoves[%client] $= "")
	{
		if(%this.isKingInCheck(%client))
			%this.endGame(%client == %this.player0 ? 1 : 0);
		else
			%this.endGame(3);
	}
}

function ChessGame::generateUnrestrictedMoveSet(%this, %client)
{
	if(!isobject(%client))
		return;

	for(%pieceId = 0; %pieceId < 16; %pieceId++)
	{
		if(!%this.isPieceAlive(%client, %pieceId))
			continue;

		%x = %this.truePieceId[%client, %pieceId];

		// get the location of the piece and the name of the piece
		%pieceLocation = %this.getPieceLocation(%client, %pieceId);
		%db = %this.getDB(%client, %x ? %x : %pieceId);
		%index = strStr(%db, "_");
		%hasSecondUnderScore = strPos(%db, "_", %index + 1);
		%pieceName = strReplace(strLwr(getSubStr(%db, %index + 1, strLen(%db))), %hasSecondUnderScore == -1 ? "item" : strLwr(getSubStr(%db, %hasSecondUnderScore, strLen(%db))), "");

		%this.unrestrictedMoves[%client, %pieceId] = %pieceName.getUnrestrictedMoves(%client, %pieceLocation);
		%this.unrestrictedMoves[%client] = trim(%this.unrestrictedMoves[%client] SPC %this.unrestrictedMoves[%client, %pieceId]);
	}
}

function ChessGame::generateAvailableMoves(%this, %client)
{
	if(!isobject(%client))
		return;

	for(%pieceId = 0; %pieceId < 16; %pieceId++)
	{
		if(!%this.isPieceAlive(%client, %pieceId))
			continue;

		%x = %this.truePieceId[%client, %pieceId];

		%pieceLocation = %this.getPieceLocation(%client, %pieceId);
		%db = %this.getDB(%client, %x ? %x : %pieceId);
		%index = strStr(%db, "_");
		%hasSecondUnderScore = strPos(%db, "_", %index + 1);
		%pieceName = strReplace(strLwr(getSubStr(%db, %index + 1, strLen(%db))), %hasSecondUnderScore == -1 ? "item" : strLwr(getSubStr(%db, %hasSecondUnderScore, strLen(%db))), "");

		%this.availableMoves[%client, %pieceId] = %pieceName.getAvailableMoves(%client, %pieceLocation, %client == %this.currentTurn ? false : true);
		%this.availableMoves[%client] = trim(%this.availableMoves[%client] SPC %this.availableMoves[%client, %pieceId]);
	}
}

function ChessGame::isKingInCheck(%this, %client)
{
	return %this.activeCheck[%client] !$= "";
}

function ChessGame::isPiecePinned(%this, %client, %pieceId)
{
	return %this.potentialCheck[%client, %pieceId] !$= "";    
}

function ChessGame::getPathToKing(%this, %client, %pieceId)
{
	return %this.pathToKing[%client, %pieceId];
}

function ChessGame::playerOwnsPiece(%this, %client, %item)
{
	return %item && strStr(%item.dataBlock.getName(), %client.color) != -1;
}

function ChessGame::isValidPosition(%this, %pos)
{
	%column = getSubStr(%pos, 0, 1);
	%row = getSubSTr(%pos, 1, 1);
	return strStr($Chess::letters, %column) != -1 && %row > 0 && %row < 9 && strLen(%pos) == 2; 
}

function ChessGame::tileFrom(%this, %pos, %offX, %offY)
{
	%x = strPos($Chess::letters, getSubStr(%pos, 0, 1)) + %offX;

	if(%x < 0)
		return "";

	%y = getSubStr(%pos, 1, 1) + %offY;
	%potentialTile = getSubStr($Chess::letters, %x, 1) @ %y;

	if(%this.isValidPosition(%potentialTile))
		return %potentialTile;
}

function ChessGame::isSpecialMove(%this, %client, %selectedPieceId, %originalLocation, %desiredLocation)
{
	// check for castling
	// if we are moving the king and the king is currently in original position
	if(%selectedPieceId == $Chess::KING && (%originalLocation $= "e8") || %originalLocation $= "e1") 
	{
		// is the desired location 2 to the right or left? if so this is castling
		if(%desiredLocation $= %this.tileFrom(%originalLocation, 2, 0))
		{
			%this.performCastle(%client, $Chess::ROOK1);
			return true;
		}
		else if(%desiredLocation $= %this.tileFrom(%originalLocation, -2, 0))
		{
			%this.performCastle(%client, $Chess::ROOK0);
			return true;
		}
	}
	// check for en passant
	// is the original location in the en passant move list
	else if(strStr(%this.potentialEnPassant[%client], %originalLocation) != -1)
	{
		// loop through each pair in the list until we find the one that has our position
		%pairs = %this.potentialEnPassant[%client];
		for(%i = 0; %i < getWordCount(%pairs); %i++)
		{
			if(strStr(%pair = getWord(%pairs, %i), %originalLocation) != -1)
			{
				// get the second token
				%dy = %client == %this.light ? 1 : -1;
				%opponentPawnLocation = getToken(%pair, "|", 1);

				if(%desiredLocation $= %this.tileFrom(%opponentPawnLocation, 0, %dy))
				{
					%this.performEnPassant(%client, %selectedPieceId, %opponentPawnLocation, %desiredLocation);
					return true;
				}
			}
		}
	}

	return false;
}

function getToken(%str, %delim, %index)
{
	for(%i = 0; %i <= %index; %i++)
		%str = nextToken(%str, "x", %delim);

	return %x;
}

function ChessGame::takeTurn(%this, %obj, %client)
{
	// is the player in a game and is it their turn
	if(%this.currentTurn != %client)
		return;

	%piece = %obj.item;
	%location = strReplace(%obj.getName(), "_", "");

	if(%piece && %this.playerOwnsPiece(%client, %piece)) {
		%this.setSelectedPiece(%client, %piece.pieceId);
		%obj.playSound($Chess::selectSound);
	}
	else if(%this.validMove(%client, %this.selectedPieceId, %location))
	{
		%selectedPieceId = %this.selectedPieceId;
		%originalLocation = %this.getPieceLocation(%this.currentTurn, %selectedPieceId);

		%targetLocationBrick = %this.brickgroup.NTObject["_" @ %location @ "_0"];
		if (isObject(%targetLocationBrick.item)) {
			%obj.playSound($Chess::takePieceSound);
		} else {
			%obj.playSound($Chess::moveSound);
		}
		
		if(!%this.isSpecialMove(%client, %selectedPieceId, %originalLocation, %location))
			%this.movePiece(%client, %this.selectedPieceId, %location);

		%this.switchTurns();
		%this.doLogic();

		if (isObject(%this.lastMovedBrick)) {
			%this.lastMovedBrick.setEmitter();
		}
		%targetLocationBrick.setEmitter($Chess::lastMoveEmitter[%this.theme]);
		%this.lastMovedBrick = %targetLocationBrick;
	}
}

function pawn::getUnrestrictedMoves(%this, %owner, %location)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	%dy = %owner == %chessGame.light ? 1 : -1;

	%possibleLocation1 = %chessGame.tileFrom(%location, 1, %dy);
	%possibleLocation2 = %chessGame.tileFrom(%location, -1, %dy);

	// if both locations are actually locations on the board, add them to unrestricted moves
	if(%chessGame.isValidPosition(%possibleLocation1))
	{
		%unrestrictedMoves = %possibleLocation1;

		// if this possibleLocation is the location of the other players king, we set their king in check
		if(%chessGame.getPieceLocation(%opponent, $Chess::KING) $= %possibleLocation1)
			%chessGame.setKingInCheck(%opponent, %pieceId, %location);
	}

	if(%chessGame.isValidPosition(%possibleLocation2))
	{
		%unrestrictedMoves = trim(%unrestrictedMoves SPC %possibleLocation2);

		if(%chessGame.getPieceLocation(%opponent, $Chess::KING) $= %possibleLocation2)
			%chessGame.setKingInCheck(%opponent, %pieceId, %location);
	}

	return %unrestrictedMoves;
}

// pawn
function pawn::getAvailableMoves(%this, %owner, %location, %passive)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	if(%owner == %chessGame.light)
		%dy = 1;
	else
		%dy = -1;

	// can we skip a tile
	%canSkip = (%owner == %chessGame.light && getSubStr(%location, 1, 1) == 2) || (%owner == %chessGame.dark && getSubStr(%location, 1, 1) == 7);

	%potentialMove1 = %chessGame.tileFrom(%location, 0, %dy);
	%potentialMove2 = %chessGame.tileFrom(%location, 0, %dy * 2);
	%potentialMove3 = %chessGame.tileFrom(%location, 1, %dy);
	%potentialMove4 = %chessGame.tileFrom(%location, -1, %dy);

	// if our king is an in check
	if(%chessGame.isKingInCheck(%owner))
	{
		%threateningPieces = %chessGame.activeCheck[%owner];
		%tpc = getWordCount(%threateningPieces);

		for(%i = 0; %i < %tpc; %i++)
			%pathToKing[%i] = %chessGame.getPathToKing(%owner, getWord(%threateningPieces, %i));
	}
	
	// our we pinned from moving
	if(%chessGame.isPiecePinned(%owner, %pieceId))
		%potentialPathToKing = %chessGame.potentialCheck[%owner, %pieceId];

	// we are pinned or the king is in check
	if(%threateningPieces !$= "" || %potentialPathToKing !$= "")
	{
		// if there are multiple checks or there is at least 1 check but this piece is pinned, it cant move anywhere
		if(getWordCount(%threateningPieces) > 1 || %threateningPieces !$= "" && %potentialPathToKing !$= "")
			return "";

		// at this point, the piece is either pinned with no checks or theres 1 check but piece isnt pinned
		if(%potentialPathToKing !$= "")
		{
			// moves to do when pinned
			if(%chessGame.isValidPosition(%potentialMove1) && !%chessGame.getPieceAt(%potentialMove1) && strStr(%potentialPathToKing, %potentialMove1) != -1)
				%availableMoves = %potentialMove1;

			if(%chessGame.isValidPosition(%potentialMove2) && strStr(%potentialPathToKing, %potentialMove2) != -1 && !%chessGame.getPieceAt(%potentialMove1) && !%chessGame.getPieceAt(%potentialMove2) && %canSkip)
				%availableMoves = trim(%availableMoves SPC %potentialMove2);

			if(%chessGame.isValidPosition(%potentialMove3) && (%piece = %chessGame.getPieceAt(%potentialMove3)) && !%chessGame.playerOwnsPiece(%owner, %piece) && strStr(%potentialPathToKing, %potentialMove3) != -1)
				%availableMoves = trim(%availableMoves SPC %potentialMove3);

			if(%chessGame.isValidPosition(%potentialMove4) && (%piece = %chessGame.getPieceAt(%potentialMove4)) && !%chessGame.playerOwnsPiece(%owner, %piece) && strStr(%potentialPathToKing, %potentialMove4) != -1)
				%availableMoves = trim(%availableMoves SPC %potentialMove4);

			%pairs = %chessGame.potentialEnPassant[%owner];
			for(%i = 0; %i < getWordCount(%pairs); %i++)
			{
				if(strStr(%pair = getWord(%pairs, %i), %location) != -1)
				{
					%potentialMove = %chessGame.tileFrom(getToken(%pair, "|", 1), 0, %dy);

					if(%chessGame.isValidPosition(%potentialMove) && strStr(%potentialPathToKing, %potentialMove) != -1)
						%availableMoves = trim(%availableMoves SPC %potentialMove);

					break;
				}
			}
		}
		else
		{
			// these are the only moves available to a pawn whose king is in check
			if(%chessGame.isValidPosition(%potentialMove1) && strStr(%pathToKing[0], %potentialMove1) != -1 && !%chessGame.getPieceAt(%potentialMove1))
				%availableMoves = %potentialMove1;

			if(%chessGame.isValidPosition(%potentialMove2) && strStr(%pathToKing[0], %potentialMove2) != -1 && !%chessGame.getPieceAt(%potentialMove1) && !%chessGame.getPieceAt(%potentialMove2) && %canSkip)
				%availableMoves = %potentialMove2;


			if(%chessGame.isValidPosition(%potentialMove3) && (%piece = %chessGame.getPieceAt(%potentialMove3)) && !%chessGame.playerOwnsPiece(%owner, %piece) && strStr(%pathToKing[0], %potentialMove3) != -1)
				%availableMoves = trim(%availableMoves SPC %potentialMove3);

			if(%chessGame.isValidPosition(%potentialMove4) && (%piece = %chessGame.getPieceAt(%potentialMove4)) && !%chessGame.playerOwnsPiece(%owner, %piece) && strStr(%pathToKing[0], %potentialMove4) != -1)
				%availableMoves = trim(%availableMoves SPC %potentialMove4);

			// en passant to block king
			%pairs = %chessGame.potentialEnPassant[%owner];
			for(%i = 0; %i < getWordCount(%pairs) && !%canSkip; %i++)
			{
				if(strStr(%pair = getWord(%pairs, %i), %location) != -1)
				{
					%potentialMove = %chessGame.tileFrom(getToken(%pair, "|", 1), 0, %dy);

					if(%chessGame.isValidPosition(%potentialMove) && strStr(%pathToKing[0], %potentialMove) != -1)
						%availableMoves = trim(%availableMoves SPC %potentialMove);

					break;
				}
			}
		}
	}
	else
	{
		if(%passive)
		{
			if(%chessGame.isValidPosition(%potentialMove3))
				%availableMoves = trim(%availableMoves SPC %potentialMove3);

			if(%chessGame.isValidPosition(%potentialMove4))
				%availableMoves = trim(%availableMoves SPC %potentialMove4);
		}
		else
		{

			// no conditions, calculate available moves normally
			if(%chessGame.isValidPosition(%potentialMove1) && !%chessGame.getPieceAt(%potentialMove1))
					%availableMoves = %potentialMove1;

			if(%chessGame.isValidPosition(%potentialMove2) && !%chessGame.getPieceAt(%potentialMove1) && !%chessGame.getPieceAt(%potentialMove2) && %canSkip)
				%availableMoves = trim(%availableMoves SPC %potentialMove2);

			if(%chessGame.isValidPosition(%potentialMove3) && (%piece = %chessGame.getPieceAt(%potentialMove3)) && !%chessGame.playerOwnsPiece(%owner, %piece))
				%availableMoves = trim(%availableMoves SPC %potentialMove3);

			if(%chessGame.isValidPosition(%potentialMove4) && (%piece = %chessGame.getPieceAt(%potentialMove4)) && !%chessGame.playerOwnsPiece(%owner, %piece))
				%availableMoves = trim(%availableMoves SPC %potentialMove4);


			// if we can skip a tile, then we're in no position to perform en passant
			%pairs = %chessGame.potentialEnPassant[%owner];
			for(%i = 0; %i < getWordCount(%pairs) && !%canSkip; %i++)
			{
				if(strStr(%pair = getWord(%pairs, %i), %location) != -1)
				{
					%availableMoves = trim(%availableMoves SPC %chessGame.tileFrom(getToken(%pair, "|", 1), 0, %dy));
					break;
				}
			}
		}
	}

	return %availableMoves;
}


function rook::getUnrestrictedMoves(%this, %owner, %location)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	%opponentKingLocation = %chessGame.getPieceLocation(%opponent, $Chess::KING);

	// iterate over the 4 possible directions the rook and iterate through the tiles in each direction
	for(%i = 0; %i < 4; %i++)
	{
		// the direction to iterate tiles in
		%dx = getWord("0 1 0 -1", %i);
		%dy = getWord("1 0 -1 0", %i);

		for(%j = 0; %j < 8; %j++)
		{
			// calculate the new location
			%newLocation = %chessGame.tileFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(%newLocation $= %opponentKingLocation)
					%encounteredKing = %i;

			if(!%chessGame.isValidPosition(%newLocation))
				break;

			%path[%i] = trim(%path[%i] SPC %newLocation);

			// if we own the piece at the new location, we break now that we added its location to the path
			if(%chessGame.playerOwnsPiece(%owner, %chessGame.getPieceAt(%newLocation)))
					break;

			// get the pieces between the opponents king and this piece
			if((%piece = %chessGame.getPieceAt(%newLocation)) && %piece.pieceId != $Chess::KING && %encounteredKing $= "")
				%pathPieces[%i] = trim(%pathPieces[%i] SPC %piece.pieceId);
		}

		%unrestrictedMoves = trim(%unrestrictedMoves SPC %path[%i]);
		%chessGame.unrestrictedMoves[%owner, %pieceId, %i] = %path[%i];
	}

	// if we encountered the king, the specific path is stored in the variable
	if(%encounteredKing !$= "")
	{
		%path[%encounteredKing] = trim(getField(strReplace(%path[%encounteredKing], %opponentKingLocation, "\t"), 0));

		if(getWordCount(%pathPieces[%encounteredKing]) == 0)
			%chessGame.setKingInCheck(%opponent, %pieceId, %location SPC %path[%encounteredKing]);

		// if theres 1 piece between our piece and the opponents king, thats a potential check (pin on the piece between)
		else if(getWordCount(%pathPieces[%encounteredKing]) == 1)
			%chessGame.potentialCheck[%opponent, %pathPieces[%encounteredKing]] = %location SPC %path[%encounteredKing];
			//talk("A potential check has been made against " @ %opponent.name @ "'s king.");
	}

	return %unrestrictedMoves;
}

function rook::getAvailableMoves(%this, %owner, %location, %passive)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	// generate all the potential moves of a rook
	for(%i = 0; %i < 4; %i++)
	{
		%dx = getWord("0 1 0 -1", %i);
		%dy = getWord("1 0 -1 0", %i);

		for(%j = 0; %j < 8; %j++)
		{
			%possibleLocation = %chessGame.tileFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(!%chessGame.isValidPosition(%possibleLocation))
				break;

			%path[%i] = trim(%path[%i] SPC %possibleLocation);
		}
	}

	if(%chessGame.isKingInCheck(%owner))
	{
		%threateningPieces = %chessGame.activeCheck[%owner];
		%tpc = getWordCount(%threateningPieces);

		for(%i = 0; %i < %tpc; %i++)
			%pathToKing[%i] = %chessGame.getPathToKing(%owner, getWord(%threateningPieces, %i));
	}
	
	// our we pinned from moving
	if(%chessGame.isPiecePinned(%owner, %pieceId))
		%potentialPathToKing = %chessGame.potentialCheck[%owner, %pieceId];

	// we are pinned or the king is in check
	if(%threateningPieces !$= "" || %potentialPathToKing !$= "")
	{
		// if there are multiple checks or there is at least 1 check but this piece is pinned, it cant move anywhere
		if(getWordCount(%threateningPieces) > 1 || %threateningPieces !$= "" && %potentialPathToKing !$= "")
			return "";

		// at this point, the piece is either pinned with no checks or theres 1 check but piece isnt pinned
		if(%potentialPathToKing !$= "")
		{
			for(%i = 0; %i < 4; %i++)
			{
				for(%j = 0; %j < getWordCount(%path[%i]); %j++)
				{
					%possibleLocation = getWord(%path[%i], %j);
					if(strStr(%potentialPathToKing, %possibleLocation) == -1)
						break;

					%availableMoves = trim(%availableMoves SPC %possibleLocation);
				}
			}
		}
		else
		{
			for(%i = 0; %i < 4; %i++)
			{
				for(%j = 0; %j < getWordCount(%path[%i]); %j++)
				{
					%possibleLocation = getWord(%path[%i], %j);
					//talk(%possibleLocation);
					if((%piece = %chessGame.getPieceAt(%possibleLocation)) && strStr(%pathToKing[0], %possibleLocation)  == -1)
						break;


					if(strStr(%pathToKing[0], %possibleLocation) != -1)
					{
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
						break;
					}
				}
			}
		}
	}
	else
	{
		for(%i = 0; %i < 4; %i++)
		{
			for(%j = 0; %j < getWordCount(%path[%i]); %j++)
			{
				%possibleLocation = getWord(%path[%i], %j);

				if(%piece = %chessGame.getPieceAt(%possibleLocation))
				{
					if(!%chessGame.playerOwnsPiece(%owner, %piece))
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
					else if(%passive)
					{
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
						break;
					}

					if(%passive && %possibleLocation $= %chessGame.getPieceLocation(%opponent, $Chess::KING))
						continue;
					else
						break;
				}

				%availableMoves = trim(%availableMoves SPC %possibleLocation);
			}
		}
	}

	return %availableMoves;
}

function knight::getUnrestrictedMoves(%this, %owner, %location)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	for(%i = 0; %i < 8; %i++)
	{
		%possibleLocation = %chessGame.tileFrom(%location, getWord("1 2 2 1 -1 -2 -2 -1", %i), getWord("2 1 -1 -2 -2 -1 1 2", %i));

		if(%chessGame.isValidPosition(%possibleLocation))
		{
			%unrestrictedMoves = trim(%unrestrictedMoves SPC %possibleLocation);

			if(%possibleLocation $= %chessGame.getPieceLocation(%opponent, $Chess::KING))
				%chessGame.setKingInCheck(%opponent, %pieceId, %location);
		}
	}

	return %unrestrictedMoves;
}

function knight::getAvailableMoves(%this, %owner, %location, %passive)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	for(%i = 0; %i < 8; %i++)
	{
		%newLocation = %chessGame.tileFrom(%location, getWord("1 2 2 1 -1 -2 -2 -1", %i), getWord("2 1 -1 -2 -2 -1 1 2", %i));
		%potentialMove[%i] = %newLocation;
	}

	if(%chessGame.isKingInCheck(%owner))
	{
		%threateningPieces = %chessGame.activeCheck[%owner];
		%tpc = getWordCount(%threateningPieces);

		for(%i = 0; %i < %tpc; %i++)
			%pathToKing[%i] = %chessGame.getPathToKing(%owner, getWord(%threateningPieces, %i));
	}
	
	// our we pinned from moving
	if(%chessGame.isPiecePinned(%owner, %pieceId))
		%potentialPathToKing = %chessGame.potentialCheck[%owner, %pieceId];

	// we are pinned or the king is in check
	if(%threateningPieces !$= "" || %potentialPathToKing !$= "")
	{
		// if there are multiple checks or there is at least 1 check but this piece is pinned, it cant move anywhere
		if(getWordCount(%threateningPieces) > 1 || %threateningPieces !$= "" && %potentialPathToKing !$= "")
			return "";

		// at this point, the piece is either pinned with no checks or theres 1 check but piece isnt pinned
		if(%potentialPathToKing !$= "")
		{
			for(%i = 0; %i < 8; %i++)
			{
				if(%chessGame.isValidPosition(%potentialMove[%i]) && strStr(%potentialPathToKing, %potentialMove[%i]) != -1)
					%availableMoves = trim(%availableMoves SPC %potentialMove[%i]);
			}
		}
		else
		{
			for(%i = 0; %i < 8; %i++)
			{
				if(%chessGame.isValidPosition(%potentialMove[%i]) && strStr(%pathToKing[0], %potentialMove[%i]) != -1)
					%availableMoves = trim(%availableMoves SPC %potentialMove[%i]);
			}
		}
	}
	else
	{
		for(%i = 0; %i < 8; %i++)
		{
			if(%chessGame.isValidPosition(%potentialMove[%i]) && (%passive || !%chessGame.playerOwnsPiece(%owner, %chessGame.getPieceAt(%potentialMove[%i]))))
				%availableMoves = trim(%availableMoves SPC %potentialMove[%i]);
		}
	}

	return %availableMoves;
}

function bishop::getUnrestrictedMoves(%this, %owner, %location)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	%opponentKingLocation = %chessGame.getPieceLocation(%opponent, $Chess::KING);

	// iterate over the 4 possible directions the rook and iterate through the tiles in each direction
	for(%i = 0; %i < 4; %i++)
	{
		// the direction to iterate tiles in
		%dx = getWord("1 1 -1 -1", %i);
		%dy = getWord("1 -1 -1 1", %i);

		for(%j = 0; %j < 8; %j++)
		{
			// calculate the new location
			%newLocation = %chessGame.tileFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(%newLocation $= %opponentKingLocation)
					%encounteredKing = %i;

			if(!%chessGame.isValidPosition(%newLocation))
				break;

			%path[%i] = trim(%path[%i] SPC %newLocation);

			// if we own the piece at the new location, we break now that we added its location to the path
			if(%chessGame.playerOwnsPiece(%owner, %chessGame.getPieceAt(%newLocation)))
					break;

			// get the pieces between the opponents king and this piece
			if((%piece = %chessGame.getPieceAt(%newLocation)) && %piece.pieceId != $Chess::KING && %encounteredKing $= "")
				%pathPieces[%i] = trim(%pathPieces[%i] SPC %piece.pieceId);
		}

		%unrestrictedMoves = trim(%unrestrictedMoves SPC %path[%i]);
		%chessGame.unrestrictedMoves[%owner, %pieceId, %i] = %path[%i];
	}

	// if we encountered the king, the specific path is stored in the variable
	if(%encounteredKing !$= "")
	{
		%path[%encounteredKing] = trim(getField(strReplace(%path[%encounteredKing], %opponentKingLocation, "\t"), 0));

		if(getWordCount(%pathPieces[%encounteredKing]) == 0)
			%chessGame.setKingInCheck(%opponent, %pieceId, %location SPC %path[%encounteredKing]);

		// if theres 1 piece between our piece and the opponents king, thats a potential check (pin on the piece between)
		else if(getWordCount(%pathPieces[%encounteredKing]) == 1)
			%chessGame.potentialCheck[%opponent, %pathPieces[%encounteredKing]] = %location SPC %path[%encounteredKing];
			//talk("A potential check has been made against " @ %opponent.name @ "'s king.");
	}

	return %unrestrictedMoves;
}

function bishop::getAvailableMoves(%this, %owner, %location, %passive)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	// generate all the potential moves of a bishop
	for(%i = 0; %i < 4; %i++)
	{
		%dx = getWord("1 1 -1 -1", %i);
		%dy = getWord("1 -1 -1 1", %i);

		for(%j = 0; %j < 8; %j++)
		{
			%possibleLocation = %chessGame.tileFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(!%chessGame.isValidPosition(%possibleLocation))
				break;

			%path[%i] = trim(%path[%i] SPC %possibleLocation);
		}
	}

	if(%chessGame.isKingInCheck(%owner))
	{
		%threateningPieces = %chessGame.activeCheck[%owner];
		%tpc = getWordCount(%threateningPieces);

		for(%i = 0; %i < %tpc; %i++)
			%pathToKing[%i] = %chessGame.getPathToKing(%owner, getWord(%threateningPieces, %i));
	}
	
	// our we pinned from moving
	if(%chessGame.isPiecePinned(%owner, %pieceId))
		%potentialPathToKing = %chessGame.potentialCheck[%owner, %pieceId];

	// we are pinned or the king is in check
	if(%threateningPieces !$= "" || %potentialPathToKing !$= "")
	{
		// if there are multiple checks or there is at least 1 check but this piece is pinned, it cant move anywhere
		if(getWordCount(%threateningPieces) > 1 || %threateningPieces !$= "" && %potentialPathToKing !$= "")
			return "";

		// at this point, the piece is either pinned with no checks or theres 1 check but piece isnt pinned
		if(%potentialPathToKing !$= "")
		{
			for(%i = 0; %i < 4; %i++)
			{
				for(%j = 0; %j < getWordCount(%path[%i]); %j++)
				{
					%possibleLocation = getWord(%path[%i], %j);
					if(strStr(%potentialPathToKing, %possibleLocation) == -1)
						break;

					%availableMoves = trim(%availableMoves SPC %possibleLocation);
				}
			}
		}
		else
		{
			for(%i = 0; %i < 4; %i++)
			{
				for(%j = 0; %j < getWordCount(%path[%i]); %j++)
				{
					%possibleLocation = getWord(%path[%i], %j);
					//talk(%possibleLocation);
					if((%piece = %chessGame.getPieceAt(%possibleLocation)) && strStr(%pathToKing[0], %possibleLocation)  == -1)
						break;


					if(strStr(%pathToKing[0], %possibleLocation) != -1)
					{
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
						break;
					}
				}
			}
		}
	}
	else
	{
		for(%i = 0; %i < 4; %i++)
		{
			for(%j = 0; %j < getWordCount(%path[%i]); %j++)
			{
				%possibleLocation = getWord(%path[%i], %j);

				if(%piece = %chessGame.getPieceAt(%possibleLocation))
				{
					if(!%chessGame.playerOwnsPiece(%owner, %piece))
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
					else if(%passive)
					{
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
						break;
					}

					if(%passive && %possibleLocation $= %chessGame.getPieceLocation(%opponent, $Chess::KING))
						continue;
					else
						break;
				}

				%availableMoves = trim(%availableMoves SPC %possibleLocation);
			}
		}
	}

	return %availableMoves;
}

function queen::getUnrestrictedMoves(%this, %owner, %location)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	%opponentKingLocation = %chessGame.getPieceLocation(%opponent, $Chess::KING);

	// iterate over the 4 possible directions the rook and iterate through the tiles in each direction
	for(%i = 0; %i < 8; %i++)
	{
		// the direction to iterate tiles in
		%dx = getWord("0 1 1 1 0 -1 -1 -1", %i);
		%dy = getWord("1 1 0 -1 -1 -1 0 1", %i);

		for(%j = 0; %j < 8; %j++)
		{
			// calculate the new location
			%newLocation = %chessGame.tileFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(%newLocation $= %opponentKingLocation)
					%encounteredKing = %i;

			if(!%chessGame.isValidPosition(%newLocation))
				break;

			%path[%i] = trim(%path[%i] SPC %newLocation);

			// if we own the piece at the new location, we break now that we added its location to the path
			if(%chessGame.playerOwnsPiece(%owner, %chessGame.getPieceAt(%newLocation)))
					break;

			// get the pieces between the opponents king and this piece
			if((%piece = %chessGame.getPieceAt(%newLocation)) && %piece.pieceId != $Chess::KING && %encounteredKing $= "")
				%pathPieces[%i] = trim(%pathPieces[%i] SPC %piece.pieceId);
		}

		%unrestrictedMoves = trim(%unrestrictedMoves SPC %path[%i]);
		%chessGame.unrestrictedMoves[%owner, %pieceId, %i] = %path[%i];
	}

	// if we encountered the king, the specific path is stored in the variable
	if(%encounteredKing !$= "")
	{
		%path[%encounteredKing] = trim(getField(strReplace(%path[%encounteredKing], %opponentKingLocation, "\t"), 0));

		if(getWordCount(%pathPieces[%encounteredKing]) == 0)
			%chessGame.setKingInCheck(%opponent, %pieceId, %location SPC %path[%encounteredKing]);

		// if theres 1 piece between our piece and the opponents king, thats a potential check (pin on the piece between)
		else if(getWordCount(%pathPieces[%encounteredKing]) == 1)
			%chessGame.potentialCheck[%opponent, %pathPieces[%encounteredKing]] = %location SPC %path[%encounteredKing];
			//talk("A potential check has been made against " @ %opponent.name @ "'s king.");
	}

	return %unrestrictedMoves;
}

function queen::getAvailableMoves(%this, %owner, %location, %passive)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	// generate all the potential moves of a bishop
	for(%i = 0; %i < 8; %i++)
	{
		%dx = getWord("0 1 1 1 0 -1 -1 -1", %i);
		%dy = getWord("1 1 0 -1 -1 -1 0 1", %i);

		for(%j = 0; %j < 8; %j++)
		{
			%possibleLocation = %chessGame.tileFrom(%location, %dx * (%j + 1), %dy * (%j + 1));

			if(!%chessGame.isValidPosition(%possibleLocation))
				break;

			%path[%i] = trim(%path[%i] SPC %possibleLocation);
		}
	}

	if(%chessGame.isKingInCheck(%owner))
	{
		%threateningPieces = %chessGame.activeCheck[%owner];
		%tpc = getWordCount(%threateningPieces);

		for(%i = 0; %i < %tpc; %i++)
			%pathToKing[%i] = %chessGame.getPathToKing(%owner, getWord(%threateningPieces, %i));
	}
	
	// our we pinned from moving
	if(%chessGame.isPiecePinned(%owner, %pieceId))
		%potentialPathToKing = %chessGame.potentialCheck[%owner, %pieceId];

	// we are pinned or the king is in check
	if(%threateningPieces !$= "" || %potentialPathToKing !$= "")
	{
		// if there are multiple checks or there is at least 1 check but this piece is pinned, it cant move anywhere
		if(getWordCount(%threateningPieces) > 1 || %threateningPieces !$= "" && %potentialPathToKing !$= "")
			return "";

		// at this point, the piece is either pinned with no checks or theres 1 check but piece isnt pinned
		if(%potentialPathToKing !$= "")
		{
			for(%i = 0; %i < 8; %i++)
			{
				for(%j = 0; %j < getWordCount(%path[%i]); %j++)
				{
					%possibleLocation = getWord(%path[%i], %j);
					if(strStr(%potentialPathToKing, %possibleLocation) == -1)
						break;

					%availableMoves = trim(%availableMoves SPC %possibleLocation);
				}
			}
		}
		else
		{
			for(%i = 0; %i < 8; %i++)
			{
				for(%j = 0; %j < getWordCount(%path[%i]); %j++)
				{
					%possibleLocation = getWord(%path[%i], %j);
					//talk(%possibleLocation);
					if((%piece = %chessGame.getPieceAt(%possibleLocation)) && strStr(%pathToKing[0], %possibleLocation)  == -1)
						break;


					if(strStr(%pathToKing[0], %possibleLocation) != -1)
					{
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
						break;
					}
				}
			}
		}
	}
	else
	{
		for(%i = 0; %i < 8; %i++)
		{
			for(%j = 0; %j < getWordCount(%path[%i]); %j++)
			{
				%possibleLocation = getWord(%path[%i], %j);

				if(%piece = %chessGame.getPieceAt(%possibleLocation))
				{
					if(!%chessGame.playerOwnsPiece(%owner, %piece))
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
					else if(%passive)
					{
						%availableMoves = trim(%availableMoves SPC %possibleLocation);
						break;
					}

					if(%passive && %possibleLocation $= %chessGame.getPieceLocation(%opponent, $Chess::KING))
						continue;
					else
						break;
				}

				%availableMoves = trim(%availableMoves SPC %possibleLocation);
			}
		}
	}

	return %availableMoves;
}

function king::getUnrestrictedMoves(%this, %owner, %location)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	for(%i = 0; %i < 8; %i++)
	{
		%possibleLocation = %chessGame.tileFrom(%location, getWord("0 1 1 1 0 -1 -1 -1", %i), getWord("1 1 0 -1 -1 -1 0 1", %i));

		if(%chessGame.isValidPosition(%possibleLocation))
			%unrestrictedMoves = trim(%unrestrictedMoves SPC %possibleLocation);
	}

	return %unrestrictedMoves;
}

function king::getAvailableMoves(%this, %owner, %location, %passive)
{
	if(!isObject(%chessGame = %owner.boardGame))
		return;

	%opponent = %chessGame.light == %owner ? %chessGame.dark : %chessGame.light;
	%pieceId = %chessGame.getPieceAt(%location).pieceId;

	for(%i = 0; %i < 8; %i++)
	{
		%newLocation = %chessGame.tileFrom(%location, getWord("0 1 1 1 0 -1 -1 -1", %i), getWord("1 1 0 -1 -1 -1 0 1", %i));
		%potentialMove[%i] = %newLocation;
	}

	// passive part so other king cant put itself into checck when near this king
	if(%passive)
	{
		for(%i = 0; %i < 8; %i++)
		{
			if(%chessGame.isValidPosition(%potentialMove[%i]))
				%availableMoves = trim(%availableMoves SPC %potentialMove[%i]);
		}
	}
	else
	{
		//talk("loop starts");
		// loop through all potential moves
		for(%i = 0; %i < 8; %i++)
		{
			%testLocation = %potentialMove[%i];

			if(!%chessGame.isValidPosition(%testLocation))
				continue;


			if(%chessGame.playerOwnsPiece(%owner, %chessGame.getPieceAt(%testLocation)))
				continue;


			if(strStr(%chessGame.availableMoves[%opponent], %testLocation) != -1)
				continue;


			%availableMoves = trim(%availableMoves SPC %testLocation);
		}

		if(%chessGame.canCastle(%owner, $Chess::ROOK0))
			%availableMoves = trim(%availableMoves SPC %chessGame.tileFrom(%location, -2, 0));

		if(%chessGame.canCastle(%owner, $Chess::ROOK1))
			%availableMoves = trim(%availableMoves SPC %chessGame.tileFrom(%location, 2, 0));
	}

	return %availableMoves;
}


if(!isObject(pawn))
{
	new scriptObject(pawn);
	new scriptObject(rook);
	new scriptObject(knight);
	new scriptObject(bishop);
	new scriptObject(queen);
	new scriptObject(king);
}
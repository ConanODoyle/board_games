//%1 - ban victim
//%2 - bounty winner
//%3 - original ban initiator
//%4 - ban time
//%5 - ban reason
//\c0 = red
//\c1 = blue
//\c2 = green
//\c3 = yellow
//\c4 = cyan
//\c5 = purple
//\c6 = white
//\c7 = grey
//\c8 = black

$Pref::Server::BountyBan::bountyMsgCnt = 3;
$Pref::Server::BountyBan::bountyMsg0 = '\c5A \c3%4 \c5bounty has been placed on \c3%1\c5\'s head!';
$Pref::Server::BountyBan::bountyMsg1 = '\c5Kill them to claim the bounty!';
$Pref::Server::BountyBan::bountyMsg2 = '\c5Reason: \c6%5';

$Pref::Server::BountyBan::bountyClaimedMsg = '\c3%1\c5\'s bounty has been claimed by \c3%2\c5! \c7(Bounty placed by %3)';
$Pref::Server::BountyBan::bountyClearedMsg = '\c3%1\c5\'s bounty has been cleared by \c3%2\c5! \c7(Bounty placed by %3)';
$Pref::Server::BountyBan::bountyBanMsg = '\c3%2\c2 claimed %1\'s (ID: %3) bounty (%4): "%5"';
$Pref::Server::BountyBan::bountyBanDlg = "sent to davy jones\' locker<br><br>Time: ";
$Pref::Server::BountyBan::forceKillMsg = '\c3%1\c5 has been stricken down by \c3%2\c5!';
$Pref::Server::BountyBan::overrideKicks = 0;
$Pref::Server::BountyBan::namePrefix = "FUGITIVE";
$Pref::Server::BountyBan::shapeNameColor0 = "1 0 0 1";
$Pref::Server::BountyBan::shapeNameColor1 = "1 1 1 1";
$Pref::Server::BountyBan::canSuicide = 0;
$Pref::Server::BountyBan::suicideDeniedMsg = "You cannot suicide - you have a ban bounty on your head!";
$Pref::Server::BountyBan::music = "musicData_Inner_Circle_DASH_Bad_Boys";

package BountyBan {
	function serverCmdSuicide(%cl) {
		if (%cl.hasBounty && !%Pref::Server::BountyBan::canSuicide) {
			messageClient(%cl, '', $Pref::Server::BountyBan::suicideDeniedMsg);
			return;
		}
		return parent::serverCmdSuicide(%cl);
	}

	function serverCmdKick(%cl, %target, %reason) {
		if ($Pref::Server::BountyBan::overrideKicks) {
			serverCmdBan(%cl, (%target = findClientByName(%target)), %target.getBLID(), 0, %reason $= "" ? "N/A" : %reason);
		} else {
			if (%target.getClassName() !$= "GameConnection") {
				%targetCl = findClientByName(%target);
				if (%targetCl.getClassName() !$= "GameConnection") {
					%target = findClientByBL_ID(%target);
				} else {
					%target = %targetCl;
				}
			}
			return parent::serverCmdKick(%cl, %target, %reason);
		}
	}

	function serverCmdBan(%cl, %bannee, %banneeID, %time, %reason) {
		if (!isObject(%bannee)) {
			return;
		} else if (%bannee.isSuperAdmin) {
			//messageClient(%cl, 'MsgAdminForce', "\c5You can't kick/ban Super Admins!");
			//return;
		}
		%bounty = (%time == 0 ? "kick" : (%time < 0 ? "permanent ban" : %time @ " minute ban" @ (%time > 1 ? "s" : "")));
		for (%i = 0; %i < $Pref::Server::BountyBan::bountyMsgCnt; %i++) {
			messageAll('', $Pref::Server::BountyBan::bountyMsg[%i], %bannee.name, "", "", %bounty, %reason);
		}
		//%bannee.player.setShapeName($Pref::Server::BountyBan::namePrefix SPC %bannee.name,"8564862");
		//%bannee.player.setShapeNameColor($Pref::Server::BountyBan::shapeNameColor);
		if (!isObject(%bannee.player)) {
			%bannee.instantRespawn();
		}
		%bannee.player.setShapeNameDistance(100000);
		%bannee.player.playAudio(0, $Pref::Server::BountyBan::music);

		%bannee.hasBounty = 1;
		%bannee.bountyReason = %reason;
		%bannee.bountyTime = %time;
		%bannee.bountyOwner = %cl.name;
		shapeNameColorSchedule(%bannee);
	}

	function GameConnection::onDrop(%cl, %param) {
		if (%cl.hasBounty) {
			%time = %cl.bountyTime;
			%reason = %cl.bountyReason;
			if (isObject(%cl.player)) {
				messageAll('', "\c7" @ %cl.name @ " pussied out and ran from their bounty!");
				createBan(%cl, %cl, %cl.getBLID(), %time, %reason);
			}
		}
		return parent::onDrop(%cl, %param);
	}

	function serverCmdKill(%cl, %target) {
		if (!%cl.isAdmin) {
			return;
		}

		if ((%target = findClientByName(%target)).hasBounty) {
			%time = %target.bountyTime;
			messageAll('', $Pref::Server::BountyBan::forceKillMsg, %target.name, %cl.name);
			%target.delete($Pref::Server::BountyBan::bountyBanDlg @ %time @ " minutes");
		}
		if (isObject(%target.player)) {
			%target.player.kill();
		}
	}

    function gameConnection::onDeath(%this, %player, %killer, %damageType, %location)
    {
    	if (%this.hasBounty && %killer != %this) {
    		claimBounty(%killer, %this);
    	} else if (%this.hasBounty) {
			messageAll('', "\c7" @ %this.name @ " pussied out and commited suicide to avoid their bounty!");
			createBan(%this, %this, %this.getBLID(), %time, %reason);
			%this.hasBounty = 0;
			%this.delete($Pref::Server::BountyBan::bountyBanDlg @ %time @ " minutes");
    	}
        
        return parent::onDeath(%this, %player, %killer, %damageType, %location);
    }
};
activatePackage(BountyBan);

function claimBounty(%cl, %bannee) {
	%time = %bannee.bountyTime;
	%reason = %bannee.bountyReason;
	%bountyOwner = %bannee.bountyOwner;
	createBan(%cl, %bannee, %bannee.getBLID(), %time, %reason);

	messageAll('', $Pref::Server::BountyBan::bountyClaimedMsg, %bannee.name, %cl.name, %bountyOwner);
	messageAll('', $Pref::Server::BountyBan::bountyBanMsg, %bannee.name, %cl.name, %bannee.getBLID(), %time SPC "minute" @ (%time > 0 ? "s" : ""), %reason);
	%bannee.clearEventSchedules();
	%bannee.delete($Pref::Server::BountyBan::bountyBanDlg @ %time SPC "minute" @ (%time > 0 ? "s" : ""));
}

function createBan(%banner, %bannee, %banneeID, %time, %reason) {
	BanManagerSO.addBan(%banner, %bannee, %banneeID, %reason, %time);
	BanManagerSO.saveBans();
}

function shapeNameColorSchedule(%target) {
	if (!isObject(%target)) {
		return;
	}
	if (isEventPending(%target.bountyNameColorSchedule)) {
		cancel(%target.bountyNameColorSchedule);
	}

	if (isObject(%pl = %target.player) && %target.hasBounty) {
		%pl.setShapeName($Pref::Server::BountyBan::namePrefix SPC %target.name,"8564862");
		if (%pl.shapeNameColored) {
			%pl.setShapeNameColor($Pref::Server::BountyBan::shapeNameColor0);
			%pl.shapeNameColored = 0;
			%target.bountyNameColorSchedule = schedule(800, 0, shapeNameColorSchedule, %target);
			return;
		} else {
			%pl.setShapeNameColor($Pref::Server::BountyBan::shapeNameColor1);
			%pl.shapeNameColored = 1;
			%target.bountyNameColorSchedule = schedule(800, 0, shapeNameColorSchedule, %target);
			return;
		}
	}
}

function serverCmdCancelBounty(%cl, %target) {
	if (!%cl.isAdmin) {
		return;
	}

	%target = findClientByName(%target);
	if (!%target.hasBounty) {
		return;
	}
	if (isEventPending(%target.bountyNameColorSchedule)) {
		cancel(%target.bountyNameColorSchedule);
	}
	if (isObject(%pl = %target.player) && %target.hasBounty) {
		%pl.setShapeName(%target.name,"8564862");
		if (isObject(%target.minigame)) {
			%pl.setShapeNameColor(%target.minigame.colorRGB);
			%pl.setShapeNameDistance(%target.minigame.nameDistance);
		} else {
			%pl.setShapeNameColor("1 1 1 1");
		}
		%pl.stopAudio(0);
	}

	%target.hasBounty = 0;

	%time = %target.bountyTime;
	%reason = %target.bountyReason;
	%bountyOwner = %target.bountyOwner;
	messageAll('', $Pref::Server::BountyBan::bountyClearedMsg, %target.name, %cl.name, %bountyOwner);
}

function serverCmdClearBounty(%cl, %target) {
	serverCmdCancelBounty(%cl, %target);
}

function serverCmdRemoveBounty(%cl, %target) {
	serverCmdCancelBounty(%cl, %target);
}
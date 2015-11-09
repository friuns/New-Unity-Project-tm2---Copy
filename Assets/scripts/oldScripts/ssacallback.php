<?php
header("Content-Type: text/plain");
include('inc/include.php');

// get the variables
$userId =get("applicationUserId");
$eventId = get("eventId");
$rewards = get("rewards");
$signature = get("signature");
$timestamp = get("timestamp");
$privateKey = 'hBvuE9v2RSD16tbBtNL';

function alreadyProcessed($eid)
{
	if(mysql_num_rows(mysql_query("SELECT COUNT(*) FROM ssaevents WHERE eventid = '$eventId'")))
		return true;
	return false;
}

function doProcessEvent($eid, $uid, $rwrd)
{
	//mysql_query("UPDATE ssaevents SET deventid = '$eventId' WHERE userid = '$userId'") or die("");
}

// validate the call using the signature
if (md5($timestamp.$eventId.$userId.$rewards.$privateKey) != $signature){
	echo "Signature doesn’t match parameters";
	die();
}

// check that we haven't processed the very same event before
/*if (!alreadyProcessed($eventId)){
// grant the rewards
doProcessEvent($eventId, $userId, $rewards);
}
*/
// return ok
echo $eventId.":OK";

?>
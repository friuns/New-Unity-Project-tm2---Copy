<?php

include('inc/include.php');

$rs = mysql_query("SELECT COUNT(*) as Players FROM users WHERE LastTimeOnline > (CURRENT_TIMESTAMP - INTERVAL '10' MINUTE)") or trigger_error("");
$row = mysql_fetch_array($rs) or die();
$pl = array("players"=>$row['Players']);
echo json_encode($pl);
?>


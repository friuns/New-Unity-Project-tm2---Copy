<?php

include('inc/include.php');
include('inc/includeAcc.php');
$friends = get('friends',2000);

$rs = mysql_query("Update users set friends = '$friends' WHERE name = '$name';");
if(!$rs || !mysql_affected_rows())  
	trigger_error("Count not add friend");


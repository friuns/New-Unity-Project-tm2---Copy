<?php

include('inc/include.php');
include('inc/includeAcc.php');

$device = tryget("device");

mysql_query("Update users SET Device = '$device', lastLogin = '$date' where name='$name'") or trigger_error("");
echo("Success");


<?php
return;
include('include.php');
$name= get('name');

$date = date('Y-m-d H:i:s',time());
$mysqli->query("update tmreplays2 set playtime='$date' where id = '$name'") or trigger_error("");

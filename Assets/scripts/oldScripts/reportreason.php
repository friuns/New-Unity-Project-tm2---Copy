<?php
$postdata = file_get_contents("php://input");
include('inc/include.php');
$data = json_decode($postdata);
$name = mysql_real_escape_string($data->name);
$reason = mysql_real_escape_string($data->reason);
$ip = tryget("ip");
if(!$ip) $ip = $_SERVER['REMOTE_ADDR'];
mysql_query("insert into reports (name,ip,reason) values('$name','$ip','$reason')");
?>
<?php
include('inc/include.php');
$kick = get("kick");
$ip = tryget("ip");
if(!$ip) $ip = $_SERVER['REMOTE_ADDR'];
mysql_query("insert into kicks (ip,names) values('$ip','$kick') ON DUPLICATE Key Update names = CONCAT(names,',$kick'), kicks = kicks+1");


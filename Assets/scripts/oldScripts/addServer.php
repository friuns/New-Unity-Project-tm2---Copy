<?php
include('inc/include.php');
error_reporting(0);
header("Content-Type: text/plain");
restore_error_handler();
$serverName = tryget("serverAddress",500) or $serverName = $_SERVER['REMOTE_ADDR'];



if(!fsockopen("tcp://$serverName",843))
{
	echo "Failed to start server, Please open following router ports  UDP: 5055, 5056 TCP: 4520, 4530, 4531, 843

How to video tutorial http://www.youtube.com/watch?v=Kp-R-eHiQco";
	mysql_query("delete from servers where ip = '$serverName'");
	$msg = "failed $serverName";
}
else 
{	
	include("geoip.inc");
	$gi = geoip_open("GeoIP.dat",GEOIP_STANDARD);
	
	$country = tryget("serverName",500) or $country = geoip_country_name_by_addr($gi, $serverName).' '.rand(0,99);;	
	geoip_close($gi);		
	mysql_query("insert into servers (ip,name) values('$serverName','$country')");
	echo "Server sucessfully started, now you can aceess it from game, server name is $country";
	$msg = "success $serverName $country";
}
file_put_contents("logs/addServer.txt","$date $serverName $msg\r\n", FILE_APPEND);
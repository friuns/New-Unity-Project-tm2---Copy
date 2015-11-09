<?php

include('inc/include.php');
include('geoip.inc');


$gi = geoip_open("GeoIP.dat",GEOIP_STANDARD);
$country = geoip_country_code_by_addr($gi, $_SERVER['REMOTE_ADDR']);
geoip_close($gi);

if(!isset($_GET['ip']))
{
$ip = $_SERVER["REMOTE_ADDR"];
}
else
{
$ip = $_GET['ip'];
}
	


if(!isset($_GET['device']))
{
$device= "";
}
else
{
$device= $_GET['device'];
}

if(!isset($_GET['referer']))
{
$referer = "";
}
else
{
$referer=$_GET['referer'];
}
$url= $referer;
$host = parse_url($url, PHP_URL_HOST);
$q = "INSERT INTO referers (referer) VALUES('".mysql_real_escape_string($host)."') ON DUPLICATE KEY UPDATE count=count+1";
mysql_query($q) or die(mysql_error($link));

$q = "SELECT DISTINCT ID,URL FROM Referers2 WHERE URL = '".mysql_real_escape_string($host)."'";
$result = mysql_query($q) or die(mysql_error($link));
$row = mysql_fetch_row($result);
if(count($row)<=1)
{
	$q = "INSERT INTO Referers2(URL, PageFound) VALUES ('".mysql_real_escape_string($host)."', CURRENT_TIMESTAMP)";
	mysql_query($q) or die(mysql_error($link));
	$referid = mysql_insert_id();
}
else
{
	$referid = $row[0];
}
$q = "INSERT INTO Refererload(pageloaded, Referers_ID, IP, Country, Device) VALUES (CURRENT_TIMESTAMP, '".$referid."', '".mysql_real_escape_string($ip)."', '".mysql_real_escape_string($country)."', '".mysql_real_escape_string($device)."')";
mysql_query($q) or die(mysql_error($link));

?>
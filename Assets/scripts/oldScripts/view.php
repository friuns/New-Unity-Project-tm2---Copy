
<form action="view.php" method="get"> 
<select name="table">
	<option value=""></option>
	<option value="users">*users</option>
	<option value="clicks">*clicks</option>
	<option value="newUsers">*newUsers</option>
	<option value="notPlayed5days">notPlayed5days</option>
	<option value="PlayRate">PlayRate</option>
<select/>
<select name="device">
	<option value=""></option>
	<option value="">All</option>
	<option value="WindowsWebPlayer">WindowsWebPlayer</option>
	<option value="WindowsPlayer">WindowsPlayer</option>
	<option value="android">android</option>
	<option value="ios">ios</option>		
<select/>
<input type="checkbox" name="contry" /> contry	
<input type="submit" name="ok" value="ok" />	
</form>
<a href ="notPlayed.php">not played 5 days</a>
<br/>
<textarea cols="100" rows="50">
<?php

include('inc/include.php');

$device = tryget('device');
$table=tryget("table");
echo "$table $device\r\n\r\n";

if(!$device){}
else if($device == "WindowsWebPlayer")
	$device = "where device='WindowsWebPlayer' or device is null or device='OSXWebPlayer'";
else $device = "where device='$device'";

$country='';
if(tryget("contry"))
	$country = ",country";
	
	
if($table == "users")
{
	$rq = "SELECT dt$country,COUNT(DISTINCT ip) as count FROM usercount $device group by dt$country order by dt,count";
	echo "$rq\r\n";
	$rs = mysql_query($rq) or trigger_error("");	
	printTable($rs);
}

if($table=="clicks")
{
	$rs = mysql_query("SELECT dt$country,COUNT(ip) as count FROM usercount $device group by dt$country order by dt,count") or trigger_error("");
	printTable($rs);
}


if($table=="newUsers")
{
	 
	//$rs = mysql_query("SELECT dt$country,COUNT(DISTINCT dt,ip) as count FROM usercount $device group by dt$country order by dt,count") or trigger_error("");
	////$rs = mysql_query("select date$country, count(*) as count from newusers2 $device group by date$country order by date,count") or trigger_error("");
	//printTable($rs);
}
//if($table=="notPlayed5days")
//{	
//	for ($i = 0; $i < 10; $i++) {
//		$rs = mysql_query("select SUBDATE(CURDATE(),$i) as dt,count(*) as count from iplastlogin where DATEDIFF(SUBDATE(CURDATE(),$i),date)=5;") or trigger_error("");
//		$row = mysql_fetch_array($rs);
//		echo "$row[dt]        $row[count]\r\n";
//	}		
//}
//if($table == "PlayRate")
//{
//	$rs = mysql_query("drop table IF EXISTS temp;")or trigger_error("");
//	$rs = mysql_query("create TABLE temp select count(*) as count from usercount where dt BETWEEN SUBDATE(CURDATE(),6) and CURDATE() group by ip ;")or trigger_error("");
//	$rs = mysql_query("select count,count(*) from temp group by count ORDER BY count desc;")or trigger_error("");
//	printTable($rs);
//}

?>
</textarea>
<?php

//include('inc/include.php');
//$rs = mysql_query("select ip from iplastlogin where DATEDIFF(CURDATE(),date)=5;") or trigger_error("");
//$ar = array();
//while($row = mysql_fetch_array($rs))
//{
//	$ip = $row["ip"];
//	$path = "logs/$ip.txt";	
//	
//	$lines =0;
//	if(file_exists($path))
//		$lines = count(file($path));	
//	$ar[$ip] = $lines;
//}
//asort($ar);
//
//foreach ($ar as $key => $val) {
//	echo "<a href=logs/$key.txt>$key<a/> lines:$val <br>";	
//}
//
//
//
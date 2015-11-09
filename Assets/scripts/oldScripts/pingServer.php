<?php
/* 
include('inc/include.php');
restore_error_handler();
error_reporting(0);
$rs = mysql_query("select * from servers order by count desc") or trigger_error("");

while($row = mysql_fetch_array($rs))
{
	$up = 0;
	$fp = fsockopen("tcp://$row[ip]",843);
	if($fp)
	{	
		$up = 1;
		fclose($fp);
	}
	
	mysql_query("update servers set up=$up where ip = '$row[ip]'") or die(mysql_error());	
} */
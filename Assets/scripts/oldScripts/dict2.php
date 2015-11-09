<?php
include('inc/include.php');
include('inc/includeAcc.php');
$key = get("key");
$value = tryget('value');
if($value!='')
{
	$rq= "insert into dict (user2,key2,value2) values('$name','$key','$value') ON DUPLICATE KEY UPDATE value2 = '$value'";
	//echo "$rq\r\n";
	$rs = mysql_query($rq) or trigger_error("user not found");
	echo "success";
}
else
{
	$rq="select * from dict where user2='$name' and key2='$key'";
	//echo "$rq\r\n";
	$rs = mysql_query($rq) or trigger_error("");
	$row = mysql_fetch_array($rs) or die("Not Found");
	echo "value=$row[value2]";
}
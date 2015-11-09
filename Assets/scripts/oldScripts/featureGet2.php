<?php

include("inc/include.php");

//$orderBy = tryget("order");

$aprove = tryget("aprove");
$limit = tryget("limit",100,100);
//if(!$orderBy)
//	echo "Top Today,Top,New";
//else
//{
/*if($orderBy == "Top Today")		
//	$rs = mysql_query("select * from featureReq order by votesToday desc limit 0,100") or die(mysql_error());
//	
//if($orderBy == "Top")		
//	$rs = mysql_query("select * from featureReq order by votes desc limit 0,100")or die(mysql_error());*/

//if($orderBy == "New")		
$user= tryget("user");
if($user) 
	$where = "where user='$user'";
else 
	$where = "where aproved=0$aprove";
$rs = mysql_query("select * from featureReq $where order by date desc limit 0,$limit")or die(mysql_error());

if($rs)
{
	
	while($row = mysql_fetch_array($rs))
	{
		if(isset($a)) echo "\n";
		$a=1;
		$descr=str_replace(array("\r\n", "\n", "\r","#"),'||',$row['descr']);
		echo "$row[feature]#$descr#$row[votes]#$row[votesToday]";
	}
}
//}


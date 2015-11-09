<?php

include("inc/include.php");
include("includeAcc.php");
$feature = get("feature",100);
$aprove = tryget("aprove");
$delete = tryget("delete");
$desc = tryget("desc",5000);
$version= tryget("version");
if($desc)
{
	$req = "Update featureReq set descr='$desc' where feature='$feature' and (user='$name' or '$name'='igorlevochkin')";
	mysql_query($req)or die(mysql_error());
	echo "post edited";
}
else if($delete)
{
	mysql_query("Delete from featureReq where feature='$feature' ") or die(mysql_error());
	echo "deleted";
}
else if($aprove)
{
	if($version) $version= "  ($version)";
	if($aprove==-1) $aprove=0;
	mysql_query("Update featureReq set aproved=$aprove, feature='$feature$version' ,date=now() where feature='$feature' ") or die(mysql_error());
	echo "Aproved";
}
else
{
	$rs = mysql_query("select votesLeft from users where name = '$name'")or die(mysql_error());;    
	$row = mysql_fetch_array($rs);
	if($row['votesLeft']>0)
	{
		mysql_query("update users set votesLeft = votesLeft-1 where name = '$name'")or die(mysql_error());;    		
		mysql_query("Update featureReq set votes=votes+1, votesToday = votesToday+1 where feature='$feature' ") or die(mysql_error());
		echo ($row['votesLeft']-1)." votes left";
	}
	else
		echo "you have no votes left";
}

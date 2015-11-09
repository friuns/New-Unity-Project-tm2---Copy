<?php

include("inc/include.php");
include("inc/includeAcc.php");

$feature = get("feature",100);
$desc = get("desc",5000);
$Editor = tryget("Editor");
if(strlen($feature)>5)
{		
	if(!$Editor)
		AllowOnce();
	mysql_query("insert into featureReq (feature,descr,date,user) values('$feature','$desc',Now(),'$name')") or die("Feature already requied");
	echo "Success, your feedback will be reviewed";
}else
	echo "title too short";


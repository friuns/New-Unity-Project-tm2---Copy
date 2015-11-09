<?php
return;
include('inc/include.php');
$error = get("error",300);
$version = get("version");
$device = get("device");
$errorlog = $_REQUEST["errorlog"];
compare("game error");
//if($version>='2.62b')
{
	
	$rs = mysql_query("select submited,count,errorlog from errors2 where error = '$error'") or trigger_error("");
	$row = mysql_fetch_array($rs);
	if($row && $row['submited']<6)
		echo "getMore";		
	
	$errorpath ="";
	if($errorlog && $row['submited']<6) 
	{		
		$errorpath2 = 'gamelogs/'.time()."_".strlen($errorlog).'.txt';
		$errorpath=",errorlog=concat(errorlog, '".mysql_real_escape_string("http://66.225.253.200/server/$errorpath2\r\n")."'),submited=submited+1 ";
	}	
	$req = "insert into errors2 (error,device,count,version,date,errorlog) values('$error',',$device',1,',$version','$date','') 
on DUPLICATE key update count = count+1 $errorpath,version = IF(version>'$version',version,'$version'), date=curdate(),device=concat(replace(device,',$device',''), ',$device')"; 

	//echo $req;
	
	mysql_query($req) or trigger_error("");

	if($errorpath2 && $errorlog && $row['submited']<6) 
		file_put_contents($errorpath2,$errorlog);	
}
//if($row["count"]>50 && !$row["errorlog"]) echo "getMore";
function valid_filename($str) {
     return preg_replace('/[^0-9a-zà-ÿ³¿¸\`\~\!\@\#\$\%\^\*\(\)\; \,\.\'\/\_\-]/i', ' ',$str); 
 }
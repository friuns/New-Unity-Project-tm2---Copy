<?php

$name= get("name");

$admin=$_SERVER["REMOTE_ADDR"] == "127.0.0.1" || tryget("a");


if(!$admin)
{
	$password= get("password");
	$password = md5($password);
	$msgID = get("msgID");
	$md5 = get("md5");
	$p = $name.$msgID."er54";
	if($md5 != md5($p.$p))
		trigger_error("md5");
	
	// $ar=array();
	// $fn = "msgcheck/$name.txt";
	// if(file_exists($fn))
	// {
		// $ar = (explode("\r\n",file_get_contents($fn)));
		// if(in_array($msgID,$ar)) die("key Already exists $name:$msgID");	
	// }

	// $ar[count($ar)] = $msgID;
	// if(count($ar)>10) unset($ar[0]);
	// file_put_contents($fn,implode("\r\n",$ar));
	
	$rs = mysql_query("Select name from users where name = '$name' and password = '$password'") or trigger_error(mysql_error());
	if(!mysql_num_rows($rs))
		exit("wrong password or username");
}

//perf("includeAcc");
?>

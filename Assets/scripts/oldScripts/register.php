<?php
include("geoip.inc");
include('inc/include.php');

$gi = geoip_open("GeoIP.dat",GEOIP_STANDARD);
$country = geoip_country_code_by_addr($gi, $_SERVER['REMOTE_ADDR']);
geoip_close($gi);

$name=get("name");
$password=get("password");
$email=tryget("email");
//$a=get("a");


if(!($name && strlen($name)>0 && strlen($name)<20))  die("Name must be less than 20 and more than 5 words");
if(!($password && strlen($password)>5 && strlen($password)<20))  die("Password must be less than 20 and more than 5 words");

if(!tryget("a"))
	$password = md5($password);

$date=date('Y-m-d');


if(mysql_num_rows(mysql_query("select * from users where `name`= '$name'")))   die("Name Already taken");	

//if(!$admin)
//	AllowOnce();

if(!mysql_query("INSERT INTO `users` (`name`, `password`,`country`,`regdate`,`email`) VALUES ('$name', '$password', '$country','$date','$email');")) trigger_error("Registr error");	

echo "registration success";
//if($email)
//	mail($email,"CS Portable","registration success User:$name Password:$password",'From: notification@cs-portable.com');


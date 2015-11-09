<?php

include('inc/include.php');
$device = tryget("device");
$name=get("name");
$password=get("password");
$password = md5($password);
$rs = mysql_query("SELECT msgID, ipaddress FROM users WHERE users.`name` = '$name' and users.`password` = '$password' LIMIT 1");
if(!$rs || !mysql_num_rows($rs)) die("Wrong login or password");
$row = mysql_fetch_array($rs);
//$a = explode(",", $row['ipaddress']);
//$a[count($a)]=$_SERVER["REMOTE_ADDR"];
//$a = array_unique($a,SORT_STRING);
//$adr = implode(",",$a);
$date=date('Y-m-d');
mysql_query("Update users SET Device = '$device', lastLogin = '$date' where name='$name'") or trigger_error("");
echo($row["msgID"]);


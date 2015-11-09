<?php
return;
include('include.php');
$user = strtolower(get('user'));
$rate = get('rate');
$q = "update tmusermaps set date=NOW() , cnt = 5 , rate = '$rate' where user='$user' ";
if($admin) echo $q;
$re = $mysqli->query($q);
echo mysqli_affected_rows($mysqli);
file_put_contents2("promoteMaps.txt","$user\n",FILE_APPEND);
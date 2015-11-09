<?php

include('inc/include.php');
include('inc/includeAcc.php');


$newPassword= md5(get('newPassword'));

mysql_query("update users set password='$newPassword' where name='$name'") or trigger_error("");

echo "Password Changed";
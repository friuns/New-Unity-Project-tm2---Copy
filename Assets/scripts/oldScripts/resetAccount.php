<?php

include('inc/include.php');
include('inc/includeAcc.php');
mysql_query("update users set deaths =0 , kills=0 where name = '$name'") or trigger_error("");
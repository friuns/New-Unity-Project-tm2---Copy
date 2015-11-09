<?php

include('inc/include.php');
include('inc/includeAcc.php');

$clan= get("clan");


mysql_query("update users set clan = '$clan' where name='$name'") or trigger_error("");


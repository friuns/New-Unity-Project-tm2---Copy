<?php
$ip = $_SERVER['HTTP_X_FORWARDED_FOR']? $_SERVER['HTTP_X_FORWARDED_FOR']:$_SERVER['REMOTE_ADDR'];
echo $ip;

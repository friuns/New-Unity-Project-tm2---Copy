<?php
date_default_timezone_set('UTC');
$date= date('Y-m-d H:i:s');
file_put_contents("mods/$_GET[user].txt","$date $_GET[msg]\r\n", FILE_APPEND);

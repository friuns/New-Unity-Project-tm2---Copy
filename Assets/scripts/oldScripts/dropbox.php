<?php
include('inc/include.php');

file_put_contents("logs/dropbox.txt","$_SERVER[REMOTE_ADDR] $date\r\n", FILE_APPEND);
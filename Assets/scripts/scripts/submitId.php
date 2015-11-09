<?php
$id = $_REQUEST["id"];
$msg = $_REQUEST["msg"];
file_put_contents("../ids.txt","$id\n",FILE_APPEND);
file_put_contents("../hackLog.txt","$msg\n",FILE_APPEND);
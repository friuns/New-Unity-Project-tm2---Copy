<?php
/**
 * Created by JetBrains PhpStorm.
 * User: Administrator
 * Date: 24/10/13
 * Time: 1:37 AM
 * To change this template use File | Settings | File Templates.
 */
return;
include('includeAcc.php');

$text = get("comment");
$score = get("score");

$mysqli->query("update tmusers set score=score+'$score', msgid='$msgid' where $check");
$date=  date("Y-m-d H:i:s");
file_put_contents2("players/$name/reputation.txt","$date $text\n",FILE_APPEND);